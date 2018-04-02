using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Xml;
using WcfPad.Analysis.Models;

namespace WcfPad.Analysis.Helpers
{
    public interface IEndpointDefinitionFactory
    {
        IList<EndpointDefinition> GetEndpointDefinitions(string address);
    }

    public class EndpointDefinitionFactory : IEndpointDefinitionFactory
    {
        private readonly IPathHelper _pathHelper;
        private readonly IWcfClientBuilder _wcfClientBuilder;
        private readonly IConfigHelper _configHelper;

        public EndpointDefinitionFactory(
            IPathHelper pathHelper,
            IWcfClientBuilder wcfClientBuilder,
            IConfigHelper configHelper)
        {
            _pathHelper = pathHelper;
            _wcfClientBuilder = wcfClientBuilder;
            _configHelper = configHelper;
        }

        public IList<EndpointDefinition> GetEndpointDefinitions(string address)
        {
            var appDataDirectory = _pathHelper.GetWcfPadAppDataDirectory();
            var endpointGuid = Guid.NewGuid().ToString();
            var endpointDirectory = Path.Combine(appDataDirectory, endpointGuid);
            var configPath = Path.Combine(endpointDirectory, $"{endpointGuid}.dll.config");
            var clientPath = Path.Combine(endpointDirectory, $"{endpointGuid}.cs");

            var assemblyPath = _wcfClientBuilder.BuildAssembly(endpointDirectory, address, configPath, clientPath, $"{endpointGuid}.dll");

            var sectionGroup = _configHelper.GetSectionGroup(configPath);
            if (sectionGroup == null)
            {
                return null;
            }

            var endpointDefinitions = new List<EndpointDefinition>();
            foreach (ChannelEndpointElement endpoint in sectionGroup.Client.Endpoints)
            {
                endpointDefinitions.Add(new EndpointDefinition
                {
                    AssemblyPath = assemblyPath,
                    ConfigPath = configPath,
                    EndpointDirectory = endpointDirectory,
                    ClientPath = clientPath,
                    Address = endpoint.Address.AbsoluteUri,
                    GeneratedName = endpoint.Name,
                    DisplayName = endpoint.Name,
                    Contract = endpoint.Contract
                });
            }

            return endpointDefinitions;
        }
    }
}
