using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    // Not just for Decimal - for any floating-point numbers (base 2 and 10)
    public class DecimalParameterType : ParameterType
    {
        public DecimalParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "0.0";
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
