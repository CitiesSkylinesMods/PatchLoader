using System;
using System.IO;
using System.Reflection;
using Utils;
using Logger = Utils.Logger;

namespace PatchLoader
{
    public class EntryPoint {
        private static Paths _paths;
        private static Logger _logger;

        /// <summary>
        ///  Entry point called from Doorstop
        /// </summary>
        /// <param name="args">First argument is the path of the currently executing process.</param>
        public static void Main(string[] args) {
            _paths = Paths.Create();
            _logger = new Logger(PathExtensions.Combine(_paths.LogsPath, "PatchLoader.log"));
            _paths.WorkshopModsPath = GetWorkshopModsPath(_paths.WorkingPath, _logger);
            
            _logger._Debug(_paths.ToString());

            try {
                AppDomain.CurrentDomain.TypeResolve += AssemblyResolver;
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;

                new InternalLoader(_logger, _paths)
                    .Run();
            } catch (Exception e) {
                _logger.Exception(e);
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolver;
                AppDomain.CurrentDomain.TypeResolve -= AssemblyResolver;
            }
        }

        private static string GetWorkshopModsPath(string workingPath, Logger logger)
        {
            var configFilePath = Path.Combine(workingPath, "PatchLoader.Config.xml");
            if (File.Exists(configFilePath))
            {
                return new ConfigManager<Config>(configFilePath, logger).Load().WorkshopPath;
            }
            else
            {
                logger.Info($"File '{configFilePath}' does not exist.");
                return null;
            }
        }

        /// <summary>
        /// Resolver callback, provide assemblies used in this patcher.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly AssemblyResolver(object sender, ResolveEventArgs args) {
            AssemblyName name = new AssemblyName(args.Name);
            _logger._Debug("Resolving global assembly " + args.Name);
            try {
                return Assembly.LoadFile(Path.Combine(_paths.WorkingPath, $"{name.Name}.dll"));
            } catch (Exception) {
                return null;
            }
        }
    }
}