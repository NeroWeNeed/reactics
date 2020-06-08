using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;
using Reactics.Battle.Map;

[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
public class EffectProcessingSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        Entity mapData = GetSingletonEntity<MapData>();
        BufferFromEntity<MapTileEffect> tileEffectBufferFromEntity = GetBufferFromEntity<MapTileEffect>(false);
        Entities/*.WithNone<WaitToProcessTag>()*/.ForEach((int entityInQueryIndex, Entity entity, ref DynamicBuffer<EffectBuffer> effectBuffer, ref UnitData unitData, in MapBody mapBody) => 
        {
            //First get the buffer from the map.
            DynamicBuffer<MapTileEffect> mapEffects = tileEffectBufferFromEntity[mapData];

            //Then loop through and find the tile the current map body is on.
            /*
            don't look at this
            for (var i = 0; i < mapEffects.Length; i++)
            {
                if (mapBody.point.ComparePoints(mapEffects[i].point))
                {
                    //Add the effect to the unit's effect buffer.
                    effectBuffer.Add(new EffectBuffer {effect = mapEffects[i].effect});
                }
            }*/

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

                        //Simple stat buff/debuff calculations
                        unitData.defense += effect.effect.defenseModifier;
                        unitData.resistance += effect.effect.resistanceModifier;
                        unitData.strength += effect.effect.strengthModifier;
                        unitData.magic += effect.effect.magicModifier;
                        unitData.speed += effect.effect.speedModifier;
                        unitData.movement += effect.effect.movementModifier;

                        //Movement (teleports, for example) calculations?
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

                        //An animation should probably happen if this is going on right? Maybe..
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
                        //i--; Probably necessary? Don't remember.
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
        }).Schedule();
    }
}
// don't look at this
[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
[UpdateAfter(typeof(EffectProcessingSystem))]
public class TileEffectProcessingSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        Entity mapData = GetSingletonEntity<MapData>();
        BufferFromEntity<MapTileEffect> tileEffectBufferFromEntity = GetBufferFromEntity<MapTileEffect>(false);
        Entities.ForEach((ref DynamicBuffer<MapTileEffect> effectBuffer) => 
        {
            //All this does is tick down map effects.
            for (var i = 0; i < effectBuffer.Length; i++)
            {
                if (effectBuffer[i].effect.totalFrames <= 1)
                {
                    //Remove any effects
                    effectBuffer.RemoveAt(i);
                    //i--; Probably necessary? Don't remember.
                }
            }
        }).Schedule();
    }
}