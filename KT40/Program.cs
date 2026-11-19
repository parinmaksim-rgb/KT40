using System;

const int Gen0 = 0;
const int Gen1 = 1;
const int Gen2 = 2;
const int SmallObjectCount = 200_000;
const int SmallObjectSizeBytes = 50;
const int LargeObjectCount = 5;
const int LargeObjectSizeBytes = 90_000;
const int InitialGeneration = -1;
const long SinkGuardValue = -1;
const double BytesPerKilobyte = 1024.0;
const double BytesPerMegabyte = 1024.0 * 1024.0;

int g0Start = GC.CollectionCount(Gen0);
int g1Start = GC.CollectionCount(Gen1);
int g2Start = GC.CollectionCount(Gen2);
long heapBefore = GC.GetGCMemoryInfo().HeapSizeBytes;
long allocBefore = GC.GetTotalAllocatedBytes(true);

Console.WriteLine(" До начала экспериментов ");
PrintState(g0Start, g1Start, g2Start, heapBefore, allocBefore);

long sink = 0;
for (int i = 0; i < SmallObjectCount; i++)
{
    byte[] shortLived = new byte[SmallObjectSizeBytes];
    shortLived[0] = (byte)i;
    sink += shortLived[0];
}

int g0Small = GC.CollectionCount(Gen0);
int g1Small = GC.CollectionCount(Gen1);
int g2Small = GC.CollectionCount(Gen2);
long heapSmall = GC.GetGCMemoryInfo().HeapSizeBytes;
long allocSmall = GC.GetTotalAllocatedBytes(true);

Console.WriteLine($"\n После {SmallObjectCount:N0} массивов new byte[{SmallObjectSizeBytes}] ");
PrintState(g0Small, g1Small, g2Small, heapSmall, allocSmall);
PrintDelta(g0Small - g0Start, g1Small - g1Start, g2Small - g2Start, allocSmall - allocBefore);

int largeObjGen = InitialGeneration;
for (int i = 0; i < LargeObjectCount; i++)
{
    byte[] largeObj = new byte[LargeObjectSizeBytes];
    largeObj[0] = (byte)i;
    largeObjGen = GC.GetGeneration(largeObj);
    sink += largeObj[0];
}

int g0Large = GC.CollectionCount(Gen0);
int g1Large = GC.CollectionCount(Gen1);
int g2Large = GC.CollectionCount(Gen2);
long heapAfter = GC.GetGCMemoryInfo().HeapSizeBytes;
long allocAfter = GC.GetTotalAllocatedBytes(true);

Console.WriteLine($"\n После {LargeObjectCount} массивов new byte[{LargeObjectSizeBytes:N0}] (LOH, Gen{largeObjGen}) ");
PrintState(g0Large, g1Large, g2Large, heapAfter, allocAfter);
PrintDelta(g0Large - g0Small, g1Large - g1Small, g2Large - g2Small, allocAfter - allocSmall);

Console.WriteLine("\n Итог (до и после) ");
Console.WriteLine($"Сборки всего:  +Gen0={g0Large - g0Start}, +Gen1={g1Large - g1Start}, +Gen2={g2Large - g2Start}");
Console.WriteLine($"HeapSizeBytes: {heapBefore:N0} -> {heapAfter:N0} байт ({heapAfter / BytesPerKilobyte:F2} КБ)");
Console.WriteLine($"Аллоцировано:  {(allocAfter - allocBefore) / BytesPerMegabyte:F2} МБ");

if (sink == SinkGuardValue)
    Console.WriteLine(sink);

void PrintState(int gen0, int gen1, int gen2, long heapBytes, long allocBytes)
{
    Console.WriteLine($"Счётчики:     Gen0={gen0}, Gen1={gen1}, Gen2={gen2}");
    Console.WriteLine($"HeapSize:     {heapBytes:N0} байт ({heapBytes / BytesPerKilobyte:F2} КБ)");
    Console.WriteLine($"Аллоцировано: {allocBytes:N0} байт ({allocBytes / BytesPerMegabyte:F2} МБ)");
}

void PrintDelta(int dGen0, int dGen1, int dGen2, long dAlloc)
{
    Console.WriteLine($"Добавилось:   +Gen0={dGen0}, +Gen1={dGen1}, +Gen2={dGen2}, +Alloc={dAlloc / BytesPerMegabyte:F2} МБ");
}