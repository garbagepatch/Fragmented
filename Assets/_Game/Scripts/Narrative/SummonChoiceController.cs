using System.Collections;
using System.Linq;
using Game.Core;
using Game.Data;
using Game.Overworld;
using Game.UI;
using UnityEngine;

namespace Game.Narrative
{
    public class SummonChoiceController : MonoBehaviour
    {
        [SerializeField] private ChoicePanelUI choicePanel;
        [SerializeField] private TopDownMover2D playerMover;

        private GameSession _session;
        private bool _awaitingChoice;

        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            StartCoroutine(ShowQueuedChoices());
        }

        private IEnumerator ShowQueuedChoices()
        {
            if (_session == null || !_session.PendingFullyMendedChoices.Any())
            {
                choicePanel.Hide();
                yield break;
            }

            while (_session.PendingFullyMendedChoices.Any())
            {
                var summonId = _session.PendingFullyMendedChoices[0];
                _session.PendingFullyMendedChoices.RemoveAt(0);
                var def = _session.GetCharacterDef(summonId);
                if (def == null) continue;

                _awaitingChoice = true;
                choicePanel.Show(def.displayName,
                    onLetRest: () => ResolveChoice(summonId, SummonChoice.LetRest),
                    onStayTogether: () => ResolveChoice(summonId, SummonChoice.StayTogether));

                while (_awaitingChoice)
                {
                    yield return null;
                }
            }

            choicePanel.Hide();
            _session.Save(playerMover.Position);
        }

        private void ResolveChoice(string summonId, SummonChoice choice)
        {
            _session.ApplyChoice(summonId, choice);
            _awaitingChoice = false;
        }
    }
}
