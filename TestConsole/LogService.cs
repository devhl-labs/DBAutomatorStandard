//using System;
//using System.Threading.Tasks;
//using Dapper.SqlWriter;
//using Dapper.SqlWriter.Interfaces;
//using Dapper.SqlWriter.Models;

//namespace TestConsole
//{
//    public class LogService
//    {
//        public LogService(SqlWriter sqlWriter)
//        {
//            sqlWriter.QueryFailure += 
//        }

//        private static readonly object _logLock = new object();

//        private static void ResetConsoleColor()
//        {
//            Console.BackgroundColor = ConsoleColor.Black;
//            Console.ForegroundColor = ConsoleColor.Gray;
//        }

//        private static void PrintLogTitle(QueryFailure queryFailure)
//        {
//            Console.ForegroundColor = ConsoleColor.White;
//            Console.BackgroundColor = ConsoleColor.Red;
//            Console.Write("[failure] ");           

//            ResetConsoleColor();
//        }


//        public Task QueryExecuted(QueryFailure queryFailure)
//        {
//            lock (_logLock)
//            {
//                PrintLogTitle(queryFailure);

//                Console.Write(queryFailure.Sql);

//                Console.Write(" " + queryFailure.Exception.Message);

//                Console.WriteLine();
//            }

//            return Task.CompletedTask;
//        }
//    }
//}
