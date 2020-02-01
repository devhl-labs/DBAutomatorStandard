using System;
using System.Threading.Tasks;

using Dapper.SqlWriter.Interfaces;
using Dapper.SqlWriter.Models;

namespace TestConsole
{
    public class LogService : ILogger
    {
        private static readonly object _logLock = new object();

        private static void ResetConsoleColor()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void PrintLogTitle(IQueryResult queryResult)
        {
            switch (queryResult)
            {
                case QuerySuccess _:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("[success] ");
                    break;

                case QueryFailure _:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write("[failure] ");
                    break;
            }

            ResetConsoleColor();
        }


        public Task QueryExecuted(IQueryResult queryResult)
        {
            lock (_logLock)
            {
                PrintLogTitle(queryResult);

                Console.Write(queryResult.Sql);

                if (queryResult is QueryFailure failure) Console.Write(" " + failure.Exception.Message);

                Console.WriteLine();
            }

            return Task.CompletedTask;
        }
    }
}
