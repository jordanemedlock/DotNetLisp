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

- Define language syntax
- Compile Hello World
- Write pre compiler

## Syntax

### ClassDecl

Notes: All of the class items inside ClassDecl are optional and can have at most one of each.
They can be in any order as well, although I think this should be the prefered order.

Justification: I want the syntax to be simple and descriptive.  And I want to keep a pattern of
"What it is" followed by its definition.  That way the process is "What to I want to make" => "What do I want to name it" => "What do I want it to be". Eg. (class Name (fields ...)).  Also I want to group fields with fields and methods with methods so that the classes can't become entirely unstructured. I want to enforce some grouping.

TODO: finish GenericsDef to include type conditions.
TODO: add properties (I don't really want them to be separate from fields, because they are semantically similar)
TODO: Attributes. Don't super care about these yet. 

```Lisp

ClassDecl := (class Id
    [(access AccessModifier)]
    [(generics GenericsDef)]
    [(implements ClassRef+)]
    [(extends ClassRef)]
    [(fields FieldDecl+)]
    [(methods MethodDecl+)]
)

AccessModifier := public | private

GenericsDef := Id+

ClassRef := Id [`<`TypeRef+`>`]
```

### FieldDecl

TODO: Add more to FieldInit. This has interesting loading rules, not sure what rules will need to be considered.

```Lisp

FieldDecl := (Id `:` TypeRef [`=` FieldInit])

FieldInit := ConstantValue

```

### MethodDecl

Notes: I want method decl to be small and similar to lambda creation. I think I want it to be default functional with explicit imperative syntax. I'm thinking the `do` keyword. I also want to implement currying, but I understand that's an advanced feature and I haven't even figured out lambdas yet lol.

TODO: Default values in parameters

```Lisp

MethodDecl := (Id `:` (Parameter+) [`->` TypeRef] `=>` MethodBody)

Parameter := Id `:` TypeRef

MethodBody := Expression

```

### Expression

Notes: This is the important thing, returns a value.

```Lisp

Expression := ConstantValue | FuncCall | MacroCall

ConstantValue := IntConstant | QString | FloatConstant | true | false | null

FuncCall := (FuncValue Expression+) | (Expression Operator Expression*)

FuncValue := Symbol | Expression

MacroCall := (Symbol Expr*)

```
