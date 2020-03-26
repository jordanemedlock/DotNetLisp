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
        [DebuggerStepThrough]
        public static Compiler<T, V> Bind<T, U, V>(this Compiler<T, U> compiler, Func<U, Compiler<T, V>> func)
        {
            return input =>
            {
                var results = compiler(input);
                return results.SelectMany(res =>
                {
                    var innerCompiler = func(res.Value);
                    return innerCompiler(res.Remainder);
                }).ToList();
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
                return results.Select(res =>
                {
                    return new CompilerResult<T, V>()
                    {
                        Value = func(res.Value),
                        Remainder = res.Remainder
                    };
                }).ToList();
            };
        }


        public static Compiler<T, U> Where<T, U>(this Compiler<T, U> compiler, Predicate<U> predicate)
        {
            return input =>
            {
                return compiler(input).Where(res => predicate(res.Value)).ToList();
            };
        }


        public static Compiler<T, V> Is<T, U, V>(this Compiler<T, U> compiler)
            where V : class
        {
            return compiler.Where(x => x is V).Select(x => x as V);
        }



        public static Compiler<T, U> Or<T, U>(this Compiler<T, U> compiler, Compiler<T, U> other)
        {
            return input => compiler(input).Concat(other(input)).ToList();
        }

        public static Compiler<T, V> Apply<T, U, V>(this Compiler<T, U> compiler, Compiler<T, V> other)
        {
            return compiler.Bind(x => other);
        }

        public static U Compile<T, U>(this Compiler<T, U> compiler, T input)
        {
            return compiler(input).First().Value;
        }

        public static Compiler<T, V> Return<T, U, V>(this Compiler<T, U> compiler, V value)
        {
            return compiler.Apply(JEM.Compile.Compile.Return<T,V>(value));
        }
    }
}
