using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ChoicePanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button letRestButton;
        [SerializeField] private Button stayTogetherButton;

        public void Show(string summonName, Action onLetRest, Action onStayTogether)
        {
            root.SetActive(true);
            titleText.text = $"{summonName} is Fully Mended. What will you do?";

            letRestButton.onClick.RemoveAllListeners();
            stayTogetherButton.onClick.RemoveAllListeners();
            letRestButton.onClick.AddListener(() => onLetRest?.Invoke());
            stayTogetherButton.onClick.AddListener(() => onStayTogether?.Invoke());
        }

        public void Hide()
        {
            root.SetActive(false);
        }
    }
}
