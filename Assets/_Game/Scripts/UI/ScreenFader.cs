using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.35f;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator FadeOut()
        {
            yield return Fade(0f, 1f);
        }

        public IEnumerator FadeIn()
        {
            yield return Fade(1f, 0f);
        }

        private IEnumerator Fade(float from, float to)
        {
            var elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / fadeDuration);
                var c = fadeImage.color;
                c.a = Mathf.Lerp(from, to, t);
                fadeImage.color = c;
                yield return null;
            }
        }
    }
}
