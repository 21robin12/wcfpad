using System;
using System.Reflection;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class CompoundParameterType : ParameterType
    {
        public CompoundParameterType(Type type, Assembly clientAssembly) 
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
            var result = Activator.CreateInstance(type);

            foreach (var member in Members)
            {
                var memberJsonValue = jsonValue[member.ParameterName];
                var property = type.GetProperty(member.ParameterName);
                if (property != null)
                {
                    var propertyValue = member.ParameterType.CreateObjectWithValues(memberJsonValue);
                    property.SetValue(result, propertyValue);
                }
                else
                {
                    var field = type.GetField(member.ParameterName);
                    if (field == null)
                    {
                        throw new Exception($"Could not set property or field value: no member '{member.ParameterName}' was found on type '{type.FullName}'"); 
                    }
                    else
                    {
                        var fieldValue = member.ParameterType.CreateObjectWithValues(memberJsonValue);
                        field.SetValue(result, fieldValue);
                    }
                }
            }

            return result;
        }
    }
}
