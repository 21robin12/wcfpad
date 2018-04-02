using WcfPad.Analysis.Models.ParameterTypes;

namespace WcfPad.Analysis.Models
{
    public class Parameter
    {
        public Parameter(string parameterName, ParameterType parameterType)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
        }

        public string ParameterName { get; set; }
        public ParameterType ParameterType { get; set; }
    }
}