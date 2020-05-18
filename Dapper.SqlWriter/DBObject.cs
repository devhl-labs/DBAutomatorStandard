using Dapper.SqlWriter.Interfaces;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{
    public abstract class DBObject
    {
        internal string OldValues { get; set; } = string.Empty;

        [NotMapped]
        [JsonProperty]
        public ObjectState ObjectState { get; internal set; } = ObjectState.New;

        [NotMapped]
        [JsonProperty]
        public QueryType? QueryType { get; internal set; }


        public async Task Save<T>(SqlWriter sqlWriter) where T : DBObject
        {
            object obj = this;

            T tObj = (T)obj;

            T? result = null;

            if (ObjectState == ObjectState.New)
            {
                await sqlWriter.Insert(tObj).QueryFirstAsync();

                tObj.QueryType = Dapper.SqlWriter.QueryType.Insert;
            }
            else if (IsDirty<T>(sqlWriter))
            {
                await sqlWriter.Update(tObj).QueryFirstAsync();

                tObj.QueryType = Dapper.SqlWriter.QueryType.Update;
            }

            Statics.FillDatabaseGeneratedProperties(tObj, result, sqlWriter);

            tObj.ObjectState = ObjectState.InDatabase; //  result.ObjectState;

            tObj.StoreState<T>(sqlWriter); // result.OldValues;

            //tObj.QueryType = result.QueryType;
        }

        private string GetState<T>(SqlWriter sqlWriter) where T : class
        {
            string result = string.Empty;

            object obj = this;

            T tObj = (T)obj;

            RegisteredClass<T> registeredClass = (RegisteredClass<T>)sqlWriter.RegisteredClasses.First(r => r is RegisteredClass<T>);

            foreach (var prop in registeredClass.RegisteredProperties.Where(p => !p.NotMapped).OrderBy(p => p.PropertyName))
            {
                result = $"{result}{prop.Property.GetValue(tObj, null)};";
            }

            return result[..^1];
        }

        internal void StoreState<T>(SqlWriter sqlWriter) where T : class => OldValues = GetState<T>(sqlWriter);

        public bool IsDirty<T>(SqlWriter sqlWriter) where T : class => OldValues != GetState<T>(sqlWriter) ? true : false;
    }
}
