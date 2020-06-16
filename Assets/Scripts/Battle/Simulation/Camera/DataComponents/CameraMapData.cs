using Unity.Entities;

//These could all just be properties in the movement data struct but for now I did it like this for w/e reason
[GenerateAuthoringComponent]
public struct CameraMapData : IComponentData
{
    //width, length, tilesize (all unnecessary most likely.)
    public float tileSize;
    public float mapWidth;
    public float mapLength;
}