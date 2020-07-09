using UnityEngine;

namespace Reactics.Editor.Graph
{
    public class EffectGraphModelEditor : BaseObjectGraphModelEditor
    {
        private ObjectGraphModel model = ScriptableObject.CreateInstance<ObjectGraphModel>();
        public override ObjectGraphModel GetModel(ObjectGraphNode node) => model;
    }
}