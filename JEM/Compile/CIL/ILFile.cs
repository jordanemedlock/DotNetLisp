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
            Util.Symbol.Where(x => !x.StartsWith('.') || !x.EndsWith('.'), "DottedName");

        // 138
        public static Compiler<Expr, string> HashAlg = new Compiler<Expr, string>("HashAlg", () => 
            Util.Next(".hash").Apply(
            Util.Next("algorithm").Apply(
            Util.Next(Util.IntConstant).Select(i =>
            
            $".hash algorithm {i}"
            
            ))));

        // 138
        public static Compiler<Expr, string> Version = new Compiler<Expr, string>("Version", () => 
            Util.Next(".ver").Apply(
            Util.Next(Util.IntConstant).Bind(major =>
            Util.Next(Util.IntConstant).Bind(minor =>
            Util.Next(Util.IntConstant).Bind(build =>
            Util.Next(Util.IntConstant).Select(revision =>
            
            $".ver {major} : {minor} : {build} : {revision}"

            ))))));

        // 138
        public static Compiler<Expr, string> Culture = new Compiler<Expr, string>("Culture", () =>
            Util.Next(".culture").Apply(
            Util.Next(Util.DQStringConstant).Select(str => 
            
            $".culture {str.Escaped()}"

            )));

        // 138
        public static Compiler<Expr, string> AsmDecl = new Compiler<Expr, string>("AsmDecl", () => 
            Compiler<Expr, string>.Or(
                HashAlg,
                Version, 
                Culture
            ));

        // 137
        public static Compiler<Expr, string> ExternAssembly = new Compiler<Expr, string>("ExternAssembly", () => 
            Util.Next(".assembly").Apply(
            Util.Next("extern").Apply(
            Util.Next(DottedName).Bind(dottedName => 
            Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
            $".assembly extern {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"
            
            )))));

        // 137
        public static Compiler<Expr, string> NonExternAssembly = new Compiler<Expr, string>("NonExternAssembly", () => 
            Util.Next(".assembly").Apply(
            Util.Next(DottedName).Bind(dottedName => 
            Util.Next(AsmDecl.Many()).Select(asmDecls =>
            
            $".assembly {dottedName} {{\n\t{string.Join('\n', asmDecls)}\n}}"

            ))));

        public static Compiler<Expr, string> Assembly = new Compiler<Expr, string>("Assembly", () => 
            ExternAssembly.Or(NonExternAssembly)
        );

        // 135
        public static Compiler<Expr, string> Decl = new Compiler<Expr, string>("Decl", () =>
            Compiler<Expr, string>.Or(
                Assembly,
                Util.Next(".corflags").Apply(Util.Next(Util.IntConstant).Select(i => $".corflags {i}")), 
                Util.Next(".custom").Apply(CustomDecl.Select(customDecl => $".custom {customDecl}")),
                Util.Next(".data").Apply(DataDecl.Select(d => $".data {d}")),
                FileDirective,
                Field,
                Util.Next(".method").Apply(MethodDecl.Select(m => $".method {m}")),
                Util.Next(".module").Apply(Util.NextOptional(Filename).Select(f => $".module {f}")),
                Util.Next(".module").Apply(Util.Next("extern").Apply(Util.NextOptional(Filename).Select(f => $".module extern {f}"))),
                Util.Next(".mresource").Apply(MResourceDecl.Select(r => $".mresource {r}")),
                Util.Next(".subsystem").Apply(Util.IntConstant.Select(i => $".subsystem {i}")),
                Util.Next(".vtfixup").Apply(VTFixupDecl.Select(vtf => $".vtfixup {vtf}")),
                ExternSourceDecl,
                SecurityDecl
           ));

        // 207
        public static Compiler<Expr, string> VTFixupDecl = new Compiler<Expr, string>("VTFixupDecl", () => 
            Util.NextOptional(Util.IntConstant.Select(i => (long?)i)).Bind(i =>
            Util.SymbolIn("fromunmanaged", "int32", "int64").AtLeastOnce().Bind(attrs => 
            Util.Next("at").Apply(
            Util.Next(DataLabel).Select(label => 

            $"{i} {string.Join(" ", attrs)} at {label}"

            ))))
        );

        // 140
        public static Compiler<Expr, string> MResourceDecl = new Compiler<Expr, string>("MResourceDecl", () => 
            Util.Next(Util.SymbolIn("public", "private")).Bind(access =>
            Util.Next(DottedName).Bind(dottedName =>
            ManResDecl.Many().Select(ress => 
            
            $"{access} {dottedName} {{\n{string.Join("\n", ress)}\n}}"
            
            )))
        );

        // 140
        public static Compiler<Expr, string> ManResDecl = new Compiler<Expr, string>("ManResDecl", () => 
            Compiler<Expr, string>.Or(
                Util.Next(".assembly").Apply(
                Util.Next("extern").Apply(
                Util.Next(DottedName).Select(dottedName =>
                
                $".assembly extern {dottedName}"

                ))),

                Util.Next(".custom").Apply(CustomDecl.Select(c => $".custom {c}")),

                Util.Next(".file").Apply(
                Util.Next(DottedName).Bind(dottedName => 
                Util.Next("at").Apply(
                Util.Next(Util.IntConstant).Select(i => 
                
                $".file {dottedName} at {i}"

                ))))
            )

        );

        // 135
        public static Compiler<Expr, string> FileDirective = new Compiler<Expr, string>("FileDirective", () =>
            Util.Next(".file").Apply(
            Util.NextOptional("nometadata").Bind(nometa => 
            Util.Next(Filename).Bind(filename => 
            Util.Next(".hash").Apply(
            Util.Next(Bytes).Bind(bytes => 
            Util.NextOptional(".entrypoint").Select(entryPoint => 

            $".file {nometa} {filename} .hash = ({bytes}) {entryPoint}"

            )))))));

        public static Compiler<Expr, string> Filename = new Compiler<Expr, string>("Filename", () => 
            Util.DQStringConstant.Select(x => x.Escaped()));

        public static Compiler<Expr, string> Bytes = new Compiler<Expr, string>("Bytes", () => 
            Util.IntConstant.Many().Select(ints =>

            $"{String.Join(' ', ints)}"

            ));

        // 210
        public static Compiler<Expr, string> Field = new Compiler<Expr, string>("Field", () => 
            Util.Next(".field").Apply(
            FieldDecl.Select(fieldDecl => 

            $".field {fieldDecl}"

            )));
        // 212
        public static Compiler<Expr, string> FieldInit = new Compiler<Expr, string>("FieldInit", () => 
            Compiler<Expr, string>.Or(
                Util.Next("bool").Apply(
                Util.Next(Util.BoolConstant).Select(b => 
                
                $"bool ({(b ? "true" : "false")})"
                
                )),

                Util.Next("bytearray").Apply(
                Util.Next(Bytes).Select(b => 
                
                $"bytearray ({b})"
                
                )),

                Util.Next("char").Apply(
                Util.Next(Util.IntConstant).Select(c => 
                
                $"char ({c})"
                
                )),

                Util.Next(Util.SymbolIn("float32","float64")).Bind(fType =>
                Util.Next(Util.FloatConstant.Select(x => x.ToString()).Or(Util.IntConstant.Select(x => x.ToString()))).Select(f => 
                
                $"{fType} ({f})"
                
                )),

                Util.NextOptional("unsigned").Bind(unsig =>
                Util.Next(Util.SymbolIn("int8", "int16", "int32", "int64")).Bind(iType => 
                Util.Next(Util.IntConstant).Select(iValue => 

                $"{unsig} {iType} ({iValue})"

                ))),

                QString,

                Util.SymbolIs("nullref")

            ));

        // 210
        public static Compiler<Expr, string> FieldInitOrDataLabel = new Compiler<Expr, string>("FieldInitOrDataLabel", () => 
            Compiler<Expr, string>.Or(
                Util.Next(Util.OperatorIs("=")).Apply(Util.Next(FieldInit).Select(f => $"= {f}")),
                Util.Next(Util.OperatorIs("=")).Apply(FieldInit.Select(f => $"= {f}")),
                Util.Next("at").Apply(Util.Next(DataLabel).Select(d => $"at {d}"))

            ));

        // 210
        public static Compiler<Expr, string> FieldDecl = new Compiler<Expr, string>("FieldDecl", () => 
            Util.NextOptional(Util.IntConstant.Select(i => "[" + i + "]")).Bind(i =>
            FieldAttr.Many().Bind(fas =>
            Util.Next(Type).Bind(typ =>
            Util.Next(Id).Bind(id =>
            FieldInitOrDataLabel.Or(Util.NextOptional(FieldInitOrDataLabel)).Select(expr =>

            $"{i} {String.Join(' ', fas ?? new List<string>())} {typ} {id} {expr}"

            ))))));

        // 144
        public static Compiler<Expr, string> Type = new Compiler<Expr, string>("Type", () =>
            Compiler<Expr, string>.Or(
                Util.Next(Util.OperatorIn("!", "!!")).Bind(exp => 
                Util.Next(Util.IntConstant).Select(i => $"{exp}{i}")),

                Util.SymbolIn(new List<string>() {
                    "bool", "char",
                    "float32", "float64",
                    "object", "string", "typedref", "void"
                }), 

                IntType, 

                ClassRef, 

                Util.Next("method").Apply(
                Util.NextOptional(CallConv).Bind(callConv =>
                Util.Next(Type).Bind(typ => 
                Util.Next(Parameters).Select(pars => 
            
                $"method {callConv} {typ} * ({pars})"

                )))), 

                TypeModifier, 

                ValueType
            ));

        public static Compiler<Expr, string> ValueType = new Compiler<Expr, string>("ValueType", () =>
            Util.Next("valuetype").Apply(
            Util.Next(TypeReference).Select(typeRef =>

            $"valuetype {typeRef}"

            )));

        public static Compiler<Expr, string> ClassRef = new Compiler<Expr, string>("ClassRef", () => 
            Util.Next("class").Apply(
            Util.Next(TypeReference).Select(typeRef =>

            $"class {typeRef}"

            )));

        // 185
        // TODO: this isn't the whole def. but it doesn't seem super important to implement the rest. idk
        public static Compiler<Expr, string> Bound = 
            Util.SymbolIs("...");

        public static Compiler<Expr, string> TypeModifier = new Compiler<Expr, string>("TypeModifier", () => 
            Compiler<Expr, string>.Or(
                Util.Next(Type).Bind(typ =>
                Util.Next(Util.OperatorIn("&", "*")).Select(sym =>

                $"{typ} {sym}"

                )),
                GenericType, 
                ArrayType, 
                ModType, 
                PinnedType
            ));

        public static Compiler<Expr, string> PinnedType = new Compiler<Expr, string>("PinnedType", () => 
            Util.Next(Type).Bind(typ =>
            Util.Next("pinned").Return<string>(

            $"{typ} pinned"

            )));

        public static Compiler<Expr, string> ModType = new Compiler<Expr, string>("ModType", () =>
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.SymbolIn("modopt", "modreq")).Bind(mod =>
            Util.Next(TypeReference).Select(typeRef =>

            $"{typ} {mod} ({typeRef})"

            ))));

        public static Compiler<Expr, string> ArrayType = new Compiler<Expr, string>("ArrayType", () => 
            Util.Next(Type).Bind(typ =>
            Util.Next(Util.OperatorIs("[")).Apply(
            Bound.Many().Bind(bnds =>
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
            Type.Many().Select(xs => String.Join(", ", xs)));

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
            Util.Many(ParamAttr).Bind(attrs =>
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
            Util.Many<string>(ClassMember).Select(members =>

            $".class {header} {{\n{String.Join("\n", members)}\n}}"

            ))));

        // 161
        public static Compiler<Expr, string> ClassHeader = new Compiler<Expr, string>("ClassHeader", () => 
            Util.Many(ClassAttr).Bind(classAttrs =>
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
            Util.Many(GenParAttribs).Bind(attribs =>
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
                
                // TODO: ClassMember Continue here
                Util.Next(".size").Apply(Util.IntConstant.Select(i => $".size {i}")),
                ExternSourceDecl,
                SecurityDecl

            ));

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
                ExternSourceDecl
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
            Util.Next(".line").Apply(
            Util.Next(Util.IntConstant).Bind(i => 
            Util.NextOptional(Util.IntConstant.Select(l => $":{l}")).Bind(c => 
            Util.NextOptional(SQString).Select(id => 
            
            $".line {i}{c} {id}"

            ))))
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
                ExternSourceDecl,
                Instr,
                Util.Next(Id).Bind(id => Util.Next(Util.OperatorIs(":").Return($"{id}:"))),
                ScopeBlock,
                SecurityDecl,
                SEHBlock

            )
        );

        // Partition III
        public static Compiler<Expr, string> Instr = new Compiler<Expr, string>("Instr", () => 
            Compiler<Expr, string>.Or(
                Util.SymbolIn("nop")
            )
            // TODO: Instr Continue here
        );

        // 205
        public static Compiler<Expr, string> ScopeBlock = new Compiler<Expr, string>("ScopeBlock", () => 
            MethodBodyItem.Many().Select(items => 
                $"{{\n{string.Join("\n", items)}\n}}"
            )
        );

        // 224
        public static Compiler<Expr, string> SecurityDecl = new Compiler<Expr, string>("SecurityBlock", () => 
            Compiler<Expr, string>.Or(
                Util.Next(".permissionset").Apply(
                Util.Next(SecAction).Bind(secAction => 
                Util.Next(Util.OperatorIs("=")).Apply(
                Util.Next(Bytes).Select(bytes => 

                $".permissionset {secAction} = ({bytes})"

                )))),

                Util.Next(".permission").Apply(
                Util.Next(SecAction).Bind(secAction => 
                Util.Next(TypeReference).Bind(typeReference =>
                Util.Next(NameValPairs).Select(nvps =>

                $".permission {secAction} {typeReference} ({nvps})"

                ))))
            )
        );

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
            MethodAttr.Many().Bind(attrs => 
            Util.NextOptional(CallConv).Bind(callConv => 
            Util.Next(Type).Bind(@type =>
            Util.NextOptional(Marshal).Bind(marshal => 
            Util.Next(".ctor").Bind(methodName =>
            GenPars.Bind(genPars => 
            Util.Next(Parameters).Bind(parameters => 
            ImplAttr.Many().Select(implAttrs => 

            $"{string.Join(" ", attrs)} {callConv} {@type} {marshal} {methodName}{genPars}({parameters}) {string.Join(" ", implAttrs)}"

            )))))))));

        // 198
        public static Compiler<Expr, string> MethodHeader = new Compiler<Expr, string>("MethodHeader", () => 
            MethodAttr.Many().Bind(attrs => 
            Util.NextOptional(CallConv).Bind(callConv => 
            Util.Next(Type).Bind(@type =>
            Util.NextOptional(Marshal).Bind(marshal => 
            Util.Next(MethodName).Bind(methodName =>
            GenPars.Bind(genPars => 
            Util.Next(Parameters).Bind(parameters => 
            ImplAttr.Many().Select(implAttrs => 

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
            PinvAttr.Many().Select(pinvAttrs => 
            
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
