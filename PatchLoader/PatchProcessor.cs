using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Patch.API;
using Utils;

namespace PatchLoader
{
    public class PatchProcessor
    {
        private readonly Dictionary<string, AssemblyDefinition> _assemblies = new Dictionary<string, AssemblyDefinition>();
        private readonly Logger _logger;

        public PatchProcessor(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ProcessPatches(IEnumerable<KeyValuePair<string, IPatch>> patches, Paths paths)
        {
            foreach (KeyValuePair<string, IPatch> keyValuePair in patches)
            {
                IPatch patch = keyValuePair.Value;
                string assemblyName = patch.PatchTarget?.Name;
                string patchFullName = patch.GetType().FullName;
                PatchStatus status = new PatchStatus(patchFullName, Path.GetDirectoryName(keyValuePair.Key));
                if (string.IsNullOrEmpty(assemblyName)) {
                    try {
                        PatchLoaderStatusInfo.Statuses.Add(status.PatchName, status);
                        _logger.Info($"Executing <NULL> patch of {keyValuePair.Key}\n");
                        patch.Execute(null, new WithPrefixLogger(_logger, "<NULL>"), Path.GetDirectoryName(keyValuePair.Key), paths);
                    } catch (Exception e) {
                        _logger.Exception(e, "Patch caused an exception");
                        status.SetError(e.ToString());
                    }
                } else {

                    if (!_assemblies.TryGetValue(assemblyName, out AssemblyDefinition definition)) {
                        definition = AssemblyDefinition.ReadAssembly(Path.Combine(paths.ManagedFolderPath, assemblyName + ".dll"));
                        _assemblies.Add(assemblyName, definition);
                    }

                    try {
                        PatchLoaderStatusInfo.Statuses.Add(status.PatchName, status);
                        _logger.Info($"Executing patch {assemblyName} of {keyValuePair.Key}\n");
                        _assemblies[assemblyName] = patch.Execute(definition, new WithPrefixLogger(_logger, assemblyName), Path.GetDirectoryName(keyValuePair.Key), paths);
                    } catch (Exception e) {
                        _logger.Exception(e, "Patch caused an exception");
                        status.SetError(e.ToString());
                    }
                }
            }
            
            _logger.Info(">>>>>>>>>> Finished processing patches. Loading patched assemblies... <<<<<<<<<<");

            foreach (KeyValuePair<string, AssemblyDefinition> keyValuePair in _assemblies)
            {
                _logger.Info($"Loading assembly: {keyValuePair.Key}");
                LoadAssemblyAndDispose(keyValuePair.Value);
            }
        }

        /// <summary>
        /// Loads modified assembly to CLR
        /// </summary>
        /// <param name="assemblyDefinition"></param>
        public void LoadAssemblyAndDispose(AssemblyDefinition assemblyDefinition)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                assemblyDefinition.Write(ms);
                Assembly.Load(ms.ToArray());
            }

            assemblyDefinition.Dispose();
        }
    }

    public class WithPrefixLogger : ILogger
    {
        private readonly Logger _logger;

        public WithPrefixLogger(Logger logger, string prefix)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        public string Prefix { get; }

        public void Info(string message)
        {
            _logger.Info($"[{Prefix}] {message}");
        }

        public void Error(string message)
        {
            _logger.Error($"[{Prefix}] {message}");
        }
    }
}