using Dapper.SqlWriter.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public abstract class DBObject
    {

    }

    public abstract class DBObject<T> where T : DBObject<T>
    {
        internal string _oldValues = string.Empty;

        [NotMapped]
        internal ObjectState ObjectState { get; set; } = ObjectState.New;

        public abstract Task Save(SqlWriter sqlWriter);

        //public virtual async Task Save(SqlWriter sqlWriter)
        //{
        //    var obj = (T) this;

        //    ObjectState objectState = sqlWriter.GetObjectState(obj);

        //    if (objectState == ObjectState.New)
        //    {
        //        var result = await sqlWriter.Insert(this).QueryFirstAsync();

        //        RegisteredClass<T> registeredClass = (RegisteredClass<T>) sqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

        //        foreach(RegisteredProperty<T> registeredProperty in registeredClass.RegisteredProperties.Where(p => p.IsAutoIncrement))
        //        {
        //            registeredProperty.Property.SetValue(obj, registeredProperty.Property.GetValue(result, null));
        //        }
        //    }
        //    else if (objectState == ObjectState.Dirty)
        //    {
        //        await sqlWriter.Update(obj).QueryFirstAsync();
        //    }
        //}
    }
}
