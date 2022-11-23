using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Patch.API;
using Utils;

namespace PatchLoader {
    public class PatchScanner {
        private readonly Logger _logger;

        public PatchScanner(Logger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<KeyValuePair<string, IPatch>> Scan(IEnumerable<string> paths) {
            bool ignoreExcluded = false;
            List<KeyValuePair<string, List<KeyValuePair<ModDirStatus, string>>>> directoriesPerPath = new List<KeyValuePair<string, List<KeyValuePair<ModDirStatus, string>>>>();  
            foreach (string path in paths)
            {
                List<KeyValuePair<ModDirStatus, string>> modDirectories = new List<KeyValuePair<ModDirStatus, string>>();
                directoriesPerPath.Add(new KeyValuePair<string, List<KeyValuePair<ModDirStatus, string>>>(path, modDirectories));
                try {
                    string[] directories = Directory.GetDirectories(path, "*");
                    for (var i = 0; i < directories.Length; i++) {
                        string directoryPath = directories[i];
                        if (!Path.GetFileName(directoryPath).StartsWith("_")) {
                            string[] files = GetFiles(directoryPath, "*.ipatch", SearchOption.AllDirectories);
                            if (files.Length == 0) {
                                // no patch files, continue
                                continue;
                            }

                            bool mayExclude = File.Exists(Path.Combine(directoryPath, ".excluded"));

                            if (!ignoreExcluded && files.Any(f => f.Contains("LoadOrder"))) {
                                ignoreExcluded = true;
                                mayExclude = false;
                            }

                            if (mayExclude) {
                                _logger.Info($"Detected exclude flag in {directoryPath}");
                            }
                            
                            modDirectories.Add(new KeyValuePair<ModDirStatus, string>(mayExclude ? ModDirStatus.Excluded : ModDirStatus.Normal, directoryPath));
                        } else {
                            _logger.Info($"Ignored Inactive mod path: {directoryPath}");
                        }
                    }
                } catch (Exception e) {
                    _logger.Exception(e, "Error");
                }
            }

            if (ignoreExcluded) {
                _logger.Info($"Detected LoadOrder patch. Skipping excluded patches if any...");
            } else {
                _logger.Info($"LoadOrder patch not found. Performing normal patch execution...");
            }

            List<KeyValuePair<string, IPatch>> patches = new List<KeyValuePair<string, IPatch>>();
            for (var i = 0; i < directoriesPerPath.Count; i++) {
                List<string> assemblies = new List<string>();
                string path = directoriesPerPath[i].Key;
                List<KeyValuePair<ModDirStatus, string>> modDirectories = directoriesPerPath[i].Value;
                foreach (KeyValuePair<ModDirStatus, string> directoriesWithStatus in modDirectories) {
                    try {
                        if (ignoreExcluded && directoriesWithStatus.Key == ModDirStatus.Excluded) {
                            _logger.Info($"Excluded mod path: {directoriesWithStatus.Value}");
                            continue;
                        }
                        assemblies.AddRange(GetFiles(directoriesWithStatus.Value, "*.ipatch", SearchOption.AllDirectories));
                    } catch (Exception e) {
                        _logger.Exception(e, "Error");
                    }
                }
                patches.AddRange(ScanInternal(path, assemblies));
            }

            return patches;
        }

        private List<KeyValuePair<string, IPatch>> ScanInternal(string path, List<string> assemblies) {
            List<KeyValuePair<string, IPatch>> patches = new List<KeyValuePair<string, IPatch>>();

            _logger._Debug("Assemblies:\n\t" + string.Join("\n\t", assemblies.ToArray()));
            for (int i = 0; i < assemblies.Count; i++) {
                if (ImplementsIPatch(assemblies[i])) {
                    Assembly ResolveEventHandler(object sender, ResolveEventArgs args) => ModDependenciesResolver(sender, args, Directory.GetParent(assemblies[i]).FullName);
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveEventHandler;
                    try {
                        Assembly assembly;
                        if (File.Exists(assemblies[i] + ".mdb")) {
                            _logger.Info($"Debug symbols found for {assemblies[i]}. Loading assembly with symbols...");
                            assembly = Assembly.Load(File.ReadAllBytes(assemblies[i]), File.ReadAllBytes(assemblies[i] + ".mdb"));
                        } else {
                            assembly = Assembly.Load(File.ReadAllBytes(assemblies[i]));
                        }

                        foreach (var iPatchImpl in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IPatch).IsAssignableFrom(t)))
                        {
                            ConstructorInfo constructor = iPatchImpl.GetConstructor(Type.EmptyTypes);
                            IPatch patch = (IPatch)constructor.Invoke(null);
                            patches.Add(new KeyValuePair<string, IPatch>(assemblies[i], patch));
                        }
                    } catch (Exception e) {
                        _logger.Exception(e, "Could not instantiate class implementing IPatch interface");
                    } finally {
                        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= ResolveEventHandler;
                    }
                }
            }

            _logger.Info($"Scan Results for path [{path}]\n{string.Join("\n", patches.Select(p => $"IPatch Assembly Path: {p.Key} TargetAssembly {p.Value.PatchTarget} Order: {p.Value.PatchOrderAsc}").ToArray())}");
            return patches;
        }
        
        private Assembly ModDependenciesResolver(object sender, ResolveEventArgs args, string path) {
            AssemblyName name = new AssemblyName(args.Name);
            _logger._Debug("Resolving local assembly " + args.Name);
            try {
                return Assembly.Load(File.ReadAllBytes(Path.Combine(path, $"{name.Name}.dll")));
            } catch (Exception) {
                return null;
            }
        }

        private bool ImplementsIPatch(string file) {
            bool hit = false;
            try {
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(file);
                List<TypeDefinition> types = assemblyDefinition.Modules.SelectMany(m => m.Types).ToList();
                StringBuilder sb = new StringBuilder();
                foreach (TypeDefinition typeDefinition in types) {
                    if (typeDefinition.IsClass && !typeDefinition.IsAbstract) {
                        if (typeDefinition.Interfaces.Any(type => type.InterfaceType.FullName.Equals(typeof(IPatch).FullName))) {
                            sb.AppendLine($">>>>>> Hit! Found Type implementing interface: {typeDefinition.FullName} <<<<<<<");
                            hit = true;
                        }
                    }
                }

                sb.AppendLine("====================================================");

                _logger.Info($"IPatch Interface implementations in [{assemblyDefinition.FullName}]:\n{sb}");
            } catch (Exception e) {
                _logger.Exception(e, $"!!! - Exception while scanning file [{file}] - !!!");
                string fileName = Path.GetFileName(file);
                PatchLoaderStatusInfo.Statuses.Add(fileName, new PatchStatus(fileName, file, "Exception while scanning"));
            }

            return hit;
        }
        
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }

        private enum ModDirStatus {
            Normal,
            Excluded
        }
    }
}