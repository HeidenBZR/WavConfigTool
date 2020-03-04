using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool
{
    class EntryPoint
    {
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            if (args.Length > 0 && System.IO.File.Exists(args[0]))
            {
                Classes.Settings.ProjectFile = args[0];
            }
#if !DEBUG
            try
            {
#endif
            WavConfigTool.App app = new WavConfigTool.App();
                app.InitializeComponent();
                app.Run();
#if !DEBUG
            }
            catch (Exception ex)
            {
                Classes.ExceptionCatcher.Current.Catch(ex);
            }
#endif
        }

    }
}
