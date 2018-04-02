using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    // Not just for Int32 - covers any fixed-point numbers with 0 dp
    public class IntegerParameterType : ParameterType
    {
        public IntegerParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "0"; 
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            var value = jsonValue == null ? "0" : (string)jsonValue.ToString();

            var parseMethod = Type.GetMethod("Parse", new Type[]
            {
                typeof (string),
                typeof (IFormatProvider)
            });

            return parseMethod.Invoke(null, new object[]
            {
                value,
                CultureInfo.CurrentUICulture
            });
        }
    }
}
