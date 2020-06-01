using Mono.Cecil;
using Utils;

namespace PatchLoader {
    public class CustomAssemblyResolver: DefaultAssemblyResolver {

        private readonly Logger _logger;
        
        public CustomAssemblyResolver(Logger logger) {
            _logger = logger;
        }
        
        public new void RegisterAssembly(AssemblyDefinition assemblyDefinition) {
            _logger.Info($"Registering assembly definition: {assemblyDefinition.FullName}");
            base.RegisterAssembly(assemblyDefinition);
        }
    }
}