using System;
using UnityEngine;
using UnityEngine.Events;

namespace NeroWeNeed.Commons.Editor {



    public abstract class CompileableObject : ScriptableObject {
        public string outputFileName;
        public string outputDirectory;
        [NonSerialized]
        public bool upToDate;
        public CompileOptions options = CompileOptions.All;
        public abstract void Compile(CompileOptions hint = CompileOptions.None, bool forceCompilation = false);

    }
    [Flags]
    public enum CompileOptions : byte {
        [InspectorName("Off")]
        None = 0,
        Build = 1,
        Play = 2,
        [InspectorName("Build, Play")]
        Build_Play = Build | Play,
        ScriptReload = 4,
        [InspectorName("Build, Script Reload")]
        Build_ScriptReload = Build | ScriptReload,
        [InspectorName("Play, Script Reload")]
        Play_ScriptReload = Play | ScriptReload,
        [InspectorName("Build, Play, Script Reload")]
        All = Build | Play | ScriptReload
    }
    public static class CompileOptionsExtensions {
        public static bool ShouldCompile(this CompileOptions options, CompileOptions hint) => (hint == CompileOptions.None) || ((hint & options) != 0);
    }
}