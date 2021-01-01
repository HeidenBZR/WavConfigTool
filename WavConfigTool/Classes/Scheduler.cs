using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WavConfigTool.Classes
{
    class Scheduler
    {

        public void Schedule(int timeSeconds, Action task)
        {
            DateTime timeToGo = DateTime.Now.AddSeconds(timeSeconds);
            var timer = new Timer(x =>
            {
                task.Invoke();
            }, null, TimeSpan.FromSeconds(timeSeconds), TimeSpan.FromSeconds(timeSeconds));
            timers.Add(timer);
        }

        public void CancellAll()
        {
            foreach (var timer in timers)
            {
                timer.Dispose();
            }
            timers.Clear();
        }

        private List<Timer> timers = new List<Timer>();
    }
}
