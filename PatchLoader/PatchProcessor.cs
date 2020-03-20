using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Patch.API;
using PatchLoader.Utils;

namespace PatchLoader {
    public static class PatchProcessor {
        private static readonly Dictionary<string, AssemblyDefinition> Assemblies = new Dictionary<string, AssemblyDefinition>();

        public static void ProcessPatches(IEnumerable<KeyValuePair<string, IPatch>> patches) {
            foreach (KeyValuePair<string,IPatch> keyValuePair in patches) {
                IPatch patch = keyValuePair.Value;
                string assemblyName = patch.PatchTarget.Name;

                if (!Assemblies.TryGetValue(assemblyName, out AssemblyDefinition definition))
                {
                    definition = AssemblyDefinition.ReadAssembly(Path.Combine(Paths.ManagedFolderPath, assemblyName + ".dll"));
                    Assemblies.Add(assemblyName, definition);
                }

                try
                {
                    Log.Info($"Executing patch {assemblyName} of {keyValuePair.Key}");
                    Assemblies[assemblyName] = patch.Execute(definition, new PatcherLog(), Path.GetDirectoryName(keyValuePair.Key));
                }
                catch (Exception e)
                {
                    Log.Exception(e, "Patch caused an exception");
                }
            }

            foreach (KeyValuePair<string, AssemblyDefinition> keyValuePair in Assemblies) {
                Log.Info($"Loading assembly: {keyValuePair.Key}");
                LoadAssemblyAndDispose(keyValuePair.Value);
            }
        }

        /// <summary>
        /// Loads modified assembly to CLR
        /// </summary>
        /// <param name="assemblyDefinition"></param>
        public static void LoadAssemblyAndDispose(AssemblyDefinition assemblyDefinition) {
            using (MemoryStream ms = new MemoryStream()) {
                assemblyDefinition.Write(ms);
                Assembly.Load(ms.ToArray());
            }

            assemblyDefinition.Dispose();
        }
    }

    public class PatcherLog : ILogger {
        public void Info(string message) {
            Log.Info($"[{Prefix}] {message}");
        }

        public void Error(string message) {
            Log.Error($"[{Prefix}] {message}");
        }

        public string Prefix { get; set; }
    }
}