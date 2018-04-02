using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using WcfPad.Analysis.Models;
using WcfPad.Analysis.Models.ParameterTypes;

namespace WcfPad.Analysis.Helpers
{
    public interface IParameterTypeFactory
    {
        ParameterType From(Type type, Assembly clientAssembly, IList<string> ancestorStack = null);
    }

    public class ParameterTypeFactory : IParameterTypeFactory
    {
        private IDictionary<string, ParameterType> parameterTypeCache = new Dictionary<string, ParameterType>();

        private List<string> integerTypeNames = new[]
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        }.Select(x => x.FullName).ToList();

        private List<string> decimalTypeNames = new[]
        {
            typeof(float),
            typeof(double),
            typeof(decimal)
        }.Select(x => x.FullName).ToList();

        /// <summary>
        /// We almost always use [DataMember] when marking members of a data contract. However, other attributes are 
        /// sometimes needed: for example if a method MethodName has any "out" or "ref" parameters, C# creates MethodNameRequest 
        /// and MethodNameResponse classes to wrap the in/out parameters. The parameters (now as members on the request
        /// and response objects) are not marked with [DataMember], so a full check for the attributes below is required.
        /// </summary>
        private List<Type> WcfDataMemberAttributes = new List<Type>
        {
            typeof (DataMemberAttribute),
            typeof (MessageBodyMemberAttribute),
            typeof (MessageHeaderArrayAttribute),
            typeof (MessageHeaderAttribute),
            typeof (XmlArrayAttribute),
            typeof (XmlAttributeAttribute),
            typeof (XmlElementAttribute),
            typeof (XmlTextAttribute)
        };

        public ParameterType From(Type type, Assembly clientAssembly, IList<string> ancestorStack = null)
        {
            if (parameterTypeCache.ContainsKey(type.FullName))
            {
                return parameterTypeCache[type.FullName];
            }

            if (type.FullName == typeof(string).FullName)
                return new StringParameterType(type, clientAssembly);
            if (type.FullName == typeof(char).FullName)
                return new CharParameterType(type, clientAssembly);
            if (type.FullName == typeof(bool).FullName)
                return new BooleanParameterType(type, clientAssembly);
            if (integerTypeNames.Contains(type.FullName))
                return new IntegerParameterType(type, clientAssembly);
            if (decimalTypeNames.Contains(type.FullName))
                return new DecimalParameterType(type, clientAssembly);
            if (type.IsEnum)
                return new EnumParameterType(type, clientAssembly);
            if (type.FullName == typeof(Guid).FullName)
                return new GuidParameterType(type, clientAssembly);
            if (type.FullName == typeof(DateTime).FullName)
                return new DateTimeParameterType(type, clientAssembly);
            if (type.FullName == typeof(TimeSpan).FullName)
                return new TimeSpanParameterType(type, clientAssembly);
            if (type.FullName == typeof(Uri).FullName)
                return new UriParameterType(type, clientAssembly);
            if (type.IsArray)
            {
                // array
                var arrayParameterType = new ArrayParameterType(type, clientAssembly);
                var arrayItemType = FromPreventStackOverflow(arrayParameterType.Type.GetElementType(), clientAssembly, type, ancestorStack);
                arrayParameterType.Members.Add(new Parameter("[0]", arrayItemType));
                return arrayParameterType;
            }
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    // nullable
                    var nullableParameterType = new NullableParameterType(type, clientAssembly);
                    var innerType = FromPreventStackOverflow(nullableParameterType.Type.GetGenericArguments()[0], clientAssembly, type, ancestorStack);
                    nullableParameterType.Members.Add(new Parameter("Value", innerType));
                    return nullableParameterType;
                }
                if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    // dictionary
                    var dictionaryParameterType = new DictionaryParameterType(type, clientAssembly);
                    Type[] genericArguments = dictionaryParameterType.Type.GetGenericArguments();
                    var parameterType = FromPreventStackOverflow(typeof(KeyValuePair<,>).MakeGenericType(genericArguments[0], genericArguments[1]), clientAssembly, type, ancestorStack);
                    dictionaryParameterType.Members.Add(new Parameter("[0]", parameterType));
                    return dictionaryParameterType;
                }
                if (genericTypeDefinition == typeof(KeyValuePair<,>))
                {
                    // kvp
                    var keyValuePairParameterType = new KeyValuePairParameterType(type, clientAssembly);
                    var genericArguments = keyValuePairParameterType.Type.GetGenericArguments();
                    var keyType = FromPreventStackOverflow(genericArguments[0], clientAssembly, type, ancestorStack);
                    var valueType = FromPreventStackOverflow(genericArguments[1], clientAssembly, type, ancestorStack);
                    keyValuePairParameterType.Members.Add(new Parameter("Key", keyType));
                    keyValuePairParameterType.Members.Add(new Parameter("Value", valueType));
                    return keyValuePairParameterType;
                }
            }
                
            var compoundParameterType = new CompoundParameterType(type, clientAssembly);
            AddMembers(compoundParameterType, clientAssembly, ancestorStack);

            return compoundParameterType;
        }

        /// <summary>
        /// Ensures that no StackOverflowException occurs when building a parameter type from a type containing a circular reference
        /// </summary>
        private ParameterType FromPreventStackOverflow(Type type, Assembly clientAssembly, Type parentType, IList<string> ancestorStack)
        {
            if (ancestorStack == null)
            {
                ancestorStack = new List<string>();
            }

            ancestorStack.Add(parentType.FullName);
            if (ancestorStack.Contains(type.FullName))
            {
                return new CircularParameterType(type, clientAssembly, this);
            }

            return From(type, clientAssembly, ancestorStack);
        }

        private void AddMembers(CompoundParameterType compoundParameterType, Assembly clientAssembly, IList<string> ancestorStack)
        {
            bool isWcfDataMember(MemberInfo member)
            {
                foreach (var attribute in WcfDataMemberAttributes)
                {
                    if (member.GetCustomAttributes(attribute, true).Any())
                    {
                        return true;
                    }
                }

                return false;
            }

            var properties = compoundParameterType.Type
                .GetProperties()
                .Where(isWcfDataMember)
                .Select(x => new Parameter(x.Name, FromPreventStackOverflow(x.PropertyType, clientAssembly, compoundParameterType.Type, ancestorStack)));

            ((List<Parameter>)compoundParameterType.Members).AddRange(properties);

            var fields = compoundParameterType.Type
                .GetFields()
                .Where(isWcfDataMember)
                .Select(x => new Parameter(x.Name, FromPreventStackOverflow(x.FieldType, clientAssembly, compoundParameterType.Type, ancestorStack)));

            ((List<Parameter>)compoundParameterType.Members).AddRange(fields);
        }
    }
}
