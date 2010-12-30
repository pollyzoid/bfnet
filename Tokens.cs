using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace bfCompiler
{
    [Token(Op = '>')]
    public class IdxInc : TokenBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4, amount);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_0);
        }
    }

    [Token(Op = '<')]
    public class IdxDec : TokenBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4, amount);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Stloc_0);
        }
    }

    [Token(Op = '+')]
    public class DataInc : TokenBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldelema, typeof(int));
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldobj, typeof (int));
            il.Emit(OpCodes.Ldc_I4, amount);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stobj, typeof (int));
        }
    }

    [Token(Op = '-')]
    public class DataDec : TokenBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldelema, typeof(int));
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldobj, typeof(int));
            il.Emit(OpCodes.Ldc_I4, amount);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Stobj, typeof(int));
        }
    }

    [Token(Op = '.')]
    public class Spit : TokenBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldelem_I4);
            il.Emit(OpCodes.Conv_U2);

            if (amount > 1)
            {
                il.Emit(OpCodes.Ldc_I4_S, amount);
                il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char), typeof(int) }));
            }

            il.EmitCall(OpCodes.Call,
                        typeof (Console).GetMethod("Write", new[] {amount > 1 ? typeof (string) : typeof (char)}), null);
        }
    }

    [Token(Op = ',')]
    public class Swallow : TokenBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            var cki = il.DeclareLocal(typeof(ConsoleKeyInfo));

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.EmitCall(OpCodes.Call, typeof (Console).GetMethod("ReadKey", new Type[] {}), null);
            il.Emit(OpCodes.Stloc, cki);
            il.Emit(OpCodes.Ldloca, cki);
            il.EmitCall(OpCodes.Call, typeof (ConsoleKeyInfo).GetProperty("KeyChar").GetGetMethod(), null);
            il.Emit(OpCodes.Stelem_I4);
        }
    }

    public abstract class LoopBase : TokenBase
    {
        protected static Stack<Label> Labels = new Stack<Label>();
        protected static Stack<Label> StartWhileLabels = new Stack<Label>();
    }

    [Token(Op = '[')]
    public class StartLoop : LoopBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            for (int i = 0; i < amount; ++i)
            {
                var startLbl = il.DefineLabel();
                StartWhileLabels.Push(startLbl);

                il.Emit(OpCodes.Br, startLbl);

                var lbl = il.DefineLabel();
                il.MarkLabel(lbl);
                Labels.Push(lbl);
            }
        }
    }

    [Token(Op = ']')]
    public class EndLoop : LoopBase
    {
        public override void Emit(ILGenerator il, int amount = 1)
        {
            for (int i = 0; i < amount; ++i)
            {
                il.MarkLabel(StartWhileLabels.Pop());
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldelem_I4);
                var lbl = Labels.Pop();
                il.Emit(OpCodes.Brtrue, lbl);
            }
        }
    }
}
