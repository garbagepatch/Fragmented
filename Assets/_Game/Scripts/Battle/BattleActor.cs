using System;
using Game.Data;
using UnityEngine;

namespace Game.Battle
{
    public class BattleActor : MonoBehaviour
    {
        public CharacterDef Def { get; private set; }
        public int CurrentHP { get; private set; }
        public bool IsAlly { get; private set; }
        public float Integrity { get; private set; }
        public bool HasTriggeredFullyMended { get; private set; }

        public bool IsDead => CurrentHP <= 0;

        public event Action<BattleActor> OnReachedFullyMended;

        public void Initialize(CharacterDef def, bool isAlly, float startingIntegrity)
        {
            Def = def;
            IsAlly = isAlly;
            CurrentHP = def.maxHP;
            Integrity = Mathf.Clamp(startingIntegrity, def.minIntegrity, def.maxIntegrity);
        }

        public int DealDamage(int rawAmount)
        {
            var damage = Mathf.Max(1, rawAmount - Def.defense);
            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            return damage;
        }

        public int ReceiveHealing(int amount)
        {
            var oldHp = CurrentHP;
            CurrentHP = Mathf.Min(Def.maxHP, CurrentHP + amount);
            return CurrentHP - oldHp;
        }

        public void ModifyIntegrity(float amount)
        {
            if (!Def.isSummon) return;

            Integrity = Mathf.Clamp01(Integrity + amount);
            if (!HasTriggeredFullyMended && Mathf.Approximately(Integrity, 1f))
            {
                HasTriggeredFullyMended = true;
                OnReachedFullyMended?.Invoke(this);
            }
        }

        public IntegrityStage GetIntegrityStage()
        {
            if (Mathf.Approximately(Integrity, 1f)) return IntegrityStage.FullyMended;
            if (Integrity >= 0.85f) return IntegrityStage.Reclaimed;
            if (Integrity >= 0.4f) return IntegrityStage.Stabilized;
            return IntegrityStage.Fragmented;
        }
    }
}
