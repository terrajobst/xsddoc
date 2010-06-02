using System;

namespace XsdDocumentation
{
    internal static class Casting
    {
        public static bool TryCast<TBase, TDerived>(TBase baseObj, out TDerived derivedObj)
            where TDerived : class, TBase
        {
            return (derivedObj = baseObj as TDerived) != null;
        }
    }
}