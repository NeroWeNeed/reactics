
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.Tests {
    public abstract class ECSTestFixture {
        protected World World { get; private set; }
        protected InitializationSystemGroup InitializationSystemGroup { get; private set; }
        protected SimulationSystemGroup SimulationSystemGroup { get; private set; }
        protected PresentationSystemGroup PresentationSystemGroup { get; private set; }
        protected EntityManager EntityManager { get; private set; }
        [SetUp]
        public void SetUp() {
            World = new World("Test World");
            EntityManager = World.EntityManager;
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default, requireExecuteAlways: false);
            //systems.Add(typeof(ConstantDeltaTimeSystem));
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World, systems);
            InitializationSystemGroup = World.GetExistingSystem<InitializationSystemGroup>();
            SimulationSystemGroup = World.GetExistingSystem<SimulationSystemGroup>();
            PresentationSystemGroup = World.GetExistingSystem<PresentationSystemGroup>();
        }
        [TearDown]
        public void TearDown() {
            EntityManager.DestroyEntity(EntityManager.UniversalQuery);
            World.Dispose();
        }
    }
}