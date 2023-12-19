## Storing value types and reference types in a uniform way ##

The idea is to avoid boxing and instead use a 16 byte struct for each value.

Unfortunately (?) .NET doesn't allow overlapping value type fields and
reference type fields.

Platform:
```
BenchmarkDotNet v0.13.11, Ubuntu 20.04.6 LTS (Focal Fossa)
Intel Core i7-5600U CPU 2.60GHz (Broadwell), 1 CPU, 4 logical and 2 physical cores
.NET SDK 6.0.414
[Host]     : .NET 6.0.22 (6.0.2223.42425), X64 RyuJIT AVX2
DefaultJob : .NET 6.0.22 (6.0.2223.42425), X64 RyuJIT AVX2
```

| Method   | PDouble | PLong | Mean     | Error   | StdDev  | Median   | Ratio | RatioSD | Gen0       | Allocated | Alloc Ratio |
|--------- |-------- |------ |---------:|--------:|--------:|---------:|------:|--------:|-----------:|----------:|------------:|
| Unboxing | 10      | 10    | 109.3 ms | 2.18 ms | 5.93 ms | 109.0 ms |  1.00 |    0.08 |  9400.0000 |  18.78 MB |        0.80 |
| Boxing   | 10      | 10    | 109.1 ms | 2.18 ms | 6.07 ms | 106.9 ms |  1.00 |    0.00 | 11600.0000 |  23.45 MB |        1.00 |
|          |         |       |          |         |         |          |       |         |            |           |             |
| Unboxing | 10      | 25    | 104.1 ms | 0.18 ms | 0.15 ms | 104.0 ms |  0.97 |    0.01 |  7600.0000 |  15.25 MB |        0.65 |
| Boxing   | 10      | 25    | 107.3 ms | 0.57 ms | 0.53 ms | 107.4 ms |  1.00 |    0.00 | 11600.0000 |  23.45 MB |        1.00 |
|          |         |       |          |         |         |          |       |         |            |           |             |
| Unboxing | 10      | 50    | 105.5 ms | 0.83 ms | 0.77 ms | 105.6 ms |  0.92 |    0.01 |  4600.0000 |   9.39 MB |        0.40 |
| Boxing   | 10      | 50    | 115.2 ms | 0.42 ms | 0.37 ms | 115.2 ms |  1.00 |    0.00 | 11600.0000 |  23.45 MB |        1.00 |
|          |         |       |          |         |         |          |       |         |            |           |             |
| Unboxing | 25      | 10    | 101.5 ms | 0.41 ms | 0.34 ms | 101.6 ms |  0.96 |    0.01 |  7600.0000 |  15.25 MB |        0.65 |
| Boxing   | 25      | 10    | 105.8 ms | 0.97 ms | 0.86 ms | 105.4 ms |  1.00 |    0.00 | 11600.0000 |  23.45 MB |        1.00 |
|          |         |       |          |         |         |          |       |         |            |           |             |
| Unboxing | 25      | 25    | 103.6 ms | 0.36 ms | 0.30 ms | 103.7 ms |  0.73 |    0.02 |  5800.0000 |  11.74 MB |        0.50 |
| Boxing   | 25      | 25    | 141.5 ms | 2.71 ms | 4.53 ms | 143.3 ms |  1.00 |    0.00 | 11750.0000 |  23.45 MB |        1.00 |
|          |         |       |          |         |         |          |       |         |            |           |             |
| Unboxing | 25      | 50    | 103.8 ms | 0.58 ms | 0.51 ms | 103.7 ms |  0.95 |    0.01 |  2800.0000 |   5.89 MB |        0.25 |
| Boxing   | 25      | 50    | 109.7 ms | 0.80 ms | 0.75 ms | 109.4 ms |  1.00 |    0.00 | 11600.0000 |  23.45 MB |        1.00 |
