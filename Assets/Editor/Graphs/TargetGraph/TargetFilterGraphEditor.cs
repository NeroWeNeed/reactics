using Reactics.Core.Battle;
using Reactics.Core.Effects;
using UnityEditor.Callbacks;

namespace Reactics.Core.Editor.Graph {


    public class TargetFilterGraphEditor : ObjectGraphEditor<TargetFilterAsset> {

        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceId, int line) => OnOpen<TargetFilterGraphEditor>(instanceId, line, (asset) => $"Target Filter Graph ({asset.name})");
        protected override string SaveFileInPanelTitle => "Save Target Filter As...";

        protected override string SaveFileInPanelDefaultName => "TargetFilter";

        protected override string SaveFileInPanelPath => "Assets/ResourceData/TargetFilters";

        public TargetFilterGraphEditor() : base(new TargetFilterGraphModule()) { }

    }
}