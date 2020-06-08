using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore;

namespace Reactics.UI
{

    public static class UILayoutHandlers
    {
        /*         private static void ApplyMargin(ref UIElementBounds bounds, in Margin margin)
                {
                    bounds.left += margin.left.RealValue;
                    bounds.top += margin.top.RealValue;
                    bounds.right -= margin.right.RealValue;
                    bounds.bottom -= margin.bottom.RealValue;
                } */
        public static void Vertical(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info)
        {


            var children = dependencies.GetValuesForKey(self);
            var parentRegion = bounds[parent];
            var region = bounds[parent];

            bounds[self] = region;
            var maxWidth = 0f;
            var totalHeight = 0f;
            Value gap = 0.Px();
            if (manager.TryGetComponent(self, out DirectionalLayoutSettings settings))
                gap = settings.gap;
            while (children.MoveNext())
            {
                var child = children.Current;
                bounds[self] = region;
                manager.GetSharedComponentData<UIElementLayout>(child).Invoke(manager, child, self, dependencies, bounds, info, false);
                totalHeight += bounds[child].Height + gap.Get(info, ValueConverterHint.USE_HEIGHT);
                region.top += bounds[child].Height + gap.Get(info, ValueConverterHint.USE_HEIGHT);
                maxWidth = math.max(bounds[child].Width, maxWidth);
            }
            bounds[self] = new UIElementBounds(parentRegion.left, parentRegion.top, parentRegion.left + maxWidth, parentRegion.top + totalHeight);


        }
        public static void Horizontal(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info)
        {

            var children = dependencies.GetValuesForKey(self);
            var parentRegion = bounds[parent];
            var region = bounds[parent];
            bounds[self] = region;
            var maxHeight = 0f;
            var totalWidth = 0f;
            Value gap = 0.Px();
            if (manager.TryGetComponent(self, out DirectionalLayoutSettings settings))
                gap = settings.gap;
            var childCount = dependencies.CountValuesForKey(self);
            if (childCount > 0)
            {
                info.deadWidth = gap.Get(info, ValueConverterHint.USE_WIDTH) * (childCount - 1);
                info.deadHeight = gap.Get(info, ValueConverterHint.USE_HEIGHT) * (childCount - 1);
            }
            while (children.MoveNext())
            {
                var child = children.Current;
                bounds[self] = region;
                manager.GetSharedComponentData<UIElementLayout>(child).Invoke(manager, child, self, dependencies, bounds, info, false);

                totalWidth += bounds[child].Width;
                region.left += bounds[child].Width + gap.Get(info, ValueConverterHint.USE_WIDTH);
                maxHeight = math.max(bounds[child].Height, maxHeight);
            }
            var deadspace = childCount > 0 ? gap.Get(info, ValueConverterHint.USE_WIDTH) * (childCount - 1) : 0;
            bounds[self] = new UIElementBounds(parentRegion.left, parentRegion.top, parentRegion.left + totalWidth + deadspace, parentRegion.top + maxHeight);
        }
        public static void Grid(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds)
        {

        }

        public static void Fixed(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info)
        {
            manager.TryGetComponent(self, out FixedSize size);
            if (size.Equals(default))
                size = FixedSize.zero;
            bounds[self] = new UIElementBounds(bounds[parent].left, bounds[parent].top, bounds[parent].left + size.width.Get(info, ValueConverterHint.USE_WIDTH), bounds[parent].top + size.height.Get(info, ValueConverterHint.USE_HEIGHT));
        }
        public static void Text(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info)
        {
            manager.TryGetSharedComponent(self, out UIText text);
            manager.TryGetSharedComponent(self, out UIFont font);
            if (text.Equals(default) || font.Equals(default))
                bounds[self] = new UIElementBounds(bounds[parent].left, bounds[parent].top, bounds[parent].left, bounds[parent].top);
            else
            {
                float width = 0f, height = 0f;
                Glyph glyph;
                for (int i = 0; i < text.value.Length; i++)
                {
                    glyph = font.value.characterLookupTable[text.value[i]].glyph;
                    width += glyph.metrics.horizontalAdvance;
                    height = math.max(height, glyph.metrics.height);
                }
                bounds[self] = new UIElementBounds(bounds[parent].left, bounds[parent].top, bounds[parent].left + width, bounds[parent].top + height);
            }
        }


        public static void Region(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info)
        {
            var size = manager.GetComponentData<FixedSize>(self);
            var pos = manager.GetComponentData<FixedOffset>(self);
            bounds[self] = new UIElementBounds(pos.x.Get(info), pos.y.Get(info), size.width.Get(info),size.height.Get(info));



            if (dependencies.ContainsKey(self))
            {
                var children = dependencies.GetValuesForKey(self);
                while (children.MoveNext())
                {
                    var child = children.Current;
                    manager.GetSharedComponentData<UIElementLayout>(child).Invoke(manager, child, self, dependencies, bounds, info);
                }
            }
        }
    }
}