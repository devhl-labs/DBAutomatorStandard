using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DBAutomatorStandard.Enums;

namespace DBAutomatorStandard
{
    public class OrderByClause <T>
    {
        private readonly DBAutomator _dBAutomator;

        private readonly RegisteredClass _registeredClass;


        private List<(string, OrderBy)> _order = new List<(string, OrderBy)>();

        public OrderByClause(DBAutomator dBAutomator, params (string, OrderBy)[] orderBy)
        {
            _dBAutomator = dBAutomator;

            _registeredClass = _dBAutomator.RegisteredClasses.First(r => r.SomeClass.GetType() == typeof(T));

            foreach(var order in orderBy)
            {
                _order.Add((order.Item1, order.Item2));
            }    
        }

        public void Add((string, OrderBy orderBy) item)
        {
            _order.Add(item);
        }

        public void Asc(string nameOfProperty)
        {
            _order.Add((nameOfProperty, OrderBy.ASC));
        }

        public void Desc(string nameOfProperty)
        {
            _order.Add((nameOfProperty, OrderBy.DESC));
        }

        public string GetOrderByClause()
        {
            string result = "ORDER BY";

            foreach(var item in _order)
            {
                string columnName = _registeredClass.RegisteredProperties.First(p => p.PropertyName == item.Item1).ColumnName;

                result = $"{result} \"{columnName}\" {item.Item2}, ";
            }

            result = result[0..^2];

            return result;
        }
    }


}
