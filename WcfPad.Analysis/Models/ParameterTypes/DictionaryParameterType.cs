using System;
using System.Reflection;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class DictionaryParameterType : ParameterType
    {
        public DictionaryParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            IsEnumerable = true;
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            if (jsonValue == null)
            {
                return null;
            }

            var type = GetAssemblyType(Type.FullName);
            var result = Activator.CreateInstance(type);
            var addMethod = type.GetMethod("Add");

            foreach (var kvp in jsonValue)
            {
                var kvpType = (KeyValuePairParameterType)Members[0].ParameterType;

                var keyType = kvpType.Members[0].ParameterType;
                var valueType = kvpType.Members[1].ParameterType;

                var keyObject = keyType.CreateObjectWithValues(GetKey(kvp));
                var valueObject = valueType.CreateObjectWithValues(kvp.Value.Value);

                var parameters = new object[]
                {
                    keyObject,
                    valueObject
                };

                addMethod.Invoke(result, parameters);
            }

            return result;
        }

        private dynamic GetKey(dynamic kvp)
        {
            try
            {
                return kvp.Key.Value;
            }
            catch
            {
                return kvp.Name;
            }
        }
    }
}
