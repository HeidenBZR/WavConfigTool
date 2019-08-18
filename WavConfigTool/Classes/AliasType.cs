using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        private static AliasTypeResolver _current;

        public static AliasTypeResolver GetInstance()
        {
            if (_current == null)
            {
                _current = new AliasTypeResolver();
            }
            return _current;
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
