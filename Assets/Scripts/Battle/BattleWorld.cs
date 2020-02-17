using Unity.Entities;

namespace Reactics.Battle {

    public class BattleWorld : World
    {

        private Map map;
        public BattleWorld(Map map) : base($"Battle World ({map.Name})")
        {
        }


    }
}