using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.SqlWriter
{
    public enum Comparison
    {
        Equals,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo
    }

    public enum ObjectState
    {
        //Clean,
        //Dirty,
        New,
        Deleted,
        InDatabase
    }

    public enum QueryType
    {
        Select,
        Insert,
        Update,
        Delete
    }

    public enum Parens
    {
        Left,
        Right
    }
}
