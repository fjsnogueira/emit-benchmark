To run this benchmark execute `run.cmd` from the commandline.

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.248)
Intel Xeon CPU E5-1620 0 3.60GHz, 1 CPU, 8 logical cores and 4 physical cores
Frequency=3507174 Hz, Resolution=285.1299 ns, Timer=TSC
  [Host] : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0

Job=Clr  Runtime=Clr

```
         Method |       Mean |     Error |     StdDev | Scaled |
--------------- |-----------:|----------:|-----------:|-------:|
 EmitSequential | 1,926.2 ms | 38.967 ms | 109.906 ms |   1.00 |
   EmitParallel |   336.4 ms |  5.651 ms |   5.286 ms |   0.18 |
