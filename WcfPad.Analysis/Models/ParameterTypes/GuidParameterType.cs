using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class GuidParameterType : ParameterType
    {
        public GuidParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "\"00000000-0000-0000-0000-000000000000\"";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            var value = jsonValue == null ? null : (string)jsonValue.ToString();
            return new Guid(value);
        }
    }
}
