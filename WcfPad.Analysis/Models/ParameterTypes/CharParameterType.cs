using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class CharParameterType : ParameterType
    {
        public CharParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "\"c\""; 
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            var value = jsonValue == null ? null : (string)jsonValue.ToString();
            return string.IsNullOrEmpty(value) ? default(char) : value[0];
        }
    }
}
