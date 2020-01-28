using System;
namespace Reactics.Battle
{

    [Serializable]
    public class Tile
    {
        
        readonly int elevation;
        public int Elevation { get => elevation; }

        readonly int spawnGroup = -1;
        public int SpawnGroup { get => SpawnGroup; }
    }


}