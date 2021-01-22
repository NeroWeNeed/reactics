using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public static class UIDotsEditorCommands {
        private const string CONFIG_LAYOUT_PATH = "Packages/github.neroweneed.uidots/Runtime/UIConfigTables.cs";


        [MenuItem("DOTS/UI Dots/Generate Config Layout")]
        public static void GenerateConfigLayout() {
            using (var fileStream = File.Create(CONFIG_LAYOUT_PATH)) {
                using (var writer = new StreamWriter(fileStream)) {
                    GenerateConfigLayout(writer, AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetCustomAttribute<UIConfigBlockAttribute>() != null).SelectMany(assembly => assembly.GetTypes().Where(type => type.GetCustomAttribute<UIConfigBlockAttribute>() != null).ToArray()).ToArray());
                }
            }
            AssetDatabase.Refresh();
        }
        public static void GenerateConfigLayout(StreamWriter writer, Type[] configBlocks) {
            var usings = configBlocks.Select(t => t.Namespace).Distinct().ToArray();
            writer.WriteLine("using System;");
            foreach (var u in usings) {
                writer.WriteLine($"using {u};");
            }
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// AUTO-GENERATED, DO NOT EDIT.");
            writer.WriteLine("/// Tables for configuration block layouts. Type Table is separated so the layout table can be accessed from burst.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("namespace NeroWeNeed.UIDots {");
            writer.WriteLine("    public static class UIConfigLayoutTable {");
            for (int i = 0; i < configBlocks.Length; i++) {
                writer.WriteLine($"        public const byte {configBlocks[i].Name} = {i};");
            }
            writer.WriteLine("        public static readonly int[] Lengths = new int[] {");
            for (int i = 0; i < configBlocks.Length; i++) {
                writer.WriteLine($"            {UnsafeUtility.SizeOf(configBlocks[i])}{(i + 1 < configBlocks.Length ? "," : string.Empty)}");
            }
            writer.WriteLine("        };");
            writer.WriteLine("    }");
            writer.WriteLine();
            writer.WriteLine("    public static class UIConfigTypeTable {");
            writer.WriteLine("        public static readonly Type[] Types = new Type[] {");
            for (int i = 0; i < configBlocks.Length; i++) {
                writer.WriteLine($"            typeof({configBlocks[i].Name}){(i + 1 < configBlocks.Length ? "," : string.Empty)}");
            }
            writer.WriteLine("        };");
            writer.WriteLine("    }");
            writer.WriteLine("}");

        }
    }
}