using System.Collections.Generic;
using System.Threading.Tasks;
using Reactics.Battle.Unit;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace Reactics.Battle {
    [CreateAssetMenu(fileName = "UnitAsset", menuName = "Reactics/Unit", order = 0)]
    public class UnitAsset : ScriptableObject, IUnit {
        [SerializeField]
        [LocalizationTableName("UnitInfo")]
        private EmbeddedLocalizedAsset<UnitInfoAsset> info;

        public EmbeddedLocalizedAsset<UnitInfoAsset> Info { get => info; }

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

        public AssetReference test;

        [SerializeField]
        public AssetReference<ActionAsset>[] personalActions;
        public async void Convert(Entity entity, EntityManager dstManager) {

            dstManager.AddComponentData(entity, new HealthPointData(this));
            dstManager.AddComponentData(entity, new MagicPointData(this));
            dstManager.AddComponentData(entity, new UnitStatData(this));
            dstManager.AddComponentData(entity, ActionMeterData.Create());
            dstManager.AddSharedComponentData(entity, new UnitAssetReference(this));
            var tasks = new Task<ActionAsset>[personalActions.Length];
            for (int i = 0; i < personalActions.Length; i++)
                tasks[i] = personalActions[i].LoadAssetAsync().Task;

            dstManager.AddSharedComponentData(entity, new ActionList { value = new List<ActionAsset>(await Task.WhenAll(tasks)) });
        }
    }
}