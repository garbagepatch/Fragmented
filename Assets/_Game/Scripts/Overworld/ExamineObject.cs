using UnityEngine;

namespace Game.Overworld
{
    public class ExamineObject : MonoBehaviour
    {
        [TextArea] public string message = "A weathered grave marker hums with faint warmth.";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            Debug.Log(message);
        }
    }
}
