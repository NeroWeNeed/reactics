using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Core.UI {
    [UpdateInGroup(typeof(UISystemGroup))]
    public class UISizeProviderSystemGroup : ComponentSystemGroup { }
    //Rectangle
    [UpdateInGroup(typeof(UISizeProviderSystemGroup))]
    public class UIBoxSizeProvider : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<UIRectangle>().ForEach((ref UISize size, in UIRectangle rect) =>
            {
                size.Width = rect.width;
                size.Height = rect.height;
            }).ScheduleParallel();
        }
    }
    //Text
    //TODO: Line wrapping, font scaling.
    public class UITextSizeProvider : SystemBase {
        protected override void OnUpdate() {
            Entities
            .WithChangeFilter<UIFont>()
            .WithChangeFilter<UIText>()
            .ForEach((ref UISize size, in UIFont font, in UIText text) =>
            {
                var width = 0f;
                var height = 0f;
                var faceInfo = font.value.faceInfo;
                for (int i = 0; i < text.text.Length; i++) {
                    var character = font.value.characterLookupTable[text.text[i]];
                    var ratio = (character.glyph.metrics.width / character.glyph.metrics.height) * font.size;
                    width += (character.glyph.metrics.horizontalBearingX + character.glyph.metrics.width) * ratio;
                    height = math.max(height, (character.glyph.metrics.horizontalBearingY + character.glyph.metrics.height) * ratio);
                    if (i + 1 < text.text.Length) {
                        width += character.glyph.metrics.horizontalAdvance * ratio;
                    }
                }
                size.Width = width;
                size.Height = height;
            }).WithoutBurst().Run();

        }
    }
}