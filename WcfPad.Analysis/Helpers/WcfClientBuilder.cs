using System;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Xml;

namespace WcfPad.Analysis.Helpers
{
    /// <summary>
    /// Generates the WCF client from endpoint metadata
    /// </summary>
    public interface IWcfClientBuilder
    {
        string BuildAssembly(string endpointDirectory, string address, string configPath, string clientPath, string dllName);
    }

    public class WcfClientBuilder : IWcfClientBuilder
    {
        private readonly IPathHelper _pathHelper;

        public WcfClientBuilder(IPathHelper pathHelper)
        {
            _pathHelper = pathHelper;
        }

        public string BuildAssembly(string endpointDirectory, string address, string configPath, string clientPath, string dllName)
        {
            GenerateClientClass(endpointDirectory, address, configPath, clientPath);
            var compilerResults = CompileClientClass(endpointDirectory, clientPath, dllName);
            if (compilerResults.Errors.Count > 0)
            {
                // TODO do anything with errors?
                //var errors = compilerResults.Errors;
                return null;
            }

            return compilerResults.PathToAssembly;
        }
        
        private void GenerateClientClass(string endpointDirectory, string address, string configPath, string clientPath)
        {
            using (var process = new Process())
            {
                var defaultConfigPath = Path.Combine(endpointDirectory, "default.config");

                var processStartInfo = new ProcessStartInfo(_pathHelper.GetSvcutilPath())
                {
                    Arguments = $"/targetClientVersion:Version35 /r:\"{typeof(DataSet).Assembly.Location}\" \"{address}\" \"/out:{clientPath}\" \"/config:{defaultConfigPath}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.StartInfo = processStartInfo;
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!File.Exists(clientPath) || !File.Exists(defaultConfigPath))
                {
                    throw new Exception($"svcutil.exe failed to generate client. Output: {output}, Error: {error}");
                }

                File.Copy(defaultConfigPath, configPath, true);
            }
        }

        private CompilerResults CompileClientClass(string endpointDirectory, string clientPath, string dllName)
        {
            return new CSharpCodeProvider().CompileAssemblyFromFile(new CompilerParameters()
            {
                CompilerOptions = "/platform:x86",
                OutputAssembly = Path.Combine(endpointDirectory, dllName),
                ReferencedAssemblies = {
                    "System.dll",
                    typeof (DataSet).Assembly.Location,
                    typeof (DataContractAttribute).Assembly.Location,
                    typeof (OperationDescription).Assembly.Location,
                    typeof (TypedTableBaseExtensions).Assembly.Location,
                    typeof (XmlReader).Assembly.Location,
                },
                GenerateExecutable = false
            }, clientPath);
        }
    }
}
