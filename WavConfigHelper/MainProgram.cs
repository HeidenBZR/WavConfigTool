using System;
using System.Collections.Generic;
using System.IO;
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

            Bridge = new WavConfigBridge();

            Localization.Culture = System.Globalization.CultureInfo.CurrentCulture;
            commandList = new string[]
            {
                CONSOLE_COMMAND_EXIT,
                CONSOLE_COMMAND_HELP,
                CONSOLE_COMMAND_IMPORT_PROJECT,
                CONSOLE_COMMAND_IMPORT_RECLIST
            };
            commandListHelp = new Dictionary<string, string>
            {
                [CONSOLE_COMMAND_EXIT] = Localization.STR_COMMAND_EXIT_DESC,
                [CONSOLE_COMMAND_HELP] = Localization.STR_COMMAND_HELP_DESC,
                [CONSOLE_COMMAND_IMPORT_PROJECT] = Localization.STR_COMMAND_IMPORT_PROJECT_DESC,
                [CONSOLE_COMMAND_IMPORT_RECLIST] = Localization.STR_COMMAND_IMPORT_RECLIST_DESC
            };
            commandListExamples = new Dictionary<string, string[]>
            {
                [CONSOLE_COMMAND_IMPORT_PROJECT] = new string[] 
                {
                    Localization.STR_COMMAND_IMPORT_PROJECT_EXAMPLE1,
                    Localization.STR_COMMAND_IMPORT_PROJECT_EXAMPLE2
                },
                [CONSOLE_COMMAND_IMPORT_RECLIST] = new string[]
                {
                    Localization.STR_COMMAND_IMPORT_RECLIST_EXAMPLE1,
                    Localization.STR_COMMAND_IMPORT_RECLIST_EXAMPLE2
                }
            };
        }

        #region private

        private WavConfigBridge Bridge;

        private readonly string[] commandList;
        private readonly Dictionary<string, string> commandListHelp;
        private readonly Dictionary<string, string[]> commandListExamples;

        private const string CONSOLE_COMMAND_EXIT = "exit";
        private const string CONSOLE_COMMAND_HELP = "help";
        private const string CONSOLE_COMMAND_IMPORT_PROJECT = "import_project";
        private const string CONSOLE_COMMAND_IMPORT_RECLIST = "import_reclist";

        protected override bool TryGetAction(string command, string[] commandParams)
        {
            switch (command)
            {
                case CONSOLE_COMMAND_EXIT:
                    Finish(true);
                    break;

                case CONSOLE_COMMAND_HELP:
                    Console.WriteLine();
                    foreach (var singleCommand in commandList)
                    {
                        PrintCommandHelp(singleCommand);
                    };
                    break;

                case CONSOLE_COMMAND_IMPORT_PROJECT:
                    TryImportProject(commandParams);
                    break;

                case CONSOLE_COMMAND_IMPORT_RECLIST:
                    TryImportReclist(commandParams);
                    break;

                default:
                    return false;
            }
            return true;
        }

        private void PrintCommandHelp(string command)
        {
            Console.Write("- ");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Write(command);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($": {commandListHelp[command]}");
            if (commandListExamples.ContainsKey(command))
            {
                Console.WriteLine(Localization.STR_EXAMPLE);
                foreach (var example in commandListExamples[command])
                {
                    Console.WriteLine(example);
                }
            }
        }

        private void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.Black;
        }

        private bool CheckArgsCount(string[] commandParams, int count, string command)
        {
            if (commandParams.Length != count)
            {
                PrintError(Localization.STR_COMMAND_ERROR_WRONG_ARGS_COUNT);
                PrintCommandHelp(command);
                return false;
            }
            return true;
        }

        private bool CheckFileExists(string filename, bool mustExist)
        {
            if (File.Exists(filename) != mustExist)
            {
                var error = mustExist ? Localization.STR_COMMAND_ERROR_FILE_NOT_EXISTS : Localization.STR_COMMAND_ERROR_FILE_ALREADY_EXISTS;
                PrintError(string.Format(error, filename));
                return false;
            }
            return true;
        }

        private bool CheckHasReclist(string reclistName, bool mustHave)
        {
            if (Bridge.HasReclist(reclistName) != mustHave)
            {
                var error = mustHave ? Localization.STR_COMMAND_ERROR_RECLIST_NOT_EXISTS : Localization.STR_COMMAND_ERROR_RECLIST_ALREADY_EXISTS;
                PrintError(string.Format(error, reclistName));
                return false;
            }
            return true;
        }

        private void CommandFailed()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            PrintError(Localization.STR_COMMAND_FAILED);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void CommandSuccess()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Localization.STR_COMMAND_SUCCESS);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void TryImportProject(string[] commandParams)
        {
            if (!CheckArgsCount(commandParams, 3, CONSOLE_COMMAND_IMPORT_PROJECT)) 
                return;

            if (!CheckFileExists(commandParams[0], mustExist: true))
                return;

            if (!CheckHasReclist(commandParams[1], mustHave: true))
                return;

            if (!CheckFileExists(commandParams[2], mustExist: false))
                return;

            if (Bridge.ImportProject(commandParams[0], commandParams[1], commandParams[2]))
                CommandSuccess();
            else
                CommandFailed();
        }

        private void TryImportReclist(string[] commandParams)
        {
            if (!CheckArgsCount(commandParams, 2, CONSOLE_COMMAND_IMPORT_RECLIST))
                return;

            if (!CheckFileExists(commandParams[0], mustExist: true))
                return;

            if (!CheckHasReclist(commandParams[1], mustHave: false))
                return;

            if (Bridge.ImportReclist(commandParams[0], commandParams[1]))
                CommandSuccess();
            else
                CommandFailed();
        }

        #endregion
    }
}
