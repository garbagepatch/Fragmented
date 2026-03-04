using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Battle
{
    public static class BattleActionResolver
    {
        public static void Execute(AbilityDef ability, BattleActor source, List<BattleActor> targets)
        {
            foreach (var effect in ability.effects)
            {
                foreach (var target in targets)
                {
                    ApplyEffect(effect, source, target);
                }
            }
        }

        private static void ApplyEffect(AbilityEffect effect, BattleActor source, BattleActor target)
        {
            switch (effect.effectType)
            {
                case EffectType.Damage:
                    var dealt = target.DealDamage(effect.amount + source.Def.attack);
                    Debug.Log($"{source.Def.displayName} hits {target.Def.displayName} for {dealt}");
                    break;
                case EffectType.Heal:
                    var healed = target.ReceiveHealing(effect.amount);
                    target.ModifyIntegrity(effect.integrityDelta);
                    Debug.Log($"{source.Def.displayName} heals {target.Def.displayName} for {healed}");
                    break;
                case EffectType.ModifyIntegrity:
                    target.ModifyIntegrity(effect.integrityDelta);
                    break;
                case EffectType.AddStatus:
                    Debug.Log($"Status {effect.statusDef?.displayName} applied to {target.Def.displayName}");
                    break;
            }
        }
    }
}
