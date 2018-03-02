To run this benchmark execute `run.cmd` from the commandline.

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.248)
Intel Xeon CPU E5-1620 0 3.60GHz, 1 CPU, 8 logical cores and 4 physical cores
Frequency=3507174 Hz, Resolution=285.1299 ns, Timer=TSC
  [Host] : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  Clr    : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0

Job=Clr  Runtime=Clr

```
|         Method |      Mean |     Error |    StdDev |    Median | Scaled | ScaledSD |
|--------------- |----------:|----------:|----------:|----------:|-------:|---------:|
| EmitSequential | 160.32 ms | 3.2181 ms | 8.0141 ms | 157.21 ms |   1.00 |     0.00 |
|   EmitParallel |  58.74 ms | 0.9276 ms | 0.8677 ms |  58.94 ms |   0.37 |     0.02 |
