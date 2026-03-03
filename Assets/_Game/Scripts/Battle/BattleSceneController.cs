using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Battle
{
    public class BattleSceneController : MonoBehaviour
    {
        [Header("Spawn")]
        [SerializeField] private BattleActor actorPrefab;
        [SerializeField] private Transform[] allySpawnPoints;
        [SerializeField] private Transform[] enemySpawnPoints;

        [Header("Abilities")]
        [SerializeField] private AbilityDef warriorAttack;
        [SerializeField] private AbilityDef coraStitchBone;
        [SerializeField] private AbilityDef coraHoldTogether;
        [SerializeField] private AbilityDef enemyAttack;

        [Header("UI")]
        [SerializeField] private BattleUIController battleUI;

        private readonly List<BattleActor> _allies = new();
        private readonly List<BattleActor> _enemies = new();

        private GameSession _session;
        private SceneRouter _router;

        private AbilityDef _queuedAbility;
        private BattleActor _queuedTarget;
        private BattleState _state;

        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            _router = FindObjectOfType<SceneRouter>();

            battleUI.BindAbilities(coraStitchBone, coraHoldTogether);
            battleUI.OnAbilitySelected += HandlePlayerAbilitySelected;
            battleUI.OnTargetSelected += HandleTargetSelected;

            BuildCombatants();
            StartCoroutine(RunBattle());
        }

        private void BuildCombatants()
        {
            SpawnAlly(_session.CoraDef, 0f, 0);

            var index = 1;
            foreach (var summon in _session.ActiveSummons)
            {
                var def = _session.GetCharacterDef(summon.characterId);
                if (def == null) continue;
                SpawnAlly(def, summon.integrity, index++);
            }

            var encounter = _session.PendingEncounter;
            for (var i = 0; i < encounter.enemyDefs.Count; i++)
            {
                var enemy = Instantiate(actorPrefab, enemySpawnPoints[i].position, Quaternion.identity);
                enemy.Initialize(encounter.enemyDefs[i], false, 0f);
                _enemies.Add(enemy);
            }
        }

        private void SpawnAlly(CharacterDef def, float integrity, int spawnIndex)
        {
            var ally = Instantiate(actorPrefab, allySpawnPoints[spawnIndex].position, Quaternion.identity);
            ally.Initialize(def, true, integrity);
            ally.OnReachedFullyMended += HandleFullyMended;
            _allies.Add(ally);
        }

        private IEnumerator RunBattle()
        {
            _state = BattleState.Intro;
            battleUI.SetStateText("Battle Start");
            yield return new WaitForSeconds(0.75f);

            while (_state != BattleState.Victory && _state != BattleState.Defeat)
            {
                _state = BattleState.TurnStart;
                yield return ExecuteWarriorTurn();
                if (CheckBattleEnd()) break;

                _state = BattleState.PlayerSelectAction;
                yield return ExecuteCoraTurn();
                if (CheckBattleEnd()) break;

                _state = BattleState.EnemyTurn;
                yield return ExecuteEnemyTurn();
                if (CheckBattleEnd()) break;

                _state = BattleState.TurnEnd;
                yield return null;
            }

            if (_state == BattleState.Victory)
            {
                battleUI.SetStateText("Victory");
                PersistSummonStates();
                _session.ClearEncounter();
                yield return new WaitForSeconds(1f);
                _router.ReturnToOverworld();
            }
            else
            {
                battleUI.SetStateText("Defeat");
            }
        }

        private IEnumerator ExecuteWarriorTurn()
        {
            var warrior = _allies.FirstOrDefault(a => a.Def.id.ToLower().Contains("warrior") && !a.IsDead);
            var target = _enemies.FirstOrDefault(e => !e.IsDead);
            if (warrior == null || target == null) yield break;

            _state = BattleState.ExecuteAction;
            battleUI.SetStateText("Warrior attacks");
            BattleActionResolver.Execute(warriorAttack, warrior, new List<BattleActor> { target });
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator ExecuteCoraTurn()
        {
            var cora = _allies.FirstOrDefault(a => a.Def.id.ToLower().Contains("cora") && !a.IsDead);
            if (cora == null) yield break;

            _queuedAbility = null;
            _queuedTarget = null;
            battleUI.SetStateText("Cora: choose ability");

            while (_queuedAbility == null)
            {
                yield return null;
            }

            var targets = ResolveTargets(_queuedAbility);
            if (_queuedAbility.targetingType == TargetingType.SingleAlly)
            {
                battleUI.ShowTargetPicker(targets);
                while (_queuedTarget == null)
                {
                    yield return null;
                }

                targets = new List<BattleActor> { _queuedTarget };
            }

            _state = BattleState.ExecuteAction;
            BattleActionResolver.Execute(_queuedAbility, cora, targets);
            battleUI.ClearTargetPicker();
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator ExecuteEnemyTurn()
        {
            var enemy = _enemies.FirstOrDefault(e => !e.IsDead);
            var target = _allies.Where(a => !a.IsDead).OrderBy(_ => Random.value).FirstOrDefault();
            if (enemy == null || target == null) yield break;

            battleUI.SetStateText($"{enemy.Def.displayName} attacks");
            _state = BattleState.ExecuteAction;
            BattleActionResolver.Execute(enemyAttack, enemy, new List<BattleActor> { target });
            yield return new WaitForSeconds(0.5f);
        }

        private bool CheckBattleEnd()
        {
            if (_enemies.All(e => e.IsDead))
            {
                _state = BattleState.Victory;
                return true;
            }

            if (_allies.All(a => a.IsDead))
            {
                _state = BattleState.Defeat;
                return true;
            }

            return false;
        }

        private List<BattleActor> ResolveTargets(AbilityDef ability)
        {
            return ability.targetingType switch
            {
                TargetingType.Self => new List<BattleActor> { _allies.First(a => a.Def.id.ToLower().Contains("cora")) },
                TargetingType.SingleAlly => _allies.Where(a => !a.IsDead).ToList(),
                TargetingType.AllAllies => _allies.Where(a => !a.IsDead).ToList(),
                TargetingType.SingleEnemy => new List<BattleActor> { _enemies.First(e => !e.IsDead) },
                _ => new List<BattleActor>()
            };
        }

        private void HandlePlayerAbilitySelected(AbilityDef ability)
        {
            _queuedAbility = ability;
        }

        private void HandleTargetSelected(BattleActor actor)
        {
            _queuedTarget = actor;
        }

        private void HandleFullyMended(BattleActor actor)
        {
            if (actor.Def.isSummon)
            {
                _session.QueueFullyMendedChoice(actor.Def.id);
            }
        }

        private void PersistSummonStates()
        {
            foreach (var summon in _allies.Where(a => a.Def.isSummon))
            {
                _session.UpdateSummonIntegrity(summon.Def.id, summon.Integrity);
            }
        }
    }
}
