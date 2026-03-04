# Vertical Slice Setup Checklist (Unity 2022+ URP 2D)

## 0) One-click setup (as far as editor automation can go)
1. Open Unity Editor and let scripts compile.
2. Run `Tools/_Game/Setup Vertical Slice Project`.
3. This auto-generates: placeholder prefabs, core ScriptableObject assets, Boot/Overworld/Battle scenes, and Build Settings scene order.
4. Then only do final manual polish (sprites, animator clips/blend trees, tile assets, and Input Actions asset binding).

## 1) Folder Structure
Use these folders exactly:
- `Assets/_Game/Scripts/{Core,Data,Overworld,Battle,Narrative,UI}`
- `Assets/_Game/Prefabs/{Characters,Enemies,UI,Systems}`
- `Assets/_Game/Scenes/{Boot,Overworld,Battle,Shared}`

## 2) Input System
1. Install/enable Input System package.
2. Create `Assets/_Game/Data/Input/PlayerControls.inputactions`.
3. Add Action Map: `Player`.
4. Add Action: `Move` (Value, Vector2).
5. Bindings: WASD and Gamepad Left Stick.
6. Add `PlayerInput` to Cora prefab:
   - Default Map: `Player`
   - Behavior: `Send Messages`
   - Ensure `Move` calls `TopDownMover2D.OnMove`.

## 3) ScriptableObject Assets
Create under `Assets/_Game/Data/Definitions/`:

### CharacterDef
- `CharacterDef_Cora`: id `cora`, isSummon false, maxHP 24, attack 2, defense 2.
- `CharacterDef_Warrior`: id `warrior`, isSummon true, maxHP 36, attack 8, defense 3, startingIntegrity 0.25.
- `CharacterDef_Dryad`: id `dryad`, isSummon true, maxHP 30, attack 6, defense 2, startingIntegrity 0.2.
- `CharacterDef_DummyEnemy`: id `dummy_enemy`, isSummon false, maxHP 42, attack 5, defense 1.

### AbilityDef
- `Ability_BasicAttack`
  - Targeting: `SingleEnemy`
  - Effects: one `Damage` amount 5.
- `Ability_StitchBone`
  - Targeting: `SingleAlly`
  - Effects: one `Heal` amount 8 integrityDelta 0.2.
- `Ability_HoldTogether`
  - Targeting: `AllAllies`
  - Effects: one `Heal` amount 4 integrityDelta 0.08.
- `Ability_EnemyAttack`
  - Targeting: `SingleEnemy`
  - Effects: one `Damage` amount 4.

### EncounterDef
- `EncounterDef_Test`: add `CharacterDef_DummyEnemy` to enemy list.

### PartyDatabase
- `PartyDatabase_Main`
  - Cora: `CharacterDef_Cora`
  - Starting Summons: Warrior + Dryad
  - All Characters: all of the above

## 4) Prefabs

### Auto-generate prefabs (recommended)
- Use Unity menu: `Tools/_Game/Generate Placeholder Prefabs`.
- This generates:
  - `Prefabs/Characters/PF_Player_Cora.prefab`
  - `Prefabs/Characters/PF_BattleActor.prefab`
  - `Prefabs/UI/PF_TargetButton.prefab`
  - `Prefabs/UI/PF_ChoicePanel.prefab`
  - `Prefabs/Systems/PF_GameSession.prefab`
  - `Prefabs/Systems/PF_SceneRouter.prefab`
- After generation, assign references in scene objects as listed below.

### Systems
- `GameSession` prefab:
  - Add `GameSession` component and assign `PartyDatabase_Main`.
- `SceneRouter` prefab:
  - Add `SceneRouter`.
  - Child Canvas + full-screen `Image` (black, alpha 0) and `ScreenFader` reference.

### Characters
- `PF_Player_Cora`
  - SpriteRenderer (placeholder square sprite)
  - Rigidbody2D (Dynamic, Gravity Scale 0)
  - CapsuleCollider2D
  - Animator
  - PlayerInput
  - `TopDownMover2D` with Animator reference
  - Tag = `Player`

- `PF_BattleActor`
  - Empty + SpriteRenderer + `BattleActor`

### UI
- `PF_TargetButton` (Button + TMP Text child).

## 5) Animator (4-direction blend)
Animator parameters required by movement script:
- `MoveX` (float)
- `MoveY` (float)
- `Speed` (float)
- `LastMoveX` (float)
- `LastMoveY` (float)

Recommended setup:
1. `Idle` state: 2D Freeform Directional blend tree using `LastMoveX/LastMoveY`.
2. `Locomotion` state: 2D Freeform Directional blend tree using `MoveX/MoveY`.
3. Transition Idle -> Locomotion when `Speed > 0.01`.
4. Transition Locomotion -> Idle when `Speed <= 0.01`.

## 6) Scene: Boot_Main
1. Create scene `Assets/_Game/Scenes/Boot/Boot.unity`.
2. Add empty `BOOTSTRAP` game object with `GameBootstrapper`.
3. Assign `GameSession` prefab and `SceneRouter` prefab.
4. Add scene to Build Settings first.

## 7) Scene: Overworld_Main
1. Create scene `Assets/_Game/Scenes/Overworld/Overworld_Main.unity`.
2. Add placeholder Tilemap for floor.
3. Drag in `PF_Player_Cora` at origin.
4. Add `OverworldSceneController`; assign player mover.
5. Add `SummonChoiceController`; assign `ChoicePanelUI` and player mover.
6. Add trigger zone object:
   - BoxCollider2D (`Is Trigger` true)
   - `EncounterTrigger` with `EncounterDef_Test`
7. Add examine object:
   - Collider2D trigger
   - `ExamineObject` with test text.
8. Add a Canvas with `ChoicePanelUI` wired to panel root, label, and two buttons.

## 8) Scene: Battle_Main
1. Create scene `Assets/_Game/Scenes/Battle/Battle_Main.unity`.
2. Add empty `BattleSceneController` object.
3. Add ally spawn points (3 transforms).
4. Add enemy spawn points (1 transform).
5. Assign on `BattleSceneController`:
   - actor prefab `PF_BattleActor`
   - ability defs (warrior attack, stitch bone, hold together, enemy attack)
   - battle UI controller
6. Add canvas with:
   - `BattleUIController`
   - Buttons for `Stitch Bone` and `Hold Together`
   - State TMP label
   - Target button root + target button prefab

## 9) Build Settings Scene Order
1. `Boot.unity`
2. `Overworld_Main.unity`
3. `Battle_Main.unity`

## 10) Expected Flow
- Boot starts and auto-loads Overworld.
- Move Cora with `Move` action.
- Enter encounter trigger -> fade -> battle.
- Warrior auto-attacks, Cora chooses heal abilities, enemy attacks.
- Healing summons increases integrity.
- On first Full Mended (integrity 1.0), summon is queued for post-battle choice.
- Victory returns to Overworld at saved location.
- Choice panel appears (`Let them rest` or `Stay together`), outcome saved to JSON.
