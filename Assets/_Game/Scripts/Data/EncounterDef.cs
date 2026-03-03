using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Encounter Def", fileName = "EncounterDef_")]
    public class EncounterDef : ScriptableObject
    {
        public string id;
        public string displayName;
        public List<CharacterDef> enemyDefs = new();
    }
}
