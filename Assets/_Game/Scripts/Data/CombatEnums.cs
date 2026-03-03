namespace Game.Data
{
    public enum TargetingType
    {
        Self,
        SingleAlly,
        AllAllies,
        SingleEnemy
    }

    public enum EffectType
    {
        Heal,
        Damage,
        AddStatus,
        ModifyIntegrity
    }

    public enum IntegrityStage
    {
        Fragmented,
        Stabilized,
        Reclaimed,
        FullyMended
    }

    public enum BattleState
    {
        Intro,
        TurnStart,
        PlayerSelectAction,
        ExecuteAction,
        EnemyTurn,
        TurnEnd,
        Victory,
        Defeat
    }

    public enum SummonChoice
    {
        None,
        LetRest,
        StayTogether
    }
}
