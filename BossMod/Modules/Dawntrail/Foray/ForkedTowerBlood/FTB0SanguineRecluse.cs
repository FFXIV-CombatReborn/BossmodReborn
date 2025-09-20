namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB0SanguineRecluse;

public enum OID : uint
{
    SanguineRecluse = 0x482E,
    ArcapetrifiedNetzach = 0x45DE // R4.505
}

public enum AID : uint
{
    AutoAttack1 = 6499, // Boss->player, no cast, single-target
    AutoAttack2 = 40541, // ArcapetrifiedNetzach->player, no cast, single-target

    TartareanFlame = 43263, // Boss->self, 5.0s cast, range 8-40 donut
    TartareanThunder = 43264, // Boss->self, 5.0s cast, range 20 circle
    Cryptcall = 43265 // Boss->self, 1.0s cast, range 35 120-degree cone
}

public enum SID : uint
{
    Doom = 3364 // Boss->player, extra=0x0
}

sealed class TartareanFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TartareanFlame, new AOEShapeDonut(8f, 40f));
sealed class TartareanThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TartareanThunder, 20f);
sealed class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

sealed class Cryptcall(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(35f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TartareanThunder)
        {
            _aoe = [new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 3d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Cryptcall)
        {
            _aoe = [];
        }
    }
}

sealed class FTB0SanguineRecluseStates : StateMachineBuilder
{
    public FTB0SanguineRecluseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TartareanFlame>()
            .ActivateOnEnter<TartareanThunder>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<Cryptcall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.SanguineRecluse, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018u, NameID = 13868u, SortOrder = 1, Category = BossModuleInfo.Category.Foray, Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class FTB0SanguineRecluse(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
