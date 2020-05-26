using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;

[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
public class EffectProcessingSystem : SystemBase
{
    private EntityQuery mapBodiesQuery;
    private EntityCommandBufferSystem entityCommandBufferSystem;
    private EntityArchetype archetype;

    protected override void OnCreate()
    {
        mapBodiesQuery = GetEntityQuery(typeof(MapBody));//, 
           //ComponentType.ReadOnly<SomeType>());
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        /*archetype = EntityManager.CreateArchetype(new ComponentType[] {
            typeof(InstantEffect),
            typeof(PointsBuffer)
        });*/
    }

    protected override void OnUpdate() 
    {
        var localArchetype = archetype;
        var ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        //var mapBodyEntities = mapBodiesQuery.ToEntityArray(Allocator.TempJob);
        BufferFromEntity<PointsBuffer> pointsFromEntity = GetBufferFromEntity<PointsBuffer>(false);
        BufferFromEntity<EffectBuffer> instantEffectsFromEntity = GetBufferFromEntity<EffectBuffer>(false);
        Entities.WithNone<WaitToProcessTag>().ForEach((int entityInQueryIndex, Entity entity, ref DynamicBuffer<EffectBuffer> effectBuffer, ref UnitData unitData) => 
        {
            //if we define a class like point that has funcs and stuff, then we can do stuff. like that.
            //the instant effects data component can have one of those in it that gets written to via the projectile.
            
            ///turn this into a projectile and we have stuff.
            //but then we have to do tile phsyics which means we're like. checking somes tuff.
            //y'know like "is there an entity here" and stuff.
            //also we have to like. move it. and do logic for moving it in a straight line, which we have! sort of. but that's cool that it exists!
            /*
            new process
            one component is attached to the uhh mapbody entity, it has stuff on it. 
            That component needs to latch on to the other one via its dynamic buffer. that makes this very simple.
            */
            //var z = effect.targetedUnit; //this is the entity who's dynamic buffer we have to do.
            if (effectBuffer.Length > 0)
            {
                int totalDmg = 0;
                int totalHealing = 0;

                for (int i = 0; i < effectBuffer.Length; i++)
                {
                    EffectBuffer effect = effectBuffer[i];
                    //Initial effect
                    if (effect.effect.currentFrames == 0)
                    {
                        //Physical damage calculation
                        totalDmg += effect.effect.physicalDmg - unitData.Defense() > 0 ? effect.effect.physicalDmg - unitData.Defense() : 0;
                        //Magic damage calculation
                        totalDmg += effect.effect.magicDmg - unitData.Resistance() > 0 ? effect.effect.magicDmg - unitData.Resistance() : 0;
                        //True damage calculation
                        totalDmg += effect.effect.trueDmg;
                        //Healing calculation
                        totalHealing += effect.effect.healing;

                        //Buff and Debuff calculations
                        //KEEP THIS IN MIND: WE COULD ADD MODIFIERS TO THE UNIT DATA AND ADD/SUBTRACT FROM THEM, IF WE WERE SO INCLINED.
                        //fuck ushorts btw
                        //this doesn't work, because it cuts out some. so we should probably go with the modifiers, right?
                        //but that would have the same issue. maybe saving a "current" value on the component...?
                        //ok so what actually happens is we keep what we have but we also have calc functions that return ushorts
                        //the things we have now become ints and we call the calc functions that return ushorts that give us the calc values
                        unitData.defense += effect.effect.defenseModifier;
                        unitData.resistance += effect.effect.resistanceModifier;
                        unitData.strength += effect.effect.strengthModifier;
                        unitData.magic += effect.effect.magicModifier;
                        unitData.speed += effect.effect.speedModifier;
                        unitData.movement += effect.effect.movementModifier;
                    }
                    else if (effect.effect.currentFrames / BattleSimulationSystemGroup.SIMULATION_FRAME_RATE >= effect.effect.secondsPerTick)
                    {
                        effect.effect.currentFrames -= (ushort)(BattleSimulationSystemGroup.SIMULATION_FRAME_RATE * effect.effect.secondsPerTick);
                        //Physical damage calculation
                        totalDmg += effect.effect.physicalDOT - unitData.Defense() > 0 ? effect.effect.physicalDOT - unitData.Defense() : 0;
                        //Magic damage calculation
                        totalDmg += effect.effect.magicDOT - unitData.Resistance() > 0 ? effect.effect.magicDOT - unitData.Resistance() : 0;
                        //True damage calculation
                        totalDmg += effect.effect.trueDOT;
                        //Healing calculation
                        totalHealing += effect.effect.healingOverTime;

                        //The... graphics. That's what this is really all about.
                    }

                    //Time passes
                    if (effect.effect.totalFrames <= 1)
                    {
                        //Remove any effects
                        effectBuffer.RemoveAt(i);
                        unitData.defense -= effect.effect.defenseModifier;
                        unitData.resistance -= effect.effect.resistanceModifier;
                        unitData.strength -= effect.effect.strengthModifier;
                        unitData.magic -= effect.effect.magicModifier;
                        unitData.speed -= effect.effect.speedModifier;
                        unitData.movement -= effect.effect.movementModifier;
                    }
                    else
                    {
                        //Over time effects
                        effect.effect.totalFrames--;
                        effect.effect.currentFrames++;
                        effectBuffer[i] = effect;
                    }
                }

                int newHealthPoints = unitData.healthPoints + totalHealing - totalDmg;
                if (newHealthPoints < 0)
                {
                    unitData.healthPoints = 0;
                    effectBuffer.Clear();
                    //death flag/component goes here...
                }
                else if (newHealthPoints > unitData.maxHealthPoints)
                {
                    unitData.healthPoints = unitData.maxHealthPoints;
                }
                else
                {
                    unitData.healthPoints = (ushort)newHealthPoints;
                }
            }
        })./*WithDeallocateOnJobCompletion(mapBodyEntities).*/Run();
    }
}

public class TileEffectProcessingSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        Entity mapData = GetSingletonEntity<MapData>();
        BufferFromEntity<MapTileEffect> tileEffectBufferFromEntity = GetBufferFromEntity<MapTileEffect>(false);
        Entities.ForEach((int entityInQueryIndex, Entity entity, ref DynamicBuffer<EffectBuffer> effectBuffer, ref UnitData unitData) => 
        {

        }).Schedule();
    }
}