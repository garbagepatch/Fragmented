using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Status Def", fileName = "StatusDef_")]
    public class StatusDef : ScriptableObject
    {
        public string id;
        public string displayName;
        public int durationTurns = 1;

        [Header("Multipliers")]
        public float healMultiplier = 1f;
        public float damageMultiplier = 1f;
    }
}
