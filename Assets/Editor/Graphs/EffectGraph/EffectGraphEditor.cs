using System;
using System.IO;
using System.Linq;
using Reactics.Battle;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {


    public class EffectGraphEditor : ObjectGraphEditor<EffectAsset> {

        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceId, int line) => OnOpen<EffectGraphEditor>(instanceId, line, (asset) => $"Effect Graph ({asset.name})");
        protected override string SaveFileInPanelTitle => "Save Effect As...";

        protected override string SaveFileInPanelDefaultName => "EffectAsset";

        protected override string SaveFileInPanelPath => "Assets/ResourceData/Effects";

        public EffectGraphEditor() : base(new EffectGraphModule()) { }

    }
}