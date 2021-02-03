using Mono.Cecil;

namespace Patch.API
{
    public interface IPatch
    {
        /// <summary>
        /// Order of execution - when more than one patch found, the one with lower number will be processed first
        /// </summary>
        int PatchOrderAsc { get; }

        /// <summary>
        /// Collection of Assemblies to patch. For each target assembly <see cref="Execute"/> will be called once
        /// </summary>
        AssemblyToPatch PatchTarget { get; }

        /// <summary>
        /// Execute patch on AssemblyDefinition
        /// </summary>
        /// <param name="assemblyDefinition">Assembly definition</param>
        /// <param name="logger">Logger wrapper</param>
        /// <param name="patcherWorkingPath">Patcher directory path - Assembly.GetExecutingAssembly().Location return null (because of )</param>
        /// <param name="managedDirectoryPath">Managed directory path - path to directory with all game assemblies(completely different on MacOS)</param>
        /// <returns>Modified AssemblyDefinition, used for further processing</returns>
        AssemblyDefinition Execute(AssemblyDefinition assemblyDefinition, ILogger logger, string patcherWorkingPath, string managedDirectoryPath);
    }
}