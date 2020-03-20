using System;

namespace Patch.API
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AssemblyToPatch
    {
        /// <summary>
        /// Assembly name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Version of Assembly (currently not used)
        /// </summary>
        public Version Version { get; }

        public AssemblyToPatch(string name, Version version)
        {
            Name = name;
            Version = version;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Version: {Version}";
        }
    }
}