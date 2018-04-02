using System.Collections.Generic;

namespace WcfPad.Analysis.Models
{
    public class Method
    {
        public Method()
        {
            Parameters = new List<Parameter>();
            OutParameters = new List<Parameter>();
        }

        public string MethodName { get; set; }
        public IList<Parameter> Parameters { get; set; } // regular method parameters
        public IList<Parameter> OutParameters { get; set; } // e.g. "out int myvar" parameters AND $return if non-void   
    }
}