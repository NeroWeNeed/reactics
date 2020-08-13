using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UISizeProviderSystemGroup))]
    public class UITextSizeProvider : SystemBase {

        protected override void OnUpdate() {
            Entities.WithChangeFilter<UITextVersion>().ForEach((ref UISize size, in UIFont font, in UIText text) =>
            {
                float w = 0;
                float h = 0;
                var scale = font.size.RealValue<SimpleValueProperties>() / font.value.faceInfo.lineHeight;
                var baseline = font.value.faceInfo.baseline * scale;
                for (int i = 0; i < text.value.Length; i++) {
                    var glyph = font.value.characterLookupTable[text.value[i]].glyph;
                    w += scale * glyph.metrics.horizontalAdvance;

                    h = math.max(h, glyph.metrics.height * scale);
                }
                size.Width = w;
                size.Height = h;

            }).WithoutBurst().Run();


        }
    }
}