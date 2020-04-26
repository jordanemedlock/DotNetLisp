using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    //public delegate CompilerResult<TInput, TOutput> Compiler<TInput, TOutput>(TInput input);

    public class Compiler<TInput, TOutput>
    {
        public string Name { get; set; }

        public Func<TInput, CompilerResult<TInput, TOutput>> InternalCompile { get; set; }

        public Func<Compiler<TInput, TOutput>> CompilerGenerator { get; set; }

        public Compiler(Func<TInput, CompilerResult<TInput, TOutput>> compile) {
            InternalCompile = compile;
        }
        

        public Compiler(string name, Func<TInput, CompilerResult<TInput, TOutput>> compile)
        {
            Name = name;
            InternalCompile = compile;
        }

        public Compiler(string name, Func<Compiler<TInput, TOutput>> func)
        {
            
            CompilerGenerator = func;
            Name = name;
        }

        public CompilerResult<TInput, TOutput> Compile (TInput input) {
            if (InternalCompile == null) {
                Generate();
            }
            return InternalCompile(input);
        }

        public void Generate() {
            if (CompilerGenerator != null) {
                var compiler = CompilerGenerator();
                Name = "(" + Name + " = " + compiler.Name + ")";
                InternalCompile = compiler.Compile;
            }
        }
        
        public Compiler<TInput, V> Bind<V>(Func<TOutput, Compiler<TInput, V>> func)
        {
            return new Compiler<TInput, V>($"{Name}", input =>
            {
                var res = this.Compile(input);
                return res.Bind(val =>
                {
                    var innerCompiler = func(val);
                    var ret = innerCompiler.Compile(res.Remainder);
                    return ret;
                });
            });
        }

        public Compiler<TInput, V> Apply<V>(Compiler<TInput, V> other)
        {
            return new Compiler<TInput, V>($"{Name} {other.Name}", input =>
            {
                var res = this.Compile(input);
                if (!res.HasValue)
                {
                    return new CompilerResult<TInput, V>(res.Error);
                } 
                else
                {
                    return other.Compile(res.Remainder);
                }
            });
        }

        public Compiler<TInput, V> Select<V>(Func<TOutput, V> func)
        {
            return new Compiler<TInput, V>($"{Name}", input =>
            {
                var res = this.Compile(input);
                return res.Select(func);
            });
        }

        public Compiler<TInput, V> Return<V>(V value)
        {
            return new Compiler<TInput, V>($"{Name}=>{value}", input =>
            {
                var res = this.Compile(input);
                return res.Return(value);
            });
        }

        public Compiler<TInput, TOutput> Where(Predicate<TOutput> pred, string name = "")
        {
            return new Compiler<TInput, TOutput>($"{(string.IsNullOrEmpty(name) ? Name : name)}", input =>
            {
                var res = this.Compile(input);
                return res.Where(r => pred(r.Value), name);
            });
        }

        public Compiler<TInput, V> Is<V>() where V : class
        {
            return this.Where(x => x is V, $"{typeof(V).Name}").Select(x => x as V);
        }

        public Compiler<TInput, TOutput> Or(Compiler<TInput, TOutput> other)
        {
            return new Compiler<TInput, TOutput>($"{Name} | {other.Name}", input =>
            {
                var results1 = this.Compile(input);
                if (!results1.HasValue)
                {
                    var results2 = other.Compile(input);
                    results2.HappyPath.Add(other.Name);
                    return results2;
                }
                else
                {
                    results1.HappyPath.Add(this.Name);
                    return results1;
                }
            });
        }


        public static Compiler<TInput, TOutput> Or(params Compiler<TInput, TOutput>[] compilers)
        {
            return new Compiler<TInput, TOutput>($"{string.Join(" | ", compilers.Select(x => x.Name))}", input =>
            {
                CompilerResult<TInput, TOutput> result;
                int i = 0;
                do
                {
                    result = compilers[i++].Compile(input);
                }
                while (!result.HasValue && i < compilers.Length);
                return result;
            });
        }

        public static Compiler<TInput, TOutput> Return(TOutput output)
        {
            return new Compiler<TInput, TOutput>($"=>{(output != null ? output.ToString() : "null")}", input =>
            {
                return new CompilerResult<TInput, TOutput>(output, input);
            });
        }

        public static Compiler<TInput, TOutput> Error(string message)
        {
            return new Compiler<TInput, TOutput>(input =>
            {
                return new CompilerResult<TInput, TOutput>(message);
            });
        }
    }
}
