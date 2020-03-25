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
            string[] assemblies = new string[0];
            try {
                assemblies = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
            } catch (Exception e) {
                _logger.Exception(e, "Error");
            }

            _logger._Debug("Assemblies:\n" + string.Join("\n", assemblies));
            for (int i = 0; i < assemblies.Length; i++) {
                if (ImplementsIPatch(assemblies[i])) {
                    Assembly ResolveEventHandler(object sender, ResolveEventArgs args) => ModDependenciesResolver(sender, args, Directory.GetParent(assemblies[i]).FullName);
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveEventHandler;
                    try {
                        Assembly assembly = Assembly.Load(File.ReadAllBytes(assemblies[i]));
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

            _logger.Info($"Scan Results for path [{path}]\n {string.Join("\n", patches.Select(p => $"Type Name: {p.GetType().FullName} TargetAssembly: {p.Value.PatchTarget} Order: {p.Value.PatchOrderAsc}").ToArray())}");
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
                    // sb.AppendLine(typeDefinition.FullName);
                    if (typeDefinition.Interfaces.Any(type => type.InterfaceType.FullName.Equals(typeof(IPatch).FullName))) {
                        sb.AppendLine($">>>>>>>>>>>>>>>> Hit! Type implementing interface: {typeDefinition.FullName} <<<<<<<<<<<<<<<<<<<<<<<");
                        hit = true;
                    }
                }
            }

            sb.AppendLine("====================================================");

            _logger.Info("Result Interface search:\n" + sb);
            return hit;
        }
    }
}