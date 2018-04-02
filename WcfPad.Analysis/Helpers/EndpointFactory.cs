using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using WcfPad.Analysis.Models;

namespace WcfPad.Analysis.Helpers
{
    public interface IEndpointFactory
    {
        Endpoint LoadEndpointFromDefinition(EndpointDefinition definition);
    }

    public class EndpointFactory : IEndpointFactory
    {
        private readonly IParameterTypeFactory _parameterTypeFactory;

        public EndpointFactory(IParameterTypeFactory parameterTypeFactory)
        {
            _parameterTypeFactory = parameterTypeFactory;
        }

        public Endpoint LoadEndpointFromDefinition(EndpointDefinition definition)
        {
            var clientDomain = CreateAppDomain(definition.ConfigPath, definition.AssemblyPath);
            if (clientDomain == null)
            {
                return null;
            }

            var clientAssembly = Assembly.Load(new AssemblyName()
            {
                CodeBase = definition.AssemblyPath
            });

            var endpoint = new Endpoint(definition, Guid.NewGuid().ToString())
            {
                ClientDomain = clientDomain,
                ClientAssembly = clientAssembly
            };

            var contractType = clientAssembly.GetType(endpoint.Definition.Contract);
            if (contractType == null)
            {
                return endpoint;
            }

            endpoint.ContractTypeName = clientAssembly.GetTypes().FirstOrDefault(x => !x.IsInterface && contractType.IsAssignableFrom(x))?.FullName;
            endpoint.Methods = GetContractMethods(clientAssembly, contractType).ToList();

            return endpoint;
        }

        private IEnumerable<Method> GetContractMethods(Assembly clientAssembly, Type contractType)
        {
            foreach (var methodInfo in contractType.GetMethods())
            {
                var method = new Method
                {
                    MethodName = methodInfo.Name
                };

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    var parameterType = parameterInfo.ParameterType.IsByRef ? parameterInfo.ParameterType.GetElementType() : parameterInfo.ParameterType;
                    var parameter = new Parameter(parameterInfo.Name, _parameterTypeFactory.From(parameterType, clientAssembly));

                    if (parameterInfo.IsIn || !parameterInfo.IsOut)
                    {
                        method.Parameters.Add(parameter);
                    }
                    else
                    {
                        method.OutParameters.Add(parameter);
                    }
                }

                if (methodInfo.ReturnType != null && !methodInfo.ReturnType.Equals(typeof(void)))
                {
                    var parameter = new Parameter("$return", _parameterTypeFactory.From(methodInfo.ReturnParameter.ParameterType, clientAssembly));
                    method.OutParameters.Add(parameter);
                }

                yield return method;
            }
        }

        private AppDomain CreateAppDomain(string configPath, string clientAssemblyPath)
        {
            var appDomain = AppDomain.CreateDomain(configPath, AppDomain.CurrentDomain.Evidence, new AppDomainSetup()
            {
                ConfigurationFile = configPath,
                ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            });

            appDomain.SetData(nameof(clientAssemblyPath), clientAssemblyPath);
            return appDomain;
        }
    }
}
