#if UNITY_EDITOR
using System;
using System.IO;
using Game.Battle;
using Game.Core;
using Game.Overworld;
using Game.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.EditorTools
{
    /// <summary>
    /// Creates minimal placeholder prefabs required by the vertical slice.
    /// </summary>
    public static class PrefabGeneratorEditor
    {
        private const string Root = "Assets/_Game";

        [MenuItem("Tools/_Game/Generate Placeholder Prefabs")]
        public static void GeneratePrefabs()
        {
            EnsureDirectories();

            CreatePlayerPrefab();
            CreateBattleActorPrefab();
            CreateTargetButtonPrefab();
            CreateChoicePanelPrefab();
            CreateGameSessionPrefab();
            CreateSceneRouterPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated placeholder prefabs in Assets/_Game/Prefabs");
        }

        private static void EnsureDirectories()
        {
            EnsurePath($"{Root}/Prefabs");
            EnsurePath($"{Root}/Prefabs/Characters");
            EnsurePath($"{Root}/Prefabs/Enemies");
            EnsurePath($"{Root}/Prefabs/UI");
            EnsurePath($"{Root}/Prefabs/Systems");
        }

        private static void EnsurePath(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
                var name = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent))
                {
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        private static void CreatePlayerPrefab()
        {
            var go = new GameObject("PF_Player_Cora");
            go.tag = "Player";
            go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            go.AddComponent<CapsuleCollider2D>();
            var animator = go.AddComponent<Animator>();

            var mover = go.AddComponent<TopDownMover2D>();
            var so = new SerializedObject(mover);
            so.FindProperty("animator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Add PlayerInput only if Input System package is installed.
            var playerInputType = Type.GetType("UnityEngine.InputSystem.PlayerInput, Unity.InputSystem");
            if (playerInputType != null)
            {
                go.AddComponent(playerInputType);
            }

            SaveAsPrefab(go, $"{Root}/Prefabs/Characters/PF_Player_Cora.prefab");
        }

        private static void CreateBattleActorPrefab()
        {
            var go = new GameObject("PF_BattleActor");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<BattleActor>();
            SaveAsPrefab(go, $"{Root}/Prefabs/Characters/PF_BattleActor.prefab");
        }

        private static void CreateTargetButtonPrefab()
        {
            var root = new GameObject("PF_TargetButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(root.transform, false);

            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.text = "Target";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(220, 56);

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            SaveAsPrefab(root, $"{Root}/Prefabs/UI/PF_TargetButton.prefab");
        }

        private static void CreateChoicePanelPrefab()
        {
            var root = new GameObject("PF_ChoicePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var choiceUi = root.AddComponent<ChoicePanelUI>();
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(640, 320);

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(root.transform, false);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "Choice";
            titleText.alignment = TextAlignmentOptions.Center;

            var restButton = CreateButton("LetRestButton", "Let them rest", root.transform, new Vector2(-120, -80));
            var stayButton = CreateButton("StayTogetherButton", "Stay together", root.transform, new Vector2(120, -80));

            var so = new SerializedObject(choiceUi);
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("titleText").objectReferenceValue = titleText;
            so.FindProperty("letRestButton").objectReferenceValue = restButton;
            so.FindProperty("stayTogetherButton").objectReferenceValue = stayButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveAsPrefab(root, $"{Root}/Prefabs/UI/PF_ChoicePanel.prefab");
        }

        private static void CreateGameSessionPrefab()
        {
            var go = new GameObject("PF_GameSession");
            go.AddComponent<GameSession>();
            SaveAsPrefab(go, $"{Root}/Prefabs/Systems/PF_GameSession.prefab");
        }

        private static void CreateSceneRouterPrefab()
        {
            var root = new GameObject("PF_SceneRouter");
            var router = root.AddComponent<SceneRouter>();

            var canvasGo = new GameObject("FadeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var imageGo = new GameObject("FadeImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageGo.transform.SetParent(canvasGo.transform, false);
            var imgRect = imageGo.GetComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
            var img = imageGo.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0);

            var fader = root.AddComponent<ScreenFader>();
            var faderSO = new SerializedObject(fader);
            faderSO.FindProperty("fadeImage").objectReferenceValue = img;
            faderSO.ApplyModifiedPropertiesWithoutUndo();

            var routerSO = new SerializedObject(router);
            routerSO.FindProperty("screenFader").objectReferenceValue = fader;
            routerSO.ApplyModifiedPropertiesWithoutUndo();

            SaveAsPrefab(root, $"{Root}/Prefabs/Systems/PF_SceneRouter.prefab");
        }

        private static Button CreateButton(string name, string label, Transform parent, Vector2 anchoredPosition)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(220, 48);
            buttonRect.anchoredPosition = anchoredPosition;

            var labelGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelText = labelGo.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 20;

            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return buttonGo.GetComponent<Button>();
        }

        private static void SaveAsPrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
#endif
