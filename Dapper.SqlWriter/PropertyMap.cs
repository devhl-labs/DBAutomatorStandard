using System;

namespace Dapper.SqlWriter
{
    public class PropertyMap
    {
        public Type PropertyType;

        public Func<object, object?> ToDatabaseColumn = new Func<object, object?>((a) => a);

        public PropertyMap(Type type, Func<object, object?> toDatabaseColumn)
        {
            PropertyType = type;

            ToDatabaseColumn = toDatabaseColumn;
        }
    }
}
