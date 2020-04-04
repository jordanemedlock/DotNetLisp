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

        [Name("Some bullshit")]
        public static Compiler<Expr, string> Decl = 
            Assembly
            .Or(input => Corflags(input), "Corflags") // I don't get why I need these lambdas here???
            .Or(input => FileDirective(input), "FileDirective");

        public static Compiler<Expr, string> Corflags { get; } =
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

            $".field {i} {String.Join(' ', fas ?? new List<string>())} {typ} {id}"

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

            )))), "method type") // TODO: continue here
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

                 )
                 .Or(
                 Util.Next(GenArgs).Select(genArgs =>

                 $"{typ}<{genArgs}>"

                 ), "GenArgs")
                 .Or( 
                 Util.Next(Bound.Many()).Select(bnds =>

                 $"{typ}[{String.Join(", ", bnds)}]"

                 ), "array")
                 .Or(
                 Util.Next(Util.SymbolIn("modopt", "modreq")).Bind(mod =>
                 Util.Next(input => TypeReference(input)).Select(typeRef =>

                 $"{typ} {mod} ({typeRef})"

                 )), "mod")
                 .Or(
                 Util.Next(Util.SymbolIs("pinned")).Return(

                 $"{typ} pinned"

                 ), "pinned"));


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

        public static Compiler<Expr, string> Parameters = Param.Many().Select(xs => String.Join(", ",xs));

        public static Compiler<Expr, string> Param =
            ParamAttr.Many().Bind(attrs =>
            Util.Next(Type).Bind(typ =>
            Util.NextOptional(Marshal).Bind(marshal =>
            Util.NextOptional(Id).Select(id =>

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
                "[]", // TODO: can't parse this yet.
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
