# What's Lisp.net

Lisp.net is a compiled lisp-like language used for meta-programming.  

## Goals for Lisp.net

Be immediately useful. 
: Compiles to .net IL so it can be used anywhere C# or F# are used.

Create experimental IDE features.
: A meta language which can write and compile itself can be used to create many experimental IDE features.

## Steps to creating this language

1. Create a minimal Lisp -> IL compiler.  A complete IL definition and that's it.
2. Plan extra (non-meta) language features and add them as a pre-processor step.
3. Bootstrap the compiler in Lisp.net.
4. Create the meta systems for the language.
5. Use the meta systems to implement more complex language features.

## TODO

1. Change compiler names to BNF.
2. Keep working down the list for -> IL.