using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Reactics.Battle;
using Unity.Physics;
using Reactics.Battle.Map;

[UpdateInGroup(typeof(RenderingSystemGroup))]
[UpdateAfter(typeof(CursorFollowSystem))]
public class CursorHighlightSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        BufferFromEntity<HighlightTile> highlightTilesFromEntity = GetBufferFromEntity<HighlightTile>(false);
        Entities.ForEach((Entity entity, ref Translation trans, ref CursorData cursorData) => //remove ref trans later it doesn't need to be ref
        {
            if (highlightTilesFromEntity.Exists(entity))
            {
                DynamicBuffer<HighlightTile> highlightTiles = highlightTilesFromEntity[entity];
                Point pointInfo = new Point((ushort)((trans.Value.x) / cursorData.tileSize), (ushort)((trans.Value.z) / cursorData.tileSize));
                cursorData.currentHoverPoint = pointInfo;
                
                for (int i = 0; i < highlightTiles.Length; i++)
                {
                    if (highlightTiles[i].state == (ushort)MapLayer.Hover)
                    {
                        highlightTiles[i] = new HighlightTile { point = new Point((ushort)((trans.Value.x) / cursorData.tileSize), (ushort)((trans.Value.z) / cursorData.tileSize)), state = (ushort)MapLayer.Hover };
                        //Realistically there should only be one hover tile...
                        break;
                    }
                }
            }
        }).Run();
    }
}