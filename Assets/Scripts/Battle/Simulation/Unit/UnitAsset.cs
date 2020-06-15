using System.Collections.Generic;
using System.Threading.Tasks;
using Reactics.Battle.Unit;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Reactics.Battle
{
    [CreateAssetMenu(fileName = "UnitAsset", menuName = "Reactics/Unit", order = 0)]
    public class UnitAsset : ScriptableObject, IUnit
    {

        [SerializeField]
        private string identifier;
        public string Identifier => identifier;
        [SerializeField]
        private ushort healthPoints;
        public ushort HealthPoints => healthPoints;
        [SerializeField]
        private ushort magicPoints;
        public ushort MagicPoints => magicPoints;
        [SerializeField]
        private ushort defense;
        public ushort Defense => defense;
        [SerializeField]
        private ushort resistance;
        public ushort Resistance => resistance;
        [SerializeField]
        private ushort strength;
        public ushort Strength => strength;
        [SerializeField]
        private ushort magic;
        public ushort Magic => magic;
        [SerializeField]
        private ushort speed;
        public ushort Speed => speed;
        [SerializeField]
        private ushort movement;
        public ushort Movement => movement;

        [SerializeField]
        private Proficiency[] proficiencies;

        public Proficiency[] Proficiencies { get => proficiencies; }

        [SerializeField]
        public AssetReferenceAction[] personalActions;
        public async void Convert(Entity entity, EntityManager dstManager)
        {
            dstManager.AddComponentData(entity, new HealthPointData(this));
            dstManager.AddComponentData(entity, new MagicPointData(this));
            dstManager.AddComponentData(entity, new UnitStats(this));
            dstManager.AddComponentData(entity, ActionMeterData.Create());
            dstManager.AddSharedComponentData(entity, new UnitAssetReference(this));
            var tasks = new Task<ActionAsset>[personalActions.Length];
            for (int i = 0; i < personalActions.Length; i++)
                tasks[i] = personalActions[i].LoadAssetAsync().Task;
                
            dstManager.AddSharedComponentData(entity, new ActionList { value = new List<ActionAsset>(await Task.WhenAll(tasks)) });
        }
    }
}