using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons;
using Reactics.Core;
using Reactics.Core.AssetDefinitions;
using Unity.Burst;
using UnityEditor;

[assembly: SearchableAssembly]

namespace Reactics.Editor {
    public static class EffectDataProvider {
        [MenuItem("Assets/Create/Behaviours/Effect")]
        public static void CreateAsset() {
            BehaviourGraphModel.CreateInstance<BehaviourGraphModel, EffectDelegate>("Effect");
        }



    }
}