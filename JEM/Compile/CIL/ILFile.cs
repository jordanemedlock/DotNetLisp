using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;
using JEM.Compile;
using System.Linq;

namespace JEM.Compile.CIL
{
    public static class ILFile
    {
        public static Compiler<Expr, string> DottedName = 
            Util.Symbol.Where(x => !x.StartsWith('.') || !x.EndsWith('.'), "Value is not DottedName");

        // 138
        public static Compiler<Expr, string> HashAlg =
            Util.Next(Util.SymbolIs(".hash")).Apply(
            Util.Next(Util.SymbolIs("algorithm")).Apply(
            Util.Next(Util.IntConstant).Select(i =>
            
            $".hash algorithm {i}"
            
            )));

        // 138
        public static Compiler<Expr, string> Version =
            Util.Next(Util.SymbolIs(".ver")).Apply(
            Util.Next(Util.IntConstant).Bind(major =>
            Util.Next(Util.IntConstant).Bind(minor =>
            Util.Next(Util.IntConstant).Bind(build =>
            Util.Next(Util.IntConstant).Select(revision =>
            
            $".ver {major} : {minor} : {build} : {revision}"

            )))));

        // 138
        public static Compiler<Expr, string> Culture =
            Util.Next(Util.SymbolIs(".culture")).Apply(
            Util.Next(Util.StringConstant).Select(str => 
            
            $".culture {EscapeString(str)}"

            ));

        // TODO: public key: Page 138 in ECMA-355

        // TODO: custom: Page 225

        // TODO: security decl Page 224

        // 138
        public static Compiler<Expr, string> AsmDecl =
            HashAlg
            .Or(Version, "Version")
            .Or(Culture, "Culture");

        // 137
        public static Compiler<Expr, string> ExternAssembly =
            Util.Next(Util.SymbolIs(".assembly")).Apply(
            Util.Next(Util.SymbolIs("extern")).Apply(
            Util.Next(DottedName).Bind(dottedName => 
            Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
            $".assembly extern {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"
            
            ))));

        // 137
        public static Compiler<Expr, string> NonExternAssembly =
            Util.Next(Util.SymbolIs(".assembly")).Apply(
            Util.Next(DottedName).Bind(dottedName => 
            Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
            $".assembly {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"

            )));

        public static Compiler<Expr, string> Assembly =
            ExternAssembly.Or(NonExternAssembly, "NonExternAssembly");

        // 135
        public static Compiler<Expr, string> Decl = 
            Assembly
            .Or(input => Corflags(input), "Corflags") 
            .Or(input => FileDirective(input), "FileDirective");

        public static Compiler<Expr, string> Corflags =
            Util.Next(Util.SymbolIs(".corflags")).Apply(
            Util.Next(Util.IntConstant).Select(i =>

            $".corflags {i}"

            ));

        public static Compiler<Expr, string> FileDirective =
            Util.Next(Util.SymbolIs(".file")).Apply(
            Util.NextOptional(Util.SymbolIs("nometadata")).Bind(nometa => 
            Util.Next(Filename).Bind(filename => 
            Util.Next(Util.SymbolIs(".hash")).Apply(
            Util.Next(Bytes).Bind(bytes => 
            Util.NextOptional(Util.SymbolIs(".entrypoint")).Select(entryPoint => 

            $".file {nometa} {filename} .hash = ({bytes}) {entryPoint}"

            ))))));

        public static Compiler<Expr, string> Filename = 
            Util.StringConstant.Select(EscapeString);
        public static Compiler<Expr, string> Bytes =
            Util.IntConstant.Many().Select(ints =>

            $"{String.Join(' ', ints)}"

            );

        // 210
        public static Compiler<Expr, string> Field =
            Util.Next(Util.SymbolIs(".field")).Apply(
            CompilerExtensions.Select<Expr, string, string>((input => FieldDecl(input)), fieldDecl => 

            $".field {fieldDecl}"

            ));

        // 210
        public static Compiler<Expr, string> FieldDecl =
            Util.NextOptional(Util.IntConstant.Select(i => "[" + i + "]")).Bind(i =>
            FieldAttr.Many().Bind(fas =>
            Util.Next(Type).Bind(typ => 
            Util.Next(Id).Select(id =>

            $"{i} {String.Join(' ', fas ?? new List<string>())} {typ} {id}"

            ))));

        // 144
        public static Compiler<Expr, string> Type =
            Util.Next(Util.SymbolIn("!", "!!")).Bind(exp => Util.Next(Util.IntConstant).Select(i => $"{exp}{i}")) // TODO: same here, don't think these will parse
            .Or(Util.SymbolIn(new List<string>() {
                "bool", "char",
                "float32", "float64",
                "object", "string", "typedref", "void"
            }), "Type Symbols")
            .Or(input => IntType(input), "IntType")
            .Or(input => ClassRef(input), "class ref")
            .Or(
            Util.Next(Util.SymbolIs("method")).Apply(
            Util.NextOptional(input => CallConv(input)).Bind(callConv =>
            Util.Next(input => Type(input)).Bind(typ => 
            Util.Next(input => Parameters(input)).Select(pars => 
            
            $"method {callConv} {typ} * ({pars})"

            )))), "method type") 
            .Or(input => TypeModifier(input), "TypeModifier")
            .Or(input => ValueType(input), "valuetype");

        public static Compiler<Expr, string> ValueType =
            Util.Next(Util.SymbolIs("valuetype")).Apply(
            Util.Next(input => TypeReference(input)).Select(typeRef =>

            $"valuetype {typeRef}"

            ));

        public static Compiler<Expr, string> ClassRef =
            Util.Next(Util.SymbolIs("class")).Apply(
            Util.Next(input => TypeReference(input)).Select(typeRef =>

            $"class {typeRef}"

            ));

        // 185
        // TODO: this isn't the whole def. but it doesn't seem super important to implement the rest. idk
        public static Compiler<Expr, string> Bound = 
            Util.SymbolIs("...");

        public static Compiler<Expr, string> TypeModifier =
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.SymbolIn("&", "*")).Select(sym =>

            $"{typ} {sym}"

            ))
            .Or(input => GenericType(input), "GenericType")
            .Or(input => ArrayType(input), "ArrayType")
            .Or(input => ModType(input), "ModType")
            .Or(input => PinnedType(input), "PinnedType");

        public static Compiler<Expr, string> PinnedType =
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.SymbolIs("pinned")).Return(

            $"{typ} pinned"

            ));

        public static Compiler<Expr, string> ModType =
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.SymbolIn("modopt", "modreq")).Bind(mod =>
            Util.Next(input => TypeReference(input)).Select(typeRef =>

            $"{typ} {mod} ({typeRef})"

            )));

        public static Compiler<Expr, string> ArrayType =
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.SymbolIs("[")).Apply(
            Bound.Many().Bind(bnds =>
            Util.Next(Util.SymbolIs("]")).Return(

            $"{typ}[{String.Join(", ", bnds)}]"

            ))));

        public static Compiler<Expr, string> GenericType =
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.SymbolIs("<")).Apply(
            GenArgs.Bind(genArgs =>
            Util.Next(Util.SymbolIs(">")).Return(

            $"{typ}<{genArgs}>"

            ))));

        public static Compiler<Expr, string> GenArgs = 
            Type.Many().Select(xs => String.Join(", ", xs));

        // 197
        public static Compiler<Expr, string> CallConv =
            Util.NextOptional(Util.SymbolIs("instance")).Bind(instance =>
            (instance != null ?
                Util.NextOptional(Util.SymbolIs("explicit")) :
                Compile.Return<Expr, string>(null)).Bind(expl =>
            Util.Next(CallKind).Select(callKind =>

            $"{instance} {expl} {callKind}"

            )));

        // 197
        public static Compiler<Expr, string> CallKind =
            Util.SymbolIn("default", "vararg")
            .Or(
            Util.Next(Util.SymbolIs("unmanaged")).Apply(
            Util.Next(Util.SymbolIn("cdecl", "fastcall", "stdcall", "thiscall")).Select(x =>

            $"unmanaged {x}"

            )), "unmanaged");

        public static Compiler<Expr, string> Parameters = 
            Util.Many(input => Param(input)).Select(xs => 

            String.Join(", ",xs)

            );

        public static Compiler<Expr, string> Param =
            Util.Many(input => ParamAttr(input)).Bind(attrs =>
            Util.Next(input => Type(input)).Bind(typ =>
            Util.NextOptional(input => Marshal(input)).Bind(marshal =>
            Util.NextOptional(input => Id(input)).Select(id =>

            $"{String.Join(' ', attrs)} {typ} {marshal} {id}"

            ))));

        public static Compiler<Expr, string> ParamAttr =
            Util.SymbolIn("in", "opt", "out").Select(x => $"[{x}]");

        // 146
        public static Compiler<Expr, string> TypeReference =
            Util.NextOptional(input => ResolutionScope(input)).Bind(resScope =>
            DottedName.Many().Select(dottedNames =>

            $"{resScope} {String.Join('/', dottedNames)}"

            ));

        public static Compiler<Expr, string> ResolutionScope =
            Util.Next(Util.SymbolIs(".module")).Apply(
            Util.Next(Filename).Select(x =>

            $"[.module {x}]"

            ))
            .Or(Util.Next(DottedName.Select(x => $"[{x}]")), "resolution scope");

        public static Compiler<Expr, string> Id = Util.Symbol;

        // 211
        public static Compiler<Expr, string> FieldAttr =
            Util.SymbolIs("assembly")
            .Or(Util.SymbolIs("famandassem"), "famandassem")
            .Or(Util.SymbolIs("family"), "family")
            .Or(Util.SymbolIs("famorassem"), "famorassem")
            .Or(Util.SymbolIs("initonly"), "initonly")
            .Or(input => Marshal(input), "marshal")
            .Or(Util.SymbolIs("notserialized"), "notserialized")
            .Or(Util.SymbolIs("private"), "private")
            .Or(Util.SymbolIs("compilercontrolled"), "compilercontrolled")
            .Or(Util.SymbolIs("public"), "public")
            .Or(Util.SymbolIs("rtspecialname"), "rtspecialname")
            .Or(Util.SymbolIs("specialname"), "specialname")
            .Or(Util.SymbolIs("static"), "static");


        public static Compiler<Expr, string> Marshal =
            Util.Next(Util.SymbolIs("marshal")).Apply(
            Util.Next(input => NativeType(input)).Select(typ => 

            $"marshal ({typ})"

            ));

        public static Compiler<Expr, string> NativeType =
            Util.SymbolIn(new List<string>()
            {
                "[]", 
                "bool",
                "float32", "float64",
                "lpstr", "lpwstr",
                "method",
            })
            .Or(input => SimpleIntType(input), "IntType");

        // 148
        public static Compiler<Expr, string> NativeTypeArray =
            Util.Next(NativeType).Bind(nType =>
            Util.Next(Util.SymbolIs("[]")).Return(

            $"{nType}[]"

            )).Or(
            Util.Next(NativeType).Bind(nType => 
            Util.Next(Util.SymbolIs("[")).Apply(
            Util.Next(Util.IntConstant).Bind(i => 
            Util.Next(Util.SymbolIs("]")).Return(
            
            $"{nType}[{i}]"

            )))), "Native Array"); // TODO: Theres other versions that I'm not worrying about

        public static Compiler<Expr, string> IntType =
            Util.NextOptional(Util.SymbolIs("unsigned")).Bind(unsigned =>
            Util.Next(SimpleIntType).Select(intType => 

            $"{unsigned} {intType}"

            ))
            .Or(input => SimpleIntType(input), "SimpleIntType");

        public static Compiler<Expr, string> SimpleIntType =
            Util.SymbolIn("int", "int8", "int16", "int32", "int64");


        // 161
        public static Compiler<Expr, string> ClassDecl =
            Util.Next(Util.SymbolIs(".class")).Apply(
            CompilerExtensions.Bind<Expr, string, string>(input => ClassHeader(input), header =>
            Util.Many<string>(input => ClassMember(input)).Select(members =>

            $".class {header} {{\n{String.Join("\n", members)}\n}}"

            )));

        // 161
        public static Compiler<Expr, string> ClassHeader = Compile.Error<Expr, string>("unimplemented");

        // 145 TODO: Didn't finish def. too tired
        public static Compiler<Expr, string> TypeSpec = 
            CompilerExtensions.Or<Expr, string>(input => TypeReference(input), input => Type(input), "TypeSpec");

        // 165
        public static Compiler<Expr, string> GenPars = 
            Util.AtLeastOnce(input => GenPar(input)).Select(x => "<" + String.Join(", ", x) + ">");

        // 165
        public static Compiler<Expr, string> GenPar =
            Util.Many(input => GenParAttribs(input)).Bind(attribs =>
            Util.NextOptional(input => GenConstraints(input)).Bind(constraints =>
            Util.Next(input => Id(input)).Select(id =>

            $"{String.Join(" ", attribs)} {constraints} {id}"

            )))
            .Or(input => Id(input), "GenPar Id");

        // 165
        public static Compiler<Expr, string> GenParAttribs = 
            Util.SymbolIn("+","-","class","valuetype",".ctor");

        // 166
        public static Compiler<Expr, string> GenConstraints = 
            Util.AtLeastOnce(input => Type(input))
            .Select(typs => "("+String.Join(", ", typs)+")");


        // 162
        public static Compiler<Expr, string> ClassAttr =
            Util.SymbolIn("public", "abstract", "ansi", "autochar", "beforefieldinit", "explicit",
                "interface", "private", "rtspecialname", "sealed", "sequential", "serializable",
                "specialname", "unicode")
            .Or(
            Util.Next(Util.SymbolIs("nested")).Apply(
            Util.Next(Util.SymbolIn("assembly","famandassem","family","famorassem","private","public")).Select(inner =>

            $"nested {inner}"

            )), "nested type");
        


        public static Compiler<Expr, string> ClassMember = Compile.Return<Expr, string>("unimplemented");

        public static string EscapeString(string input)
        {
            var ret = input;
            foreach (var kvp in escapeMapping)
            {
                ret = ret.Replace(kvp.Key, kvp.Value);
            }
            return "\"" + ret + "\"";
        }

        private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()
        {
            {"\"", @"\\\"""},
            {"\\\\", @"\\"},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"},
        };
    }
}
