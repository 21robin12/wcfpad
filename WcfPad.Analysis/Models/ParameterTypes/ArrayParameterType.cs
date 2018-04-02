using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class ArrayParameterType : ParameterType
    {
        public ArrayParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            IsEnumerable = true;
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            var value = jsonValue == null ? null : jsonValue.ToString();
            if (string.IsNullOrEmpty(value) || value == "(null)")
            {
                return null;
            }

            var arrayItemTypeName = Type.FullName.Substring(0, Type.FullName.Length - 2);
            var arrayItemType = GetAssemblyType(arrayItemTypeName);

            var array = Array.CreateInstance(arrayItemType, jsonValue.Count);

            for (var i = 0; i < jsonValue.Count; i++)
            {
                // TODO what about multi-dimensional arrays?
                var member = Members.FirstOrDefault();
                var arrayItem = member.ParameterType.CreateObjectWithValues(jsonValue[i]);
                array[i] = arrayItem;
            }

            return array;
        }
    }
}
