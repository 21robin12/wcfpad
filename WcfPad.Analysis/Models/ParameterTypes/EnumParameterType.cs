using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class EnumParameterType : ParameterType
    {
        public EnumParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            var value = FirstEnumValue(GetAssemblyType(Type.FullName));
            DefaultEditorValue = "\"" + (value == null ? "" : value.ToString()) + "\"";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            var value = jsonValue == null ? null : (string)jsonValue.ToString();
            var enumType = GetAssemblyType(Type.FullName);
            if (string.IsNullOrEmpty(value))
            {
                return FirstEnumValue(enumType);
            }

            return Enum.Parse(enumType, value);
        }

        private object FirstEnumValue(Type enumType)
        {
            var enumValues = Enum.GetValues(enumType);
            if (enumValues == null || enumValues.Length == 0)
            {
                // edge case where enum has no possible values - note that this also breaks WcfTestClient
                return null;
            }

            return enumValues.GetValue(0);
        }
    }
}
