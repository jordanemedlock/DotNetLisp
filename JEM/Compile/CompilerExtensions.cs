using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public static class CompilerExtensions
    {
        public static Compiler<T, V> Bind<T, U, V>(this Compiler<T, U> compiler, Func<U, Compiler<T, V>> func)
        {
            return input =>
            {
                var res = compiler(input);
                return res.Bind(val =>
                {
                    var innerCompiler = func(val);
                    var ret = innerCompiler(res.Remainder);
                    return ret;
                });
            };
        }

        public static Compiler<T, V> SelectMany<T, U, V>(this Compiler<T, U> compiler, Func<U, Compiler<T, V>> func)
        {
            return compiler.Bind<T, U, V>(func);
        }

        public static Compiler<T, V> Select<T, U, V>(this Compiler<T, U> compiler, Func<U, V> func)
        {
            return input =>
            {
                var results = compiler(input);
                return results.Select(func);
            };
        }


        public static Compiler<T, U> Where<T, U>(this Compiler<T, U> compiler, Predicate<U> predicate, string errorMessage)
        {
            return input =>
            {
                return compiler(input).Where(res => predicate(res.Value), errorMessage);
            };
        }


        public static Compiler<T, V> Is<T, U, V>(this Compiler<T, U> compiler)
            where V : class
        {
            return compiler.Where(x => x is V, $"value is not {typeof(V)}").Select(x => x as V);
        }



        public static Compiler<T, U> Or<T, U>(this Compiler<T, U> compiler, Compiler<T, U> other, string identifier = null)
        {
            return input =>
            {
                var results1 = compiler(input);
                if (!results1.HasValue)
                {
                    var val = other(input);
                    val.HappyPath.Add(identifier);
                    return val;
                }
                else
                {
                    return results1;
                }
            };
        }

        public static Compiler<T, U> Or<T, U>(params Compiler<T, U>[] compilers)
        {
            return input =>
            {
                CompilerResult<T, U> result;
                int i = 0;
                do
                {
                    result = compilers[i++](input);
                }
                while (!result.HasValue && i < compilers.Length);
                return result;
            };
        }

        public static Compiler<T, U> Or<T, U>(string name, params Compiler<T, U>[] compilers)
        {
            return input =>
            {
                CompilerResult<T, U> result = new CompilerResult<T, U>($"{name} failed on {input}, no compiler returned value");
                int i = 0;
                do
                {
                    result = compilers[i++](input);
                }
                while (!result.HasValue && i < compilers.Length);
                result.HappyPath.Add($"{name}[{i - 1}]");
                return result;
            };
        }

        public static Compiler<T, V> Apply<T, U, V>(this Compiler<T, U> compiler, Compiler<T, V> other)
        {
            return compiler.Bind(x => other);
        }

        public static U Compile<T, U>(this Compiler<T, U> compiler, T input)
        {
            return compiler(input).Value;
        }

        public static Compiler<T, V> Return<T, U, V>(this Compiler<T, U> compiler, V value)
        {
            return compiler.Apply(JEM.Compile.Compile.Return<T,V>(value));
        }

    }
}
