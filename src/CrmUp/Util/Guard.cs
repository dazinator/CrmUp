using System;

namespace CrmUp.Util
{
    public static class Guard
    {
        public static TOut EnsureIs<TOut, TIn>(TIn instance, string argName) where TOut : class
        {
            var to = instance as TOut;
            if (to == null)
            {
                throw new ArgumentException(argName);
            }
            return to;
        }
    }
}