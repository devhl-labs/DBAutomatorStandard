using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DBAutomatorLibrary
{
    public class ConditionModel
    {
        /// <summary>
        /// name of the column
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// value in the column
        /// </summary>
        public object? Value { get; set; }
        private string _operatorName = string.Empty;


        /// <summary>
        /// text that describes a conditional operator
        /// </summary>
        public string OperatorName
        {
            get
            {
                return _operatorName;
            }

            set
            {
                _operatorName = value;
                if (_operatorName == "NotEqual")
                {
                    Operator = "IS NOT";
                }
                else if (_operatorName == "Equal")
                {
                    Operator = "=";
                }
                else if (_operatorName == "GreaterThan")
                {
                    Operator = ">";
                }
                else if (_operatorName == "LessThan")
                {
                    Operator = "<";
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// conditional operator
        /// </summary>
        public string Operator { get; private set; } = string.Empty;


    }
}
