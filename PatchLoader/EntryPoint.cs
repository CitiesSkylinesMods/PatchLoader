using System;
using System.IO;
using System.Reflection;
using PatchLoader.Utils;

namespace PatchLoader {
    public class EntryPoint {

        /// <summary>
        ///  Entry point called from Doorstop
        /// </summary>
        /// <param name="args">First argument is the path of the currently executing process.</param>
        public static void Main(string[] args) {
            Paths.LoadVars();
            Paths.LogPaths();
            try {
                AppDomain.CurrentDomain.TypeResolve += AssemblyResolver;
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;

                typeof(EntryPoint).Assembly.GetType($"PatchLoader.{nameof(InternalLoader)}")
                    ?.GetMethod(nameof(InternalLoader.Main))
                    ?.Invoke(null, new object[] {args});
            } catch (Exception e) {
                Log.Exception(e);
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolver;
                AppDomain.CurrentDomain.TypeResolve -= AssemblyResolver;
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
            Log._Debug("Resolving global assembly " + args.Name);
            try {
                return Assembly.LoadFile(Path.Combine(Paths.WorkingPath, $"{name.Name}.dll"));
            } catch (Exception) {
                return null;
            }
        }
    }
}