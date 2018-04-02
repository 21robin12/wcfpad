using System;
using System.Reflection;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class NullableParameterType : ParameterType
    {
        public NullableParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            return Members[0].ParameterType.CreateObjectWithValues(jsonValue["Value"]);
        }
    }
}
