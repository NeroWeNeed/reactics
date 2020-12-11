using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {

    public class CompileableTableObject : CompileableObject {
        [InitializeOnLoadMethod]
        private static void CompileOnExitEditMode() {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.ExitingEditMode) {
                    foreach (var asset in AssetDatabase.FindAssets($"t:{nameof(CompileableTableObject)}").OfType<CompileableTableObject>()) {
                        asset.Compile(CompileOptions.Play);
                    }
                }
            };
        }
        [DidReloadScripts]
        private static void CompileOnScriptReload() {
            foreach (var asset in AssetDatabase.FindAssets($"t:{nameof(CompileableTableObject)}").OfType<CompileableTableObject>()) {
                asset.Compile(CompileOptions.ScriptReload);
            }
        }

        [SerializeField]
        protected List<Entry> entries = new List<Entry>();
        public List<Entry> Entries => entries;
        public override void Compile(CompileOptions hint = CompileOptions.None, bool forceCompilation = false) {
            if (forceCompilation || (options.ShouldCompile(hint) && !upToDate)) {
                for (int index = this.Entries.Count - 1; index >= 0; index--) {
                    var entry = entries[index];
                    var path = AssetDatabase.GUIDToAssetPath(entry.guid);
                    if (!File.Exists(path)) {
                        this.Entries.RemoveAt(index);
                        continue;
                    }
                    if (entry.options.ShouldCompile(hint)) {
                        var compileableAsset = AssetDatabase.LoadAssetAtPath<CompileableObject>(path);
                        if (compileableAsset == null) {
                            this.Entries.RemoveAt(index);
                            continue;
                        }
                        compileableAsset.Compile(hint, forceCompilation);
                    }
                }
            }


        }

        [Serializable]
        public struct Entry {
            public string guid;
            public CompileOptions options;
        }
    }

}