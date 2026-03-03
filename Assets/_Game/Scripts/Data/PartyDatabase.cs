using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Party Database", fileName = "PartyDatabase")]
    public class PartyDatabase : ScriptableObject
    {
        public CharacterDef coraDef;
        public List<CharacterDef> startingSummons = new();
        public List<CharacterDef> allCharacters = new();

        public CharacterDef GetById(string id)
        {
            return allCharacters.FirstOrDefault(c => c != null && c.id == id);
        }
    }
}
