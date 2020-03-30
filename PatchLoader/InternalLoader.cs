using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Patch.API;
using Utils;

namespace PatchLoader {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class InternalLoader {
        private readonly Paths _paths;
        private readonly Logger _logger;

        public InternalLoader(Logger logger, Paths paths)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        }

        public void Run()
        {
            AppDomain.CurrentDomain.TypeResolve += LocalPatcherAssemblyResolver;
            AppDomain.CurrentDomain.AssemblyResolve += LocalPatcherAssemblyResolver;

            var patches = CollectPatches(_paths);
            var patchProcessor = new PatchProcessor(_logger);
            patchProcessor.ProcessPatches(patches, _paths);

            AppDomain.CurrentDomain.AssemblyResolve -= LocalPatcherAssemblyResolver;
            AppDomain.CurrentDomain.TypeResolve -= LocalPatcherAssemblyResolver;
        }

        private List<KeyValuePair<string, IPatch>> CollectPatches(Paths paths) {
            var patchScanner = new PatchScanner(_logger);

            return paths
                .AllModsFolders()
                .SelectMany(folder => patchScanner.Scan(folder))
                .OrderBy(x => x.Value.PatchOrderAsc)
                .ToList();
        }

        /// <summary>
        /// Resolver callback, provide assemblies from patchers folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly LocalPatcherAssemblyResolver(object sender, ResolveEventArgs args) {
            AssemblyName name = new AssemblyName(args.Name);
            _logger._Debug("Resolving local assembly " + args.Name);
            try {
                return Assembly.LoadFile(Path.Combine(_paths.WorkingPath, $"{name.Name}.dll"));
            } catch (Exception) {
                return null;
            }
        }
    }
}