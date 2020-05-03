using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WavConfigCore.Tools;
using WavConfigHelper.Properties;
using WavConfigCore.Reader.IO;
using YamlDotNet.Serialization;
using WavConfigCore.Reader;

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
                CONSOLE_COMMAND_IMPORT_RECLIST,
                CONSOLE_COMMAND_CHECK_RECLIST
            };
            commandListHelp = new Dictionary<string, string>
            {
                [CONSOLE_COMMAND_EXIT] = Localization.STR_COMMAND_EXIT_DESC,
                [CONSOLE_COMMAND_HELP] = Localization.STR_COMMAND_HELP_DESC,
                [CONSOLE_COMMAND_IMPORT_PROJECT] = Localization.STR_COMMAND_IMPORT_PROJECT_DESC,
                [CONSOLE_COMMAND_IMPORT_RECLIST] = Localization.STR_COMMAND_IMPORT_RECLIST_DESC,
                [CONSOLE_COMMAND_CHECK_RECLIST] = Localization.STR_COMMAND_CHECK_RECLIST_DESC
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
                },
                [CONSOLE_COMMAND_CHECK_RECLIST] = new string[]
                {
                    Localization.STR_COMMAND_CHECK_RECLIST_EXAMPLE1,
                    Localization.STR_COMMAND_CHECK_RECLIST_EXAMPLE2
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
        private const string CONSOLE_COMMAND_CHECK_RECLIST  = "check_reclist";

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

                case CONSOLE_COMMAND_CHECK_RECLIST:
                    CheckReclist(commandParams);
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
            ResetForeground();
        }

        private void PrintSuccess(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            ResetForeground();
        }

        private void ResetForeground()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void PrintMessage(string text)
        {
            Console.WriteLine(text);
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
            ResetForeground();
        }

        private void CommandSuccess()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Localization.STR_COMMAND_SUCCESS);
            ResetForeground();
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

        private void CheckReclist(string[] commandParams)
        {
            var command = CONSOLE_COMMAND_CHECK_RECLIST;

            if (!CheckArgsCount(commandParams, 1, command))
                return;

            var reclistName = commandParams[0];
            var filename = PathResolver.Current.Reclist(reclistName + PathResolver.RECLIST_EXT);
            PrintMessage("Filename: " + filename);
            if (!CheckFileExists(filename, mustExist: true))
                return;

            IOReclist ioReclist = null;
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new Deserializer();
                try
                {
                    ioReclist = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOReclist)) as IOReclist;
                }
                catch { }
            }

            if (ioReclist == null)
            {
                PrintError(Localization.STR_COMMAND_ERROR_CANT_READ_YAML);
                return;
            }

            var reclist = ReclistReader.Current.Read(reclistName);

            if (reclist == null || !reclist.IsLoaded)
            {
                PrintError(Localization.STR_COMMAND_ERROR_CANT_READ_RECLIST);
                return;
            }

            PrintSuccess(Localization.STR_COMMAND_CHECK_RECLIST_LOADED);

            var replacerFilename = PathResolver.Current.Replacer(reclist.Name);
            PrintMessage("Replacer filename: " + replacerFilename);
            if (!File.Exists(replacerFilename))
            {
                PrintMessage(Localization.STR_COMMAND_CHECK_RECLIST_WTR_IS_MISSING);
            }
            else
            {
                PrintSuccess(Localization.STR_COMMAND_CHECK_RECLIST_WTR_IS_OK);
            }

            var maskFilename = PathResolver.Current.Mask(reclist.Name);
            PrintMessage("Mask filename: " + maskFilename);
            if (!File.Exists(maskFilename))
            {
                PrintMessage(Localization.STR_COMMAND_CHECK_RECLIST_MASK_IS_MISSING);
            }
            else
            {
                var serializer = new Deserializer();
                IOWavMask ioWavMask = null;
                using (var fileStream = new FileStream(maskFilename, FileMode.OpenOrCreate))
                {
                    try
                    {
                        ioWavMask = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOWavMask)) as IOWavMask;
                    }
                    catch { }
                }
                if (ioWavMask == null)
                {
                    PrintError(Localization.STR_COMMAND_CHECK_RECLIST_MASK_CANT_READ_YAML);
                    return;
                }

                PrintSuccess(Localization.STR_COMMAND_CHECK_RECLIST_MASK_IS_OK);
            }

            PrintSuccess(Localization.STR_COMMAND_CHECK_RECLIST_FINISH);
        }

        #endregion
    }
}
