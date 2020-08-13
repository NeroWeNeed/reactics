using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UIMeshProviderSystemGroup))]
    public class UIRectangleMeshProvider : SystemBase {
        private static readonly int[] indices = new int[] { 0, 2, 1, 2, 3, 1 };
        private EntityQuery query;
        protected override void OnCreate() {
            query = GetEntityQuery(ComponentType.ReadOnly<UISize>(), ComponentType.ReadOnly<UIRectangle>(), ComponentType.ReadOnly<UIResolvedBox>(), ComponentType.ReadWrite<UIMeshVersion>(), ComponentType.ReadWrite<UIMeshVertexData>(), ComponentType.ReadWrite<RenderBounds>(), ComponentType.ReadWrite<UIMeshIndexData>());
            query.SetChangedVersionFilter(typeof(UIResolvedBox));
            RequireForUpdate(query);
        }
        protected override void OnUpdate() {
            Entities.WithAll<UISize, UIRectangle>().ForEach((ref DynamicBuffer<UIMeshIndexData> indexData, ref DynamicBuffer<UIMeshVertexData> vertexData, ref RenderBounds renderBounds, ref UIMeshVersion meshVersion, in UIResolvedBox resolvedBox) =>
             {
                 vertexData.Clear();
                 vertexData.Add(new UIMeshVertexData(new float3(0, 0, 0), new float3(0, 0, 1), new float2(0, 0)));
                 vertexData.Add(new UIMeshVertexData(new float3(resolvedBox.Width, 0, 0), new float3(0, 0, 1), new float2(1, 0)));
                 vertexData.Add(new UIMeshVertexData(new float3(0, resolvedBox.Height, 0), new float3(0, 0, 1), new float2(0, 1)));
                 vertexData.Add(new UIMeshVertexData(new float3(resolvedBox.Width, resolvedBox.Height, 0), new float3(0, 0, 1), new float2(1, 1)));
                 renderBounds.Value = new AABB
                 {
                     Center = float3.zero,
                     Extents = new float3(resolvedBox.Width / 2f, resolvedBox.Height / 2f, 0.1f)
                 };
                 indexData.Clear();
                 for (int i = 0; i < indices.Length; i++) {
                     indexData.Add(indices[i]);
                 }

                 meshVersion.Version++;
             }).Schedule(Dependency).Complete();
        }


    }
}