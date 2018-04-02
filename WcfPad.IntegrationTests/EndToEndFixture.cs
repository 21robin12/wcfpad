using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WcfPad.Analysis;
using WcfPad.Invocation;
using WcfPad.DummyWcfService;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using WcfPad.Analysis.Helpers;
using WcfPad.Analysis.Models;

namespace WcfPad.IntegrationTests
{
    [TestClass]
    public class EndToEndFixture
    {
        [TestMethod]
        public void WcfPad_GetRequestObject_EndToEnd()
        {
            Dictionary<int, string> getDictionary(int items)
            {
                return Enumerable.Range(0, items).ToDictionary(x => x, x => "String number: " + x.ToString());
            }

            IList<RequestItem> getItems(int count)
            {
                return Enumerable.Range(1, count).Select(x => new RequestItem
                {
                    Enum = x % 2 == 0 ? DummyEnum.Second : DummyEnum.First,
                    Int = x,
                    String = "your number: " + x.ToString(),
                    Double = x + 0.5,
                    Bool = x % 2 == 0,
                    DateTime = new DateTime(1953, 3, 16).AddDays(x),
                    Dictionary = getDictionary(x),
                    KeyValuePair = new KeyValuePair<double, DateTime>(x + 0.67, new DateTime(2000, 1, 1).AddDays(x)),
                    Guid = Guid.NewGuid(),
                    TimeSpan = new TimeSpan(9, 0, Math.Min(x, 30))
                }).ToList();
            }

            var parameters = new GetRequestObjectParameters
            {
                Request = new Request
                {
                    Enum = DummyEnum.Second,
                    Int = 123,
                    String = "hello world!",
                    Double = 123.321,
                    Bool = true,
                    DateTime = new DateTime(1953, 3, 16),
                    ItemsEnumerable = getItems(3),
                    ItemsList = getItems(6).ToList(),
                    ItemsArray = getItems(9).ToArray(),
                    Dictionary = getDictionary(5),
                    KeyValuePair = new KeyValuePair<double, DateTime>(8245.235235, new DateTime(2000, 1, 1)),
                    Guid = Guid.NewGuid(),
                    TimeSpan = new TimeSpan(13, 27, 45),
                    Item = new RequestItem
                    {
                        Enum = DummyEnum.First,
                        Int = 1337,
                        String = "item here",
                        Double = 1337.7331,
                        Bool = false,
                        DateTime = new DateTime(1953, 3, 16).AddDays(1337),
                        Dictionary = getDictionary(3),
                        KeyValuePair = new KeyValuePair<double, DateTime>(0.56, new DateTime(1990, 6, 21)),
                        Guid = Guid.NewGuid(),
                        TimeSpan = new TimeSpan(0, 0, 1),
                    }
                }
            };

            var response = InvokeMethod(parameters, nameof(DummyService.GetRequestObject));

            void assertWcfPropertiesEqual(IWcfProperties requestProperties, dynamic responseProperties)
            {
                Assert.IsNotNull(responseProperties);
                Assert.AreEqual(requestProperties.String, responseProperties.String);
                Assert.AreEqual(requestProperties.Bool, responseProperties.Bool);
                Assert.AreEqual(requestProperties.DateTime, responseProperties.DateTime);
                Assert.AreEqual(requestProperties.Double, responseProperties.Double);
                Assert.AreEqual((int)requestProperties.Enum, (int)responseProperties.Enum);
                Assert.AreEqual(requestProperties.Int, responseProperties.Int);
                Assert.AreEqual(requestProperties.KeyValuePair.Key, responseProperties.KeyValuePair.Key);
                Assert.AreEqual(requestProperties.KeyValuePair.Value, responseProperties.KeyValuePair.Value);
                Assert.AreEqual(requestProperties.Guid.ToString(), responseProperties.Guid.ToString());
                Assert.AreEqual(requestProperties.TimeSpan, responseProperties.TimeSpan);

                Assert.IsNotNull(responseProperties.Dictionary);
                Assert.AreEqual(requestProperties.Dictionary.Count, responseProperties.Dictionary.Count);

                foreach (var kvp in requestProperties.Dictionary)
                {
                    Assert.AreEqual(kvp.Value, responseProperties.Dictionary[kvp.Key]);
                }
            }

            void assertEnumerablesEqual(IEnumerable<RequestItem> requestEnumerable, dynamic responseEnumerable)
            {
                var requestList = new List<RequestItem>(requestEnumerable);
                var responseList = new List<dynamic>(responseEnumerable);

                Assert.IsNotNull(responseList);
                Assert.AreEqual(requestList.Count, responseList.Count);

                for(var i = 0; i < requestList.Count; i++)
                {
                    assertWcfPropertiesEqual(requestList[i], responseList[i]);
                }
            }

            assertWcfPropertiesEqual(parameters.Request, response);
            assertWcfPropertiesEqual(parameters.Request.Item, response.Item);
            assertEnumerablesEqual(parameters.Request.ItemsEnumerable, response.ItemsEnumerable);
            assertEnumerablesEqual(parameters.Request.ItemsArray, response.ItemsArray);
            assertEnumerablesEqual(parameters.Request.ItemsList, response.ItemsList);
        }

        [TestMethod]
        public void WcfPad_GetNumberFromParams_EndToEnd()
        {
            var parameters = new GetNumberFromParamsRequest
            {
                Inputs = new[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34 }
            };

            var response = InvokeMethod(parameters, nameof(DummyService.GetNumberFromParams));

            Assert.AreEqual(88, response);
        }

        [TestMethod]
        public void WcfPad_GetStringOutRefParameters_EndToEnd()
        {
            // if a method MethodName has any "out" or "ref" parameters, C# creates MethodNameRequest and MethodNameResponse classes to wrap the in/out parameters
            var parameters = new GetStringOutRefParametersParameters
            {
                Request = new GetStringOutRefParametersRequest
                {
                    B = false,
                    AnotherParameter = "This is AnotherParameter"
                }
            };

            var response = InvokeMethod(parameters, nameof(DummyService.GetStringOutRefParameters));

            Assert.AreEqual("Success! You sent b=True and anotherParameter=\"This is AnotherParameter\". i was set to 1337 and b was inverted.", response.GetStringOutRefParametersResult);
            Assert.AreEqual(true, response.b);
            Assert.AreEqual(1337, response.i);
        }

        private dynamic InvokeMethod(object parameters, string methodName)
        {
            var parameterTypeFactory = new ParameterTypeFactory();
            var endpointFactory = new EndpointFactory(parameterTypeFactory);
            var pathHelper = new PathHelper();
            var configHelper = new ConfigHelper();
            var wcfClientBuilder = new WcfClientBuilder(pathHelper);
            var endpointCache = new EndpointCache(pathHelper);
            var endpointDefinitionFactory = new EndpointDefinitionFactory(pathHelper, wcfClientBuilder, configHelper);
            var analysisService = new AnalysisService(endpointFactory, endpointDefinitionFactory, endpointCache);
            var invocationService = new InvocationService(endpointCache, configHelper);

            var port = 3030;
            var address = $"http://localhost:{port}/Services/DummyService.svc";

            IList<Endpoint> endpoints = null;
            try
            {
                endpoints = analysisService.AnalyzeAddress(address);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not analyze service at address {address}. " +
                    "This may be because the service is not running in IIS. To add the service " +
                    "in IIS, add a new website with the following parameters: path=C:\\inetpub\\wwwroot, " +
                    $"binding=http, port={port}, hostname=<blank>. Then right-click this website " +
                    "and select Add Application, using the following parameters: alias=Services, " +
                    "path=<path to WcfPad.DummyWcfService project>. You may also need to give the " +
                    "users IIS_IUSRS and IUSR full access to the WcfPad.DummyWcfService folder. " +
                    $"EXCEPTION MESSAGE: {e.Message}");
            }

            var serializedParameters = JsonConvert.SerializeObject(parameters);
            dynamic response = invocationService.InvokeMethod(endpoints.FirstOrDefault().Id, methodName, serializedParameters);
            return response;
        }

        private class GetRequestObjectParameters
        {
            [JsonProperty(PropertyName = "request")]
            public Request Request { get; set; }
        }

        private class GetNumberFromParamsRequest
        {
            [JsonProperty(PropertyName = "inputs")]
            public int[] Inputs { get; set; }
        }

        private class GetStringOutRefParametersParameters
        {
            [JsonProperty(PropertyName = "request")]
            public GetStringOutRefParametersRequest Request { get; set; }
        }

        private class GetStringOutRefParametersRequest
        {
            [JsonProperty(PropertyName = "b")]
            public bool B { get; set; }

            [JsonProperty(PropertyName = "anotherParameter")]
            public string AnotherParameter { get; set; }
        }
    }
}
