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
        private readonly CustomAssemblyResolver _resolver;

        public PatchProcessor(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resolver = new CustomAssemblyResolver(_logger);
        }

        public void ProcessPatches(IEnumerable<KeyValuePair<string, IPatch>> patches, Paths paths)
        {
            foreach (KeyValuePair<string, IPatch> keyValuePair in patches)
            {
                IPatch patch = keyValuePair.Value;
                string assemblyName = patch.PatchTarget.Name;

                if (!_assemblies.TryGetValue(assemblyName, out AssemblyDefinition definition))
                {
                    definition = AssemblyDefinition.ReadAssembly(Path.Combine(paths.ManagedFolderPath, assemblyName + ".dll"), new ReaderParameters{AssemblyResolver = _resolver} );
                    _assemblies.Add(assemblyName, definition);
                    _resolver.RegisterAssembly(definition);
                }

                try
                {
                    _logger.Info($"Executing patch {assemblyName} of {keyValuePair.Key}");
                    _assemblies[assemblyName] = patch.Execute(definition, new WithPrefixLogger(_logger, assemblyName), Path.GetDirectoryName(keyValuePair.Key));
                }
                catch (Exception e)
                {
                    _logger.Exception(e, "Patch caused an exception");
                }
            }

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