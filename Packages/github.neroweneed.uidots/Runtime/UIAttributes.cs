using System;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace NeroWeNeed.UIDots {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class UIDotsElementAttribute : Attribute {
        public string Identifier { get; set; }
        public Type ConfigurationType { get; set; }
        public UIDotsElementAttribute(string identifier, Type configurationType = null) {
            Identifier = identifier;
            ConfigurationType = configurationType;
        }
    }
    [AttributeUsage(AttributeTargets.All)]
    public sealed class HideInDecompositionAttribute : Attribute {

    }
    [AttributeUsage(AttributeTargets.All)]
    public sealed class TerminalAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class UIDotsRenderBoxHandlerAttribute : Attribute {
        public string Name;

        public UIDotsRenderBoxHandlerAttribute(string name) {
            Name = name;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AssetReferenceAttribute : Attribute {

    }
    /// <summary>
    /// Instructs Configuration object when generating paths to embed the field into the configuration object.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
    public sealed class EmbedAttribute : System.Attribute {

    }

}