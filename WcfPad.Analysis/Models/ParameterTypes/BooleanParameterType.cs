using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WcfPad.Analysis.Models.ParameterTypes
{
    public class BooleanParameterType : ParameterType
    {
        public BooleanParameterType(Type type, Assembly clientAssembly) 
            : base(type, clientAssembly)
        {
            DefaultEditorValue = "false";
        }

        public override object CreateObjectWithValues(dynamic jsonValue)
        {
            if (jsonValue == null)
            {
                return null;
            }

            return bool.Parse((string)jsonValue.ToString());
        }
    }
}
