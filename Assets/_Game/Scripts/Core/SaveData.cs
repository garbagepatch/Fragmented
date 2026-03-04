using System;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class SaveData
    {
        public Vector2 playerPosition;
        public bool coraCarriedWithMe;
        public List<SummonSaveData> summons = new();
    }

    [Serializable]
    public class SummonSaveData
    {
        public string characterId;
        public bool inParty;
        public bool isUnbound;
        public bool rested;
        public float integrity;
        public SummonChoice choice;
    }
}
