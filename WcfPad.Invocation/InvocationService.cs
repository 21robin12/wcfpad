using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using WcfPad.Analysis;
using WcfPad.Analysis.Helpers;
using WcfPad.Analysis.Models;

namespace WcfPad.Invocation
{
    public interface IInvocationService
    {
        object InvokeMethod(string endpointId, string methodName, string parametersJson);
    }

    public class InvocationService : IInvocationService
    {
        private readonly IConfigHelper _configHelper;
        private readonly IEndpointCache _endpointCache;

        public InvocationService(IEndpointCache endpointCache, IConfigHelper configHelper)
        {
            _endpointCache = endpointCache;
            _configHelper = configHelper;
        }

        public object InvokeMethod(string endpointId, string methodName, string parametersJson)
        {
            var endpoint = _endpointCache.Retrieve(endpointId);
            if (endpoint == null)
            {
                throw new Exception($"Could not invoke method: no endpoint exists with id {endpointId}");
            }

            var contractType = endpoint.ClientAssembly.GetType(endpoint.Definition.Contract);
            var methodInfo = contractType.GetMethod(methodName);
            var type = endpoint.ClientAssembly.GetType(endpoint.ContractTypeName);

            var serviceMethodInfo = endpoint.Methods.FirstOrDefault(x => x.MethodName == methodName);
            if (serviceMethodInfo == null)
            {
                throw new Exception($"No method '{methodName}' found on endpoint {endpoint.Definition.DisplayName}");
            }

            var builtParameters = BuildParameters(serviceMethodInfo.Parameters, parametersJson, endpoint.ClientAssembly);
            var invokableParameters = methodInfo.GetParameters().Select(parameterInfo =>
            {
                return builtParameters[parameterInfo.Name];
            }).ToArray();

            var ctor = type.GetConstructor(new Type[]
            {
                typeof (Binding),
                typeof (EndpointAddress)
            });

            var sectionGroup = _configHelper.GetSectionGroup(endpoint.Definition.ConfigPath);

            var client = ctor.Invoke(new object[]
            {
                ResolveBinding(endpoint.Definition.ConfigPath, endpoint.Definition.GeneratedName),
                new EndpointAddress(endpoint.Definition.Address)
            });

            async Task<object> invokeAsync()
            {
                return await (dynamic)methodInfo.Invoke(client, invokableParameters);
            }

            var result = methodInfo.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task")
                ? invokeAsync().Result
                : methodInfo.Invoke(client, invokableParameters);

            CloseClient(client);
            return result;
        }

        private IDictionary<string, object> BuildParameters(IList<Parameter> parameters, string parametersJson, Assembly clientAssembly)
        {
            var deserializedParameters = JsonConvert.DeserializeObject<dynamic>(parametersJson);

            var result = new Dictionary<string, object>();
            foreach (var parameter in parameters)
            {
                var jsonValue = deserializedParameters[parameter.ParameterName];
                result[parameter.ParameterName] = parameter.ParameterType.CreateObjectWithValues(jsonValue);
            }

            return result;
        }

        private Binding ResolveBinding(string configurationPath, string name)
        {
            var sectionGroup = _configHelper.GetSectionGroup(configurationPath);

            var bindingCollection = sectionGroup.Bindings.BindingCollections
                .FirstOrDefault(x => x.ConfiguredBindings.Count > 0 && x.ConfiguredBindings[0].Name == name);

            if (bindingCollection == null)
            {
                return null;
            }

            var bindingElement = bindingCollection.ConfiguredBindings[0];
            var binding = (Binding)Activator.CreateInstance(bindingCollection.BindingType);
            binding.Name = bindingElement.Name;
            bindingElement.ApplyConfiguration(binding);

            return binding;
        }

        private void CloseClient(object client)
        {
            var communicationObject = client as ICommunicationObject;

            void onClientClosed(IAsyncResult result)
            {
                try
                {
                    communicationObject.EndClose(result);
                }
                catch (Exception)
                {
                    communicationObject.Abort();
                }
            }

            void beginCloseCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                onClientClosed(result);
            }

            var beginCloseResult = communicationObject.BeginClose(beginCloseCallback, communicationObject);
            if (!beginCloseResult.CompletedSynchronously)
            {
                return;
            }

            onClientClosed(beginCloseResult);
        }
    }
}
