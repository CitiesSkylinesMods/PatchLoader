using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
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
            if (!IsGameVersionSupported()) {
                _logger.Info("******** Game version not supported! Further execution aborted ********");
                PatchLoaderStatusInfo.Statuses.Add("GameVersion Check", new PatchStatus("GameVersionCheck", "", "Game Version Not Supported!"));
                return;
            }
            
            AppDomain.CurrentDomain.TypeResolve += LocalPatcherAssemblyResolver;
            AppDomain.CurrentDomain.AssemblyResolve += LocalPatcherAssemblyResolver;
            
            _logger.Info(">>>> Collecting patches... <<<<");
            var patches = CollectPatches(_paths);
            _logger.Info(">>>> Patch collection finished. <<<<");
            var patchProcessor = new PatchProcessor(_logger);
            _logger.Info(">>>> Processing patches... <<<<");
            patchProcessor.ProcessPatches(patches, _paths);
            _logger.Info(">>>> Processing finished. <<<<");

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
                return Assembly.Load(File.ReadAllBytes(Path.Combine(_paths.WorkingPath, $"{name.Name}.dll")));
            } catch (Exception) {
                return null;
            }
        }

        private bool IsGameVersionSupported() {
            AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(Path.Combine(_paths.ManagedFolderPath, "Assembly-CSharp.dll"));
            TypeDefinition bc = ad.MainModule.Types.FirstOrDefault(t => t.Name.Equals("BuildConfig"));
            FieldDefinition versionNumber = bc.Fields.FirstOrDefault(f => f.Name.Equals("APPLICATION_VERSION"));
            uint value = (uint) versionNumber.Constant;
            ad.Dispose();
            return value >= 188997904U;
        }
    }
}