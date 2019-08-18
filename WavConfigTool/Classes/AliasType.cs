using System;
using System.Text.RegularExpressions;

namespace WavConfigTool.Classes
{
    public enum AliasType
    {
        undefined,
        V,
        VV,
        CV,
        CmV,
        VCV,
        VCmV,
        RV,
        RCV,
        RCmV,
        RC,
        RCm,
        VR,
        VCR,
        VCmR,
        CR,
        CmR,
        VC,
        VCm,
        CC,
        CmC
    }

    class AliasTypeResolver
    {
        private static AliasTypeResolver current;
        private AliasTypeResolver() { }

        public static AliasTypeResolver Current
        {
            get
            {
                if (current == null)
                {
                    current = new AliasTypeResolver();
                }
                return current;
            }
        }

        public AliasType GetAliasType(string aliasTypeString)
        {
            var aliasTypeStringFormed = Regex.Replace(aliasTypeString, "/CC*/", "Cm");
            if (Enum.TryParse(aliasTypeStringFormed, true, out AliasType aliasType))
            {
                return aliasType;
            }
            return AliasType.undefined;
        }
    }
}
