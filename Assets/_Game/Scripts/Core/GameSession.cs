using System.Collections.Generic;
using System.Linq;
using Game.Data;
using UnityEngine;

namespace Game.Core
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private PartyDatabase partyDatabase;

        public EncounterDef PendingEncounter { get; private set; }
        public Vector2 OverworldReturnPosition { get; private set; }
        public bool CoraCarriedWithMe { get; private set; }

        public readonly Dictionary<string, SummonSaveData> SummonStates = new();
        public readonly List<string> PendingFullyMendedChoices = new();

        private SaveSystem _saveSystem;

        private void Awake()
        {
            _saveSystem = new SaveSystem();
            Load();
        }

        public CharacterDef CoraDef => partyDatabase.coraDef;

        public IEnumerable<SummonSaveData> ActiveSummons => SummonStates.Values.Where(s => s.inParty && !s.rested);

        public CharacterDef GetCharacterDef(string id) => partyDatabase.GetById(id);

        public void QueueEncounter(EncounterDef encounterDef, Vector2 returnPosition)
        {
            PendingEncounter = encounterDef;
            OverworldReturnPosition = returnPosition;
        }

        public void ClearEncounter() => PendingEncounter = null;

        public void UpdateSummonIntegrity(string summonId, float integrity)
        {
            if (!SummonStates.TryGetValue(summonId, out var state)) return;
            state.integrity = Mathf.Clamp01(integrity);
        }

        public void QueueFullyMendedChoice(string summonId)
        {
            if (!PendingFullyMendedChoices.Contains(summonId))
            {
                PendingFullyMendedChoices.Add(summonId);
            }
        }

        public void ApplyChoice(string summonId, SummonChoice choice)
        {
            if (!SummonStates.TryGetValue(summonId, out var state)) return;
            state.choice = choice;

            if (choice == SummonChoice.LetRest)
            {
                state.inParty = false;
                state.rested = true;
                CoraCarriedWithMe = true;
            }
            else if (choice == SummonChoice.StayTogether)
            {
                state.isUnbound = true;
            }
        }

        public SaveData BuildSaveData(Vector2 playerPosition)
        {
            return new SaveData
            {
                playerPosition = playerPosition,
                coraCarriedWithMe = CoraCarriedWithMe,
                summons = SummonStates.Values.ToList()
            };
        }

        public void Save(Vector2 playerPosition)
        {
            _saveSystem.Save(BuildSaveData(playerPosition));
        }

        private void Load()
        {
            var data = _saveSystem.LoadOrCreate();
            CoraCarriedWithMe = data.coraCarriedWithMe;

            SummonStates.Clear();
            foreach (var summonDef in partyDatabase.startingSummons)
            {
                if (summonDef == null) continue;

                var loaded = data.summons.FirstOrDefault(s => s.characterId == summonDef.id);
                SummonStates[summonDef.id] = loaded ?? new SummonSaveData
                {
                    characterId = summonDef.id,
                    inParty = true,
                    integrity = summonDef.startingIntegrity,
                    choice = SummonChoice.None
                };
            }

            OverworldReturnPosition = data.playerPosition;
        }
    }
}
