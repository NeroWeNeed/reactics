using System;
using NeroWeNeed.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public class UICursorIndexUpdateSystem : SystemBase {

        private struct NodeData {
            public UINodeInfo Item1;
            public AABB Item2;

            public NodeData(UINodeInfo info, AABB bounds) {
                this.Item1 = info;
                this.Item2 = bounds;
            }
        }
        private struct IndexData {
            public float Item1;
            public int Item2;

            public IndexData(float distance, int index) {
                this.Item1 = distance;
                this.Item2 = index;
            }
        }
        protected override void OnUpdate() {
            Entities.ForEach((ref UICursor cursor, in UICursorInput input) =>
            {
                if (math.distance(float2.zero, input.direction) > 0f) {
                    var orthogonal = new float3(input.direction.y, -input.direction.x, 0f);
                    var direction = new float3(input.direction.x, input.direction.y, 0f);
                    var multiplier = new float3(input.multiplier.x, input.multiplier.y, 0f);
                    var directionAngle = math.atan2(direction.y, direction.x);
                    var nodeEntities = GetBuffer<UINode>(cursor.target).Reinterpret<Entity>();
                    var nodeData = new NativeList<ValueTuple<UINodeInfo,AABB>>(Allocator.Temp);
                    var root = GetComponent<UIRoot>(cursor.target);
                    float3 source = float3.zero;
                    for (int i = 0; i < nodeEntities.Length; i++) {
                        var t = GetComponent<WorldRenderBounds>(nodeEntities[i]).Value;
                        var current = new ValueTuple<UINodeInfo, AABB>(GetComponent<UINodeInfo>(nodeEntities[i]), new AABB { Center = t.Center * multiplier, Extents = t.Extents * multiplier });
                        if (UIConfigUtility.HasConfigBlock(root.graph.Value.nodes[current.Item1.index].configurationMask, UIConfigLayoutTable.SelectableConfig)) {
                            nodeData.Add(current);
                            if (current.Item1.index == cursor.index) {
                                source = current.Item2.Center;
                            }
                        }
                    }
                    ValueTuple<float,int> data = new ValueTuple<float, int>(float.PositiveInfinity, cursor.index);
                    for (int i = 0; i < nodeData.Length; i++) {
                        if (MathUtility.Intersects(source, direction, nodeData[i].Item2.Center, orthogonal, out float3 intersection)) {
                            intersection = math.clamp(intersection, nodeData[i].Item2.Center - nodeData[i].Item2.Extents, nodeData[i].Item2.Center + nodeData[i].Item2.Extents);
                            var angle = math.atan2(intersection.y - source.y, intersection.x - source.x);
                            var angleDiff = math.abs(angle - directionAngle);
                            if (angleDiff < input.accuracy && angleDiff >= 0f) {
                                var distance = math.distance(source, intersection);
                                if (distance < data.Item1) {
                                    data.Item1 = distance;
                                    data.Item2 = nodeData[i].Item1.index;
                                }
                            }
                        }
                    }
                    if (cursor.index != data.Item2) {
                        cursor.index = data.Item2;
                    }
                }
            }).Schedule();
        }
    }
}