using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class StringParameterType : ParameterType
    {
        public StringParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "\"\"";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            if (jsonValue == null)
            {
                return null;
            }

            return jsonValue.ToString();
        }
    }
}
