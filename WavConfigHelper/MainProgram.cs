using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WavConfigHelper.Properties;

namespace WavConfigHelper
{
    class MainProgram : ProgramBase
    {
        public MainProgram()
        {
            Prefix = "WavConfigTool";
            HelloString = Localization.STR_COMMAND_HELLO;
            ByeString = Localization.STR_COMMAND_BYE;

            Localization.Culture = System.Globalization.CultureInfo.CurrentCulture;
            commandList = new string[]
            {
                CONSOLE_COMMAND_EXIT,
                CONSOLE_COMMAND_HELP
            };
            commandListHelp = new Dictionary<string, string>
            {
                [CONSOLE_COMMAND_EXIT] = Localization.STR_COMMAND_EXIT_DESC,
                [CONSOLE_COMMAND_HELP] = Localization.STR_COMMAND_HELP_DESC
            };
        }

        #region private

        private readonly string[] commandList;
        private readonly Dictionary<string, string> commandListHelp;

        private const string CONSOLE_COMMAND_EXIT = "exit";
        private const string CONSOLE_COMMAND_HELP = "help";

        protected override bool TryGetAction(string command, string[] commandParams)
        {
            switch (command)
            {
                case CONSOLE_COMMAND_EXIT:
                    Finish(true);
                    return true;

                case CONSOLE_COMMAND_HELP:
                    foreach (var singleCommand in commandList)
                    {
                        Console.WriteLine($"{singleCommand}: {commandListHelp[singleCommand]}");
                    };
                    return true;
            }
            return false;
        }

        #endregion
    }
}
