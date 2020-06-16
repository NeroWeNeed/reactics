using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Reactics.Battle;

namespace Reactics.Battle
{

    /// <summary>
    /// Group responsible for all simulation. Should be updated at a reliable, constant rate for all users.
    /// </summary>
    //[UpdateAfter(typeof(PlayerInputProcessingSystemGroup))]
    //[UpdateInGroup(typeof(BattleSystemGroup))]FIX LATER

    public class BattleSimulationSystemGroup : ComponentSystemGroup
    {
        public const int SIMULATION_FRAME_RATE = 30;

        public const float SIMULATION_RATE = 1f / SIMULATION_FRAME_RATE;
        protected override void OnCreate()
        {
            base.OnCreate();
            FixedRateUtils.EnableFixedRateSimple(this,SIMULATION_RATE);


        }

    }
    /* 
        public class BattleSimulationSystemGroup : ComponentSystemGroup
        {
            public const int SIMULATION_FRAME_RATE = 30;

            public const double SIMULATION_RATE = 1.0 / SIMULATION_FRAME_RATE;

            public const int MAX_HISTORY = 16;

            private double elapsedTime;

            private EntityManagerDiffer differ;

            private Queue<EntityChanges> changes;

            public ulong Frame { get; private set; }

            public uint SubFrame { get; private set; }

            public override void SortSystemUpdateList()
            {
                // Extract list of systems to sort (excluding built-in systems that are inserted at fixed points)

                var toSort = new List<ComponentSystemBase>(m_systemsToUpdate.Count);
                BattleSimulationEntityCommandBufferSystem ecbSys = null;
                foreach (var s in m_systemsToUpdate)
                {
                    if (s is BattleSimulationEntityCommandBufferSystem system)
                    {
                        ecbSys = system;
                    }
                    else
                    {
                        toSort.Add(s);
                    }
                }
                m_systemsToUpdate = toSort;
                base.SortSystemUpdateList();

                var finalSystemList = new List<ComponentSystemBase>(toSort.Count);
                foreach (var s in m_systemsToUpdate)
                    finalSystemList.Add(s);
                if (ecbSys != null)
                    finalSystemList.Add(ecbSys);

                m_systemsToUpdate = finalSystemList;
            }

            protected override void OnCreate()
            {
                differ = new EntityManagerDiffer(EntityManager, Allocator.Persistent,
                new EntityQueryDesc
                {
                    Any = new ComponentType[] { typeof(GameState) },
                    All = new ComponentType[] { typeof(EntityGuid) }
                }
                );
                changes = new Queue<EntityChanges>();

            }
            protected override void OnUpdate()
            {
                elapsedTime += Time.DeltaTime;
                if (elapsedTime >= SIMULATION_RATE)
                {
                    while (elapsedTime >= SIMULATION_RATE)
                    {
                        base.OnUpdate();
                        elapsedTime -= SIMULATION_RATE;
                        changes.Enqueue(differ.GetChanges(EntityManagerDifferOptions.IncludeReverseChangeSet, Allocator.Persistent));
                        if (changes.Count > MAX_HISTORY)
                        {
                            changes.Dequeue().Dispose();
                        }
                        Frame++;
                    }
                    SubFrame = 0;
                }
                else
                {
                    SubFrame++;
                }

            }
            protected override void OnDestroy()
            {
                differ.Dispose();
                while (changes.Count > 0)
                    changes.Dequeue().Dispose();


            }



        }
     */

    [UpdateInGroup(typeof(BattleSimulationSystemGroup))]
    public class BattleSimulationEntityCommandBufferSystem : EntityCommandBufferSystem
    {

    }
}