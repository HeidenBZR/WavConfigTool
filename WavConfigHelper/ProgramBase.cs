using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WavConfigHelper.Properties;

namespace WavConfigHelper
{
    abstract class ProgramBase
    {
        public void Run()
        {
            UnknownCommandString = Localization.STR_UNKNOWN_COMMAND;
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

        private string UnknownCommandString;
        private bool isRunning;

        private void PrepareAction(string line)
        {
            if (line == null)
            {
                return;
            }
            var splited = GetCommandParams(line);
            if (splited.Length > 0)
            {
                var command = splited[0];
                var commandParams = splited.Length > 1 ? splited.Skip(1).ToArray() : new string[0];
                var gotCommand = TryGetAction(command, commandParams);
                if (!gotCommand)
                {
                    Console.WriteLine(string.Format(UnknownCommandString, command));
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

        private string[] GetCommandParams(string line)
        {
            return Regex.Split(line, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")
                .Select(n => n.Trim('\"'))
                .ToArray();
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
