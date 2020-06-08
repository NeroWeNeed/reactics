using System;
using System.Collections.Generic;
using Reactics.UI;
using Reactics.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Mesh;

namespace Reactics.UI
{




    public delegate void LayoutHandler(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info);




    public struct UIElementLayout : ISharedComponentData, IEquatable<UIElementLayout>
    {
        public LayoutHandler handler;

        public UIElementLayout(LayoutHandler provider)
        {
            this.handler = provider;
        }


        public bool Equals(UIElementLayout other)
        {
            return EqualityComparer<LayoutHandler>.Default.Equals(handler, other.handler); ;
        }

        public override int GetHashCode()
        {
            return -308314852 + EqualityComparer<LayoutHandler>.Default.GetHashCode(handler);
        }
        public void Invoke(EntityManager manager, Entity self, Entity parent, NativeMultiHashMap<Entity, Entity> dependencies, NativeHashMap<Entity, UIElementBounds> bounds, ValueInfo info, bool updateSize = true)
        {
            if (handler != null)
            {
                if (updateSize)
                {
                    info.parentWidth = bounds[parent].right - bounds[parent].left;
                    info.parentHeight = bounds[parent].bottom - bounds[parent].top;
                }
                info.siblingCount = dependencies.CountValuesForKey(parent);

                handler.Invoke(manager, self, parent, dependencies, bounds, info);
            }
        }
    }
    public struct UIElement : IComponentData
    {
        public Entity parent;
        public int updateCount;
    }
    public struct UIElementAnchor : IComponentData
    {
        public UIAnchor value;
    }
    public struct UIElementBounds : IComponentData
    {
        public static readonly UIElementBounds Null = new UIElementBounds(float.NaN, float.NaN, float.NaN, float.NaN);

        public static readonly UIElementBounds Zero = new UIElementBounds(0f, 0f, 0f, 0f);
        public float left, top, right, bottom;

        public float X { get => left; set => left = value; }
        public float Y { get => top; set => top = value; }
        public float Width
        {
            get => right - left;
            set
            {
                right = left + value;
            }
        }

        public float Height
        {
            get => bottom - top;
            set
            {
                bottom = top + value;
            }
        }
        public UIElementBounds(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public override bool Equals(object obj)
        {
            return obj is UIElementBounds bounds &&
                   left == bounds.left &&
                   top == bounds.top &&
                   right == bounds.right &&
                   bottom == bounds.bottom &&
                   X == bounds.X &&
                   Y == bounds.Y &&
                   Width == bounds.Width &&
                   Height == bounds.Height;
        }

        public override int GetHashCode()
        {
            int hashCode = 299941791;
            hashCode = hashCode * -1521134295 + left.GetHashCode();
            hashCode = hashCode * -1521134295 + top.GetHashCode();
            hashCode = hashCode * -1521134295 + right.GetHashCode();
            hashCode = hashCode * -1521134295 + bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"UIElementBounds({left},{top},{right},{bottom})";
        }
    }

    public struct UIEnvironmentData : IComponentData, IEquatable<UIEnvironmentData>
    {

        public float2 window;
        public float2 screen;


        public bool Equals(UIEnvironmentData other)
        {
            return window.Equals(other.window) && screen.Equals(other.screen);
        }

        public override int GetHashCode()
        {
            int hashCode = 792705158;
            hashCode = hashCode * -1521134295 + window.GetHashCode();
            hashCode = hashCode * -1521134295 + screen.GetHashCode();
            return hashCode;
        }
    }
    public struct FaceScreen : IComponentData { }

    //Vertical Layout
    public struct DirectionalLayoutSettings : IComponentData
    {
        public Value gap;
    }
    public struct GridLayoutSettings : IComponentData
    {
        public Value horizontalGap, verticalGap;
        public Value tileWidth, tileHeight;
        public int stride;
    }
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    public struct LocalToScreen : IComponentData
    {
        public float2 location;

        public float2 extents;
        public UIAnchor screenAnchor;

        public UIAnchor localAnchor;
    }

    public struct FixedSize : IComponentData
    {
        public static readonly FixedSize zero = new FixedSize(0, 0);
        public Value width, height;
        public FixedSize(float width, float height)
        {
            this.width = width.Px();
            this.height = height.Px();
        }
        public FixedSize(int width, int height)
        {
            this.width = width.Px();
            this.height = height.Px();
        }

        public FixedSize(Value width, Value height)
        {
            this.width = width;
            this.height = height;
        }


    }
    public struct FixedOffset : IComponentData
    {
        public static readonly FixedOffset zero = new FixedOffset(0, 0);
        public Value x, y;
        public FixedOffset(float x, float y)
        {
            this.x = x.Px();
            this.y = y.Px();
        }
        public FixedOffset(int x, int y)
        {
            this.x = x.Px();
            this.y = y.Px();
        }

        public FixedOffset(Value x, Value y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public struct Margin : IComponentData
    {
        public Value left, top, right, bottom;

        public Margin(Value left, Value top, Value right, Value bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }
    public struct ResizableSize : IComponentData
    {
        public Value minWidth, minHeight, maxWidth, maxHeight;
        public ResizableSize(Value minWidth, Value minHeight, Value maxWidth, Value maxHeight)
        {
            this.minWidth = minWidth;
            this.minHeight = minHeight;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
        }
    }
    public struct UIBoxMesh : IComponentData { }

    public struct UITextMesh : IComponentData { }

    public struct UIText : ISharedComponentData, IEquatable<UIText>
    {
        public string value;

        public UIText(string value)
        {
            this.value = value;
        }

        public bool Equals(UIText other)
        {
            return value == other.value;
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<FixedString128>.Default.GetHashCode(value);
        }
    }

    public struct UITextSettings : IComponentData
    {

    }

    public struct UIFont : ISharedComponentData, IEquatable<UIFont>
    {
        public TMP_FontAsset value;

        public UIFont(TMP_FontAsset value)
        {
            this.value = value;
        }

        public bool Equals(UIFont other)
        {
            return EqualityComparer<TMP_FontAsset>.Default.Equals(value, other.value);
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<TMP_FontAsset>.Default.GetHashCode(value);
        }
    }




}