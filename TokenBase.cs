using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace bfCompiler
{
    public abstract class TokenBase
    {
        private char? _op;

        public char Op
        {
            get { return (char)(_op ?? (_op = GetType().GetCustomAttributes(false).OfType<TokenAttribute>().Single().Op)); }
        }

        private static readonly Dictionary<char, Func<TokenBase>> TokenList;

        public abstract void Emit(ILGenerator il, int amount=1);

        #region Static methods

        static TokenBase()
        {
            TokenList = Assembly.GetExecutingAssembly().GetExportedTypes()
                .Where(
                    t =>
                    t.IsSubclassOf(typeof (TokenBase)) && t.GetCustomAttributes(false).OfType<TokenAttribute>().Any())
                .ToDictionary(
                    t => t.GetCustomAttributes(false).OfType<TokenAttribute>().Single().Op,
                    t => (Func<TokenBase>) (() => (TokenBase) Activator.CreateInstance(t)));
        }

        public static TokenBase FromOp(char op)
        {
            return !TokenList.ContainsKey(op) ? null : TokenList[op]();
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TokenAttribute : Attribute
    {
        public char Op { get; set; }
    }
}
