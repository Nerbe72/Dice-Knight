
public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,
    Stay,
}

public enum Turn
{
    PlayerSet,
    PlayerMove,
    PlayerAttack,
    EnemyMove,
    EnemyAttack,
    Count,
}

public enum DiceType
{
    Normal,
    Spear,
    Wide,
}

public enum Difficulty
{
    Tutorial = -1,
    Easy,
    Normal,
    Hard,
    Count,
}

public enum Debuff
{
    Burn,
    Slow,
}

public enum Skill
{
    No,
    Fire,
    Ice,
}

public enum Effect
{
    Rolling,
    AttackNormal,
    AttackSpear,
    AttackShotgun,
    BreakingTile,
    PutDice,

}

public enum Background
{
    Title,
    Tutorial,
    Easy,
    Normal,
    Hard,
}

public enum VolumeType
{
    MainVol,
    BGMVol,
    EffectVol,
}
