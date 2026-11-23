namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D131DarkElf;

public enum OID : uint
{
    Boss = 0x3FE2, // R=5.0
    HexingStaff = 0x3FE3, // R=1.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HexingStaves = 34777, // Boss->self, 3.0s cast, single-target
    RuinousHexVisual = 34783, // HexingStaff->self, 5.0s cast, single-target
    RuinousHex1 = 35254, // HexingStaff->self, 5.0s cast, range 40 width 8 cross
    RuinousHex2 = 34789, // Helper->self, 5.5s cast, range 40 width 8 cross
    RuinousConfluence = 35205, // Boss->self, 5.0s cast, single-target
    ShadowySigil1 = 34779, // Boss->self, 6.0s cast, single-target
    ShadowySigil2 = 34780, // Boss->self, 6.0s cast, single-target
    Explosion = 34787, // Helper->self, 6.5s cast, range 8 width 8 rect
    SorcerousShroud = 34778, // Boss->self, 5.0s cast, single-target
    VoidDarkII = 34781, // Boss->self, 2.5s cast, single-target
    VoidDarkII2 = 34788, // Helper->player, 5.0s cast, range 6 circle
    StaffSmite = 35204, // Boss->player, 5.0s cast, single-target
    AbyssalOutburst = 34782 // Boss->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    Doom = 3364 // none->player, extra=0x0
}

class HexingStaves(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(3)];
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;
    private static readonly AOEShapeCross cross = new(40f, 4f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _aoe.Casters.Count != 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var risky = _aoes[0].Activation.AddSeconds(-5d) < WorldState.CurrentTime;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            aoes[i] = aoe with { Risky = risky };
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.HexingStaff && animState2 == 1)
        {
            var delay = NumCasts switch
            {
                0 => 8.1d,
                1 => 25.9d,
                _ => 32d
            };
            _aoes.Add(new(cross, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RuinousConfluence:
                ++NumCasts;
                break;
            case (uint)AID.RuinousHex1:
            case (uint)AID.RuinousHex2:
                _aoes.Clear();
                break;
        }
    }
}

class StaffSmite(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StaffSmite);
class VoidDarkII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VoidDarkII2, 6f);
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeRect(8f, 4f));
class AbyssalOutburst(BossModule module) : Components.RaidwideCast(module, (uint)AID.AbyssalOutburst);

class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

class D131DarkElfStates : StateMachineBuilder
{
    public D131DarkElfStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StaffSmite>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<HexingStaves>()
            .ActivateOnEnter<AbyssalOutburst>()
            .ActivateOnEnter<VoidDarkII>()
            .ActivateOnEnter<Doom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12500)]
public class D131DarkElf(WorldState ws, Actor primary) : BossModule(ws, primary, new(-401f, -231f), new ArenaBoundsSquare(15.5f));
