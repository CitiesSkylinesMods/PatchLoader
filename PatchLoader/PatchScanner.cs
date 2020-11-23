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

        public List<KeyValuePair<string, IPatch>> Scan(string path) {
            List<KeyValuePair<string, IPatch>> patches = new List<KeyValuePair<string, IPatch>>();
            List<string> assemblies = new List<string>();
            try {
                string[] directories = Directory.GetDirectories(path, "*");
                for (var i = 0; i < directories.Length; i++) {
                    if (!Path.GetFileName(directories[i]).StartsWith("_")) {
                        assemblies.AddRange(Directory.GetFiles(directories[i], "*.dll", SearchOption.AllDirectories));
                    } else {
                        _logger.Info($"Ignored Inactive mod path {directories[i]}");
                    }
                }
            } catch (Exception e) {
                _logger.Exception(e, "Error");
            }

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
                return Assembly.LoadFile(Path.Combine(path, $"{name.Name}.dll"));
            } catch (Exception) {
                return null;
            }
        }

        private bool ImplementsIPatch(string file) {
            AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(file);
            List<TypeDefinition> types = assemblyDefinition.Modules.SelectMany(m => m.Types).ToList();
            bool hit = false;
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
            return hit;
        }
    }
}