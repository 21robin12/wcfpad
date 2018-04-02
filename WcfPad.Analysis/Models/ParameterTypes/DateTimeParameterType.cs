using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class DateTimeParameterType : ParameterType
    {
        public DateTimeParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "\"2017-01-31 00:00:00\"";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            var value = jsonValue == null ? null : (string)jsonValue.ToString();
            return string.IsNullOrEmpty(value) ? default(DateTime) : new DateTimeConverter().ConvertFrom(value);
        }
    }
}
