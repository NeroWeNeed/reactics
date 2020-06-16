using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;
using Reactics.Battle.Map;

//just remembered this was having issues when I put it in battle simulation system group but I'm literally actively committing right now so I'll fix it on next commit I guess
//[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
public class DeleteProjectilesSystem : SystemBase
{
    private EntityCommandBufferSystem entityCommandBufferSystem;
    protected override void OnCreate()
    {
        //entityCommandBufferSystem = World.GetExistingSystem<BattleSimulationEntityCommandBufferSystem>();
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        var ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        Entities.ForEach((int entityInQueryIndex, Entity entity, in Projectile proj) => 
        {
            //This has to be a meme right? idk.
            if (proj.delete)
            {
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }
        }).Schedule();
    }
}

//[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
[UpdateAfter(typeof(DeleteProjectilesSystem))]
public class SetTargetPosition : SystemBase
{
    protected override void OnUpdate()
    {
        //TODO: make this apply once.
        //Have some kind of system in place for whter or not it like... targets a unit or a tile.
        //ofc if not homign and targeting unit that's the same as targetnig a tile but... still.
        float mapTileSize = 1f;
        //TODO: FIgure out how to make this read only
        ComponentDataFromEntity<Translation> translationDataFromEntity = GetComponentDataFromEntity<Translation>(false);
        Entities.WithNone<MapBody>().ForEach((ref Projectile proj) => 
        {
            if (proj.effect.homing && translationDataFromEntity.Exists(proj.targetUnit))
            {
                //If it's homing, then it's locked on to an entity. So we have to grab stuff from said entity.
                proj.targetPosition = translationDataFromEntity[proj.targetUnit].Value;
            }
            else
            {
                proj.targetPosition = new float3(proj.targetPoint.x * mapTileSize + mapTileSize/2, 0, proj.targetPoint.y * mapTileSize + mapTileSize/2); //this is right. huh.
            }
        }).Schedule();
    }
}

//[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
[UpdateAfter(typeof(SetTargetPosition))]
public class RotateTowardTargetSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        Entities.ForEach((ref Rotation rot, in Translation trans, in Projectile proj) => 
        {
            float3 direction = proj.targetPosition - trans.Value;
            rot.Value = quaternion.LookRotation(math.normalize(direction), Vector3.up);
        }).WithChangeFilter<Projectile>().Schedule();
    }
}

//[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
[UpdateAfter(typeof(RotateTowardTargetSystem))]
public class ArcForwardSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        float deltaTime = Time.DeltaTime;
        Entities.WithNone<MapBody>().ForEach((ref Translation trans, ref Projectile proj, in Rotation rot) => 
        {
            float3 forward = math.mul(rot.Value, new float3(0, 0, proj.effect.speed * deltaTime));
            if (!proj.effect.arc)
            {
                //forward.y = 0;
            }
            else
            {
                //do something...
            }
            if (proj.effect.speed > 0)
            {
                proj.travelTime += deltaTime * proj.effect.speed;
                trans.Value = Vector3.Lerp(proj.originPosition, proj.targetPosition, proj.travelTime);

                //TODO: Parabola stuff instead of this trash
                //also todo: make this trash work lol
                float deltaTimeForCalc = 0.0f;
                if (proj.travelTime > 1.0f)
                {
                    proj.delete = true;
                }
                else if (proj.travelTime > 0.5f)
                    deltaTimeForCalc = math.abs(1 - deltaTime);
                else if (proj.travelTime <= 1.0f)
                    deltaTimeForCalc = deltaTime;
                trans.Value.y = proj.effect.maxArcValue * deltaTimeForCalc;
            }
            else
            {
                trans.Value = proj.targetPosition;
                proj.delete = true;
            }
        }).Schedule();
    }
}

//TODO: Make this stop running all the time (to be honest, not sure why it does anyway... weird.)
//or maybe before it..? idk.
//[UpdateInGroup(typeof(BattleSimulationSystemGroup))]
[UpdateAfter(typeof(ArcForwardSystem))]
public class DetectCollisionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //Oh? Map tile siz already?
        float mapTileSize = 1f;
        //var mapBodyEntities = mapBodiesQuery.ToEntityArray(Allocator.TempJob);
        BufferFromEntity<EffectBuffer> effectBufferFromEntity = GetBufferFromEntity<EffectBuffer>(false);
        BufferFromEntity<MapTileEffect> tileEffectBufferFromEntity = GetBufferFromEntity<MapTileEffect>(false);
        Entity mapData = GetSingletonEntity<MapData>();
        //TODO: Figure out how to makek this damn thing readonly without getting an error, maybe.
        ComponentDataFromEntity<MapBody> mapBodyDataFromEntity = GetComponentDataFromEntity<MapBody>(false);
        Entities.WithNone<MapBody>().ForEach((ref Translation trans, ref Projectile proj, in MapElement mapElement) => 
        {
            //Arc or not, getting here means the check found nothing between us and the target (nothing inaccessible, at least...?)
            //So, that means we can assume any collisions will be with map bodies. Unless we add like. Terrain stuff. Which is plausible.
            //So in that case, we'll have to come back to this and add more stuff to it. But for now... only map bodies can be hit.
            //Make current point a part of the data component and calc it somewhere else if maptilesize is annoying to get...
            Point currentPoint = new Point((ushort)((trans.Value.x) / mapTileSize), (ushort)((trans.Value.z) / mapTileSize));
            /*
            if (!proj.effect.arc)
            {

            }
            */

            //Note: currently pretends they don't arc.
            if (!currentPoint.ComparePoints(proj.originPoint) && proj.currentPierce < proj.effect.maxPierce)
            {
                if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(currentPoint, out Entity collidableEntity))
                {
                    MapBody mapBody = mapBodyDataFromEntity[collidableEntity];
                    if (currentPoint.ComparePoints(mapBody.point) && !currentPoint.ComparePoints(proj.lastPiercePoint))
                    {
                        DynamicBuffer<EffectBuffer> effects = effectBufferFromEntity[collidableEntity];
                        effects.Add(new EffectBuffer {effect = proj.effect});
                        proj.currentPierce++;
                        proj.lastPiercePoint = currentPoint;

                        //uncomment to see an aoe effect appear on tile 5,5 (not really timed... but y'know)
                        //actually don't uncomment it's broken :) 
                        /*if (tileEffectBufferFromEntity.Exists(mapData))
                        {
                            DynamicBuffer<MapTileEffect> tileEffects = tileEffectBufferFromEntity[mapData];
                            effects.Add(new EffectBuffer {effect = proj.aoeEffect});
                            tileEffects.Add(new MapTileEffect {
                                point = new Point(5,5),
                                effect = proj.aoeEffect
                            });
                        }*/
                    }
                }
            }
            
            //In either case, if we're at our max pierce it's time to go bye bye.
            if (proj.currentPierce >= proj.effect.maxPierce)
                proj.delete = true;
        }).Schedule();   
    }
}