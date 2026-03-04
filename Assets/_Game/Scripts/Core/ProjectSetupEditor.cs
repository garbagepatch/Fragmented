#if UNITY_EDITOR
using System.Collections.Generic;
using Game.Battle;
using Game.Core;
using Game.Data;
using Game.Narrative;
using Game.Overworld;
using Game.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Game.EditorTools
{
    /// <summary>
    /// One-click setup to generate prefabs, ScriptableObjects, scenes, and build settings wiring.
    /// </summary>
    public static class ProjectSetupEditor
    {
        private const string Root = "Assets/_Game";
        private const string DefinitionsPath = Root + "/Data/Definitions";

        [MenuItem("Tools/_Game/Setup Vertical Slice Project")]
        public static void SetupProject()
        {
            EnsureFolders();
            PrefabGeneratorEditor.GeneratePrefabs();

            var defs = GenerateDefinitions();
            ConfigureGameSessionPrefab(defs.partyDb);
            BuildBootScene();
            BuildOverworldScene(defs.encounter);
            BuildBattleScene(defs);
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("_Game setup complete. Open Boot scene and press Play.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder(Root + "/Data");
            EnsureFolder(DefinitionsPath);
            EnsureFolder(DefinitionsPath + "/Characters");
            EnsureFolder(DefinitionsPath + "/Abilities");
            EnsureFolder(DefinitionsPath + "/Encounters");
            EnsureFolder(DefinitionsPath + "/Statuses");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = path.Substring(0, path.LastIndexOf('/'));
            var name = path.Substring(path.LastIndexOf('/') + 1);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static GeneratedDefs GenerateDefinitions()
        {
            var cora = CreateOrLoad<CharacterDef>(DefinitionsPath + "/Characters/CharacterDef_Cora.asset");
            cora.id = "cora";
            cora.displayName = "Cora";
            cora.isSummon = false;
            cora.maxHP = 24;
            cora.attack = 2;
            cora.defense = 2;

            var warrior = CreateOrLoad<CharacterDef>(DefinitionsPath + "/Characters/CharacterDef_Warrior.asset");
            warrior.id = "warrior";
            warrior.displayName = "Warrior";
            warrior.isSummon = true;
            warrior.maxHP = 36;
            warrior.attack = 8;
            warrior.defense = 3;
            warrior.startingIntegrity = 0.25f;

            var dryad = CreateOrLoad<CharacterDef>(DefinitionsPath + "/Characters/CharacterDef_Dryad.asset");
            dryad.id = "dryad";
            dryad.displayName = "Dryad";
            dryad.isSummon = true;
            dryad.maxHP = 30;
            dryad.attack = 6;
            dryad.defense = 2;
            dryad.startingIntegrity = 0.2f;

            var enemy = CreateOrLoad<CharacterDef>(DefinitionsPath + "/Characters/CharacterDef_DummyEnemy.asset");
            enemy.id = "dummy_enemy";
            enemy.displayName = "Ash Dummy";
            enemy.isSummon = false;
            enemy.maxHP = 42;
            enemy.attack = 5;
            enemy.defense = 1;

            var stitch = CreateOrLoad<AbilityDef>(DefinitionsPath + "/Abilities/Ability_StitchBone.asset");
            stitch.id = "stitch_bone";
            stitch.displayName = "Stitch Bone";
            stitch.targetingType = TargetingType.SingleAlly;
            stitch.effects = new List<AbilityEffect> { new() { effectType = EffectType.Heal, amount = 8, integrityDelta = 0.2f } };

            var hold = CreateOrLoad<AbilityDef>(DefinitionsPath + "/Abilities/Ability_HoldTogether.asset");
            hold.id = "hold_together";
            hold.displayName = "Hold Together";
            hold.targetingType = TargetingType.AllAllies;
            hold.effects = new List<AbilityEffect> { new() { effectType = EffectType.Heal, amount = 4, integrityDelta = 0.08f } };

            var basic = CreateOrLoad<AbilityDef>(DefinitionsPath + "/Abilities/Ability_BasicAttack.asset");
            basic.id = "basic_attack";
            basic.displayName = "Basic Attack";
            basic.targetingType = TargetingType.SingleEnemy;
            basic.effects = new List<AbilityEffect> { new() { effectType = EffectType.Damage, amount = 5 } };

            var enemyAtk = CreateOrLoad<AbilityDef>(DefinitionsPath + "/Abilities/Ability_EnemyAttack.asset");
            enemyAtk.id = "enemy_attack";
            enemyAtk.displayName = "Rusty Swipe";
            enemyAtk.targetingType = TargetingType.SingleEnemy;
            enemyAtk.effects = new List<AbilityEffect> { new() { effectType = EffectType.Damage, amount = 4 } };

            var encounter = CreateOrLoad<EncounterDef>(DefinitionsPath + "/Encounters/EncounterDef_Test.asset");
            encounter.id = "test_encounter";
            encounter.displayName = "Dustroad Ambush";
            encounter.enemyDefs = new List<CharacterDef> { enemy };

            var partyDb = CreateOrLoad<PartyDatabase>(DefinitionsPath + "/PartyDatabase_Main.asset");
            partyDb.coraDef = cora;
            partyDb.startingSummons = new List<CharacterDef> { warrior, dryad };
            partyDb.allCharacters = new List<CharacterDef> { cora, warrior, dryad, enemy };

            EditorUtility.SetDirty(cora);
            EditorUtility.SetDirty(warrior);
            EditorUtility.SetDirty(dryad);
            EditorUtility.SetDirty(enemy);
            EditorUtility.SetDirty(stitch);
            EditorUtility.SetDirty(hold);
            EditorUtility.SetDirty(basic);
            EditorUtility.SetDirty(enemyAtk);
            EditorUtility.SetDirty(encounter);
            EditorUtility.SetDirty(partyDb);

            return new GeneratedDefs
            {
                partyDb = partyDb,
                encounter = encounter,
                warriorAttack = basic,
                coraStitch = stitch,
                coraHold = hold,
                enemyAttack = enemyAtk
            };
        }

        private static void ConfigureGameSessionPrefab(PartyDatabase partyDb)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Systems/PF_GameSession.prefab");
            if (prefab == null) return;

            var instance = PrefabUtility.LoadPrefabContents(Root + "/Prefabs/Systems/PF_GameSession.prefab");
            var session = instance.GetComponent<GameSession>();
            var so = new SerializedObject(session);
            so.FindProperty("partyDatabase").objectReferenceValue = partyDb;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(instance, Root + "/Prefabs/Systems/PF_GameSession.prefab");
            PrefabUtility.UnloadPrefabContents(instance);
        }

        private static void BuildBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootObj = new GameObject("BOOTSTRAP");
            var bootstrap = bootObj.AddComponent<GameBootstrapper>();

            var sessionPrefab = AssetDatabase.LoadAssetAtPath<GameSession>(Root + "/Prefabs/Systems/PF_GameSession.prefab");
            var routerPrefab = AssetDatabase.LoadAssetAtPath<SceneRouter>(Root + "/Prefabs/Systems/PF_SceneRouter.prefab");
            var so = new SerializedObject(bootstrap);
            so.FindProperty("gameSessionPrefab").objectReferenceValue = sessionPrefab;
            so.FindProperty("sceneRouterPrefab").objectReferenceValue = routerPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, Root + "/Scenes/Boot/Boot.unity");
        }

        private static void BuildOverworldScene(EncounterDef encounter)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>().orthographic = true;

            var grid = new GameObject("Grid", typeof(Grid));
            var tilemap = new GameObject("Ground", typeof(Tilemap), typeof(TilemapRenderer));
            tilemap.transform.SetParent(grid.transform, false);

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Characters/PF_Player_Cora.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = Vector3.zero;

            var overworldController = new GameObject("OverworldSceneController").AddComponent<OverworldSceneController>();
            var mover = player.GetComponent<TopDownMover2D>();
            var overworldSO = new SerializedObject(overworldController);
            overworldSO.FindProperty("playerMover").objectReferenceValue = mover;
            overworldSO.ApplyModifiedPropertiesWithoutUndo();

            var canvas = CreateCanvas("OverworldCanvas");
            EnsureEventSystem();

            var choicePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/UI/PF_ChoicePanel.prefab");
            var choiceObj = (GameObject)PrefabUtility.InstantiatePrefab(choicePrefab, canvas.transform);
            var choicePanel = choiceObj.GetComponent<ChoicePanelUI>();

            var choiceController = new GameObject("SummonChoiceController").AddComponent<SummonChoiceController>();
            var choiceSO = new SerializedObject(choiceController);
            choiceSO.FindProperty("choicePanel").objectReferenceValue = choicePanel;
            choiceSO.FindProperty("playerMover").objectReferenceValue = mover;
            choiceSO.ApplyModifiedPropertiesWithoutUndo();

            var encounterObj = new GameObject("EncounterTrigger");
            encounterObj.transform.position = new Vector3(2.5f, 0f, 0f);
            var encounterCollider = encounterObj.AddComponent<BoxCollider2D>();
            encounterCollider.isTrigger = true;
            var trigger = encounterObj.AddComponent<EncounterTrigger>();
            var triggerSO = new SerializedObject(trigger);
            triggerSO.FindProperty("encounterDef").objectReferenceValue = encounter;
            triggerSO.ApplyModifiedPropertiesWithoutUndo();

            var examineObj = new GameObject("ExamineMarker");
            examineObj.transform.position = new Vector3(-2f, 0f, 0f);
            var exCollider = examineObj.AddComponent<CircleCollider2D>();
            exCollider.isTrigger = true;
            var examine = examineObj.AddComponent<ExamineObject>();
            examine.message = "Cora kneels beside the cairn. Something remembers her.";

            EditorSceneManager.SaveScene(scene, Root + "/Scenes/Overworld/Overworld_Main.unity");
        }

        private static void BuildBattleScene(GeneratedDefs defs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EnsureEventSystem();

            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>().orthographic = true;

            var controllerObj = new GameObject("BattleSceneController");
            var controller = controllerObj.AddComponent<BattleSceneController>();

            var allyPoints = new[]
            {
                CreatePoint("AllySpawn_0", new Vector3(-4f, -1f, 0f), controllerObj.transform),
                CreatePoint("AllySpawn_1", new Vector3(-4f, 0.5f, 0f), controllerObj.transform),
                CreatePoint("AllySpawn_2", new Vector3(-4f, 2f, 0f), controllerObj.transform)
            };

            var enemyPoints = new[] { CreatePoint("EnemySpawn_0", new Vector3(4f, 0.5f, 0f), controllerObj.transform) };

            var battleCanvas = CreateCanvas("BattleCanvas");
            var uiObj = new GameObject("BattleUIController");
            uiObj.transform.SetParent(battleCanvas.transform, false);
            var battleUi = uiObj.AddComponent<BattleUIController>();

            var stateLabel = CreateTmpLabel("StateLabel", uiObj.transform, new Vector2(0, 220), "Battle");
            var stitchButton = CreateButton("StitchBoneButton", uiObj.transform, new Vector2(-160, -200), "Stitch Bone");
            var holdButton = CreateButton("HoldTogetherButton", uiObj.transform, new Vector2(160, -200), "Hold Together");
            var targetRoot = new GameObject("TargetButtonRoot", typeof(RectTransform));
            targetRoot.transform.SetParent(uiObj.transform, false);
            var targetRootRect = targetRoot.GetComponent<RectTransform>();
            targetRootRect.anchoredPosition = new Vector2(0, -80);

            var targetButtonPrefab = AssetDatabase.LoadAssetAtPath<Button>(Root + "/Prefabs/UI/PF_TargetButton.prefab");

            var uiSO = new SerializedObject(battleUi);
            uiSO.FindProperty("stitchBoneButton").objectReferenceValue = stitchButton;
            uiSO.FindProperty("holdTogetherButton").objectReferenceValue = holdButton;
            uiSO.FindProperty("targetButtonRoot").objectReferenceValue = targetRoot.transform;
            uiSO.FindProperty("targetButtonPrefab").objectReferenceValue = targetButtonPrefab;
            uiSO.FindProperty("stateLabel").objectReferenceValue = stateLabel;
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            var actorPrefab = AssetDatabase.LoadAssetAtPath<BattleActor>(Root + "/Prefabs/Characters/PF_BattleActor.prefab");
            var controllerSO = new SerializedObject(controller);
            controllerSO.FindProperty("actorPrefab").objectReferenceValue = actorPrefab;
            controllerSO.FindProperty("battleUI").objectReferenceValue = battleUi;
            controllerSO.FindProperty("warriorAttack").objectReferenceValue = defs.warriorAttack;
            controllerSO.FindProperty("coraStitchBone").objectReferenceValue = defs.coraStitch;
            controllerSO.FindProperty("coraHoldTogether").objectReferenceValue = defs.coraHold;
            controllerSO.FindProperty("enemyAttack").objectReferenceValue = defs.enemyAttack;
            controllerSO.FindProperty("allySpawnPoints").arraySize = allyPoints.Length;
            for (var i = 0; i < allyPoints.Length; i++)
            {
                controllerSO.FindProperty("allySpawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = allyPoints[i];
            }
            controllerSO.FindProperty("enemySpawnPoints").arraySize = enemyPoints.Length;
            for (var i = 0; i < enemyPoints.Length; i++)
            {
                controllerSO.FindProperty("enemySpawnPoints").GetArrayElementAtIndex(i).objectReferenceValue = enemyPoints[i];
            }
            controllerSO.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, Root + "/Scenes/Battle/Battle_Main.unity");
        }

        private static Transform CreatePoint(string name, Vector3 pos, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            return go.transform;
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasGo = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return canvas;
        }

        private static TMP_Text CreateTmpLabel(string name, Transform parent, Vector2 anchoredPos, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700, 60);
            rect.anchoredPosition = anchoredPos;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 36;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchoredPos, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 52);
            rect.anchoredPosition = anchoredPos;

            var label = CreateTmpLabel("Label", go.transform, Vector2.zero, text);
            label.fontSize = 22;
            return go.GetComponent<Button>();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.position = Vector3.zero;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(Root + "/Scenes/Boot/Boot.unity", true),
                new EditorBuildSettingsScene(Root + "/Scenes/Overworld/Overworld_Main.unity", true),
                new EditorBuildSettingsScene(Root + "/Scenes/Battle/Battle_Main.unity", true)
            };
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private struct GeneratedDefs
        {
            public PartyDatabase partyDb;
            public EncounterDef encounter;
            public AbilityDef warriorAttack;
            public AbilityDef coraStitch;
            public AbilityDef coraHold;
            public AbilityDef enemyAttack;
        }
    }
}
#endif
