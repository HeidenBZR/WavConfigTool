using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigCore
{
    public class OremoPackGenerator
    {
        public OremoPack Generate(Reclist reclist)
        {
            var oremoPack = new OremoPack();
            var oremoReclist = new StringBuilder();
            var oremoComment = new StringBuilder();
            foreach (var line in reclist.Reclines)
            {
                oremoReclist.AppendLine(line.Name);
                oremoComment.AppendLine(line.Name + "\t" + line.Description);
            }

            oremoPack.Comment = oremoComment.ToString();
            oremoPack.Reclist = oremoReclist.ToString();
            oremoPack.Name = reclist.Name;
            return oremoPack;
        }
    }

    public class OremoPack
    {
        public string Reclist;
        public string Comment;
        public string Name;
    }
}
