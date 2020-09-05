using System;
using System.IO;
using System.Linq;
using Reactics.Core.Battle;
using Reactics.Core.Effects;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {


    public class EffectGraphEditor : ObjectGraphEditor<EffectGraphAsset> {
        public static string CreateTitle(EffectGraphAsset asset) => $"Effect Graph ({asset.name})";

        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceId, int line) => OnOpen<EffectGraphEditor>(instanceId, line, CreateTitle);
        protected override string SaveFileInPanelTitle => "Save Effect As...";

        protected override string SaveFileInPanelDefaultName => "EffectAsset";

        protected override string SaveFileInPanelPath => "Assets/ResourceData/Effects";

        public EffectGraphEditor() : base(new EffectGraphModule()) { }

    }
}