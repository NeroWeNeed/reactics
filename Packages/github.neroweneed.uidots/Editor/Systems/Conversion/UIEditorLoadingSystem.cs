using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots {
/*         [UpdateInGroup(typeof(UIInitializationSystemGroup), OrderFirst = true)]
        [WorldSystemFilter(WorldSystemFilterFlags.Editor)]
        public class UIEditorLoadingSystem : SystemBase {
            private EntityCommandBufferSystem entityCommandBufferSystem;
            //private NativeList<UIGraphData> graphs;
            protected override unsafe void OnCreate() {
                base.OnCreate();
                entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
                //graphs = new NativeList<UIGraphData>(8, Allocator.Persistent);
            }
            protected override unsafe void OnDestroy() {
                base.OnDestroy();
            }
            //TODO: Optimize loading
            protected unsafe override void OnUpdate() {
                var ecb = entityCommandBufferSystem.CreateCommandBuffer();
                Entities
                .WithoutBurst()
                .WithNone<UIGraphData>()
                .ForEach((Entity entity, in UIGraph ui) =>
                {
                    IntPtr ptr;
                    long allocatedLength;
                    using (var fs = File.OpenRead(UnityEditor.AssetDatabase.GUIDToAssetPath(ui.value.ToHex()))) {
                        
                        Debug.Log(fs.Length);
                        allocatedLength = math.ceilpow2(fs.Length);
                        ptr = (IntPtr)UnsafeUtility.Malloc(allocatedLength, 0, Allocator.Persistent);
                        using (var us = new UnmanagedMemoryStream((byte*)ptr.ToPointer(), 0, fs.Length, FileAccess.Write)) {
                            fs.CopyTo(us);
                        }
                    }
                    //int id = this.graphs.Length;
                    var graph = new UIGraphData { value = ptr, allocatedLength = allocatedLength };
                    //this.graphs.Add(graph);
                    Debug.Log($"Loading   ({ptr})");
                    ecb.AddComponent(entity, graph);
                    ecb.AddBuffer<UINode>(entity);
                })
                .Run();
            }
        } */


}