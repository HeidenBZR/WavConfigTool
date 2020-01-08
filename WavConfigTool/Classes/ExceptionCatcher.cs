using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavConfigTool.Classes
{
    class ExceptionCatcher
    {
        private static ExceptionCatcher current;
        private ExceptionCatcher() { }

        public static ExceptionCatcher Current
        {
            get
            {
                if (current == null)
                {
                    current = new ExceptionCatcher();
                }
                return current;
            }
        }

        public void Catch(Exception ex, string message = "")
        {
            var text = $"{message}\n{ex.Message}\n\n{ex.StackTrace}";
            MessageBox.Show(text, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void Greeting()
        {
            MessageBox.Show("Crash catch is enabled", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void End()
        {
            MessageBox.Show("End of program", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

