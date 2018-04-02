using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class TimeSpanParameterType : ParameterType
    {
        public TimeSpanParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "\"00:00:00\"";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            if (jsonValue == null)
            {
                return null;
            }

            return new TimeSpanConverter().ConvertFrom(jsonValue.ToString());
        }
    }
}
