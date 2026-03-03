using Game.Core;
using UnityEngine;

namespace Game.Overworld
{
    public class OverworldSceneController : MonoBehaviour
    {
        [SerializeField] private TopDownMover2D playerMover;

        private GameSession _session;

        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            if (_session == null) return;

            if (_session.OverworldReturnPosition != Vector2.zero)
            {
                playerMover.transform.position = _session.OverworldReturnPosition;
            }
        }

        private void OnApplicationQuit()
        {
            if (_session != null)
            {
                _session.Save(playerMover.Position);
            }
        }
    }
}
