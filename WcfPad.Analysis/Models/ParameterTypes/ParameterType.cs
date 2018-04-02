using System;
using System.Collections.Generic;
using System.Reflection;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public abstract class ParameterType
    {
        protected readonly Assembly _clientAssembly;

        public Type Type { get; set; }
        public IList<Parameter> Members { get; private set; }
        public string DefaultEditorValue { get; set; }
        public bool IsEnumerable { get; set; }        

        public ParameterType(Type type, Assembly clientAssembly)
        {
            Type = type;
            _clientAssembly = clientAssembly;
            Members = new List<Parameter>();
        }

        public abstract object CreateObjectWithValues(dynamic jsonValue);

        /// <summary>
        /// Helper to retrieve a type from the client assembly. Sometimes equal to Type property, but not always. 
        /// For example, an array parameter type (e.g. Int32[]) needs to retrieve it's element type (e.g. Int32).
        /// </summary>
        /// <param name="typeName">FullName of the type to retrieve</param>
        /// <returns>Type from client assembly</returns>
        protected Type GetAssemblyType(string typeName)
        {
            var fromClientAssembly = _clientAssembly.GetType(typeName);
            if (fromClientAssembly != null)
            {
                return fromClientAssembly;
            }

            var fromType = Type.GetType(typeName);
            if (fromType != null)
            {
                return fromType;
            }

            // also search referenced assemblies
            foreach (var assembly in _clientAssembly.GetReferencedAssemblies())
            {
                var loaded = Assembly.Load(assembly);
                if (loaded != null)
                {
                    var fromReferencedAssembly = loaded.GetType(typeName);
                    if (fromReferencedAssembly != null)
                    {
                        return fromReferencedAssembly;
                    }
                }
            }

            return null;
        }
    }
}
