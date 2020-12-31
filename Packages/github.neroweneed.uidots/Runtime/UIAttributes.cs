using System;
using System.Reflection;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace NeroWeNeed.UIDots {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class UIDotsElementAttribute : Attribute {
        public string Identifier { get; set; }
        public ulong Mask { get; set; }
        public UIDotsElementAttribute(string identifier, params byte[] configs) {
            Identifier = identifier;
            Mask = UIConfigUtility.CreateMask(configs);
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
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class UIConfigBlockAttribute : Attribute {
        public int Priority { get; }

        public string Name { get; }

        public UIConfigBlockAttribute(string name = null, int priority = 0) {
            Name = name;
            this.Priority = priority;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Method)]
    public sealed class UICallbackAttribute : Attribute {
        public string Identifier { get; }

        public UICallbackAttribute(string name = null) {
            
            Identifier = name;
        }
    }
}