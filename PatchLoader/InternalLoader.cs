using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Patch.API;
using PatchLoader.Utils;

namespace PatchLoader {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class InternalLoader {
        public static void Main(string[] args) {
            AppDomain.CurrentDomain.TypeResolve += LocalPatcherAssemblyResolver;
            AppDomain.CurrentDomain.AssemblyResolve += LocalPatcherAssemblyResolver;

            List<KeyValuePair<string, IPatch>> patches = new List<KeyValuePair<string, IPatch>>();
            CollectPatches(patches);
            patches.Sort(SortByPriority);
            PatchProcessor.ProcessPatches(patches);
            
            AppDomain.CurrentDomain.AssemblyResolve -= LocalPatcherAssemblyResolver;
            AppDomain.CurrentDomain.TypeResolve -= LocalPatcherAssemblyResolver;
        }

        private static int SortByPriority(KeyValuePair<string, IPatch> x, KeyValuePair<string, IPatch> y) {
            return x.Value.PatchOrderAsc - y.Value.PatchOrderAsc;
        }

        private static void CollectPatches(List<KeyValuePair<string, IPatch>> patches) {
            patches.AddRange(PatchScanner.Scan(Paths.ModsPath));
            patches.AddRange(PatchScanner.Scan(Paths.AppDataModsPath));
            Config config = Config.Instance;
            if (Directory.Exists(config.WorkshopPath)) {
                patches.AddRange(PatchScanner.Scan(config.WorkshopPath));
            } else {
                Log.Error("Workshop directory not found. Skipping!");
            }
        }

        /// <summary>
        /// Resolver callback, provide assemblies from patchers folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly LocalPatcherAssemblyResolver(object sender, ResolveEventArgs args) {
            AssemblyName name = new AssemblyName(args.Name);
            Log._Debug("Resolving local assembly " + args.Name);
            try {
                return Assembly.LoadFile(Path.Combine(Paths.WorkingPath, $"{name.Name}.dll"));
            } catch (Exception) {
                return null;
            }
        }
    }
}