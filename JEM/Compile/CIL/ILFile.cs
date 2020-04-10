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
            Util.Next(".hash").Apply(
            Util.Next("algorithm").Apply(
            Util.Next(Util.IntConstant).Select(i =>
            
            $".hash algorithm {i}"
            
            )));

        // 138
        public static Compiler<Expr, string> Version =
            Util.Next(".ver").Apply(
            Util.Next(Util.IntConstant).Bind(major =>
            Util.Next(Util.IntConstant).Bind(minor =>
            Util.Next(Util.IntConstant).Bind(build =>
            Util.Next(Util.IntConstant).Select(revision =>
            
            $".ver {major} : {minor} : {build} : {revision}"

            )))));

        // 138
        public static Compiler<Expr, string> Culture =
            Util.Next(".culture").Apply(
            Util.Next(Util.StringConstant).Select(str => 
            
            $".culture {EscapeString(str)}"

            ));

        // TODO: public key: Page 138 in ECMA-355

        // TODO: custom: Page 225

        // TODO: security decl Page 224

        // 138
        public static Compiler<Expr, string> AsmDecl =
            CompilerExtensions.Or(
                "AsmDecl",

                HashAlg,
                Version, 
                Culture
            );

        // 137
        public static Compiler<Expr, string> ExternAssembly =
            Util.Next(".assembly").Apply(
            Util.Next("extern").Apply(
            Util.Next(DottedName).Bind(dottedName => 
            Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
            $".assembly extern {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"
            
            ))));

        // 137
        public static Compiler<Expr, string> NonExternAssembly =
            Util.Next(".assembly").Apply(
            Util.Next(DottedName).Bind(dottedName => 
            Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
            $".assembly {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"

            )));

        public static Compiler<Expr, string> Assembly =
            ExternAssembly.Or(NonExternAssembly, "NonExternAssembly");

        // 135
        public static Compiler<Expr, string> Decl = 
            CompilerExtensions.Or<Expr, string>(
                "Decl",

                input => Assembly(input),
                input => Corflags(input), 
                input => FileDirective(input)
           );

        public static Compiler<Expr, string> Corflags =
            Util.Next(".corflags").Apply(
            Util.Next(Util.IntConstant).Select(i =>

            $".corflags {i}"

            ));

        public static Compiler<Expr, string> FileDirective =
            Util.Next(".file").Apply(
            Util.NextOptional("nometadata").Bind(nometa => 
            Util.Next(Filename).Bind(filename => 
            Util.Next(".hash").Apply(
            Util.Next(Bytes).Bind(bytes => 
            Util.NextOptional(".entrypoint").Select(entryPoint => 

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
            Util.Next(".field").Apply(
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
            CompilerExtensions.Or(
                "Type",

                Util.Next(Util.SymbolIn("!", "!!")).Bind(exp => 
                Util.Next(Util.IntConstant).Select(i => $"{exp}{i}")),

                Util.SymbolIn(new List<string>() {
                    "bool", "char",
                    "float32", "float64",
                    "object", "string", "typedref", "void"
                }), 

                input => IntType(input), 

                input => ClassRef(input), 

                Util.Next("method").Apply(
                Util.NextOptional(input => CallConv(input)).Bind(callConv =>
                Util.Next(input => Type(input)).Bind(typ => 
                Util.Next(input => Parameters(input)).Select(pars => 
            
                $"method {callConv} {typ} * ({pars})"

                )))), 

                input => TypeModifier(input), 

                input => ValueType(input)
            );

        public static Compiler<Expr, string> ValueType =
            Util.Next("valuetype").Apply(
            Util.Next(input => TypeReference(input)).Select(typeRef =>

            $"valuetype {typeRef}"

            ));

        public static Compiler<Expr, string> ClassRef =
            Util.Next("class").Apply(
            Util.Next(input => TypeReference(input)).Select(typeRef =>

            $"class {typeRef}"

            ));

        // 185
        // TODO: this isn't the whole def. but it doesn't seem super important to implement the rest. idk
        public static Compiler<Expr, string> Bound = 
            Util.SymbolIs("...");

        public static Compiler<Expr, string> TypeModifier =
            CompilerExtensions.Or(
                "TypeModifier",
                
                Util.Next(Type).Bind(typ =>
                Util.Next(Util.SymbolIn("&", "*")).Select(sym =>

                $"{typ} {sym}"

                )),
                input => GenericType(input), 
                input => ArrayType(input), 
                input => ModType(input), 
                input => PinnedType(input)
            );

        public static Compiler<Expr, string> PinnedType =
            Util.Next(Type).Bind(typ =>
            Util.Next("pinned").Return(

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
            Util.Next("[").Apply(
            Bound.Many().Bind(bnds =>
            Util.Next("]").Return(

            $"{typ}[{String.Join(", ", bnds)}]"

            ))));

        public static Compiler<Expr, string> GenericType =
            Util.Next(Type).Bind(typ =>
            Util.Next("<").Apply(
            GenArgs.Bind(genArgs =>
            Util.Next(">").Return(

            $"{typ}<{genArgs}>"

            ))));

        public static Compiler<Expr, string> GenArgs = 
            Type.Many().Select(xs => String.Join(", ", xs));

        // 197
        public static Compiler<Expr, string> CallConv =
            Util.NextOptional("instance").Bind(instance =>
            (instance != null ?
                Util.NextOptional("explicit") :
                Compile.Return<Expr, string>(null)).Bind(expl =>
            Util.Next(CallKind).Select(callKind =>

            $"{instance} {expl} {callKind}"

            )));

        // 197
        public static Compiler<Expr, string> CallKind =
            Util.SymbolIn("default", "vararg")
            .Or(
            Util.Next("unmanaged").Apply(
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
            Util.Next(".module").Apply(
            Util.Next(Filename).Select(x =>

            $"[.module {x}]"

            ))
            .Or(Util.Next(DottedName.Select(x => $"[{x}]")), "resolution scope");

        public static Compiler<Expr, string> Id = Util.Symbol;

        // 211
        public static Compiler<Expr, string> FieldAttr =
            Util.SymbolIn("assembly", "famandassem", "family", "famorassem", 
                "initonly", "notserialized", "private", "compilercontrolled",
                "public", "rtspecialname", "specialname", "static")
            .Or(input => Marshal(input), "marshal");


        public static Compiler<Expr, string> Marshal =
            Util.Next("marshal").Apply(
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
            Util.Next("[]").Return(

            $"{nType}[]"

            )).Or(
            Util.Next(NativeType).Bind(nType => 
            Util.Next("[").Apply(
            Util.Next(Util.IntConstant).Bind(i => 
            Util.Next("]").Return(
            
            $"{nType}[{i}]"

            )))), "Native Array"); // TODO: Theres other versions that I'm not worrying about

        public static Compiler<Expr, string> IntType =
            Util.NextOptional("unsigned").Bind(unsigned =>
            Util.Next(SimpleIntType).Select(intType => 

            $"{unsigned} {intType}"

            ))
            .Or(input => SimpleIntType(input), "SimpleIntType");

        public static Compiler<Expr, string> SimpleIntType =
            Util.SymbolIn("int", "int8", "int16", "int32", "int64");


        // 161
        public static Compiler<Expr, string> ClassDecl =
            Util.Next(".class").Apply(
            CompilerExtensions.Bind<Expr, string, string>(input => ClassHeader(input), header =>
            Util.Many<string>(input => ClassMember(input)).Select(members =>

            $".class {header} {{\n{String.Join("\n", members)}\n}}"

            )));

        // 161
        public static Compiler<Expr, string> ClassHeader =
            Util.Many(input => ClassAttr(input)).Bind(classAttrs =>
            Util.Next(input => Id(input)).Bind(id =>
            Util.NextOptional(input => GenPars(input)).Bind(genPars =>
            Util.NextOptional(Extends).Bind(baseClass =>
            Util.NextOptional(Implements).Select(interfaces =>

            $"{string.Join(" ", classAttrs)} {id}{genPars} {baseClass} {interfaces}"

            )))));

        public static Compiler<Expr, string> Extends =
            Util.Next("extends").Apply(
            Util.Next(input => Id(input)).Select(id =>

            $"extends {id}"

            ));

        public static Compiler<Expr, string> Implements =
            Util.Next("implements").Apply(
            Util.Many(input => Id(input)).Select(ids =>

            $"implements {string.Join(", ", ids)}"

            ));

        // 145 TODO: Didn't finish def. too tired
        public static Compiler<Expr, string> TypeSpec = 
            CompilerExtensions.Or<Expr, string>(
                "TypeSpec",

                Util.Next("[").Apply(
                Util.NextOptional(".module").Bind(mod => 
                Util.Next(DottedName).Bind(n => 
                Util.Next("]").Return(

                $"[{mod} {n}]"

                )))),
                input => Type(input), 
                input => TypeReference(input)
            );

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
            Util.Next("nested").Apply(
            Util.Next(Util.SymbolIn("assembly","famandassem","family","famorassem","private","public")).Select(inner =>

            $"nested {inner}"

            )), "nested type");


        // 169
        public static Compiler<Expr, string> ClassMember = input =>
            CompilerExtensions.Or(
                "ClassMember",
                Util.Next(".custom").Apply(CustomDecl.Select(c => $".custom {c}")),
                Util.Next(".data").Apply(DataDecl.Select(d => $".data {d}")),
                Util.Next(".field").Apply(FieldDecl.Select(f => $".field {f}"))

            )(input);

        // 225
        public static Compiler<Expr, string> CustomDecl = Compile.Error<Expr, string>("unimplemented");

        // 213
        public static Compiler<Expr, string> DataDecl =
            Util.NextOptional(input => DataLabel(input)).Bind(label =>
            Util.Next(input => DdBody(input)).Select(body =>

            label != null ? $"{label} = {body}" : $"{body}"

            ));

        public static Compiler<Expr, string> DataLabel = input => Id(input);

        // 213
        public static Compiler<Expr, string> DdBody = 
            CompilerExtensions.Or(
                "DdBody",

                input => DdItem(input),
                Util.Many(input => DdItem(input)).Select(items => $"{{{string.Join(", ", items)}}}")
            );

        // 213
        public static Compiler<Expr, string> DdItem =
            CompilerExtensions.Or(
                "DdItem",


                Util.Next("&").Apply(Util.Next(input => Id(input)).Select(id => $"& ({id})")),

                Util.Next("bytearray").Apply(Util.Next(input => Bytes(input)).Select(bytes => $"bytearray ({bytes})")),

                Util.Next("char").Apply(Util.Next("*").Apply(Util.Next(input => QString(input)).Select(qstr => $"char * ({qstr})"))),

                Util.Next(Util.SymbolIn("float32","float64")).Bind(fType => 
                Util.NextOptional(Util.FloatConstant.Select(x => (double?)x)).Bind(fValue => 
                Util.NextOptional(Util.IntConstant.Select(x => (long?)x)).Select(iValue => 

                $"{fType} " + (fValue != null ? $"({fValue}) " : "") + (iValue != null ? $"[{iValue}]" : "")

                ))),

                Util.Next(Util.SymbolIn("int8", "int16", "int32", "int64")).Bind(iType => // TODO: this is not perfectly defined
                Util.NextOptional(Util.IntConstant.Select(x => (long?)x)).Bind(iValue =>  // int32 [20] is not possible
                Util.NextOptional(Util.IntConstant.Select(x => (long?)x)).Select(cValue => // only int32 (10) is possible

                $"{iType} " + (iValue != null ? $"({iValue}) " : "") + (cValue != null ? $"[{cValue}]" : "")

                )))
            );

        public static Compiler<Expr, string> QString = Util.StringConstant.Select(s => $"{EscapeString(s)}");

        public static Compiler<Expr, string> SQString = Compile.Error<Expr, string>("Unimplemented (needs parser update)");

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
