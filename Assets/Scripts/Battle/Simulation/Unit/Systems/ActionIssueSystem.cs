using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Rendering;
using Reactics.Battle.Map;

public class ActionIssueSystem : SystemBase
{
    private EntityCommandBufferSystem entityCommandBufferSystem;
    private EntityArchetype archetype;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        archetype = EntityManager.CreateArchetype(new ComponentType[] {
            typeof(Projectile),
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MapElement)
        });
    }
    protected override void OnUpdate()
    {
        var localArchetype = archetype;
        var ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        //TODO: Figure out how to make this read only. Figure out whyt hte fuck it runs immediately.
        var mapBodyDataFromEntity = GetComponentDataFromEntity<MapBody>();
        var mapElementDataFromEntity = GetComponentDataFromEntity<MapElement>();
        //hello temp map size
        var mapTileSize = 1f;
        //Only run if the unit isn't actively trying to reach its destination
        Entities.WithNone<FindingPathInfo/*,MapBodyTranslationStep*/>().ForEach((int entityInQueryIndex, Entity entity, ref Projectile proj, ref ActionMeterData meter, in Translation translation, in UnitStatData unitData,in HealthPointData healthPointData, in MagicPointData magicPointData) =>
        {
            //Get rid of the component immediately.//what if we skip makingentities and go straight to applying the dynamic buffer?
            ecb.RemoveComponent<Projectile>(entityInQueryIndex, entity);
            //if in range use move
            //aoe should get calculated here, methinks.
            //then it can be set in the buffer...
            if (proj.hasTargetPoint && mapBodyDataFromEntity.Exists(entity))
            {
                //if ()
            }
            else if (mapBodyDataFromEntity.Exists(proj.targetUnit) && mapBodyDataFromEntity.Exists(entity))
            {
                Point currentPoint = mapBodyDataFromEntity[entity].point;
                Point targetPoint = mapBodyDataFromEntity[proj.targetUnit].point;
                if (currentPoint.InRange(targetPoint, proj.effect.range))
                {
                    meter.Current -= proj.effect.cost;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            //This successfully went through so here we create the entity to hold the action to perform on the target.
            Entity projEntity = ecb.CreateEntity(entityInQueryIndex, localArchetype);
            ecb.SetComponent(entityInQueryIndex, projEntity, new Translation
            {
                Value = translation.Value
            });

            //Get a reference to the map for collision state stuff
            if (mapElementDataFromEntity.Exists(entity))
            {
                Entity mapEntity = mapElementDataFromEntity[entity].value;

                ecb.SetComponent(entityInQueryIndex, projEntity, new MapElement
                {
                    value = mapEntity
                });
            }
            Projectile newProj = proj;
            newProj.originPoint = mapBodyDataFromEntity[entity].point;
            newProj.originPosition = new float3(mapBodyDataFromEntity[entity].point.x * mapTileSize + mapTileSize / 2, 0,
                                            mapBodyDataFromEntity[entity].point.y * mapTileSize + mapTileSize / 2);

            newProj.targetPoint = mapBodyDataFromEntity[proj.targetUnit].point;
            newProj.lastPiercePoint = newProj.originPoint;

            //This is where we take stats into account for damage/healing.
            if (newProj.effect.physicalDmg > 0)
                newProj.effect.physicalDmg += unitData.Strength;
            if (newProj.effect.magicDmg > 0)
                newProj.effect.magicDmg += unitData.Magic;
            if (newProj.effect.healing > 0)
                newProj.effect.healing += unitData.Magic; //this is gonna be a solid maybe. for now we're throwing it in b/c whatever.

            //projectile. this is wher we'd like. edit projectile fields or something isn't it? no... not really. just the targets, maybe.
            ecb.SetComponent(entityInQueryIndex, projEntity, newProj);
        }).Schedule();
    }
}