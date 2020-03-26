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
        //public static Compiler<Expr, string> Compiler = Decl.Many();

        //public static Compiler<Expr, string> Decl = Assembly; // TODO: Or() a bunc of other shit

        //public static Compiler<Expr, string> Assembly = Compile.Id<Expr>().Select(x => x.ToString()); // TODO: fix this
        
        public static Compiler<Expr, string> DottedName = 
            Util.Symbol.Where(x => !x.StartsWith('.') || !x.EndsWith('.'));

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
            .Or(Version)
            .Or(Culture);

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
            ExternAssembly.Or(NonExternAssembly);

        public static Compiler<Expr, string> Decl =
            Assembly
            .Or(Corflags)
            .Or(FileDirective);

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

        public static Compiler<Expr, string> Field =
            Util.Next(Util.SymbolIs(".field")).Apply(
            FieldDecl.Select(fieldDecl => 

            $".field {fieldDecl}"

            ));

        // 210
        public static Compiler<Expr, string> FieldDecl =
            Util.NextOptional(Util.IntConstant.Select(i => "[" + i + "]")).Bind(i =>
            Util.NextOptional(FieldAttr.Many()).Bind(fas =>
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
            }))
            .Or(IntType)
            .Or(
            Util.Next(Util.SymbolIs("class")).Apply(
            Util.Next(TypeReference).Select(typeRef =>

            $"class {typeRef}"

            )))
            .Or(
            Util.Next(Util.SymbolIs("method")).Apply(
            Util.NextOptional(CallConv).Bind(callConv =>
            Util.Next(Type).Bind(typ => 
            Util.Next(Parameters).Select(pars => 
            
            $"method {callConv} {typ} * ({pars})"

            ))))) // TODO: continue here
            .Or(TypeModifier)
            .Or(
            Util.Next(Util.SymbolIs("valuetype")).Apply(
            Util.Next(TypeReference).Select(typeRef => 

            $"valuetype {typeRef}"

            )));

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

                 ))
                 .Or( // TODO: continue here  
                 Util.Next(Bound.Many()).Select(bnds =>

                 $"{typ}[{String.Join(", ", bnds)}]"

                 ))
                 .Or(
                 Util.Next(Util.SymbolIn("modopt", "modreq")).Bind(mod =>
                 Util.Next(TypeReference).Select(typeRef =>

                 $"{typ} {mod} ({typeRef})"

                 )))
                 .Or(
                 Util.Next(Util.SymbolIs("pinned")).Return(

                 $"{typ} pinned"

                 )));


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

            )));

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
            Util.NextOptional(ResolutionScope).Bind(resScope =>
            DottedName.Many().Select(dottedNames =>

            $"{resScope} {String.Join('/', dottedNames)}"

            ));

        public static Compiler<Expr, string> ResolutionScope =
            Util.Next(Util.SymbolIs(".module")).Apply(
            Util.Next(Filename).Select(x =>

            $"[.module {EscapeString(x)}]"

            ))
            .Or(DottedName.Select(x => $"[{x}]"));

        public static Compiler<Expr, string> Id = Util.Symbol;

        // 211
        public static Compiler<Expr, string> FieldAttr =
            Util.SymbolIs("assembly")
            .Or(Util.SymbolIs("famandassem"))
            .Or(Util.SymbolIs("family"))
            .Or(Util.SymbolIs("famorassem"))
            .Or(Util.SymbolIs("initonly"))
            .Or(Marshal)
            .Or(Util.SymbolIs("notserialized"))
            .Or(Util.SymbolIs("private"))
            .Or(Util.SymbolIs("compilercontrolled"))
            .Or(Util.SymbolIs("public"))
            .Or(Util.SymbolIs("rtspecialname"))
            .Or(Util.SymbolIs("specialname"))
            .Or(Util.SymbolIs("static"));


        public static Compiler<Expr, string> Marshal =
            Util.Next(Util.SymbolIs("marshal")).Apply(
            Util.Next(NativeType).Select(typ => 

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
            .Or(IntType);

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

            ))))); // TODO: Theres other versions that I'm not worrying about

        public static Compiler<Expr, string> IntType =
            Util.NextOptional(Util.SymbolIs("unsigned")).Bind(unsigned =>
            Util.Next(Util.SymbolIn("int", "int8", "int16", "int32", "int64")).Select(intType => 

            $"{unsigned} {intType}"

            ));

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
