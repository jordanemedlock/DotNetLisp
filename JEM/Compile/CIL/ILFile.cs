﻿using System;
using System.Collections.Generic;
using System.Text;
using JEM.Model;
using JEM.Compile;
using System.Linq;

namespace JEM.Compile.CIL
{
    public static class ILFile
    {
        public static Compiler<Expr, string> DottedName = (
            from x in Util.Symbol
            where !x.StartsWith('.') || !x.EndsWith('.')
            select x);

            // Util.Symbol.Where(x => !x.StartsWith('.') || !x.EndsWith('.'), "DottedName");

        // 138
        public static Compiler<Expr, string> HashAlg = (
            from hash in Util.Next(".hash")
            from algorithm in Util.Next("algorithm")
            from i in Util.Next(Util.IntConstant)
            select $"{hash} {algorithm} {i}"
        );
        // new Compiler<Expr, string>("HashAlg", () => 
        //     Util.Next(".hash").Apply(
        //     Util.Next("algorithm").Apply(
        //     Util.Next(Util.IntConstant).Select(i =>
            
        //     $".hash algorithm {i}"
            
        //     ))));

        // 138
        public static Compiler<Expr, string> Version = (
            from ver in Util.Next(".ver")
            from major in Util.Next(Util.IntConstant)
            from minor in Util.Next(Util.IntConstant)
            from build in Util.Next(Util.IntConstant)
            from revision in Util.Next(Util.IntConstant)
            select $"{ver} {major} : {minor} : {build} : {revision}"
        );
        
        // new Compiler<Expr, string>("Version", () => 
        //     Util.Next(".ver").Apply(
        //     Util.Next(Util.IntConstant).Bind(major =>
        //     Util.Next(Util.IntConstant).Bind(minor =>
        //     Util.Next(Util.IntConstant).Bind(build =>
        //     Util.Next(Util.IntConstant).Select(revision =>
            
        //     $".ver {major} : {minor} : {build} : {revision}"

        //     ))))));

        // 138
        public static Compiler<Expr, string> Culture = (
            from culture in Util.Next(".culture")
            from str in Util.Next(Util.DQStringConstant)
            select $"{culture} {str.Escaped()}"
        );
        
        // new Compiler<Expr, string>("Culture", () =>
        //     Util.Next(".culture").Apply(
        //     Util.Next(Util.DQStringConstant).Select(str => 
            
        //     $".culture {str.Escaped()}"

        //     )));

        // 138
        public static Compiler<Expr, string> AsmDecl = new Compiler<Expr, string>("AsmDecl", () => 
            Compiler<Expr, string>.Or(
                HashAlg,
                Version, 
                Culture
            ));

        // 137
        public static Compiler<Expr, string> AssemblyDecl = (
            from @extern in Util.NextOptional("extern")
            from dottedName in Util.Next(DottedName)
            from asmDecls in Util.Next(AsmDecl.Many())
            select $"{@extern} {dottedName} {{\n{string.Join('\n', asmDecls)}\n}}"
        );
        
        // new Compiler<Expr, string>("AssemblyDecl", () => 
        //     Util.NextOptional("extern").Bind(ex =>
        //     Util.Next(DottedName).Bind(dottedName => 
        //     Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
        //     $"{ex} {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"
            
        //     ))));

        // 137
        // public static Compiler<Expr, string> NonExternAssembly = new Compiler<Expr, string>("NonExternAssembly", () => 
        //     Util.Next(".assembly").Apply(
        //     Util.Next(DottedName).Bind(dottedName => 
        //     Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
        //     $".assembly {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"

        //     ))));

        // public static Compiler<Expr, string> Assembly = new Compiler<Expr, string>("Assembly", () => 
        //     ExternAssembly.Or(NonExternAssembly)
        // );

        // 135
        public static Compiler<Expr, string> Decl = new Compiler<Expr, string>("Decl", () =>
            Util.StartsWith(new Dictionary<string, Compiler<Expr, string>>() {
                [".assembly"] = AssemblyDecl.Select(a => $".assembly {a}"),
                [".corflags"] = Util.Next(Util.IntConstant).Select(i => $".corflags {i}"),
                [".custom"] = CustomDecl.Select(c => $".custom {c}"),
                [".data"] = DataDecl.Select(d => $".data {d}"),
                [".file"] = FileDecl.Select(f => $".file {f}"),
                [".field"] = FieldDecl.Select(f => $".field {f}"),
                [".method"] = MethodDecl.Select(m => $".method {m}"),
                [".module"] = ModuleDecl.Select(m => $".module {m}"),
                [".mresource"] = MResourceDecl.Select(m => $".mresource {m}"),
                [".subsystem"] = Util.Next(Util.IntConstant).Select(i => $".subsystem {i}"),
                [".vtfixup"] = VTFixupDecl.Select(v => $".vtfixup {v}"),
                [".class"] = ClassDecl.Select(c => $".class {c}"),
                [".line"] = ExternSourceDecl.Select(e => $".line {e}"),
                [".permissionset"] = PermissionSetDecl.Select(p => $".permissionset {p}"),
                [".permission"] = PermissionDecl.Select(p => $".permission {p}")
            })
        );

        public static Compiler<Expr, string> ModuleDecl = (
            from @extern in Util.NextOptional("extern")
            from fn in Util.NextOptional(Filename)
            select $"{@extern} {fn}"
        );
        
        // new Compiler<Expr, string>("ModuleDecl", () => 
        //     Util.NextOptional("extern").Bind(ex => 
        //     Util.NextOptional(Filename).Select(fn => 
            
        //     $"{ex} {fn}"

        //     ))
        // );

        //     Compiler<Expr, string>.Or(
        //         Assembly,
        //         Util.Next(".corflags").Apply(Util.Next(Util.IntConstant).Select(i => $".corflags {i}")), 
        //         Util.Next(".custom").Apply(CustomDecl.Select(customDecl => $".custom {customDecl}")),
        //         Util.Next(".data").Apply(DataDecl.Select(d => $".data {d}")),
        //         FileDirective,
        //         Field,
        //         Util.Next(".method").Apply(MethodDecl.Select(m => $".method {m}")),
        //         Util.Next(".module").Apply(Util.NextOptional(Filename).Select(f => $".module {f}")),
        //         Util.Next(".module").Apply(Util.Next("extern").Apply(Util.NextOptional(Filename).Select(f => $".module extern {f}"))),
        //         Util.Next(".mresource").Apply(MResourceDecl.Select(r => $".mresource {r}")),
        //         Util.Next(".subsystem").Apply(Util.IntConstant.Select(i => $".subsystem {i}")),
        //         Util.Next(".vtfixup").Apply(VTFixupDecl.Select(vtf => $".vtfixup {vtf}")),
        //         ClassDecl,
        //         ExternSourceDecl,
        //         SecurityDecl
        //    ));

        // 135
        public static Compiler<Expr, string> Compiler = new Compiler<Expr, string>("Compiler", () => 
            Decl.Many().Select(decls => string.Join("\n\n", decls))
        );

        // 207
        public static Compiler<Expr, string> VTFixupDecl = (
            from i in Util.NextOptional(Util.IntConstant.Select(i => (long?)i))
            from attrs in Util.SymbolIn("fromunmanaged", "int32", "int64").AtLeastOnce()
            from @at in Util.Next("at")
            from label in Util.Next(DataLabel)
            select $"{i} {string.Join(" ", attrs)} {@at} {label}"
        );
        
        // new Compiler<Expr, string>("VTFixupDecl", () => 
        //     Util.NextOptional(Util.IntConstant.Select(i => (long?)i)).Bind(i =>
        //     Util.SymbolIn("fromunmanaged", "int32", "int64").AtLeastOnce().Bind(attrs => 
        //     Util.Next("at").Apply(
        //     Util.Next(DataLabel).Select(label => 

        //     $"{i} {string.Join(" ", attrs)} at {label}"

        //     ))))
        // );

        // 140
        public static Compiler<Expr, string> MResourceDecl = (
            from access in Util.Next(Util.SymbolIn("public", "private"))
            from dottedName in Util.Next(DottedName)
            from ress in ManResDecl.Many()
            select $"{access} {dottedName} {{\n{string.Join("\n", ress)}\n}}"
        );
        
        // new Compiler<Expr, string>("MResourceDecl", () => 
        //     Util.Next(Util.SymbolIn("public", "private")).Bind(access =>
        //     Util.Next(DottedName).Bind(dottedName =>
        //     ManResDecl.Many().Select(ress => 
            
        //     $"{access} {dottedName} {{\n{string.Join("\n", ress)}\n}}"
            
        //     )))
        // );

        // 140
        public static Compiler<Expr, string> ManResDecl = new Compiler<Expr, string>("ManResDecl", () => 
            Compiler<Expr, string>.Or(
                from assembly in Util.Next(".assembly")
                from @extern in Util.Next("extern")
                from dottedName in Util.Next(DottedName)
                select $"{assembly} {@extern} {dottedName}",

                from custom in Util.Next(".custom")
                from decl in CustomDecl
                select $"{custom} {decl}",

                from file in Util.Next(".file")
                from dottedName in Util.Next(DottedName)
                from @at in Util.Next("at")
                from i in Util.Next(Util.IntConstant)
                select $"{file} {dottedName} {@at} {i}"
            )

        );

        // 135
        public static Compiler<Expr, string> FileDecl = (
            from nometa in Util.NextOptional("nometadata")
            from filename in Util.Next(Filename)
            from hash in Util.Next(".hash")
            from bytes in Util.Next(Bytes)
            from entrypoint in Util.NextOptional(".entrypoint")
            select $"{nometa} {filename} {hash} = ({bytes}) {entrypoint}"
        );
        
        // new Compiler<Expr, string>("FileDecl", () =>
        //     Util.NextOptional("nometadata").Bind(nometa => 
        //     Util.Next(Filename).Bind(filename => 
        //     Util.Next(".hash").Apply(
        //     Util.Next(Bytes).Bind(bytes => 
        //     Util.NextOptional(".entrypoint").Select(entryPoint => 

        //     $"{nometa} {filename} .hash = ({bytes}) {entryPoint}"

        //     ))))));

        public static Compiler<Expr, string> Filename = (
            from x in Util.DQStringConstant
            select x.Escaped()
        );
        // new Compiler<Expr, string>("Filename", () => 
        //     Util.DQStringConstant.Select(x => x.Escaped()));

        public static Compiler<Expr, string> Bytes = (
            from ints in Util.IntConstant.Many()
            select String.Join(' ', ints)
        );
        
        // new Compiler<Expr, string>("Bytes", () => 
        //     Util.IntConstant.Many().Select(ints =>

        //     $"{String.Join(' ', ints)}"

        //     ));

        // 210
        public static Compiler<Expr, string> Field = (
            from field in Util.Next(".field")
            from decl in FieldDecl
            select $"{field} {decl}"
        );
        
        // new Compiler<Expr, string>("Field", () => 
        //     Util.Next(".field").Apply(
        //     FieldDecl.Select(fieldDecl => 

        //     $".field {fieldDecl}"

        //     )));
        // 212
        public static Compiler<Expr, string> FieldInit = new Compiler<Expr, string>("FieldInit", () => 
            Compiler<Expr, string>.Or(
                from @bool in Util.Next("bool")
                from b in Util.Next(Util.BoolConstant)
                select $"{@bool} ({(b ? "true" : "false")})",

                // Util.Next("bool").Apply(
                // Util.Next(Util.BoolConstant).Select(b => 
                
                // $"bool ({(b ? "true" : "false")})"
                
                // )),

                from @bytearray in Util.Next("bytearray")
                from b in Util.Next(Bytes)
                select $"{@bytearray} ({b})",

                // Util.Next("bytearray").Apply(
                // Util.Next(Bytes).Select(b => 
                
                // $"bytearray ({b})"
                
                // )),

                from @char in Util.Next("char")
                from c in Util.Next(Util.IntConstant)
                select $"{@char} ({c})",

                // Util.Next("char").Apply(
                // Util.Next(Util.IntConstant).Select(c => 
                
                // $"char ({c})"
                
                // )),

                from fType in Util.Next(Util.SymbolIn("float32", "float64"))
                from fValue in Util.Next(Util.FloatConstant.Select(x => x.ToString()).Or(Util.IntConstant.Select(x => x.ToString())))
                select $"{fType} ({fValue})",

                // Util.Next(Util.SymbolIn("float32","float64")).Bind(fType =>
                // Util.Next(Util.FloatConstant.Select(x => x.ToString()).Or(Util.IntConstant.Select(x => x.ToString()))).Select(f => 
                
                // $"{fType} ({f})"
                
                // )),

                from unsig in Util.NextOptional("unsigned")
                from iType in Util.Next(Util.SymbolIn("int8", "int16", "int32", "int64"))
                from iValue in Util.Next(Util.IntConstant)
                select $"{unsig} {iType} ({iValue})",

                // Util.NextOptional("unsigned").Bind(unsig =>
                // Util.Next(Util.SymbolIn("int8", "int16", "int32", "int64")).Bind(iType => 
                // Util.Next(Util.IntConstant).Select(iValue => 

                // $"{unsig} {iType} ({iValue})"

                // ))),

                QString,

                Util.SymbolIs("nullref")

            ));

        // 210
        public static Compiler<Expr, string> FieldInitOrDataLabel = new Compiler<Expr, string>("FieldInitOrDataLabel", () => 
            Compiler<Expr, string>.Or(
                from eq in Util.Next(Util.OperatorIs("="))
                from f in FieldInit.Or(Util.Next(FieldInit))
                select $"{eq} {f}",

                // Util.Next(Util.OperatorIs("=")).Apply(Util.Next(FieldInit).Select(f => $"= {f}")),
                // Util.Next(Util.OperatorIs("=")).Apply(FieldInit.Select(f => $"= {f}")),

                from @at in Util.Next("at")
                from label in Util.Next(DataLabel)
                select $"{@at} {label}"

                // Util.Next("at").Apply(Util.Next(DataLabel).Select(d => $"at {d}"))

            ));

        // 210
        public static Compiler<Expr, string> FieldDecl = (
            from i in Util.NextOptional(Util.IntConstant.Select(i => "[" + i + "]"))
            from fas in FieldAttr.ManyUntil()
            from type in Util.Next(Type)
            from id in Util.Next(Id)
            from expr in FieldInitOrDataLabel.Or(Util.NextOptional(FieldInitOrDataLabel))
            select $"{i} {String.Join(' ', fas ?? new List<string>())} {type} {id} {expr}"
        );
        
        // new Compiler<Expr, string>("FieldDecl", () => 
        //     Util.NextOptional(Util.IntConstant.Select(i => "[" + i + "]")).Bind(i =>
        //     FieldAttr.ManyUntil().Bind(fas =>
        //     Util.Next(Type).Bind(typ =>
        //     Util.Next(Id).Bind(id =>
        //     FieldInitOrDataLabel.Or(Util.NextOptional(FieldInitOrDataLabel)).Select(expr =>

        //     $"{i} {String.Join(' ', fas ?? new List<string>())} {typ} {id} {expr}"

        //     ))))));

        // 144
        public static Compiler<Expr, string> Type = new Compiler<Expr, string>("Type", () =>
            Compiler<Expr, string>.Or(
                from exp in Util.Next(Util.OperatorIn("!", "!!"))
                from i in Util.Next(Util.IntConstant)
                select $"{exp}{i}",

                // Util.Next(Util.OperatorIn("!", "!!")).Bind(exp => 
                // Util.Next(Util.IntConstant).Select(i => $"{exp}{i}")),

                Util.SymbolIn(new List<string>() {
                    "bool", "char",
                    "float32", "float64",
                    "object", "string", "typedref", "void"
                }), 

                IntType, 

                ClassRef, 

                from method in Util.Next("method")
                from callConv in Util.NextOptional(CallConv)
                from type in Util.Next(Type)
                from pars in Util.Next(Parameters)
                select $"{method} {callConv} {type} * ({pars})",

                // Util.Next("method").Apply(
                // Util.NextOptional(CallConv).Bind(callConv =>
                // Util.Next(Type).Bind(typ => 
                // Util.Next(Parameters).Select(pars => 
            
                // $"method {callConv} {typ} * ({pars})"

                // )))), 

                TypeModifier, 

                ValueType
            ));

        public static Compiler<Expr, string> ValueType = (
            from valuetype in Util.Next("valuetype")
            from typeRef in Util.Next(TypeReference)
            select $"{valuetype} {typeRef}"
        );
        
        // new Compiler<Expr, string>("ValueType", () =>
        //     Util.Next("valuetype").Apply(
        //     Util.Next(TypeReference).Select(typeRef =>

        //     $"valuetype {typeRef}"

        //     )));

        public static Compiler<Expr, string> ClassRef = (
            from @class in Util.Next("class")
            from typeRef in Util.Next(TypeReference)
            select $"{@class} {typeRef}"
        );
        
        // new Compiler<Expr, string>("ClassRef", () => 
        //     Util.Next("class").Apply(
        //     Util.Next(TypeReference).Select(typeRef =>

        //     $"class {typeRef}"

        //     )));

        // 185
        // TODO: this isn't the whole def. but it doesn't seem super important to implement the rest. idk
        public static Compiler<Expr, string> Bound = 
            Util.SymbolIs("...");

        public static Compiler<Expr, string> TypeModifier = new Compiler<Expr, string>("TypeModifier", () => 
            Compiler<Expr, string>.Or(
                from type in Util.Next(Type)
                from sym in Util.Next(Util.OperatorIn("&", "*"))
                select $"{type} {sym}",

                // Util.Next(Type).Bind(typ =>
                // Util.Next(Util.OperatorIn("&", "*")).Select(sym =>

                // $"{typ} {sym}"

                // )),

                GenericType, 
                ArrayType, 
                ModType, 
                PinnedType
            ));

        public static Compiler<Expr, string> PinnedType = (
            from type in Util.Next(Type)
            from pinned in Util.Next("pinned")
            select $"{type} {pinned}"
        );
        
        // new Compiler<Expr, string>("PinnedType", () => 
        //     Util.Next(Type).Bind(typ =>
        //     Util.Next("pinned").Return<string>(

        //     $"{typ} pinned"

        //     )));

        public static Compiler<Expr, string> ModType = (
            from type in Util.Next(Type)
            from mod in Util.Next(Util.SymbolIn("modopt", "modreq"))
            from typeRef in Util.Next(TypeReference)
            select $"{type} {mod} ({typeRef})"
        );
        
        // new Compiler<Expr, string>("ModType", () =>
        //     Util.Next(Type).Bind(typ =>
        //     Util.Next(Util.SymbolIn("modopt", "modreq")).Bind(mod =>
        //     Util.Next(TypeReference).Select(typeRef =>

        //     $"{typ} {mod} ({typeRef})"

        //     ))));

        public static Compiler<Expr, string> ArrayType = new Compiler<Expr, string>("ArrayType", () => 
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.OperatorIs("[")).Apply(
            Bound.ManyUntil().Bind(bnds =>
            Util.Next(Util.OperatorIs("]")).Return<string>(

            $"{typ}[{String.Join(", ", bnds)}]"

            )))));

        public static Compiler<Expr, string> GenericType = new Compiler<Expr, string>("GenericType", () => 
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.OperatorIs("<")).Apply(
            GenArgs.Bind(genArgs =>
            Util.Next(Util.OperatorIs(">")).Return<string>(

            $"{typ}<{genArgs}>"

            )))));

        public static Compiler<Expr, string> GenArgs = new Compiler<Expr, string>("GenArgs", () => 
            Type.ManyUntil().Select(xs => String.Join(", ", xs)));

        // 197
        public static Compiler<Expr, string> CallConv = new Compiler<Expr, string>("CallConv", () => 
            Util.NextOptional("instance").Bind(instance =>
            (instance != null ?
                Util.NextOptional("explicit") :
                Compiler<Expr, string>.Return(null)).Bind(expl =>
            Util.Next(CallKind).Select(callKind =>

            $"{instance} {expl} {callKind}"

            ))));

        // 197
        public static Compiler<Expr, string> CallKind = new Compiler<Expr, string>("CallKind", () => 
            Util.SymbolIn("default", "vararg")
            .Or(
            Util.Next("unmanaged").Apply(
            Util.Next(Util.SymbolIn("cdecl", "fastcall", "stdcall", "thiscall")).Select(x =>

            $"unmanaged {x}"

            ))));

        public static Compiler<Expr, string> Parameters = new Compiler<Expr, string>("Parameters", () =>
            Util.Many(Param).Select(xs => 

            String.Join(", ",xs)

            ));

        public static Compiler<Expr, string> Param = new Compiler<Expr, string>("Param", () => 
            Util.ManyUntil(ParamAttr).Bind(attrs =>
            Util.Next(Type).Bind(typ =>
            Util.NextOptional(Marshal).Bind(marshal =>
            Util.NextOptional(Id).Select(id =>

            $"{String.Join(' ', attrs)} {typ} {marshal} {id}"

            )))));

        public static Compiler<Expr, string> ParamAttr = new Compiler<Expr, string>("ParamAttr", () => 
            Util.SymbolIn("in", "opt", "out").Select(x => $"[{x}]"));

        // 146
        public static Compiler<Expr, string> TypeReference = new Compiler<Expr, string>("TypeReference", () =>
            Util.NextOptional(ResolutionScope).Bind(resScope =>
            DottedName.Many().Select(dottedNames =>

            $"{resScope} {String.Join('/', dottedNames)}"

            )));

        public static Compiler<Expr, string> ResolutionScope = new Compiler<Expr, string>("ResolutionScope", () => 
            Util.Next(".module").Apply(
            Util.Next(Filename).Select(x =>

            $"[.module {x}]"

            ))
            .Or(Util.Next(DottedName.Select(x => $"[{x}]"))));

        public static Compiler<Expr, string> Id = Util.Symbol;

        // 211
        public static Compiler<Expr, string> FieldAttr = new Compiler<Expr, string>("FieldAttr", () =>
            Util.SymbolIn("assembly", "famandassem", "family", "famorassem", 
                "initonly", "notserialized", "private", "compilercontrolled",
                "public", "rtspecialname", "specialname", "static")
            .Or(Marshal));


        public static Compiler<Expr, string> Marshal = new Compiler<Expr, string>("Marshal", () => 
            Util.Next("marshal").Apply(
            Util.NextOptional(NativeType).Select(typ => 

            $"marshal ({typ})"

            )));

        public static Compiler<Expr, string> NativeType = new Compiler<Expr, string>("NativeType", () =>
            Util.SymbolIn(new List<string>()
            {
                "bool",
                "float32", "float64",
                "lpstr", "lpwstr",
                "method",
            })
            .Or(SimpleIntType))
            .Or(Util.OperatorIs("[]"));

        // 148
        public static Compiler<Expr, string> NativeTypeArray =
            Util.Next(NativeType).Bind(nType =>
            Util.Next(Util.OperatorIs("[]")).Return<string>(

            $"{nType}[]"

            )).Or(
            Util.Next(NativeType).Bind(nType => 
            Util.Next(Util.OperatorIs("[")).Apply(
            Util.Next(Util.IntConstant).Bind(i => 
            Util.Next(Util.OperatorIs("]")).Return<string>(
            
            $"{nType}[{i}]"

            ))))); // TODO: Theres other versions that I'm not worrying about

        public static Compiler<Expr, string> IntType = new Compiler<Expr, string>("IntType", () => 
            Util.NextOptional("unsigned").Bind(unsigned =>
            Util.Next(SimpleIntType).Select(intType => 

            $"{unsigned} {intType}"

            ))
            .Or(SimpleIntType));

        public static Compiler<Expr, string> SimpleIntType = new Compiler<Expr, string>("SimpleIntType", () => 
            Util.SymbolIn("int", "int8", "int16", "int32", "int64"));


        // 161
        public static Compiler<Expr, string> ClassDecl = new Compiler<Expr, string>("ClassDecl", () => 
            Util.Next(".class").Apply(
            ClassHeader.Bind(header =>
            Util.Next(Util.Many<string>(ClassMember)).Select(members =>

            $".class {header} {{\n{String.Join("\n", members)}\n}}"

            ))));

        // 161
        public static Compiler<Expr, string> ClassHeader = new Compiler<Expr, string>("ClassHeader", () => 
            Util.ManyUntil(ClassAttr).Bind(classAttrs =>
            Util.Next(Id).Bind(id =>
            GenPars.Bind(genPars =>
            Util.NextOptional(Extends).Bind(baseClass =>
            Util.NextOptional(Implements).Select(interfaces =>

            $"{string.Join(" ", classAttrs)} {id}{genPars} {baseClass} {interfaces}"

            ))))));

        public static Compiler<Expr, string> Extends = new Compiler<Expr, string>("Extends", () => 
            Util.Next("extends").Apply(
            Util.Next(Id).Select(id =>

            $"extends {id}"

            )));

        public static Compiler<Expr, string> Implements = new Compiler<Expr, string>("Implements", () =>
            Util.Next("implements").Apply(
            Util.AtLeastOnce(Id).Select(ids =>

            $"implements {string.Join(", ", ids)}"

            )));

        // 145s
        public static Compiler<Expr, string> TypeSpec = new Compiler<Expr, string>("TypeSpec", () =>
            Compiler<Expr, string>.Or(
                Util.Next(Util.OperatorIs("[")).Apply(
                Util.NextOptional(".module").Bind(mod => 
                Util.Next(DottedName).Bind(n => 
                Util.Next(Util.OperatorIs("]")).Return<string>(

                $"[{mod} {n}]"

                )))),
                Type, 
                TypeReference
            ));

        // 165
        public static Compiler<Expr, string> GenPars = new Compiler<Expr, string>("GenPars", () => 
            Util.Next(Util.OperatorIs("<")).Apply(
            GenPar.AtLeastOnce().Bind(genPars => 
            Util.Next(Util.OperatorIs(">")).Return(

            $"<{string.Join(", ", genPars)}>"

            )))
            .Or(Util.Return("")));

        // 165
        public static Compiler<Expr, string> GenPar = new Compiler<Expr, string>("GenPar", () =>
            Util.ManyUntil(GenParAttribs).Bind(attribs =>
            Util.NextOptional(GenConstraints).Bind(constraints =>
            Util.Next(Id).Select(id =>

            $"{String.Join(" ", attribs)} {constraints} {id}"

            )))
            .Or(Id));

        // 165
        public static Compiler<Expr, string> GenParAttribs = new Compiler<Expr, string>("GenParAttribs", () => 
            Util.SymbolIn("class","valuetype",".ctor")
            .Or(Util.OperatorIn("+","-")));

        // 166
        public static Compiler<Expr, string> GenConstraints = new Compiler<Expr, string>("GenConstraints", () => 
            Util.AtLeastOnce(Type)
            .Select(typs => "("+String.Join(", ", typs)+")"));


        // 162
        public static Compiler<Expr, string> ClassAttr = new Compiler<Expr, string>("ClassAttr", () => 
            Util.SymbolIn("public", "abstract", "ansi", "autochar", "beforefieldinit", "explicit",
                "interface", "private", "rtspecialname", "sealed", "sequential", "serializable",
                "specialname", "unicode")
            .Or(
            Util.Next("nested").Apply(
            Util.Next(Util.SymbolIn("assembly","famandassem","family","famorassem","private","public")).Select(inner =>

            $"nested {inner}"

            ))));


        // 169
        public static Compiler<Expr, string> ClassMember = new Compiler<Expr, string>("ClassMember", () =>
            Compiler<Expr, string>.Or(
                ClassDecl,
                Util.Next(".custom").Apply(CustomDecl.Select(c => $".custom {c}")), 
                Util.Next(".data").Apply(DataDecl.Select(d => $".data {d}")),
                Util.Next(".event").Apply(EventDecl.Select(e => $".event {e}")),
                Util.Next(".field").Apply(FieldDecl.Select(f => $".field {f}")),
                Util.Next(".method").Apply(MethodDecl.Select(m => $".method {m}")),
                Util.Next(".override").Apply(OverrideDecl.Select(o => $".override {o}")),
                Util.Next(".pack").Apply(Util.IntConstant.Select(i => $".pack {i}")),
                Util.Next(".param").Apply(Util.Next("type").Apply(Util.Next(Util.IntConstant.Select(i => $".param type [{i}]")))),
                Util.Next(".property").Apply(PropHeader.Bind(propHeader => PropMember.ManyUntil().Select(members => $".property {propHeader} {{\n{string.Join("\n", members)}\n}}"))),
                Util.Next(".size").Apply(Util.IntConstant.Select(i => $".size {i}")),
                Util.Next(".line").Apply(ExternSourceDecl.Select(e => $".line {e}")),
                Util.Next(".permissionset").Apply(PermissionSetDecl.Select(p => $".permissionset {p}")),
                Util.Next(".permission").Apply(PermissionDecl.Select(p => $".permission {p}"))

            ));

        // 216
        public static Compiler<Expr, string> PropHeader = new Compiler<Expr, string>("PropHeader", () => 
            Util.NextOptional("specialname").Bind(specialName =>
            Util.NextOptional("rtspecialname").Bind(rtSpecialName => 
            CallConv.Bind(callConv => 
            Util.Next(Type).Bind(@type =>
            Util.Next(Id).Bind(id =>
            Util.Next(Parameters).Select(@params =>

            $"{specialName} {rtSpecialName} {callConv} {@type} {id} ({@params})"

            ))))))
        );

        // 216
        public static Compiler<Expr, string> PropMember = new Compiler<Expr, string>("PropMember", () =>
            Compiler<Expr, string>.Or(
                Util.Next(".custom").Apply(CustomDecl.Select(c => $".custom {c}")),

                Util.Next(Util.SymbolIn(".get", ".set", ".other")).Bind(propItem =>
                CallConv.Bind(callConv =>
                Util.Next(Type).Bind(@type =>
                Util.NextOptional(TypeSpec.Select(t => t + "::")).Bind(typeSpec => 
                Util.Next(MethodName).Bind(methodName => 
                Util.Next(Parameters).Select(@params => 

                $"{propItem} {callConv} {@type} {typeSpec}{methodName}({@params})"

                )))))),

                Util.Next(".line").Apply(ExternSourceDecl.Select(e => $".line {e}"))
            )
        );

        // 169
        public static Compiler<Expr, string> OverrideDecl = new Compiler<Expr, string>("OverrideDecl", () => 
            Util.Next(TypeSpec).Bind(ts1 => 
            Util.Next(MethodName).Bind(m1 => 
            Util.Next("with").Apply(
            CallConv.Bind(callConv =>
            Util.Next(Type).Bind(@type =>
            Util.Next(TypeSpec).Bind(ts2 =>
            Util.Next(MethodName).Bind(m2 => 
            Util.Next(Parameters).Select(@params =>
            
            $"{ts1}::{m1} with {callConv} {@type} {ts2}::{m2}({@params})"

            ))))))))
        );

        // 218
        public static Compiler<Expr, string> EventDecl = new Compiler<Expr, string>("EventDecl", () => 
            Util.Next(EventHeader).Bind(header => 
            Util.Next(EventMember.Many()).Select(members => 
            
            $"{header} {{\n{string.Join("\n", members)}\n}}"

            ))
        );

        // 218
        public static Compiler<Expr, string> EventHeader = new Compiler<Expr, string>("EventHeader", () => 
            Util.NextOptional("specialname").Bind(sn => 
            Util.NextOptional("rtspecialname").Bind(rtsn => 
            Util.NextOptional(TypeSpec).Bind(typeSpec => 
            Util.Next(Id).Select(id => 

            $"{sn} {rtsn} {typeSpec} {id}"

            )))).Or(Id)
        );

        // 218
        public static Compiler<Expr, string> EventMember = new Compiler<Expr, string>("EventMember", () => 
            Compiler<Expr, string>.Or(
                Util.Next(".addon").Apply(FireDecl).Select(a => $".addon {a}"),
                Util.Next(".custom").Apply(CustomDecl).Select(c => $".custom {c}"),
                Util.Next(".fire").Apply(FireDecl).Select(f => $".fire {f}"),
                Util.Next(".other").Apply(FireDecl).Select(o => $".other {o}"),
                Util.Next(".removeon").Apply(FireDecl).Select(r => $".removeon {r}"),
                Util.Next(".line").Apply(ExternSourceDecl.Select(e => $".line {e}"))
            )
        );

        // 218
        public static Compiler<Expr, string> FireDecl = new Compiler<Expr, string>("AddonDecl", () => 
            Util.Next(CallConv).Bind(callConv => 
            Util.Next(Type).Bind(type => 
            Util.NextOptional(TypeSpec.Select(ts => ts + " ::")).Bind(typeSpec => 
            Util.Next(MethodName).Bind(methodName => 
            Util.Next(Parameters).Select(parameters => 

            $"{callConv} {type} {typeSpec} {methodName}({parameters})"

            )))))
        );

        // 134
        public static Compiler<Expr, string> ExternSourceDecl = new Compiler<Expr, string>("ExternSourceDecl", () => 
            Util.Next(Util.IntConstant).Bind(i => 
            Util.NextOptional(Util.IntConstant.Select(l => $":{l}")).Bind(c => 
            Util.NextOptional(SQString).Select(id => 
            
            $"{i}{c} {id}"

            )))
        );


        // 225
        // MethodHeader should be ctor (a special case for MethodHeader, not sure if I should sep. it)
        public static Compiler<Expr, string> CustomDecl = new Compiler<Expr, string>("CustomDecl", () =>
            Ctor.Bind(ctor => 
            Util.Next(Util.OperatorIs("=")).Apply(
            Util.Next(Bytes).Select(bytes => 

            $"{ctor} = ({bytes})"

            ))));

        // 169
        public static Compiler<Expr, string> MethodDecl = new Compiler<Expr, string>("MethodDecl", () =>
            MethodHeader.Bind(header => 
            Util.Next(MethodBodyItem.Many()).Select(items => 
            
            $"{header} {{\n{string.Join("\n", items)}\n}}"

            ))
        );

        // 199
        public static Compiler<Expr, string> MethodBodyItem = new Compiler<Expr, string>("MethodBodyItem", () =>
            Compiler<Expr, string>.Or(
                Util.Next(".custom").Apply(CustomDecl.Select(c => $".custom {c}")),
                Util.Next(".data").Apply(DataDecl.Select(d => $".data {d}")),
                Util.Next(".emitbyte").Apply(Util.Next(Util.IntConstant).Select(i => $".emitbyte {i}")),
                Util.Next(".entrypoint"),
                Util.Next(".locals").Apply(LocalsDecl.Select(l => $".locals {l}")),
                Util.Next(".maxstack").Apply(Util.IntConstant.Select(i => $".maxstack {i}")),
                Util.Next(".override").Apply(OverrideDir.Select(o => $".override {o}")),
                Util.Next(".param").Apply(ParamDir.Select(p => $".param {p}")),
                Util.Next(".line").Apply(ExternSourceDecl.Select(e => $".line {e}")),
                Instr,
                InstrPrefix,
                ObjectModel,
                Util.Next(Id).Bind(id => Util.Next(Util.OperatorIs(":").Return($"{id}:"))),
                ScopeBlock,
                Util.Next(".permissionset").Apply(PermissionSetDecl.Select(p => $".permissionset {p}")),
                Util.Next(".permission").Apply(PermissionDecl.Select(p => $".permission {p}")),
                SEHBlock

            )
        );

        // 349
        public static Compiler<Expr, string> InstrPrefix = new Compiler<Expr, string>("InstrPrefix", () => 
            Compiler<Expr, string>.Or(
                Util.Next("constrained.").Apply(
                Util.Next(Type).Select(@type => 

                $"constrained. {@type}"

                )),

                Util.Next("no.").Apply(
                Util.Next(Util.SymbolIn("typecheck", "rangecheck", "nullcheck")).Select(check =>
                
                $"no. {check}"
                
                )),

                Util.SymbolIs("readonly."),

                Util.SymbolIs("tail."),

                // 354
                Util.Next("unaligned.").Apply( 
                Util.Next(Util.IntConstant).Select(i => 
                
                $"unaligned. {i}"
                
                )),

                Util.SymbolIs("volatile.")
            )
        );

        // 357
        public static Compiler<Expr, string> Instr = new Compiler<Expr, string>("Instr", () => 
            Compiler<Expr, string>.Or(

                // Arithmetic Operations
                Util.SymbolIn(
                    "add", "add.ovf", "add.ovf.un",
                    "div", "div.un",
                    "mul", "mul.ovf", "mul.ovf.un", 
                    "rem", "rem.un",
                    "sub", "sub.ovf", "sub.ovf.un",
                    "neg"
                ),
                
                // Binary Operations
                Util.SymbolIn(
                    "and", "neg", "not", "or", "xor"
                ),

                // Stack Operations
                Util.SymbolIn(
                    "dup", "pop"
                ),

                // Breaks
                Util.Next(Util.SymbolIn(
                    "beq", "beq.s", 
                    "bne.un", "bne.un.s", 
                    "bge", "bge.s", "bge.un", "bge.un.s", 
                    "bgt", "bgt.s", "bgt.un", "bgt.un.s", 
                    "ble", "ble.s", "ble.un", "ble.un.s", 
                    "blt", "blt.s", "blt.un", "blt.un.s",
                    "brfalse", "brfalse.s", 
                    "brinst", "brinst.s",
                    "brnull", "brnull.s",
                    "brtrue", "brtrue.s",
                    "brzero", "brzero.s"
                    )
                ).Bind(instr =>
                Util.Next(Util.IntConstant).Select(i => 
                
                $"{instr} {i}"
                
                )),
                Util.SymbolIn("break"),

                // Compare
                Util.SymbolIn(
                    "ceq", "cgt", "cgt.un", "ckfinite", "clt", "clt.un"
                ),

                // Calls 
                Util.Next(Util.SymbolIn("call", "callvirt")).Bind(instr =>
                // I read the examples and it looks like this is the right way to reference a method
                MethodHeader.Select(method => 

                $"{instr} {method}"

                )),
                Util.Next(Util.SymbolIn("calli")).Bind(instr =>
                // TODO: Find an example of this. this calls for a "call site descriptor", 
                // which I think is like methodheader but without the name, idk I haven't made a compiler for that
                MethodHeader.Select(method => 

                $"{instr} {method}"

                )),
                Util.Next(Util.SymbolIn("jmp")).Bind(instr =>
                // TODO: Looks like its similar to call* but with a tighter method req. so idk
                MethodHeader.Select(method => 

                $"{instr} {method}"

                )),


                // Convert
                Util.SymbolIn(
                    "conv.i1", "conv.i2", "conv.i4", "conv.i8", 
                    "conv.u1", "conv.u2", "conv.u4", "conv.u8", 
                    "conv.r4", "conv.r8",
                    "conv.i", "conv.u", "conv.r.un",

                    "conv.ovf.i1", "conv.ovf.i2", "conv.ovf.i4", "conv.ovf.i8", 
                    "conv.ovf.u1", "conv.ovf.u2", "conv.ovf.u4", "conv.ovf.u8", 
                    "conv.ovf.i", "conv.ovf.u",

                    "conv.ovf.i1.un", "conv.ovf.i2.un", "conv.ovf.i4.un", "conv.ovf.i8.un", 
                    "conv.ovf.u1.un", "conv.ovf.u2.un", "conv.ovf.u4.un", "conv.ovf.u8.un", 
                    "conv.ovf.i.un", "conv.ovf.u.un"
                ),
                
                
                // Load/Store
                Util.SymbolIn("ldarg.0", "ldarg.1", "ldarg.2", "ldarg.3"),

                Util.Next(Util.SymbolIn("ldarg", "ldarg.s", "ldarga", "ldarga.s")).Bind(inst => 
                Util.Next(Util.IntConstant).Select(i =>
                
                $"{inst} {i}"
                
                )),
                Util.Next(Util.SymbolIn("ldc.i4", "ldc.i8", "ldc.i4.s")).Bind(instr => 
                Util.Next(Util.IntConstant).Select(i => 
                
                $"{instr} {i}"
                
                )),
                Util.Next(Util.SymbolIn("ldc.r4", "ldc.r8")).Bind(instr => 
                Util.Next(Util.FloatConstant).Select(i => 
                
                $"{instr} {i}"
                
                )),
                Util.SymbolIn(
                    "ldc.i4.0", "ldc.i4.1", "ldc.i4.2", "ldc.i4.3", "ldc.i4.4", 
                    "ldc.i4.5", "ldc.i4.6", "ldc.i4.7", "ldc.i4.8", 
                    "ldc.i4.m1", "ldc.i4.M1"
                ),
                Util.Next(Util.SymbolIn("ldftn")).Bind(instr =>
                // TODO: I'm duplicating all of these signatures because I dont fully understand them lol
                MethodHeader.Select(method => 

                $"{instr} {method}"

                )),
                Util.SymbolIn(
                    "ldind.i1", "ldind.i2", "ldind.i4", "ldind.i8",
                    "ldind.u1", "ldind.u2", "ldind.u4", "ldind.u8",
                    "ldind.r4", "ldind.r8",
                    "ldind.i", "ldind.ref"
                ),
                Util.SymbolIn(
                    "ldloc.1", "ldloc.2", "ldloc.3", "ldloc.4",
                    "stloc.1", "stloc.2", "stloc.3", "stloc"
                ),
                Util.Next(Util.SymbolIn(
                    "ldloc", "ldloc.s", "ldloca", "ldloca.s",
                    "stloc", "stloc.s", "stloca", "stloca.s"
                    )).Bind(instr => 
                Util.Next(Util.IntConstant).Select(i => 
                
                $"{instr} {i}"
                
                )),
                Util.SymbolIn("ldnull"),
                Util.SymbolIn("starg", "starg.s"),
                Util.SymbolIn(
                    "stind.i1", "stind.i2", "stind.i4", "stind.i8",
                    "stind.r4", "stind.r8", 
                    "stind.i", "stind.ref"
                ),


                Util.Next(Util.SymbolIn("leave", "leave.s")).Bind(instr => 
                Util.Next(Util.IntConstant).Select(i => 
                
                $"{instr} {i}"

                )),

                // Everything else

                Util.SymbolIn("endfilter", "endfault", "endfinally"),

                Util.SymbolIn("cpblk", "initblk"),
                Util.SymbolIn("localloc"),

                Util.SymbolIn("ret"),

                Util.SymbolIn("nop"),

                Util.SymbolIn("shl", "shr", "shr.un"),


                Util.Next("switch").Apply(
                Util.IntConstant.AtLeastOnce().Select(@is => 

                $"switch {string.Join(" ", @is)}"

                )),

                Util.SymbolIn("arglist")
            )
        );

        // 427
        public static Compiler<Expr, string> ObjectModel = new Compiler<Expr, string>("ObjectModel", () => 
            Compiler<Expr, string>.Or(
                Util.Next(Util.SymbolIn(
                    "callvirt", 
                    "ldvirtftn",
                    "newobj"
                )).Bind(instr =>
                Util.Next(MethodHeader).Select(header => 
                
                $"{instr} {header}"
                
                )),

                Util.Next(Util.SymbolIn(
                    "box", "unbox", "unbox.any",
                    "castclass", "cpobj", "initobj", "isinst", 
                    "ldelem", "ldelema", "stelem",
                    "ldobj", "stobj",
                    "mkrefany",
                    "newarr",
                    "refanyval",
                    "sizeof"
                )).Bind(instr =>
                Util.Next(Type).Select(@type => 
                
                $"{instr} {@type}"
                
                )),

                Util.SymbolIn(
                    "ldelem.i1", "ldelem.i2", "ldelem.i4", "ldelem.i8",
                    "ldelem.u1", "ldelem.u2", "ldelem.u4", "ldelem.u8", 
                    "ldelem.r4", "ldelem.r8", 
                    "ldelem.i", "ldelem.ref",
                    "stelem.i1", "stelem.i2", "stelem.i4", "stelem.i8",
                    "stelem.u1", "stelem.u2", "stelem.u4", "stelem.u8", 
                    "stelem.r4", "stelem.r8", 
                    "stelem.i", "stelem.ref"
                ),

                Util.Next(Util.SymbolIn(
                    "ldfld", "ldflda", 
                    "ldsfld", "ldsflda",
                    "stfld", "stsfld"
                )).Bind(instr =>
                Util.Next(FieldDecl).Select(field =>
                
                $"{instr} {field}"

                )),

                Util.SymbolIn("ldlen"),

                Util.Next("ldstr").Apply(
                Util.Next(QString).Select(str =>
                
                $"ldstr {str}"
                
                )),

                // FIXME: not sure if this is the full definition or the right one, 
                // gonna need to experiment
                Util.Next("ldtoken").Apply(
                Util.Next(Type.Or(MethodHeader).Or(FieldDecl)).Select(token => 
                
                $"ldtoken {token}"
                
                )),

                Util.SymbolIn("refanytype", "rethrow", "throw")

            )
        );


        // 205
        public static Compiler<Expr, string> ScopeBlock = new Compiler<Expr, string>("ScopeBlock", () => 
            MethodBodyItem.Many().Select(items => 
                $"{{\n{string.Join("\n", items)}\n}}"
            )
        );

        public static Compiler<Expr, string> PermissionSetDecl = new Compiler<Expr, string>("PermissionSetDecl", () => 
            Util.Next(SecAction).Bind(secAction => 
            Util.Next(Util.OperatorIs("=")).Apply(
            Util.Next(Bytes).Select(bytes => 

            $"{secAction} = ({bytes})"

            )))
        );
        public static Compiler<Expr, string> PermissionDecl = new Compiler<Expr, string>("PermissionDecl", () => 
            Util.Next(SecAction).Bind(secAction => 
            Util.Next(TypeReference).Bind(typeReference =>
            Util.Next(NameValPairs).Select(nvps =>

            $"{secAction} {typeReference} ({nvps})"

            )))
        );

        // // 224
        // public static Compiler<Expr, string> SecurityDecl = new Compiler<Expr, string>("SecurityBlock", () => 
        //     Compiler<Expr, string>.Or(
        //         Util.Next(".permissionset").Apply(
        //         Util.Next(SecAction).Bind(secAction => 
        //         Util.Next(Util.OperatorIs("=")).Apply(
        //         Util.Next(Bytes).Select(bytes => 

        //         $".permissionset {secAction} = ({bytes})"

        //         )))),

        //         Util.Next(".permission").Apply(
        //         Util.Next(SecAction).Bind(secAction => 
        //         Util.Next(TypeReference).Bind(typeReference =>
        //         Util.Next(NameValPairs).Select(nvps =>

        //         $".permission {secAction} {typeReference} ({nvps})"

        //         ))))
        //     )
        // );

        // 224
        public static Compiler<Expr, string> SecAction = new Compiler<Expr, string>("SecAction", () => 
            Util.SymbolIn("assert", "demand", "deny", "inheritcheck", "linkcheck", 
            "permitonly", "reqopt", "reqrefuse", "request"));

        // 224
        public static Compiler<Expr, string> NameValPairs = new Compiler<Expr, string>("NameValPairs", () => 
            NameValPair.AtLeastOnce().Select(nvps => string.Join(", ", nvps))
        );

        // 224
        public static Compiler<Expr, string> NameValPair = new Compiler<Expr, string>("NameValPair", () => 
            Util.Next(SQString).Bind(key => 
            Util.Next(Util.OperatorIs("=")).Apply(
            Util.Next(SQString).Select(value =>
            
            $"{key} = {value}"

            )))
        );

        // 221
        public static Compiler<Expr, string> SEHBlock = new Compiler<Expr, string>("SEHBlock", () => 
            Util.Next(TryBlock).Bind(tryBlock => 
            SEHClause.AtLeastOnce().Select(clauses => 
            
            $"{tryBlock}\n{string.Join("\n", clauses)}"

            ))
        );


        // 221
        public static Compiler<Expr, string> TryBlock = new Compiler<Expr, string>("TryBlock", () => 
            Compiler<Expr, string>.Or(
                Util.Next(".try").Apply(
                Util.Next(DataLabel).Bind(start => 
                Util.Next("to").Apply(
                Util.Next(DataLabel).Select(end => 
                
                $".try {start} to {end}"

                )))),

                Util.Next(".try").Apply(
                Util.Next(ScopeBlock).Or(ScopeBlock).Select(scopeBlock => 
                
                $".try {scopeBlock}"

                ))
            )
        );

        public static Compiler<Expr, string> SEHClause = new Compiler<Expr, string>("SEHClause", () => 
            Compiler<Expr, string>.Or(
                Util.Next("catch").Apply(
                Util.Next(TypeReference).Bind(typeReference =>
                Util.Next(HandlerBlock).Or(HandlerBlock).Select(handlerBlock => 

                $"catch {typeReference} {handlerBlock}"

                ))),

                Util.Next("fault").Apply(
                Util.Next(HandlerBlock).Or(HandlerBlock).Select(handlerBlock =>
                
                $"fault {handlerBlock}"

                )),

                Util.Next("filter").Apply(
                Util.Next(DataLabel).Bind(label =>
                Util.Next(HandlerBlock).Or(HandlerBlock).Select(handlerBlock =>
                
                $"filter {label} {handlerBlock}"
                
                ))),

                Util.Next("finally").Apply(
                Util.Next(HandlerBlock).Or(HandlerBlock).Select(handlerBlock =>
                
                $"finally {handlerBlock}"

                ))
            )
        );


        public static Compiler<Expr, string> HandlerBlock = new Compiler<Expr, string>("HandlerBlock", () =>
            Util.Next("handler").Apply(
            Util.Next(DataLabel).Bind(start => 
            Util.Next("to").Apply(
            Util.Next(DataLabel).Select(end => 

            $"handler {start} to {end}"

            )))).Or(ScopeBlock)
        );

        // 201
        public static Compiler<Expr, string> ParamDir = new Compiler<Expr, string>("ParamDir", () => 
            Compiler<Expr, string>.Or(
                Util.Next(Util.IntConstant).Bind(i => 
                Util.Next(Util.OperatorIs("=")).Apply(
                Util.Next(FieldInit).Select(fieldInit => 

                $"[{i}] = {fieldInit}"

                ))),

                Util.Next(Util.IntConstant).Select(i => $"[{i}]"),

                Util.Next("type").Apply(
                Util.Next(Util.IntConstant).Select(i => 

                $"type [{i}]"

                ))
            
            )
        );

        // 170
        public static Compiler<Expr, string> OverrideDir = new Compiler<Expr, string>("OverrideDir", () => 
            Compiler<Expr, string>.Or(
                TypeSpec.Or(Util.Next(TypeSpec)).Bind(typeSpec => 
                Util.Next(MethodName).Select(methodName => 
                
                $"{typeSpec}::{methodName}"

                )),

                Util.Next("method").Apply(
                CallConv.Bind(callConv => 
                Util.Next(Type).Bind(@type => 
                TypeSpec.Or(Util.Next(TypeSpec)).Bind(typeSpec => 
                Util.Next(MethodName).Bind(methodName => 
                GenArity.Bind(genArity => 
                Util.Next(Parameters).Select(parameters => 
                
                $"method {callConv} {@type} {typeSpec}::{methodName} {genArity}({parameters})"

                )))))))
            )
        );

        // 170
        public static Compiler<Expr, string> GenArity = new Compiler<Expr, string>("GenArity", () => 
            Util.Next(Util.OperatorIs("<[")).Apply(
            Util.Next(Util.IntConstant).Bind(i => 
            Util.Next(Util.OperatorIs("]>")).Return(

            $"<[{i}]>"

            ))).Or(Util.Return(""))
        );


        // 201
        public static Compiler<Expr, string> LocalsDecl = new Compiler<Expr, string>("LocalsDecl", () => 
            Util.NextOptional("init").Bind(init => 
            Util.Next(LocalsSignature).Select(localsSig => 
            
            $"{init} ({localsSig})"

            ))
        );

        // 201 
        public static Compiler<Expr, string> LocalsSignature = new Compiler<Expr, string>("LocalsSignature", () => 
            Local.AtLeastOnce().Select(ls => string.Join(", ", ls))
        );

        // 201
        public static Compiler<Expr, string> Local = new Compiler<Expr, string>("Local", () => 
            Util.Next(Type).Bind(@type => 
            Util.Next(Id).Select(id => 

            $"{@type} {id}"

            )).Or(Type)
        );

        // 198 I know this is a duplicate, I just think its going to be better like this
        public static Compiler<Expr, string> Ctor = new Compiler<Expr, string>("Ctor", () => 
            MethodAttr.ManyUntil().Bind(attrs => 
            Util.NextOptional(CallConv).Bind(callConv => 
            Util.Next(Type).Bind(@type =>
            Util.NextOptional(Marshal).Bind(marshal => 
            Util.Next(".ctor").Bind(methodName =>
            GenPars.Bind(genPars => 
            Util.Next(Parameters).Bind(parameters => 
            ImplAttr.ManyUntil().Select(implAttrs => 

            $"{string.Join(" ", attrs)} {callConv} {@type} {marshal} {methodName}{genPars}({parameters}) {string.Join(" ", implAttrs)}"

            )))))))));

        // 198
        public static Compiler<Expr, string> MethodHeader = new Compiler<Expr, string>("MethodHeader", () => 
            MethodAttr.ManyUntil().Bind(attrs => 
            Util.NextOptional(CallConv).Bind(callConv => 
            Util.Next(Type).Bind(@type =>
            Util.NextOptional(Marshal).Bind(marshal => 
            Util.Next(MethodName).Bind(methodName =>
            GenPars.Bind(genPars => 
            Util.Next(Parameters).Bind(parameters => 
            ImplAttr.ManyUntil().Select(implAttrs => 

            $"{string.Join(" ", attrs)} {callConv} {@type} {marshal} {methodName}{genPars}({parameters}) {string.Join(" ", implAttrs)}"

            )))))))));

        // 198
        public static Compiler<Expr, string> MethodName = new Compiler<Expr, string>("MethodName", () =>
            Compiler<Expr, string>.Or(
                Util.SymbolIn(".cctor", ".ctor"),
                DottedName
            ) 
        );

        // 204
        public static Compiler<Expr, string> ImplAttr = new Compiler<Expr, string>("ImplAttr", () => 
            Util.SymbolIn("cil", "forwardref", "internalcall", "managed", "native",
            "noinlining", "runtime", "synchronized", "unmanaged")
        );

        // 202
        public static Compiler<Expr, string> MethodAttr = new Compiler<Expr, string>("MethodAttr", () => 
            Util.SymbolIn("abstract", "assembly", "compilercontrolled", "famandassem",
            "family", "famorassem", "final", "hidebysig", "newslot", "private",
            "public", "rtspecialname", "specialname", "static", "virtual", "strict")
            .Or(
            
            Util.Next("pinvokeimpl").Apply(
            Util.Next(QString).Bind(qString => 
            Util.NextOptional(
                Util.Next("as").Apply(Util.Next(QString)).Select(x => $"as {x}")
            ).Bind(asQString => 
            PinvAttr.ManyUntil().Select(pinvAttrs => 
            
            $"pinvokeimpl ({qString} {asQString} {string.Join(" ", pinvAttrs)})"
            
            ))))));

        // 208
        public static Compiler<Expr, string> PinvAttr = new Compiler<Expr, string>("PinvAttr", () => 
            Util.SymbolIn("ansi", "autochar", "cdecl", "fastcall", "stdcall", 
            "thiscall", "unicode", "platformapi")
        );

        // 213
        public static Compiler<Expr, string> DataDecl = new Compiler<Expr, string>("DataDecl", () =>
            Util.NextOptional(DataLabel).Bind(label =>
            Util.Next(DdBody).Select(body =>

            label != null ? $"{label} = {body}" : $"{body}"

            )));

        public static Compiler<Expr, string> DataLabel = new Compiler<Expr, string>("DataLabel", () => Id);

        // 213
        public static Compiler<Expr, string> DdBody = new Compiler<Expr, string>("DdBody", () => 
            Compiler<Expr, string>.Or(
                DdItem,
                Util.Many(DdItem).Select(items => $"{{{string.Join(", ", items)}}}")
            ));

        // 213
        public static Compiler<Expr, string> DdItem = new Compiler<Expr, string>("DdItem", () => 
            Compiler<Expr, string>.Or(
                Util.Next(Util.OperatorIs("&")).Apply(Util.Next(Id).Select(id => $"& ({id})")),

                Util.Next("bytearray").Apply(Util.Next(Bytes).Select(bytes => $"bytearray ({bytes})")),

                Util.Next("char").Apply(Util.Next(Util.OperatorIs("*")).Apply(Util.Next(QString).Select(qstr => $"char * ({qstr})"))),

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
            ));

        public static Compiler<Expr, string> QString = Util.DQStringConstant.Select(s => $"{s.Escaped()}");

        public static Compiler<Expr, string> SQString = Util.SQStringConstant.Select(s => $"{s.Escaped()}");

    }
}
