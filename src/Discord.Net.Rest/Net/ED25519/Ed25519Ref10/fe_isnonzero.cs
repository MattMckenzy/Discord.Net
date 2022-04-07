using System;

namespace Discord.Net.ED25519.Ed25519Ref10
{
    internal static partial class FieldOperations
    {
        internal static int fe_isnonzero(ref FieldElement f)
        {
            FieldElement fr;
            fe_reduce(out fr, ref f);
            int differentBits = 0;
            differentBits |= fr.x0;
            differentBits |= fr.x1;
            differentBits |= fr.x2;
            differentBits |= fr.x3;
            differentBits |= fr.x4;
            differentBits |= fr.x5;
            differentBits |= fr.x6;
            differentBits |= fr.x7;
            differentBits |= fr.x8;
            differentBits |= fr.x9;
            return (int)((unchecked((uint)differentBits - 1) >> 31) ^ 1);
        }
    }
}
