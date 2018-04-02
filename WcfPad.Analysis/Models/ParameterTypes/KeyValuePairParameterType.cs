using System;
using System.Linq;
using System.Reflection;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class KeyValuePairParameterType : ParameterType
    {
        public KeyValuePairParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            if (jsonValue == null)
            {
                return null;
            }

            var type = GetAssemblyType(Type.FullName);
            var key = Convert.ChangeType(jsonValue["Key"], Members.First(x => x.ParameterName == "Key").ParameterType.Type);
            var value = Convert.ChangeType(jsonValue["Value"], Members.First(x => x.ParameterName == "Value").ParameterType.Type);
            var result = Activator.CreateInstance(type, key, value);
            return result;
        }
    }
}
