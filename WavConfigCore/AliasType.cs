using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WavConfigCore
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
        Cm,
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
            var aliasTypeStringFormed = Regex.Replace(aliasTypeString, "CCC*", "Cm");
            if (Enum.TryParse(aliasTypeStringFormed, true, out AliasType aliasType))
            {
                return aliasType;
            }
            return AliasType.undefined;
        }

        public string GetAliasTypeFormat(AliasType aliasType)
        {
            if (aliasType == AliasType.undefined)
                return "";
            return string.Join(" ", aliasType.ToString().ToCharArray().Select(n => $"${n}")).Replace(" $m", "*");
        }

        public bool IsFormatValid(AliasType aliasType, string format)
        {
            var aliasTypeParts = aliasType.ToString().ToCharArray().Select(n => n.ToString()).ToList();
            for (var i = 0; i + 1 < format.Count() && aliasTypeParts.Count > 0; i++)
            {
                if (format[i] == '$')
                {
                    var formatString = format[i + 1].ToString();
                    if (format[i + 1].ToString() == aliasTypeParts[0])
                    {
                        if (i + 2 < format.Count() && format[i + 2] == '*')
                        {
                            if (formatString == "C" && aliasTypeParts.Count > 1 && aliasTypeParts[1] == "m")
                            {
                                aliasTypeParts.RemoveAt(0);
                                aliasTypeParts.RemoveAt(0);
                                i++;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            aliasTypeParts.RemoveAt(0);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
