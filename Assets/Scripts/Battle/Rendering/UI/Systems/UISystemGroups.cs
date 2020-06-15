using System;
using Reactics.UI;
using Reactics.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Reactics.UI
{
    [UpdateAfter(typeof(UILayoutSystem))]
    [UpdateInGroup(typeof(UISystemGroup))]

    public class UIMeshSystemGroup : ComponentSystemGroup { }

    [UpdateAfter(typeof(UIMeshSystemGroup))]
    [UpdateInGroup(typeof(UISystemGroup))]
    public class UIPaintSystemGroup : ComponentSystemGroup { }

    [UpdateAfter(typeof(UIPaintSystemGroup))]
    public class UIEntityCommandBufferSystem : EntityCommandBufferSystem
    {

    }







    public class UISystemGroup : ComponentSystemGroup
    {

        public LayoutArchetypes Layouts { get; private set; }
        public ElementArchetypes Elements { get; private set; }

        public Camera Camera { get; private set; }

        public Camera UICamera { get; private set; }


        protected override void OnCreate()
        {
            base.OnCreate();
            Layouts = new LayoutArchetypes(EntityManager);
            Elements = new ElementArchetypes(EntityManager);

            foreach (Camera t in UnityEngine.Object.FindObjectsOfType(typeof(Camera)))
            {
                if (t.tag == "MainCamera")
                {
                    Camera = t;
                    break;
                }
            }
            if (Camera == null)
            {
                //throw new UnityException("Missing Main Camera in Scene");
            }
        }

        public struct LayoutArchetypes
        {

            public readonly EntityArchetype General;
            public readonly EntityArchetype Directional;

            public readonly EntityArchetype Region;


            public readonly EntityManager EntityManager;

            public LayoutArchetypes(EntityManager manager)
            {
                this.EntityManager = manager;
                NativeArray<ComponentType> basis = new NativeArray<ComponentType>(new ComponentType[] { ComponentType.ReadOnly<UIElementLayout>(), typeof(UIElement), typeof(UIElementBounds) }, Allocator.Temp);
                General = EntityManager.CreateArchetype(basis.ToArray());
                Directional = FromGeneral(EntityManager, basis, ComponentType.ReadOnly<DirectionalLayoutSettings>());
                Region = FromGeneral(EntityManager, basis, ComponentType.ReadOnly<FixedSize>(), ComponentType.ReadOnly<FixedOffset>());
            }

            public ElementBuilder VerticalLayoutBuilder(Value vgap = default, Entity parent = default) => VerticalLayoutBuilder(EntityManager.CreateEntity(Directional), vgap, parent);
            public ElementBuilder VerticalLayoutBuilder(Entity entity, Value vgap = default, Entity parent = default) => DirectionalLayoutBuilder(entity, UILayoutHandlers.Vertical, vgap, parent);
            public ElementBuilder HorizontalLayoutBuilder(Value hgap = default, Entity parent = default) => HorizontalLayoutBuilder(EntityManager.CreateEntity(Directional), hgap, parent);
            public ElementBuilder HorizontalLayoutBuilder(Entity entity, Value hgap = default, Entity parent = default) => DirectionalLayoutBuilder(entity, UILayoutHandlers.Horizontal, hgap, parent);
            private ElementBuilder DirectionalLayoutBuilder(Entity entity, LayoutHandler layoutHandler, Value gap = default, Entity parent = default)
            {
                EntityManager.SetArchetype(entity, Directional);
                EntityManager.SetComponentData(entity, new UIElement
                {
                    parent = parent.Equals(default) ? Entity.Null : parent
                });
                EntityManager.SetSharedComponentData(entity, new UIElementLayout(layoutHandler));
                if (!gap.Equals(default))
                    EntityManager.SetComponentData(entity, new DirectionalLayoutSettings { gap = gap });

                return new ElementBuilder(entity, EntityManager);
            }




            private static EntityArchetype FromGeneral(EntityManager manager, NativeArray<ComponentType> basis, params ComponentType[] componentTypes)
            {
                var types = new ComponentType[basis.Length + componentTypes.Length];
                NativeArray<ComponentType>.Copy(basis, types, basis.Length);
                Array.Copy(componentTypes, 0, types, basis.Length, componentTypes.Length);
                return manager.CreateArchetype(types);
            }





            public ElementBuilder RegionBuilder(Value x = default, Value y = default, Value width = default, Value height = default) => RegionBuilder(EntityManager.CreateEntity(General), x, y, width, height);
            public ElementBuilder RegionBuilder(Entity entity, Value x = default, Value y = default, Value width = default, Value height = default)
            {
                EntityManager.SetArchetype(entity, Region);

                EntityManager.SetSharedComponentData(entity, new UIElementLayout(UILayoutHandlers.Region));
                EntityManager.SetComponentData(entity, new FixedSize(x, y));
                EntityManager.SetComponentData(entity, new FixedOffset(width.Equals(default) ? Screen.currentResolution.width.Px() : width, height.Equals(default) ? Screen.currentResolution.height.Px() : height));
                return new ElementBuilder(entity, EntityManager);

            }


        }
        public struct ElementArchetypes
        {
            //public readonly EntityArchetype Text;
            public readonly EntityManager EntityManager;
            public readonly EntityArchetype Box;

            public readonly EntityArchetype Text;
            public ElementArchetypes(EntityManager manager)
            {
                this.EntityManager = manager;

                Box = EntityManager.CreateArchetype(ComponentType.ReadOnly<UIElementLayout>(), typeof(UIElement), typeof(BuiltinMaterialPropertyUnity_RenderingLayer), typeof(UIElementBounds), typeof(RenderMesh), typeof(RenderBounds), typeof(UIBoxMesh), typeof(FixedSize), typeof(LocalToScreen));
                Text = EntityManager.CreateArchetype(ComponentType.ReadOnly<UIElementLayout>(), typeof(UIElement), typeof(BuiltinMaterialPropertyUnity_RenderingLayer), typeof(UIElementBounds), typeof(RenderMesh), typeof(RenderBounds), typeof(UITextMesh), typeof(LocalToScreen), typeof(UIFont), typeof(UIText), typeof(UITextSettings));
            }
            public Entity BoxBuilder(Value width, Value height, Entity parent = default) => BoxBuilder(EntityManager.CreateEntity(Box), width, height, parent);
            public Entity BoxBuilder(Entity entity, Value width, Value height, Entity parent = default)
            {
                EntityManager.SetArchetype(entity, Box);
                EntityManager.SetComponentData(entity, new UIElement
                {
                    parent = parent.Equals(default) ? Entity.Null : parent
                });
                EntityManager.SetSharedComponentData(entity, new UIElementLayout(UILayoutHandlers.Fixed));
                EntityManager.SetComponentData(entity, new BuiltinMaterialPropertyUnity_RenderingLayer
                {
                    Value = 5
                });
                EntityManager.SetComponentData(entity, new FixedSize(width, height));

                return entity;
            }
            public Entity BoxBuilder(Entity entity, Value size, Entity parent = default) => BoxBuilder(size, size, parent);

            public Entity BoxBuilder(Value size, Entity parent = default) => BoxBuilder(EntityManager.CreateEntity(Box), size, parent);

            public Entity TextBuilder(string text, TMP_FontAsset font, Entity parent = default) => TextBuilder(EntityManager.CreateEntity(Text), text, font, parent);
            public Entity TextBuilder(Entity entity, string text, TMP_FontAsset font, Entity parent = default)
            {
                EntityManager.SetArchetype(entity, Text);
                EntityManager.SetComponentData(entity, new UIElement
                {
                    parent = parent.Equals(default) ? Entity.Null : parent
                });
                EntityManager.SetSharedComponentData(entity, new UIElementLayout(UILayoutHandlers.Text));
                EntityManager.SetComponentData(entity, new BuiltinMaterialPropertyUnity_RenderingLayer
                {
                    Value = 5
                });
                EntityManager.SetSharedComponentData(entity, new UIText(text));
                EntityManager.SetSharedComponentData(entity, new UIFont(font));


                return entity;
            }
        }

        public struct ElementBuilder
        {
            private Entity entity;

            private EntityManager EntityManager;

            bool hasMargin;

            public ElementBuilder(Entity entity, EntityManager entityManager)
            {
                this.entity = entity;
                this.EntityManager = entityManager;
                hasMargin = false;
            }

            public Entity Build() => entity;

            public ElementBuilder WithMargin(Value all) => WithMargin(all, all, all, all);
            public ElementBuilder WithMargin(Value left, Value top, Value right, Value bottom)
            {
                if (!hasMargin)
                {
                    EntityManager.AddComponentData(entity, new Margin(left, top, right, bottom));
                    hasMargin = true;
                }
                else
                {
                    EntityManager.SetComponentData(entity, new Margin(left, top, right, bottom));
                }

                return this;
            }


        }





    }
    [UpdateInGroup(typeof(UISystemGroup))]
    [UpdateAfter(typeof(UIEnvironmentData))]

    [UpdateBefore(typeof(UIMeshSystemGroup))]
    public class UILayoutSystemGroup : ComponentSystemGroup { }

}