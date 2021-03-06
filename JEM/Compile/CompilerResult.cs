﻿using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;

namespace JEM.Compile
{
    public class CompilerResult<TInput, TOutput>
    {
        public TOutput Value { get; set; }

        public TInput Remainder { get; set; }

        public string Error { get; set; }

        public List<string> HappyPath { get; set; } = new List<string>();

        public bool HasValue { get; set; } = true;

        public CompilerResult(TOutput value, TInput remainder)
        {
            Value = value;
            Remainder = remainder;
            HasValue = true;
        }

        public CompilerResult(string errorMessage)
        {
            Error = errorMessage;
            HasValue = false;
        }

        public CompilerResult() { }

        public CompilerResult<TInput, U> Select<U>(Func<TOutput, U> func)
        {
            if (HasValue)
            {
                return new CompilerResult<TInput, U>(func(Value), Remainder);
            }
            else
            {
                return new CompilerResult<TInput, U>(Error);
            }
        }

        public CompilerResult<TInput, TOutput> Or(CompilerResult<TInput, TOutput> other)
        {
            if (!HasValue)
            {
                return other;
            }
            else
            {
                return this;
            }
        }

        public CompilerResult<TInput, U> Bind<U>(Func<TOutput, CompilerResult<TInput, U>> func)
        {
            if (HasValue)
            {
                return func(Value);
            }
            else
            {
                return new CompilerResult<TInput, U>(Error);
            }
        }

        public CompilerResult<TInput, TOutput> Where(Predicate<CompilerResult<TInput, TOutput>> predicate, string errorMessage)
        {
            if (this.HasValue && predicate(this))
            {
                return this;
            }
            else
            {
                return new CompilerResult<TInput, TOutput>(String.Format(errorMessage, this.Value));
            }
        }

        public CompilerResult<TInput, V> Return<V>(V value)
        {
            if (HasValue)
            {
                return new CompilerResult<TInput, V>(value, Remainder);
            }
            else
            {
                return new CompilerResult<TInput, V>(Error);
            }
        }

        public CompilerResult<TInput, V> CastEmpty<V>() 
        {
            return new CompilerResult<TInput, V>(this.Error);
        }
    }
}
