using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WcfPad.Analysis.Helpers;
using WcfPad.Analysis.Models;

namespace WcfPad.Analysis
{
    public interface IEndpointCache
    {
        void Clear();
        void Add(Endpoint endpoint);
        void Remove(string id);
        Endpoint Retrieve(string id);
        IList<Endpoint> RetrieveAll();
        bool Rename(string id, string newName);
        IList<EndpointDefinition> LoadDefinitionsFromSettings();
        void DeleteEndpointFolders(IList<string> except);
    }

    public class EndpointCache : IEndpointCache
    {
        private static IDictionary<string, Endpoint> _endpointsById;

        private readonly IPathHelper _pathHelper;

        public EndpointCache(IPathHelper pathHelper)
        {
            _pathHelper = pathHelper;

            if (_endpointsById == null)
            {
                _endpointsById = new Dictionary<string, Endpoint>();
            }
        }

        public void Clear()
        {
            _endpointsById = new Dictionary<string, Endpoint>();
        }

        public void Add(Endpoint endpoint)
        {
            _endpointsById[endpoint.Id] = endpoint;
            WriteEndpointDefinitionsToSettings();
        }

        public void Remove(string id)
        {
            if (_endpointsById.ContainsKey(id))
            {
                var endpoint = _endpointsById[id];

                // TODO do we need to unload the assembly when removing??? might be hard since several endpoints use the same assembley
                // individual assemblies can't be unloaded, only the app domains that contain them
                // AppDomain.Unload(endpoint.ClientDomain);
                _endpointsById.Remove(id);
                WriteEndpointDefinitionsToSettings();
            }
        }

        public Endpoint Retrieve(string id)
        {
            if (_endpointsById.ContainsKey(id))
            {
                return _endpointsById[id];
            }

            return null;
        }

        public IList<Endpoint> RetrieveAll()
        {
            return _endpointsById.Values.ToList();
        }

        public bool Rename(string id, string newName)
        {
            var endpoint = Retrieve(id);
            if (endpoint == null)
            {
                return false;
            }

            endpoint.Definition.DisplayName = newName;

            WriteEndpointDefinitionsToSettings();

            return true;
        }

        public IList<EndpointDefinition> LoadDefinitionsFromSettings()
        {
            var settingsFilePath = GetSettingsFilePath();
            if (!File.Exists(settingsFilePath))
            {
                return new List<EndpointDefinition>();
            }

            var settingsJson = File.ReadAllText(settingsFilePath);
            var settings = JsonConvert.DeserializeObject<WcfPadSettings>(settingsJson);

            return settings.EndpointDefinitions ?? new List<EndpointDefinition>();
        }

        public void DeleteEndpointFolders(IList<string> except)
        {
            string format(string path)
            {
                return path.ToLower().Trim().TrimEnd('/', '\\');
            }

            var formatted = except.Select(format).ToList();
            var appDataDirectory = _pathHelper.GetWcfPadAppDataDirectory();
            var subfolders = Directory.EnumerateDirectories(appDataDirectory).ToList();
            var toDelete = subfolders.Where(x => !formatted.Contains(format(x)));

            foreach (var folder in toDelete)
            {
                Directory.Delete(folder, true);
            }
        }

        private void WriteEndpointDefinitionsToSettings()
        {
            var settings = new WcfPadSettings
            {
                EndpointDefinitions = _endpointsById.Select(x => x.Value.Definition).ToList()
            };

            var settingsFilePath = GetSettingsFilePath();
            var settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(settingsFilePath, settingsJson);
        }

        private string GetSettingsFilePath()
        {
            var appDataDirectory = _pathHelper.GetWcfPadAppDataDirectory();
            var settingsFilePath = Path.Combine(appDataDirectory, "wcfpad-settings.json");
            return settingsFilePath;
        }

        private class WcfPadSettings
        {
            public IList<EndpointDefinition> EndpointDefinitions { get; set; }
        }
    }
}
