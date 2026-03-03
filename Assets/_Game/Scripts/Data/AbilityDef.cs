using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Ability Def", fileName = "AbilityDef_")]
    public class AbilityDef : ScriptableObject
    {
        public string id;
        public string displayName;
        public TargetingType targetingType = TargetingType.SingleEnemy;
        public List<AbilityEffect> effects = new();
    }

    [Serializable]
    public class AbilityEffect
    {
        public EffectType effectType;
        public int amount = 5;
        [Range(0f, 1f)] public float integrityDelta;
        public StatusDef statusDef;
    }
}
