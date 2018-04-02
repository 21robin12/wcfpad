namespace WcfPad.Analysis.Models
{
    public class EndpointDefinition 
    {
        public string EndpointDirectory { get; set; }
        public string ConfigPath { get; set; }
        public string AssemblyPath { get; set; }
        public string ClientPath { get; set; }
        public string Address { get; set; }
        public string GeneratedName { get; set; }
        public string DisplayName { get; set; } 
        public string Contract { get; set; }
    }
}
