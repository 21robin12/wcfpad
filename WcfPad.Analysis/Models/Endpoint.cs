using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WcfPad.Analysis.Models
{
    public class Endpoint 
    {
        public Endpoint(EndpointDefinition definition, string id)
        {
            Id = id;
            Definition = definition;
            Methods = new List<Method>();
        }

        public string Id { get; set; }
        public EndpointDefinition Definition { get; set; }

        public string ContractTypeName { get; set; }
        public IList<Method> Methods { get; set; }

        [JsonIgnore] // don't want to serialize this when returning to UI
        public AppDomain ClientDomain { get; set; }

        [JsonIgnore] // don't want to serialize this when returning to UI
        public Assembly ClientAssembly { get; set; }
    }
}

