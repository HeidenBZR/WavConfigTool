using WavConfigCore;

namespace WavConfigTool.Classes.Creators
{
    static class WavMaskCreator
    {

        public static void CreateCvcRusWavMask()
        {
            var V_lines = "a|e|o|u|y|i".Split('|');
            var CV_VC_C_lines = "babab|b'ab'ab'|vavav|v'av'av'|gagag|g'ag'ag'|dadad|d'ad'ad'|zazaz|z'az'az'|kakak|k'ak'ak'|lalal|l'al'al'|mamam|m'am'am'|nanan|n'an'an'|papap|p'ap'ap'|rarar|r'ar'ar'|sasas|s'as'as'|tatat|t'at'at'|fafaf|f'af'af'|hahah|h'ah'ah'|wawaw|w'aw'aw'|jajaj|~a~a~|cacac|4'a4'a4'".Split('|');
            var CV_VC_lines = "babab|bebeb|bobob|bubub|bybyb|b'ab'ab'|b'eb'eb'|b'ob'ob'|b'ub'ub'|b'ib'ib'|byb'ib|b'ibyb'|vavav|vevev|vovov|vuvuv|vyvyv|v'av'av'|v'ev'ev'|v'ov'ov'|v'uv'uv'|v'iv'iv'|vyv'iv|v'ivyv'|gagag|gegeg|gogog|gugug|gygyg|g'ag'ag'|g'eg'eg'|g'og'og'|g'ug'ug'|g'ig'ig'|gyg'ig|g'igyg'|dadad|deded|dodod|dudud|dydyd|d'ad'ad'|d'ed'ed'|d'od'od'|d'ud'ud'|d'id'id'|dyd'id|d'idyd'|zazaz|zezez|zozoz|zuzuz|zyzyz|z'az'az'|z'ez'ez'|z'oz'oz'|z'uz'uz'|z'iz'iz'|zyz'iz|z'izyz'|kakak|kekek|kokok|kukuk|kykyk|k'ak'ak'|k'ek'ek'|k'ok'ok'|k'uk'uk'|k'ik'ik'|kyk'ik|k'ikyk'|lalal|lelel|lolol|lulul|lylyl|l'al'al'|l'el'el'|l'ol'ol'|l'ul'ul'|l'il'il'|lyl'il|l'ilyl'|mamam|memem|momom|mumum|mymym|m'am'am'|m'em'em'|m'om'om'|m'um'um'|m'im'im'|mym'im|m'imym'|nanan|nenen|nonon|nunun|nynyn|n'an'an'|n'en'en'|n'on'on'|n'un'un'|n'in'in'|nyn'in|n'inyn'|papap|pepep|popop|pupup|pypyp|p'ap'ap'|p'ep'ep'|p'op'op'|p'up'up'|p'ip'ip'|pyp'ip|p'ipyp'|rarar|rerer|roror|rurur|ryryr|r'ar'ar'|r'er'er'|r'or'or'|r'ur'ur'|r'ir'ir'|ryr'ir|r'iryr'|sasas|seses|sosos|susus|sysys|s'as'as'|s'es'es'|s'os'os'|s'us'us'|s'is'is'|sys'is|s'isys'|tatat|tetet|totot|tutut|tytyt|t'at'at'|t'et'et'|t'ot'ot'|t'ut'ut'|t'it'it'|tyt'it|t'ityt'|fafaf|fefef|fofof|fufuf|fyfyf|f'af'af'|f'ef'ef'|f'of'of'|f'uf'uf'|f'if'if'|fyf'if|f'ifyf'|hahah|heheh|hohoh|huhuh|hyhyh|h'ah'ah'|h'eh'eh'|h'oh'oh'|h'uh'uh'|h'ih'ih'|hyh'ih|h'ihyh'|wawaw|wewew|wowow|wuwuw|wywyw|w'aw'aw'|w'ew'ew'|w'ow'ow'|w'uw'uw'|w'iw'iw'|wyw'iw|w'iwyw'|jajaj|jejej|jojoj|jujuj|jyjyj|~a~a~|~e~e~|~o~o~|~u~u~|~y~y~|~i~i~|cacac|cecec|cococ|cucuc|cycyc|4'a4'a4'|4'e4'e4'|4'o4'o4'|4'u4'u4'|4'i4'i4'".Split('|');
            var wavMask = new WavMask();
            wavMask.MaxDuplicates = 1;
            var CV_VCgroup = new WavGroup();
            CV_VCgroup.Name = "CV-VC";
            CV_VCgroup.AddAliasTypeMask(AliasType.RCV);
            CV_VCgroup.AddAliasTypeMask(AliasType.CV, new AliasTypeMask(positions: new int[] { 2 }));
            CV_VCgroup.AddAliasTypeMask(AliasType.VC, new AliasTypeMask(positions: new int[] { 1 }));
            CV_VCgroup.AddAliasTypeMask(AliasType.VCR);
            foreach (var line in CV_VC_C_lines)
                CV_VCgroup.AddWav(line);

            var CV_VC_Cgroup = new WavGroup();
            CV_VC_Cgroup.Name = "C";
            CV_VC_Cgroup.AddAliasTypeMask(AliasType.RC);
            CV_VC_Cgroup.AddAliasTypeMask(AliasType.CR);
            foreach (var line in CV_VC_lines)
                CV_VC_Cgroup.AddWav(line);

            var V_group = new WavGroup();
            V_group.Name = "V";
            V_group.AddAliasTypeMask(AliasType.RV);
            V_group.AddAliasTypeMask(AliasType.V, new AliasTypeMask(positions: new int[] { 2 }));
            V_group.AddAliasTypeMask(AliasType.VR);
            V_group.AddAliasTypeMask(AliasType.VC);
            V_group.AddAliasTypeMask(AliasType.CV);
            foreach (var line in V_lines)
                V_group.AddWav(line);

            wavMask.AddGroup(CV_VCgroup);
            wavMask.AddGroup(CV_VC_Cgroup);
            wavMask.AddGroup(V_group);

            WavConfigCore.Reader.WavMaskReader.Current.Write("cvc_rus.mask", wavMask);
        }
    }
}
