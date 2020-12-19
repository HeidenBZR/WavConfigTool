using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore;

namespace WavConfigTool.Classes
{
    public class PagerContentBase
    {
        public bool IsEnabled { get; protected set; }
        public bool IsCompleted { get; protected set; }

        public event SimpleHandler PointsChanged = delegate { };

        public virtual string GetViewName()
        {
            return "#NAME#";
        }

        public void FirePointsChanged()
        {
            PointsChanged();
        }
    }
}
