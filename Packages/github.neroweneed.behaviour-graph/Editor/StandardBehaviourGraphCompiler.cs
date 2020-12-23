using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
[assembly: SearchableAssembly]

namespace NeroWeNeed.BehaviourGraph.Editor {
    public class StandardBehaviourGraphCompiler : BehaviourGraphCompiler {

        protected override bool Compile(BehaviourGraphModel model, string outputFile, out string error) {
            try {
                this.GetType().GetMethod(nameof(CompileModel)).MakeGenericMethod(model.BehaviourType.Value).Invoke(this, new object[] { model, outputFile });
                error = null;
                return true;
            }
            catch (Exception e) {
                error = e.Message;
                return false;
            }
        }


        public unsafe string CompileModel<T>(BehaviourGraphModel model, string output) where T : Delegate {

            using var builder = new BlobBuilder(Allocator.Temp);
            ref BehaviourGraph<T> graph = ref builder.ConstructRoot<BehaviourGraph<T>>();
            var entries = model.Entries.OfType<BehaviourEntry>().ToArray();
            var nodes = builder.Allocate<BehaviourNode<T>>(ref graph.nodes, entries.Length);
            var settings = model.Settings;
            var initialState = new List<byte>();
            var referenced = new List<string>();
            var dataSize = 0;
            for (int i = 0; i < entries.Length; i++) {
                nodes[i].next = Array.FindIndex(entries, e => e.Id == entries[i].Output);

                nodes[i].action = BurstCompiler.CompileFunctionPointer<T>((T)settings.Behaviours[entries[i].BehaviourIdentifier].method.Value.CreateDelegate(typeof(T)));
                nodes[i].dataLength = entries[i].Data.Length;
                dataSize += entries[i].Data.Length;
                if (!string.IsNullOrEmpty(entries[i].Output)) {
                    referenced.Add(entries[i].Output);
                }
            }
            var entryRoots = entries.Where(e => !referenced.Contains(e.Id)).Select(e => e.Id).ToArray();
            var roots = builder.Allocate<int>(ref graph.roots, entryRoots.Length);
            for (int i = 0; i < entryRoots.Length; i++) {
                roots[i] = Array.FindIndex(entries, e => e.Id == entryRoots[i]);
            }
            var variables = builder.Allocate<BehaviourVariableDefinition>(ref graph.variables, 0);
            var data = builder.Allocate<byte>(ref graph.data, dataSize);
            var offset = 0;
            var ptr = (IntPtr)data.GetUnsafePtr();
            for (int i = 0; i < entries.Length; i++) {
                entries[i].ConfigureMemory(settings.Behaviours[entries[i].BehaviourIdentifier].configurationType.Value, entries[i].Fields, entries[i].Data, out byte[] result, out BehaviourEntry.FieldData[] _);
                Marshal.Copy(result, 0, ptr + offset, entries[i].Data.Length);
                offset += entries[i].Data.Length;
            }
            using var asset = builder.CreateBlobAssetReference<BehaviourGraph<T>>(Allocator.Temp);
            using var writer = new StreamBinaryWriter(output);
            writer.Write(asset);
            return null;
        }

    }
}