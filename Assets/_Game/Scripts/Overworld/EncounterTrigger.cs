using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Overworld
{
    [RequireComponent(typeof(Collider2D))]
    public class EncounterTrigger : MonoBehaviour
    {
        [SerializeField] private EncounterDef encounterDef;

        private bool _consumed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed || !other.CompareTag("Player")) return;

            var session = FindObjectOfType<GameSession>();
            var router = FindObjectOfType<SceneRouter>();
            var mover = other.GetComponent<TopDownMover2D>();
            if (session == null || router == null || mover == null) return;

            _consumed = true;
            session.QueueEncounter(encounterDef, mover.Position);
            session.Save(mover.Position);
            router.StartEncounter();
        }
    }
}
