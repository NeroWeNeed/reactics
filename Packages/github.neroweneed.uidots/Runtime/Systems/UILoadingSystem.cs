using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(UIInitializationSystemGroup),OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public class UILoadingSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;
        private List<AsyncOperationHandle<TextAsset>> handles = new List<AsyncOperationHandle<TextAsset>>();
        //private NativeList<UIGraphData> graphs;
        protected override unsafe void OnCreate() {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            //graphs = new NativeList<UIGraphData>(8, Allocator.Persistent);
        }
        protected override unsafe void OnDestroy() {
            base.OnDestroy();
            foreach (var handle in handles) {
                Addressables.Release(handle);
            }
        }
        protected unsafe override void OnUpdate() {
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            Entities
            .WithoutBurst()
            .WithNone<UIGraphData, UIGraphHandleData>()
            .ForEach((Entity entity, in UIGraph ui) =>
            {
                var handle = Addressables.LoadAssetAsync<TextAsset>(ui.value.ToHex());
                var id = this.handles.Count;
                ecb.AddComponent(entity, new UIGraphHandleData { id = id });
                this.handles.Add(handle);
            })
            .Run();
            Entities
            .WithoutBurst()
            .WithNone<UIGraphData>()
            .ForEach((Entity entity, in UIGraphHandleData handleData, in UIGraph ui) =>
            {
                var handle = this.handles[handleData.id];
                if (handle.IsDone) {
                    IntPtr ptr;
                    long allocatedLength;
                    using (var ms = new MemoryStream(handle.Result.bytes)) {
                        allocatedLength = math.ceilpow2(ms.Length);
                        ptr = (IntPtr)UnsafeUtility.Malloc(allocatedLength, 0, Allocator.Persistent);
                        using (var us = new UnmanagedMemoryStream((byte*)ptr.ToPointer(), 0, ms.Length, FileAccess.Write)) {
                            ms.CopyTo(us);
                        }
                    }
                    //int id = this.graphs.Length;
                    var graph = new UIGraphData { value = ptr, allocatedLength = allocatedLength };
                    //this.graphs.Add(graph);
                    ecb.AddComponent(entity, graph);
                }
            })
.Run();
        }
    }
}