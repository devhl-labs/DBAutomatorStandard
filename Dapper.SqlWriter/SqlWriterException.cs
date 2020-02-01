using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.SqlWriter
{
    [Serializable]
    public class SqlWriterException : Exception
    {
        public SqlWriterException(string message, Exception exception) : base(message, exception)
        {

        }

    }
}
