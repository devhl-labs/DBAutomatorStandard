using Dapper.SqlWriter.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public abstract class DBObject
    {
        internal string _oldValues = string.Empty;

        [NotMapped]
        public ObjectState ObjectState { get; internal set; } = ObjectState.New;

        public virtual async Task Save<T>(SqlWriter sqlWriter) where T : DBObject
        {
            object obj = this;

            T tObj = (T) obj;

            T? result = null;

            if (ObjectState == ObjectState.New)
            {
                result = await sqlWriter.Insert(tObj).QueryFirstAsync();
            }
            else if (IsDirty<T>(sqlWriter))
            {
                result = await sqlWriter.Update(tObj).QueryFirstAsync();
            }

            if (result == null) return;

            RegisteredClass<T> registeredClass = (RegisteredClass<T>) sqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            foreach (RegisteredProperty<T> registeredProperty in registeredClass.RegisteredProperties.Where(p => p.IsAutoIncrement))
            {
                registeredProperty.Property.SetValue(tObj, registeredProperty.Property.GetValue(result, null));
            }

            tObj.ObjectState = result.ObjectState;

            tObj._oldValues = result._oldValues;
        }

        private string GetState<T>(SqlWriter sqlWriter) where T : class
        {
            string result = string.Empty;

            object obj = this;

            T tObj = (T) obj;

            RegisteredClass<T> registeredClass = (RegisteredClass<T>) sqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            foreach (var prop in registeredClass.RegisteredProperties.Where(p => !p.NotMapped).OrderBy(p => p.PropertyName))
            {
                result = $"{result}{prop.Property.GetValue(tObj, null)};";
            }

            return result[..^1];
        }

        internal void StoreState<T>(SqlWriter sqlWriter) where T : class => _oldValues = GetState<T>(sqlWriter);

        public bool IsDirty<T>(SqlWriter sqlWriter) where T : class => _oldValues != GetState<T>(sqlWriter) ? true : false;
    }
}
