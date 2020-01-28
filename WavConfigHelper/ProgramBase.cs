using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WavConfigHelper
{
    abstract class ProgramBase
    {
        public void Run()
        {
            isRunning = true;
            Hello();

            do
            {
                Console.WriteLine();
                Console.Write($"> {Prefix}: ");
                var command = Console.ReadLine();
                PrepareAction(command);
            }
            while (isRunning);

        }

        #region private

        protected string Prefix;
        protected string HelloString;
        protected string ByeString;

        private bool isRunning;

        private void PrepareAction(string command)
        {
            if (command == null)
            {
                return;
            }
            var commandParams = command.Split(' ');
            if (commandParams.Length > 0)
            {
                var keyWord = commandParams[0];
                var actualParams = commandParams.Length > 1 ? commandParams.Skip(1).ToArray() : new string[0];
                var gotCommand = TryGetAction(keyWord, actualParams);
                if (!gotCommand)
                {
                    Console.WriteLine($"Unknown command '{keyWord}'");
                }
            }
        }

        /// <summary>
        /// return false when command is wrong
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandParams"></param>
        /// <returns></returns>
        protected virtual bool TryGetAction(string command, string[] commandParams)
        {
            return false;
        }

        protected void Finish(bool withDelay = false)
        {
            Bye();
            if (withDelay)
            {
                Thread.Sleep(1000);
            }
            isRunning = false;
        }

        private void Hello()
        {
            if (HelloString != null)
                Console.WriteLine(HelloString);
        }

        private void Bye()
        {
            if (ByeString != null)
            {
                Console.WriteLine();
                Console.WriteLine(ByeString);
            }
        }

        #endregion
    }
}
