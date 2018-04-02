using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using WcfPad.Analysis.Helpers;
using WcfPad.Analysis.Models;
using System.IO;
using System.Reflection;
using System.Data;
using System.ServiceModel.Configuration;
using System.Xml;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Linq;
using System.Diagnostics;

namespace WcfPad.Analysis
{
    /// <summary>
    /// For getting information about a WCF service hosted at a given address, 
    /// and creating a WCF client for calling that service
    /// </summary>
    public interface IAnalysisService
    {
        IList<Endpoint> AnalyzeAddress(string address);
    }

    public class AnalysisService : IAnalysisService
    {
        private readonly IEndpointFactory _endpointFactory;
        private readonly IEndpointDefinitionFactory _endpointDefinitionFactory;
        private readonly IEndpointCache _endpointCache;

        public AnalysisService(
            IEndpointFactory endpointFactory,
            IEndpointDefinitionFactory endpointDefinitionFactory,
            IEndpointCache endpointCache)
        {
            _endpointFactory = endpointFactory;
            _endpointDefinitionFactory = endpointDefinitionFactory;
            _endpointCache = endpointCache;
        }

        public IList<Endpoint> AnalyzeAddress(string address)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out Uri result))
            {
                var definitions = _endpointDefinitionFactory.GetEndpointDefinitions(address);
                if (definitions == null)
                {
                    throw new Exception($"Could not parse service at address {address}");
                }

                // TODO avoid running _endpointFactory.LoadEndpointFromDefinition multiple times for the same address (unless address actually makes a difference?)
                var endpoints = definitions.Select(x => _endpointFactory.LoadEndpointFromDefinition(x)).ToList();
                if (endpoints.Any(x => x == null))
                {
                    throw new Exception($"Could not parse service at address {address}"); 
                }

                foreach(var endpoint in endpoints)
                {
                    _endpointCache.Add(endpoint);
                }

                return endpoints;
            }
            else
            {
                var error = $"{address} is not a valid URL for a WCF Service";
                throw new Exception(error);
            }
        }  
    }
}
