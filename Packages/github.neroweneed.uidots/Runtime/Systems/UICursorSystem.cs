using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public class UICursorIndexUpdateSystem : SystemBase {
        public const float accuracy = math.PI/6f;
        protected override void OnCreate() {

            base.OnCreate();
        }
        protected override void OnUpdate() {
            Entities.ForEach((Entity entity, ref UICursor cursor, in UICursorInput input) =>
            {
                
                
                if (!float.IsNaN(input.direction)) {
                    var nodeEntities = GetBuffer<UINode>(cursor.target).Reinterpret<Entity>();
                    var nodeInfo = new NativeArray<UINodeInfo>(nodeEntities.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);
                    var bounds = new NativeArray<WorldRenderBounds>(nodeEntities.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);

                    float3 source = float3.zero;
                    for (int i = 0; i < nodeEntities.Length; i++) {
                        nodeInfo[i] = GetComponent<UINodeInfo>(nodeEntities[i]);
                        bounds[i] = GetComponent<WorldRenderBounds>(nodeEntities[i]);
                        if (nodeInfo[i].index == cursor.index) {
                            source = bounds[i].Value.Center;
                        }

                    }
                    ValueTuple<float, int> data = (float.PositiveInfinity, cursor.index);
                    for (int i = 0; i < nodeEntities.Length; i++) {
                        if (nodeInfo[i].index == cursor.index)
                            continue;

                        var angle = math.atan2(bounds[i].Value.Center.y - source.y, bounds[i].Value.Center.x - source.x);
                        var angleDiff = math.distance(angle, input.direction) % math.PI;
                        Debug.Log($"Index: {nodeInfo[i].index}, Angle: {angle}, Accuracy: {accuracy}, Angle Diff: {angleDiff}");
                        if (angleDiff < accuracy) {
                            var distance = math.distance(source, bounds[i].Value.Center);
                            if (distance < data.Item1) {
                                data.Item1 = distance;
                                data.Item2 = nodeInfo[i].index;
                            }
                        }
                    }
                    if (cursor.index != data.Item2) {
                        Debug.Log("UPDATED CURSOR");
                        cursor.index = data.Item2;
                    }
                }
            }).WithoutBurst().Run();
        }
    }
}