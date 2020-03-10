using System;
using System.Collections.Generic;
using Reactics.Util;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TextCore;

namespace Reactics.UI
{

    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    public struct LocalToScreen : IComponentData
    {
        public float2 location;

        public float2 extents;
        public UIAnchor screenAnchor;

        public UIAnchor localAnchor;
    }

    public struct UIElement : ISharedComponentData, IEquatable<UIElement>
    {
        public IUIConfigurator configurator;



        public bool Equals(UIElement other)
        {
            if (configurator == null)
                return other.configurator == null;
            else
                return configurator.Equals(other.configurator);
        }

        public override int GetHashCode()
        {

            return 1169645722 + EqualityComparer<IUIConfigurator>.Default.GetHashCode(configurator);
        }
    }
    public struct UIClean : IComponentData
    {

    }

    public struct UIText : ISharedComponentData, IEquatable<UIText>
    {
        public string value;

        public bool Equals(UIText obj)
        {
            return obj is UIText text &&
                   value == text.value;
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<string>.Default.GetHashCode(value);
        }
    }
    public struct UITextSettings : IComponentData {
        public float fontSize;
    }

    public struct UIFont : ISharedComponentData, IEquatable<UIFont> {
        public TMP_FontAsset value;

        public bool Equals(UIFont obj)
        {
            return obj is UIFont font &&
                   EqualityComparer<TMP_FontAsset>.Default.Equals(value, font.value);
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<TMP_FontAsset>.Default.GetHashCode(value);
        }
    }





}