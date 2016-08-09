using System;

namespace opcode4.utilities
{
    public class EnumUtils<TKey> where TKey : struct, IConvertible
    {
        static readonly TKey[] Arr = (TKey[])Enum.GetValues(typeof(TKey));
        public static void ForEach(Action<TKey> act)
        {
            for (var i = 0; i < Arr.Length; i++)
            {
                act(Arr[i]);
            }
        }
    }
}
