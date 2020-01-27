using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WavConfigHelper
{
    class ConsoleApplication
    {
        public ConsoleApplication()
        {
            commandList = new string[]
            {
                CONSOLE_COMMAND_EXIT,
                CONSOLE_COMMAND_HELP
            };
            // TODO: move to localization
            commandListHelp = new Dictionary<string, string>
            {
                [CONSOLE_COMMAND_EXIT] = "exit helper",
                [CONSOLE_COMMAND_HELP] = "get command list"
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
            Console.WriteLine("Hello! Welcome to WavConfigHelper. Type 'help' to get commands list, or 'exit' to exit.");
        }

        private void Bye()
        {
            Console.WriteLine();
            Console.WriteLine("Bye-bye!");
            Thread.Sleep(1000);
        }

        #endregion
    }
}
