using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WcfPad.Analysis;
using WcfPad.Analysis.Helpers;
using WcfPad.Analysis.Models;
using WcfPad.Invocation;
using WcfPad.UI.Models;
using WcfPad.UI.Models.Pages;

namespace WcfPad.UI.Controllers
{
    public class MainPageController : Controller
    {
        private readonly IAnalysisService _analysisService;
        private readonly IEndpointFactory _endpointFactory;
        private readonly IInvocationService _invocationService;
        private readonly IEndpointCache _endpointCache;

        public MainPageController(
            IAnalysisService analysisService,
            IEndpointFactory endpointFactory,
            IInvocationService invocationService,
            IEndpointCache endpointCache)
        {
            _analysisService = analysisService;
            _endpointFactory = endpointFactory;
            _invocationService = invocationService;
            _endpointCache = endpointCache;
        }

        public void MainPage()
        {
            var vm = new MainPageViewModel();
            Render(vm, "Views/Main.cshtml");
        }

        public string LoadEndpointsFromSettings()
        {
            try
            {
                _endpointCache.Clear();

                var definitions = _endpointCache.LoadDefinitionsFromSettings();
                _endpointCache.DeleteEndpointFolders(definitions.Select(x => x.EndpointDirectory).ToList());

                var endpoints = new List<Endpoint>();
                foreach (var definition in definitions)
                {
                    var endpoint = _endpointFactory.LoadEndpointFromDefinition(definition);
                    _endpointCache.Add(endpoint);
                    endpoints.Add(endpoint);
                }

                return Json(endpoints);
            }
            catch (Exception e)
            {
                return Json(new Error(e));
            }
        }

        public string AddEndpoints(string address)
        {
            try
            {
                var endpoints = _analysisService.AnalyzeAddress(address);
                return Json(endpoints);
            }
            catch (Exception e)
            {
                return Json(new Error(e));
            }
        }

        public bool RenameEndpoint(string id, string newName)
        {
            var success = _endpointCache.Rename(id, newName);
            return success;
        }

        public void DeleteEndpoint(string id)
        {
            _endpointCache.Remove(id);
        }

        public string RebuildWcfClient(string address)
        {
            try
            {
                var allEndpoints = _endpointCache.RetrieveAll();

                // This should only ever be a single endpoint directory since each address should only correspond to a 
                // single endpoint, but I'll check for multiple endpoints just in case
                var endpointDirectories = allEndpoints
                    .Where(x => x.Definition.Address == address)
                    .Select(x => x.Definition.EndpointDirectory)
                    .Distinct()
                    .ToList();

                if (!endpointDirectories.Any())
                {
                    return Json(new Error($"Couldn't rebuild WCF client: no endpoint found with address {address}"));
                }

                var endpointsToRebuild = allEndpoints
                    .Where(x => endpointDirectories.Contains(x.Definition.EndpointDirectory));

                foreach(var endpointToRebuild in endpointsToRebuild)
                {
                    _endpointCache.Remove(endpointToRebuild.Id);
                }

                return AddEndpoints(address);
            }
            catch (Exception e)
            {
                return Json(new Error(e));
            }
        }

        public string Invoke(string endpointId, string methodName, string parametersJson)
        {
            try
            {
                var result = _invocationService.InvokeMethod(endpointId, methodName, parametersJson);
                return Json(result);
            }
            catch (Exception e)
            {
                return Json(new Error(e));
            }            
        }

        public bool OpenClientFolder(string endpointId)
        {
            var endpoint = _endpointCache.Retrieve(endpointId);
            if (endpoint == null)
            {
                return false;
            }

            Process.Start("explorer.exe", endpoint.Definition.EndpointDirectory);
            return true;
        }

        private void Render<T>(T model, string view)
        {
            CefForm.Instance.Render(model, view);
        }
    }
}
