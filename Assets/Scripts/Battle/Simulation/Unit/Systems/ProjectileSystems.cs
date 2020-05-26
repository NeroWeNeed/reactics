using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;

public class DeleteProjectilesSystem : SystemBase
{
    private EntityCommandBufferSystem entityCommandBufferSystem;
    protected override void OnCreate()
    {
        //Change whenver we figure out where this is actually going...
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

[UpdateAfter(typeof(DeleteProjectilesSystem))]
public class SetTargetPosition : SystemBase
{
    protected override void OnUpdate()
    {
        //TODO: make this apply once.
        //Have some kind of system in place for whter or not it like... targets a unit or a tile.
        //ofc if not homign and targeting unit that's the same as targetnig a tile but... still.
        float mapTileSize = 1f;
        //TODO: FIgure out how to make this read only, dammit.
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

[UpdateAfter(typeof(SetTargetPosition))]
public class RotateTowardTargetSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        Entities.ForEach((ref Rotation rot, in Translation trans, in Projectile proj) => 
        {
            float3 direction = proj.targetPosition - trans.Value;
            rot.Value = quaternion.LookRotation(math.normalize(direction), Vector3.up);
        })/*.WithChangeFilter<Projectile>()*/.Schedule();
    }
}

[UpdateAfter(typeof(RotateTowardTargetSystem))]
public class ArcForwardSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        float deltaTime = Time.DeltaTime;
        Entities.WithNone<MapBody>().ForEach((ref Translation trans, ref Projectile proj, in Rotation rot) => 
        {
            /*float3 forward = math.mul(rot.Value, new float3(0, 0, proj.speed * deltaTime));
            if (!proj.arc)
            {
                //forward.y = 0;
            }
            else
            {
                //do something...
            }*/
            if (proj.effect.speed > 0)
            {
                proj.travelTime += deltaTime * proj.effect.speed;
                //float3 originPosition = new float3(proj.originPoint.x * mapTileSize + mapTileSize/2, 0, proj.originPoint.y * mapTileSize + mapTileSize/2);
                //float3 targetPosition = new float3(proj.targetPoint.x * mapTileSize, 0, proj.targetPoint.y * mapTileSize);
                //change it from lerp and use the camera movement data stuff instead of it gets fucky when they move around too much...
                //At the very least, the y value stuff we do for arcs won't really change, so that's nice~
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
            //we then take that and after we lerp trans.value we just set the y to what we want it to be ^w^
            //And of course, for the lerp calc, we'd want to make sure to zero the y.... that only makes sense~
        }).Schedule();
    }
}

//or maybe before it..? idk.
[UpdateAfter(typeof(ArcForwardSystem))]
public class DetectCollisionSystem : SystemBase
{
    private EntityQuery mapBodiesQuery;
    protected override void OnCreate()
    {
        //query all map bodies to see if we are colliding with one of them...
        mapBodiesQuery = GetEntityQuery(typeof(MapBody));//, 
           //ComponentType.ReadOnly<SomeType>());
    }

    //It's probably the case that this applies a bunch of itself to the map body as it passes through
    //So do we just... give it a dynamic buffer of like... something? to mark the things it's affected already?
    //Guess that'll just be a problem for the piercing ones that we can get to when we get to it... bleh.
    //If the trans.value calculations end up being annoying we can add a system to calculate the current point. and use that. not the worst idea.
    //what if instead of this... we set some value on the map body, right...? and *that* did stuff?
    //so what if we had like... all of the uhhh. 
    protected override void OnUpdate()
    {
        //Oh? Map tile siz already?
        float mapTileSize = 1f;
        var mapBodyEntities = mapBodiesQuery.ToEntityArray(Allocator.TempJob);
        BufferFromEntity<EffectBuffer> effectBufferFromEntity = GetBufferFromEntity<EffectBuffer>(false);
        BufferFromEntity<MapTileEffect> tileEffectBufferFromEntity = GetBufferFromEntity<MapTileEffect>(false);
        Entity mapData = GetSingletonEntity<MapData>();
        //TODO: Figure out how to makek this damn thing readonly without getting an error, maybe.
        ComponentDataFromEntity<MapBody> mapBodyDataFromEntity = GetComponentDataFromEntity<MapBody>(false);
        Entities.WithNone<MapBody>().ForEach((ref Translation trans, ref Projectile proj, in Rotation rot) => 
        {
            //Arc or not, getting here means the check found nothing between us and the target (nothing inaccessible, at least...?)
            //So, that means we can assume any collisions will be with map bodies. Unless we add like. Terrain stuff. Which is plausible.
            //So in that case, we'll have to come back to this and add more stuff to it. But for now... only map bodies can be hit.
            //Make current point a part of the data component and calc it somewhere else if maptilesize is annoying to get...
            Point currentPoint = new Point((ushort)((trans.Value.x) / mapTileSize), (ushort)((trans.Value.z) / mapTileSize));
            if (!proj.effect.arc)
            {
                //do actual logic stuff here regarding hitting stuff
                //this is where the stuff we commented earlier works. with the whole, 0.5 thing? y'know. that stuff.
            }

            //Ah so how... exactly.. do we read this? With an entity query, of course~
            //If this ends up being too slow we may have to... think of something else.
            //Like, for example, just having a reference to every map body on each projectile. Wouldn't that be a damn meme and a half.
            //foreach in entity query {
            //the only issue with this is that... well.. we haven't actually checked if they've collided.
            //this just adds the effect to literally... every map body that has a buffer. every time it runs. which is... bad.
            //isn't this entire thing horribly inefficient?
            if (!currentPoint.ComparePoints(proj.originPoint) && proj.currentPierce < proj.effect.maxPierce)
            {
                for (int i = 0; i < mapBodyEntities.Length; i++)
                {
                    if (effectBufferFromEntity.Exists(mapBodyEntities[i]) && mapBodyDataFromEntity.Exists(mapBodyEntities[i]))
                    {
                        MapBody mapBody = mapBodyDataFromEntity[mapBodyEntities[i]];
                        if (currentPoint.ComparePoints(mapBody.point) && !currentPoint.ComparePoints(proj.lastPiercePoint))
                        {
                            DynamicBuffer<EffectBuffer> effects = effectBufferFromEntity[mapBodyEntities[i]];
                            effects.Add(new EffectBuffer {effect = proj.effect});
                            proj.currentPierce++;
                            proj.lastPiercePoint = currentPoint;
                            if (tileEffectBufferFromEntity.Exists(mapData))
                            {
                                DynamicBuffer<MapTileEffect> tileEffects = tileEffectBufferFromEntity[mapData];
                                effects.Add(new EffectBuffer {effect = proj.aoeEffect});
                                tileEffects.Add(new MapTileEffect {
                                    point = new Point(5,5),
                                    effect = proj.aoeEffect
                                });
                            }   
                            break;
                        }
                    }
                }
            }
            
            //In either case, if we're at our max pierce it's time to go bye bye.
            if (proj.currentPierce >= proj.effect.maxPierce)
                proj.delete = true;
        }).WithDeallocateOnJobCompletion(mapBodyEntities).Schedule();   
    }
}

/*
 else if (rotData.rotating && rotData.rotationTime < 1) //still rotating to the target position
            {
                rotData.rotationTime += deltaTime * rotData.speed;
                trans.Value = Vector3.Lerp(rotData.lastPosition, rotData.targetPosition, rotData.rotationTime);
            }
            else
            {
                rotData.rotating = false;
            }
*/

/*
else if (rotData.rotating && rotData.rotationTime < 1) //still rotating to the target position
            {
                rotData.rotationTime += deltaTime * rotData.speed;
                trans.Value = Vector3.Lerp(rotData.lastPosition, rotData.targetPosition, rotData.rotationTime);
            }
            else
            {
                rotData.rotating = false;
            }
*/

//side note thsi is what he was talking about
//if we calc the position of the enmy on every frame then we can figure out what tile they're in (if they're past or before the 0.5 mark, treat them as in or out)
//by doing that, we can do a bfs based on where they are, and set our movement accordingly!