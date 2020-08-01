using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.UI {
    public class UIMeshGenerationSystem : SystemBase {
        public Dictionary<Entity, MeshContext> contexts;
        protected override void OnCreate() {
            contexts = new Dictionary<Entity, MeshContext>();
        }
        protected override void OnUpdate() {

        }
    }
    public class MeshContext {
        public Mesh mesh;
        public Entity root;
        public Dictionary<Entity, int> entities;
    }
}