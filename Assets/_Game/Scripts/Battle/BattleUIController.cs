using System;
using System.Collections.Generic;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Action Buttons")]
        [SerializeField] private Button stitchBoneButton;
        [SerializeField] private Button holdTogetherButton;

        [Header("Target Buttons")]
        [SerializeField] private Transform targetButtonRoot;
        [SerializeField] private Button targetButtonPrefab;

        [Header("Labels")]
        [SerializeField] private TMP_Text stateLabel;

        private readonly List<Button> _spawnedTargetButtons = new();

        public event Action<AbilityDef> OnAbilitySelected;
        public event Action<BattleActor> OnTargetSelected;

        public void BindAbilities(AbilityDef stitchBone, AbilityDef holdTogether)
        {
            stitchBoneButton.onClick.RemoveAllListeners();
            holdTogetherButton.onClick.RemoveAllListeners();

            stitchBoneButton.onClick.AddListener(() => OnAbilitySelected?.Invoke(stitchBone));
            holdTogetherButton.onClick.AddListener(() => OnAbilitySelected?.Invoke(holdTogether));
        }

        public void SetStateText(string text)
        {
            stateLabel.text = text;
        }

        public void ShowTargetPicker(IEnumerable<BattleActor> candidates)
        {
            ClearTargetPicker();

            foreach (var actor in candidates)
            {
                var button = Instantiate(targetButtonPrefab, targetButtonRoot);
                button.GetComponentInChildren<TMP_Text>().text = actor.Def.displayName;
                button.onClick.AddListener(() => OnTargetSelected?.Invoke(actor));
                _spawnedTargetButtons.Add(button);
            }
        }

        public void ClearTargetPicker()
        {
            foreach (var button in _spawnedTargetButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            _spawnedTargetButtons.Clear();
        }
    }
}
