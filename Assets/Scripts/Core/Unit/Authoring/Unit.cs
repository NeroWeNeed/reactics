using System.Collections;
using System.Collections.Generic;
using Reactics.Core.Battle;
using Reactics.Core.Map;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Core.Unit.Authoring {


    public class Unit : MonoBehaviour, IConvertGameObjectToEntity {
        [SerializeField]
        public UnitAsset unit;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            /*dstManager.AddComponentData(entity, new FindingPathInfo
{
    destination = new Point(10, 4),
    speed = 8,
    maxElevationDifference = 1
});*/
            dstManager.AddComponentData(entity, new ActionMeterData
            {
                /*rechargeRate = 10f,
                chargeable = true,
                charge = 100f*/
            });
            dstManager.AddComponentData(entity, new UnitStatData
            {

            });
            //dstManager.AddComponentData(entity, unit.CreateComponent()); 
            //dstManager.AddComponentData(entity, new MoveTilesTag());
            dstManager.AddBuffer<HighlightTile>(entity);
            dstManager.AddBuffer<Reactics.Core.Effects.EffectBuffer>(entity);
        }
    }

}
