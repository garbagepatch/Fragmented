using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character Def", fileName = "CharacterDef_")]
    public class CharacterDef : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public bool isSummon;

        [Header("Combat Stats")]
        public int maxHP = 20;
        public int attack = 5;
        public int defense = 1;

        [Header("Visuals")]
        public Sprite overworldSprite;
        public Sprite battleSprite;

        [Header("Integrity")]
        [Range(0f, 1f)] public float startingIntegrity = 0.2f;
        [Range(0f, 1f)] public float minIntegrity = 0f;
        [Range(0f, 1f)] public float maxIntegrity = 1f;
    }
}
