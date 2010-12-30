using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace bfCompiler
{
    public class BrainfuckApp : IDisposable
    {
        public string Name { get; set; }

        private readonly AssemblyBuilder _asBuilder;
        private readonly ModuleBuilder _modBuilder;
        private readonly TypeBuilder _typeBuilder;
        private readonly MethodBuilder _methBuilder;
        private readonly ILGenerator _il;

        public BrainfuckApp(string name)
        {
            Name = name;
            _asBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName { Name = "Brainfuck app" }, AssemblyBuilderAccess.Save);
            _modBuilder = _asBuilder.DefineDynamicModule("bf", name + ".exe");
            _typeBuilder = _modBuilder.DefineType("Program", TypeAttributes.Public);
            _methBuilder = _typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static);
            _asBuilder.SetEntryPoint(_methBuilder);
            _il = _methBuilder.GetILGenerator();

            _il.DeclareLocal(typeof(int));
            _il.DeclareLocal(typeof(int[]));

            // Initialize idx to 0
            _il.Emit(OpCodes.Ldc_I4_0);
            _il.Emit(OpCodes.Stloc_0);

            // Initialize cell with 30000 fields of int
            _il.Emit(OpCodes.Ldc_I4, 30000);
            _il.Emit(OpCodes.Newarr, typeof(int));
            _il.Emit(OpCodes.Stloc_1);
        }

        public void BuildApp(string data)
        {
            data = new string(data.Where(c => TokenBase.FromOp(c) != null).ToArray());

            // Make a list of tuples, containing the token and
            // amount of repetitions

            var ops = new List<Tuple<char, int>>();

            while (!string.IsNullOrEmpty(data))
            {
                char matchThis = data[0];

                var chars = data.TakeWhile(c => c == matchThis).ToArray();

                data = data.Substring(chars.Length);

                ops.Add(new Tuple<char, int>(matchThis, chars.Length));
            }

            foreach (var pair in from t in ops where TokenBase.FromOp(t.Item1) != null select new { Token = TokenBase.FromOp(t.Item1), Count = t.Item2 })
            {
                pair.Token.Emit(_il, pair.Count);
            }

            _il.Emit(OpCodes.Ret);
        }

        public void Dispose()
        {
            _typeBuilder.CreateType();
            _asBuilder.Save(Name + ".exe");
        }
    }
}
