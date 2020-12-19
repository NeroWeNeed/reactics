using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
namespace Reactics.Core.Camera.Authoring {
    public class Cursor : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            EntityQuery cameraQuery = dstManager.CreateEntityQuery(typeof(CameraMovementData));
            EntityQuery mapQuery = dstManager.CreateEntityQuery(typeof(MapRenderInfo));
            var mapData = mapQuery.GetSingleton<MapRenderInfo>();
            var cameraArray = cameraQuery.ToEntityArray(Allocator.TempJob); //there should only be one... maybe two actually. for right now this is fine.

            dstManager.AddComponentData(entity, new CursorData
            {
                cameraEntity = cameraArray[0], //do this but better somehow maybe idk maybe its fine since theres only one camera
                rayMagnitude = 10000f, //arbitrary long number so it always collides for now
                tileSize = mapData.tileSize
            });
            dstManager.AddComponentData(entity, new ControlSchemeData());
            DynamicBuffer<HighlightTile> highlights = dstManager.AddBuffer<HighlightTile>(entity);
            highlights.Add(new HighlightTile { point = new Point(0, 0), state = (ushort)MapLayer.Hover });
#if UNITY_EDITOR
            dstManager.SetName(entity, "Cursor");
#endif
            cameraArray.Dispose();
            //TODO: fix rhombus
        }
    }
}