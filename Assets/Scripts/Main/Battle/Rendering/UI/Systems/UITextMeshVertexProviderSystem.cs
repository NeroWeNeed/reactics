using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.TextCore;

namespace Reactics.Core.UI {

    public class UITextWidthProvider : SystemBase {
        protected override void OnUpdate() {
            /* Entities.WithChangeFilter<UIFont>()
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
              }).WithoutBurst().Run(); */
        }
    }
    /* public abstract class UITextMeshVertexProviderSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((Entity entity))
        }

    } */
}