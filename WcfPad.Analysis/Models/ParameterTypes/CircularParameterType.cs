using System;
using System.Reflection;
using WcfPad.Analysis.Helpers;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class CircularParameterType : ParameterType
    {
        private readonly IParameterTypeFactory _parameterTypeFactory;

        public CircularParameterType(Type type, Assembly clientAssembly, IParameterTypeFactory parameterTypeFactory) : base(type, clientAssembly)
        {
            _parameterTypeFactory = parameterTypeFactory;

            DefaultEditorValue = "null";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            if (jsonValue == null)
            {
                return null;
            }

            // only evaluate circular parameter if a value is provided
            var parameter = _parameterTypeFactory.From(Type, _clientAssembly);
            var objectWithValues = parameter.CreateObjectWithValues(jsonValue);
            return objectWithValues;
        }
    }
}
