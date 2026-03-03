using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameSession gameSessionPrefab;
        [SerializeField] private SceneRouter sceneRouterPrefab;

        private void Awake()
        {
            if (FindObjectOfType<GameSession>() == null)
            {
                Instantiate(gameSessionPrefab);
            }

            if (FindObjectOfType<SceneRouter>() == null)
            {
                Instantiate(sceneRouterPrefab);
            }

            SceneManager.LoadScene("Overworld_Main");
        }
    }
}
