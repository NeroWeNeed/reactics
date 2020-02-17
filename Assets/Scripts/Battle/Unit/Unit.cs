using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Reactics.Battle
{
    public class Unit : ScriptableObject
    {

        [SerializeField]
        private string identifier;
        public string Identifier => identifier;
        [SerializeField]
        private ushort healthPoints;
        public ushort HealthPoints => healthPoints;
        [SerializeField]
        private ushort magicPoints;
        public ushort MagicPoitns => magicPoints;
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

        


    }
}

