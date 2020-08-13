using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.TextCore;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UIMeshProviderSystemGroup))]
    public class UITextMeshProvider : SystemBase {
        private static readonly int[] indices = new int[] { 0, 2, 1, 2, 3, 1 };
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UITextVersion>()
            .WithChangeFilter<UIResolvedBox>()
            .ForEach((ref DynamicBuffer<UIMeshVertexData> vertexData, ref DynamicBuffer<UIMeshIndexData> indexData, ref RenderBounds renderBounds, in UIResolvedBox resolvedBox, in UIFont font, in UIText text) =>
            {


                vertexData.Clear();
                indexData.Clear();
                var scale = font.size.RealValue<SimpleValueProperties>() / font.value.faceInfo.lineHeight;
                float2 offset = new float2(0, font.value.faceInfo.baseline * scale);

                int index = 0;
                for (int i = 0; i < text.value.Length; i++) {
                    var glyph = font.value.characterLookupTable[text.value[i]].glyph;
                    var emSize = new float2(glyph.metrics.width * scale, glyph.metrics.height * scale);
                    AddRect(vertexData, indexData, offset, new float2(glyph.metrics.horizontalBearingX * scale, glyph.metrics.horizontalBearingY * scale), emSize, glyph, new float2(font.value.atlasWidth, font.value.atlasHeight), ref index);
                    offset.x += scale * glyph.metrics.horizontalAdvance;

                }
                renderBounds.Value = new AABB
                {
                    Center = float3.zero,
                    Extents = new float3(resolvedBox.Width / 2f, resolvedBox.Height / 2f, 0.1f)
                };
            }).WithoutBurst().Run();

        }

        private void AddRect(DynamicBuffer<UIMeshVertexData> vertexData, DynamicBuffer<UIMeshIndexData> indexData, float2 offset, float2 bearings, float2 size, Glyph glyph, float2 atlas, ref int currentIndex) {
            vertexData.Add(new UIMeshVertexData
            {

                vertex = new float3(offset.x + bearings.x, offset.y - (size.y - bearings.y), 0),
                normal = new float3(0, 0, 1),
                uv = new float2(glyph.glyphRect.x / atlas.x, glyph.glyphRect.y / atlas.y)
            });
            vertexData.Add(new UIMeshVertexData
            {
                vertex = new float3(offset.x + size.x + bearings.x, offset.y - (size.y - bearings.y), 0),
                normal = new float3(0, 0, 1),
                uv = new float2((glyph.glyphRect.x + glyph.glyphRect.width) / atlas.x, glyph.glyphRect.y / atlas.y)
            });
            vertexData.Add(new UIMeshVertexData
            {
                vertex = new float3(offset.x + bearings.x, offset.y + bearings.y, 0),
                normal = new float3(0, 0, 1),
                uv = new float2(glyph.glyphRect.x / atlas.x, (glyph.glyphRect.y + glyph.glyphRect.height) / atlas.y)
            });
            vertexData.Add(new UIMeshVertexData
            {
                vertex = new float3(offset.x + size.x + bearings.x, offset.y + bearings.y, 0),
                normal = new float3(0, 0, 1),
                uv = new float2((glyph.glyphRect.x + glyph.glyphRect.width) / atlas.x, (glyph.glyphRect.y + glyph.glyphRect.height) / atlas.y)
            });
            foreach (var index in indices) {
                indexData.Add(currentIndex + index);
            }
            currentIndex += 4;
        }
    }
    /* public class UITextWidthProvider : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UIFont>()
            .WithChangeFilter<UIFontSettings>()
            .WithChangeFilter<UIText>().ForEach((ref UIWidth width, ref UIHeight height, in UIFont font, in UIFontSettings fontSettings, in UIText text) =>
              {
                  float w = 0;
                  float h = 0;
                  for (int i = 0; i < text.text.Length; i++) {

                      var metrics = font.value.glyphLookupTable[text.text[i]].metrics;
                      var scale = metrics.width / metrics.height;
                      w += scale * fontSettings.size;
                      if (i + 1 < text.text.Length)
                          w += scale * f

                      h = math.max(h, scale * metrics.height);
                  }
              }).WithoutBurst().Run();
        }
    } */
    /* public abstract class UITextMeshVertexProviderSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((Entity entity))
        }

    } */
}