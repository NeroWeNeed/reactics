using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Reactics.Battle.Map;
namespace Reactics.Battle
{

    public interface IUnit
    {

        ushort HealthPoints { get; }

        ushort MagicPoints { get; }

        ushort Defense { get; }

        ushort Resistance { get; }

        ushort Strength { get; }

        ushort Magic { get; }

        ushort Speed { get; }

        ushort Movement { get; }

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

