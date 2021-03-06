using System;
using System.Reflection;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace NeroWeNeed.UIDots {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class UIDotsElementAttribute : Attribute {
        public const string DefaultLayoutPass = "Layout";
        public const string DefaultRenderPass = "Render";
        public const string DefaultRenderBoxCountPass = "RenderBoxCount";
        public string Identifier { get; set; }
        public string LayoutPass { get; set; }
        public string RenderPass { get; set; }
        public string RenderBoxCounter { get; set; }
        public UIConfigBlock ConfigBlocks { get; set; } = UIConfigBlock.Empty;
        public UIConfigBlock OptionalConfigBlocks { get; set; }= UIConfigBlock.Empty;
        public UIDotsElementAttribute(string identifier) {
            Identifier = identifier;
            RenderBoxCounter = DefaultRenderBoxCountPass;
            LayoutPass = DefaultLayoutPass;
            RenderPass = DefaultRenderPass;
        }
    }
    
    /*     [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
        public sealed class UIDotsElementAttribute : Attribute {
            public string Identifier { get; set; }
            public string RenderBoxCounter { get; set; }
            public string 
            public ulong Mask { get; set; }
            public UIDotsElementAttribute(string identifier, params byte[] configs) {
                Identifier = identifier;
                RenderBoxCounter = null;
                Mask = UIConfigUtility.CreateMask(configs);
            }
            public UIDotsElementAttribute(string identifier,string renderBoxHandlerer, params byte[] configs) {
                Identifier = identifier;
                RenderBoxCounter = renderBoxHandlerer;
                Mask = UIConfigUtility.CreateMask(configs);
            }
        } */
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