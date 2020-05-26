using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;

public class ConvertCameraSystem : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        /*EntityQuery query = EntityManager.CreateEntityQuery(typeof(CameraMovementData));
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);*/
        EntityQuery query = dstManager.CreateEntityQuery(typeof(MapData));
        var mapData = query.GetSingleton<MapData>();
        
        float mapTileSize = 1f;
        float cameraOffsetValue = 5f;
        float3 startingCameraLookAtPoint = new float3(mapData.Length * mapTileSize * 0.5f, 0, mapData.Width * mapTileSize * 0.5f);
        float3 startingCameraPosition = startingCameraLookAtPoint + math.normalize(new float3(1,1,0)) * cameraOffsetValue; //temp values sorryyyyy
        float2 startingCameraTeleportPoint = new float2(startingCameraLookAtPoint.x, startingCameraLookAtPoint.z);
        dstManager.AddComponentData(entity, new CopyTransformToGameObject());
        dstManager.AddComponentData(entity, new CameraMovementData {
                speed = 5f,
                offsetValue = cameraOffsetValue,
                cameraLookAtPoint = startingCameraLookAtPoint,
                zoomMagnitude = 1f,
                lowerZoomLimit = 0.1f,
                upperZoomLimit = 2.0f
            });
            dstManager.SetComponentData(entity, new Translation {
                Value = startingCameraPosition
            });
            dstManager.AddComponentData(entity, new CameraMapData{
                tileSize = mapTileSize, 
                mapLength = mapTileSize * mapData.Length,
                mapWidth = mapTileSize * mapData.Width
            });
            dstManager.AddComponentData(entity, new CameraRotationData{
                speed = 5f,
                horizontalAngles = 8,
                verticalAngles = 8
            });
            dstManager.AddComponentData(entity, new ControlSchemeData{
                //currentControlScheme = ControlSchemes.Gamepad
            });

        //Set cursor component data
        //TODO: Actually render a cursor rhombus guy.
        //var highlightEntity = dstManager.CreateEntity(typeof(HighlightTile), typeof(InitializeTag));
        var cursor = dstManager.CreateEntity(typeof(CursorData), typeof(LocalToWorld), typeof(ControlSchemeData), typeof(Translation));//GameObjectConversionUtility.ConvertGameObjectHierarchy(cursorGO, World.Active);//EntityManager.CreateEntity(typeof(CursorData), typeof(LocalToWorld), typeof(ControlSchemeData), typeof(InitializeTag), typeof(Translation));
        dstManager.SetComponentData(cursor, new CursorData
        {
            // = highlightEntity,
            cameraEntity = entity, //do this but better somehow maybe idk maybe its fine since theres only one camera
            rayMagnitude = 10000f, //arbitrary long number so it always collides for
            tileSize = mapTileSize
        });
        DynamicBuffer<HighlightTile> highlights = dstManager.AddBuffer<HighlightTile>(cursor);
        highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });
        dstManager.SetName(cursor, "Cursor");
    }
}
