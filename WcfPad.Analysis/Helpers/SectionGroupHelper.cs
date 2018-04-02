using System.Configuration;
using System.ServiceModel.Configuration;

namespace WcfPad.Analysis.Helpers
{
    public interface IConfigHelper
    {
        Configuration GetConfiguration(string configurationFilePath);
        ServiceModelSectionGroup GetSectionGroup(Configuration configuration);
        ServiceModelSectionGroup GetSectionGroup(string configurationFilePath);
    }

    public class ConfigHelper : IConfigHelper
    {
        public Configuration GetConfiguration(string configurationFilePath)
        {
            var fileMap = new ExeConfigurationFileMap();
            var filePath = ConfigurationManager.OpenMachineConfiguration().FilePath;
            fileMap.MachineConfigFilename = filePath;
            fileMap.ExeConfigFilename = configurationFilePath;
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            return configuration;
        }

        public ServiceModelSectionGroup GetSectionGroup(Configuration configuration)
        {
            var sectionGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
            return sectionGroup;
        }

        public ServiceModelSectionGroup GetSectionGroup(string configurationFilePath)
        {
            var configuration = GetConfiguration(configurationFilePath);
            var sectionGroup = GetSectionGroup(configuration);
            return sectionGroup;
        }
    }
}
