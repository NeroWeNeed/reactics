using System.Collections;
using System.Collections.Generic;
using Reactics.Battle;
using Reactics.Battle.Map;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Komota {


    //Why are we separating unit effects and tile effects?
    //I feel like they can be too different, depending on what we're doing.
    //Like what if tile effects can... teleport you somewhere or something.
    //Like a two way warp, among other things?
    //Apparently we could use a dynamic buffer for the tiles affected... sigh.
    //Those are related to entities specifically though. so.. yeah.(I think they are at least.)
    //Well.. those tile warps? They could be like. Switching unit positions or something. Couldn't they? 
    //Either way we probably have to check if a unit is there or something. For now let's just....
    //Assume they can be the same thing. Alright? Seems fine.
    //
    //Yeah let's make tile and action effects the same.
    public struct Effect {
        //Having ten billion of these... not the best idea? Hmm.
        //Really unsure if having a check for each one is really the best idea. Then again, not sure if like... having a component for each one is the best idea either?
        //Maybe that is the strat though. Just having like, a million systems. If they aren't running then... Maybe that's better for it. Hm.
        //We can just prototype it as it is, and only fiddle with these three possible effects.
        //Targeting/Positioning data
        public ushort range;
        public ushort cost;
        public ushort speed;
        public bool arc;
        public float maxArcValue;
        public bool pierce;
        public bool homing;
        public bool affectsTiles;
        public bool affectsEnemies;
        public bool affectsAllies;
        public bool affectsSelf;
        public ushort maxPierce;
        //Damage/Healing data
        public bool harmful;
        public ushort physicalDmg;
        public ushort physicalDOT;
        public ushort magicDmg;
        public ushort magicDOT;
        public ushort trueDmg;
        public ushort trueDOT;
        public ushort healing;
        public ushort healingOverTime;
        //Stat buff/debuff data
        public int defenseModifier;
        public int resistanceModifier;
        public int strengthModifier;
        public int magicModifier;
        public int speedModifier;
        public int movementModifier;
        //Time data
        public ushort totalFrames;
        public ushort currentFrames;
        public ushort secondsPerTick;
        public float time; //not sure how to use this yet, I'll figure it out I suppose. for now just using ticks...
                           //AOE data
        public ushort aoeRange;
        public bool aoeOnPierce;
        public bool mapEffect;
        public bool permanent;
    }

    //is this right? idk
    public struct EffectBuffer : IBufferElementData {
        public Effect effect;
    }

    public struct ConstantEffect : IComponentData {
        public ushort movementBonus;
        public Entity targetedUnit;
    }

    public struct WaitToProcessTag : IComponentData { }

    //these most definitely need a translation btw. or maybe they don't. weird.
    public struct Projectile : IComponentData {
        //reference to something that holds damage? or should it hold the damage?
        //maybe effects should just hold like, everything?
        //Or we should split relevant component data up, like someone with intelligence.
        //Ex: Projectile can hold all the stuff related to how it moves.
        //Then we can have one that relates to the damage calculations and such.
        //a lot of these should be effects.
        public Entity targetUnit;
        public ushort currentPierce;
        public float travelTime;
        public bool delete;
        public Point originPoint;
        public float3 originPosition;
        public Point targetPoint;
        public bool hasTargetPoint;
        public float3 targetPosition;
        public Point lastPiercePoint;
        public Effect effect;
        public Effect aoeEffect;
    }
}