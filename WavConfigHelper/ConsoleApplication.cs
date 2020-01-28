using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WavConfigHelper.Properties;

namespace WavConfigHelper
{
    class ConsoleApplication
    {
        public ConsoleApplication()
        {
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

        public void Run()
        {
            isRunning = true;
            Hello();

            do
            {
                Console.WriteLine();
                Console.Write("> WavConfigHelper: ");
                var command = Console.ReadLine();
                Action(command);
            }
            while (isRunning);

            Bye();
        }

        #region private

        private bool isRunning;
        private readonly string[] commandList;
        private readonly Dictionary<string, string> commandListHelp;

        private const string CONSOLE_COMMAND_EXIT = "exit";
        private const string CONSOLE_COMMAND_HELP = "help";

        private void Action(string command)
        {
            if (command == null)
            {
                return;
            }
            var commandParams = command.Split(' ');
            if (commandParams.Length > 0)
            {
                var keyWord = commandParams[0];
                switch (keyWord)
                {
                    case CONSOLE_COMMAND_EXIT:
                        isRunning = false;
                        break;

                    case CONSOLE_COMMAND_HELP:
                        foreach (var singleCommand in commandList)
                        {
                            Console.WriteLine($"{singleCommand}: {commandListHelp[singleCommand]}");
                        };
                        break;

                    default:
                        Console.WriteLine($"Unknown command '{keyWord}'");
                        break;
                }
            }
        }

        private void Hello()
        {
            Console.WriteLine(Localization.STR_COMMAND_HELLO);
        }

        private void Bye()
        {
            Console.WriteLine();
            Console.WriteLine(Localization.STR_COMMAND_BYE);
            Thread.Sleep(1000);
        }

        #endregion
    }
}
