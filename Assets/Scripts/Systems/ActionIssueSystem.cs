using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Rendering;

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
            typeof(LocalToWorld)
            //needs rendermesh and localtoworld...
        });
    }
    protected override void OnUpdate() 
    {
        var localArchetype = archetype;
        var ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        //TODO: Figure out how to make this read only. Figure out whyt hte fuck it runs immediately.
        var mapBodyDataFromEntity = GetComponentDataFromEntity<MapBody>();
        //hello temp map size
        var mapTileSize = 1f;
        //Only run if the unit isn't actively trying to reach its destination
        Entities.WithNone<MapBodyTranslation,MapBodyTranslationStep>().ForEach((int entityInQueryIndex, Entity entity, ref Projectile proj, ref ActionMeter meter, in Translation translation, in UnitData unitData) => 
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
                    meter.charge -= proj.effect.cost;
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
            // var createdActionEntity = ecb.CreateEntity(entityInQueryIndex, localArchetype);
            Entity projEntity = ecb.CreateEntity(entityInQueryIndex, localArchetype);
            ecb.SetComponent(entityInQueryIndex, projEntity, new Translation {
                Value = translation.Value
            });
            Projectile newProj = proj;
            newProj.originPoint = mapBodyDataFromEntity[entity].point;
            newProj.originPosition = new float3(mapBodyDataFromEntity[entity].point.x * mapTileSize + mapTileSize/2, 0, 
                                            mapBodyDataFromEntity[entity].point.y * mapTileSize + mapTileSize/2);

            newProj.targetPoint = mapBodyDataFromEntity[proj.targetUnit].point;
            newProj.lastPiercePoint = newProj.originPoint;

            //This is where we take stats into account for damage/healing.
            if (newProj.effect.physicalDmg > 0)
                newProj.effect.physicalDmg += unitData.Strength();
            if (newProj.effect.magicDmg > 0)
                newProj.effect.magicDmg += unitData.Magic();
            if (newProj.effect.healing > 0)
                newProj.effect.healing += unitData.Magic(); //this is gonna be a solid maybe. for now we're throwing it in b/c whatever.

            //projectile. this is wher we'd like. edit projectile fields or something isn't it? no... not really. just the targets, maybe.
            ecb.SetComponent(entityInQueryIndex, projEntity, newProj);//new Projectile { //make 3rd var here just newProj, after we do the modifications to it.
                /*arc = false, SET THESE WHEN WE ADD THE PROJECTILE TO THE ENTITY THAT'S SHOOTING IT!
                pierce = true,
                friendlyFire = true,
                maxPierce = 1,
                instantTravel = false,
                speed = 1,
                maxArcValue = 3,*/
                //originPoint = mapBodyDataFromEntity[entity].point,
                //originPosition = translation.Value, doesn't work atm...
                //originPosition = new float3(mapBodyDataFromEntity[entity].point.x * mapTileSize + mapTileSize/2, 0, 
                //                            mapBodyDataFromEntity[entity].point.y * mapTileSize + mapTileSize/2),
                //targetPoint = mapBodyDataFromEntity[proj.targetUnit].point,
                //lastPiercePoint = mapBodyDataFromEntity[entity].point,
                //effect = proj.effect,
                //aoeEffect = proj.aoeEffect
            //});
                //targetPosition calculated in SetTargetPosition system.
            /*ecb.SetComponent(entityInQueryIndex, projectile, new RenderMesh {

            });*/

            /*ecb.AddComponent(entityInQueryIndex, entity, new InstantEffect {
                damage = action.magnitude,
                targetedUnit = action.targetedUnit
            });*/
            if (proj.effect.aoeRange > 0)
            {
                /*DynamicBuffer<PointsBuffer> buffer = ecb.SetBuffer<PointsBuffer>(entityInQueryIndex, createdActionEntity);
                //fill in aoe values
                buffer.Add(new PointsBuffer {point = new Point(1,2)});
                //ok this works apparently lul*/
            }
        }).Schedule();
    }
}
/*

    public bool arc;
    public bool pierce;
    public bool friendlyFire;
    public ushort maxPierce;
    public ushort currentPierce;
    public bool instantTravel;
    public ushort speed;
    public Point originPoint;
    public Point targetPoint;
*/
