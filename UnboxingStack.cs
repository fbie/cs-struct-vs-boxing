using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace StackExperiments
{
    public readonly struct StackValue
    {
        private readonly long _int64;
        private readonly object _reference;

        public StackValue(object reference)
        {
            _int64 = 0L;
            _reference = reference;
        }

        public StackValue(long n)
        {
            _int64 = n;
            _reference = IsLongTag;
        }

        public StackValue(double d)
        {
            _int64 = BitConverter.DoubleToInt64Bits(d);
            _reference = IsDoubleTag;
        }

        public object AsReference() => _reference;

        public long AsLong() => _int64;

        public double AsDouble() => BitConverter.Int64BitsToDouble(_int64);

        private static readonly object IsLongTag = new object();
        public bool IsLong() => _reference == IsLongTag;

        private static readonly object IsDoubleTag = new object();
        public bool IsDouble() => _reference == IsDoubleTag;

        private bool IsReference() => !IsLong() && !IsDouble();

        public static readonly StackValue Null = new StackValue(0);
    }

    public interface IStack<T> {
        void Push(object o);
        void Push(long n);
        void Push(double d);
        T Pop();
    }

    public class UnboxingStack : IStack<StackValue>
    {
        private readonly StackValue[] _stack = new StackValue[1024];
        private int _index = -1;

        public bool TopIsLong() => _stack[_index].IsLong();

        public bool TopIsDouble() => _stack[_index].IsDouble();

        public StackValue Pop()
        {
            var x = _stack[_index];
            _stack[_index] = StackValue.Null;
            _index--;
            return x;
        }

        private void Push(StackValue v) => _stack[++_index] = v;

        public void Push(long n) => Push(new StackValue(n));

        public void Push(double d) => Push(new StackValue(d));

        public void Push(object o) => Push(new StackValue(o));
    }

    public class BoxingStack : IStack<object>
    {
        private readonly object[] _stack = new object[1024];
        private int _index = -1;

        public bool TopIsLong() => _stack[_index] is long;

        public bool TopIsDouble() => _stack[_index] is double;

        public object Pop()
        {
            var x = _stack[_index];
            _stack[_index] = null;
            _index--;
            return x;
        }

        public void Push(long n) => Push((object)n);

        public void Push(double d) => Push((object)d);

        public void Push(object o) => _stack[++_index] = o;
    }

    public class Program
    {
        private Random _rnd;
        private const int N = 1024;

        [Params(10, 25)]
        public int PDouble { get; set; }

        [Params(10, 25, 50)]
        public int PLong { get; set; }

        public double Run<T>(int ops, IStack<T> stack, Func<T, double> toDouble)
        {
            var acc = 0.0D;
            for (var j = 0; j < 1000; j++)
            {
                for (var i = 0; i < ops; i++)
                {
                    var p = _rnd.NextInt64(100);
                    if (p < PDouble) {
                        stack.Push(_rnd.NextDouble());
                    } else if (p < PDouble + PLong) {
                        stack.Push(_rnd.NextInt64(1_000_000));
                    } else {
                        stack.Push(new object());
                    }
                }
                for (var i = 0; i < ops; i++)
                {
                    acc *= toDouble(stack.Pop());
                }
            }
            return acc;
        }

        [GlobalSetup]
        public void Setup()
        {
            _rnd = new Random(42);
        }


        [Benchmark]
        public double Unboxing() => Run(N, new UnboxingStack(), x => {
            if (x.IsDouble()) return x.AsDouble();
            if (x.IsLong()) return x.AsLong();
            return x.AsReference().GetHashCode();
            });

        [Benchmark(Baseline = true)]
        public double Boxing() => Run(N, new BoxingStack(), x => {
            switch (x) {
                case double d: return d;
                case long n: return n;
                default: return x.GetHashCode();
            }
            });

        public static void Main(string[] args)
            => BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args);
    }
}
