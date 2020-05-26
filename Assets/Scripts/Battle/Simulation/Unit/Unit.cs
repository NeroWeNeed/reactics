using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Battle
{

    public interface IUnit
    {


        string Identifier { get; }

        ushort HealthPoints { get; }

        ushort MagicPoints { get; }

        ushort Defense { get; }

        ushort Resistance { get; }

        ushort Strength { get; }

        ushort Magic { get; }

        ushort Speed { get; }

        ushort Movement { get; }

    }
    [CreateAssetMenu(fileName = "Unit", menuName = "Reactics/Unit", order = 0)]
    public class Unit : ScriptableObject, IUnit
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

        private static ushort identifierInt = 1;
        public UnitData CreateComponent()
        {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref UnitBlob blob = ref builder.ConstructRoot<UnitBlob>();
            blob.defense = defense;
            blob.strength = strength;
            blob.magicPoints = magicPoints;
            blob.magic = magic;
            blob.movement = movement;
            blob.resistance = resistance;
            blob.healthPoints = healthPoints;
            blob.speed = speed;
            blob.identifierInt = identifierInt;
            
            builder.AllocateString(ref blob.identifier, identifier);
            BlobAssetReference<UnitBlob> reference = builder.CreateBlobAssetReference<UnitBlob>(Allocator.Persistent);
            builder.Dispose();
            identifierInt += 1;
            EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var highlightEntity = EntityManager.CreateEntity(typeof(HighlightTile));
            DynamicBuffer<HighlightTile> highlights = EntityManager.AddBuffer<HighlightTile>(highlightEntity);
            //highlights.Add(new HighlightTile { point = new Point(3, 3), layer = MapLayer.HOVER });
            return new UnitData
            {
                unit = reference,
                healthPoints = healthPoints,
                maxHealthPoints = healthPoints,
                magicPoints = magicPoints,
                maxMagicPoints = magicPoints,
                defense = defense,
                resistance = resistance,
                strength = strength,
                magic = magic,
                speed = speed,
                movement = movement
                //highlightEntity = highlightEntity
            };

        }

    }
    public struct UnitBlob : IUnit
    {
        public BlobString identifier;

        public ushort  identifierInt;
        public ushort IdentifierInt => identifierInt;

        public string Identifier => identifier.ToString();

        public ushort healthPoints;
        public ushort HealthPoints => healthPoints;

        public ushort magicPoints;
        public ushort MagicPoints => magicPoints;

        public ushort defense;
        public ushort Defense => defense;

        public ushort resistance;
        public ushort Resistance => resistance;
        public ushort strength;
        public ushort Strength => strength;

        public ushort magic;
        public ushort Magic => magic;
        public ushort speed;
        public ushort Speed => speed;
        public ushort movement;
        public ushort Movement => movement;


    }

}

