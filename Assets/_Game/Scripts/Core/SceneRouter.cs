using System.Collections;
using Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class SceneRouter : MonoBehaviour
    {
        [SerializeField] private ScreenFader screenFader;

        public void StartEncounter()
        {
            StartCoroutine(LoadBattleRoutine());
        }

        public void ReturnToOverworld()
        {
            StartCoroutine(LoadOverworldRoutine());
        }

        private IEnumerator LoadBattleRoutine()
        {
            yield return screenFader.FadeOut();
            yield return SceneManager.LoadSceneAsync("Battle_Main");
            yield return screenFader.FadeIn();
        }

        private IEnumerator LoadOverworldRoutine()
        {
            yield return screenFader.FadeOut();
            yield return SceneManager.LoadSceneAsync("Overworld_Main");
            yield return screenFader.FadeIn();
        }
    }
}
