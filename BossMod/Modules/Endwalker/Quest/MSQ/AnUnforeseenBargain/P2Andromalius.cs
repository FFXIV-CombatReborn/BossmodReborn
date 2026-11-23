namespace BossMod.Endwalker.Quest.MSQ.AnUnforeseenBargain.P2Andromalius;

public enum OID : uint
{
    Boss = 0x3D76, // R6.0
    Voidcluster = 0x3D7D, // R0.6
    Voidcrystal = 0x3D7C, // R0.35
    VisitantOgre1 = 0x3EA6, // R1.56
    VisitantOgre2 = 0x3D79, // R1.56
    VisitantBlackguard1 = 0x3EA5, // R1.7
    VisitantBlackguard2 = 0x3EE5, // R1.7
    VisitantVoidskipper1 = 0x3D78, // R1.08
    VisitantVoidskipper2 = 0x3D77, // R1.08
    VisitantTaurus = 0x3EE4, // R1.68
    Zero = 0x3D80, // R0.54
    AlphinaudShield = 0x1EB87A, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 6499, // VisitantOgre1/VisitantVoidskipper1/VisitantOgre2/VisitantVoidskipper2->tank, no cast, single-target
    AutoAttack2 = 6497, // VisitantBlackguard1/VisitantBlackguard2->tank, no cast, single-target
    AutoAttack3 = 19052, // Boss->tank, no cast, single-target
    AutoAttack4 = 870, // VisitantTaurus->tank, no cast, single-target

    Cackle = 31820, // Boss->tank, 4.0s cast, single-target
    ChainOfCommands = 31813, // Boss->self, 9.0s cast, single-target
    SinisterSphere = 33009, // Boss->self, 4.0s cast, single-target
    VoidEvocationVisual1 = 31821, // Boss->self, no cast, single-target, limit break start
    VoidEvocationVisual2 = 31822, // Boss->self, no cast, single-target, limit break end
    VoidEvocation = 31823, // Helper->self, 1.5s cast, range 60 circle
    StraightSpindleFast = 31808, // VisitantVoidskipper1->self, 5.0s cast, range 50+R width 5 rect
    StraightSpindleSlow = 31809, // VisitantVoidskipper1->self, 9.0s cast, range 50+R width 5 rect
    StraightSpindleAdds = 33174, // VisitantVoidskipper2->self, 8.0s cast, range 50+R width 5 rect

    DarkVisual = 31814, // Boss->self, 5.0s cast, single-target
    Dark = 31815, // Helper->location, 5.0s cast, range 10 circle
    EvilMist = 31825, // Boss->self, 5.0s cast, range 60 circle
    Explosion = 33010, // Helper->self, 10.0s cast, range 5 circle
    Hellsnap = 31816, // Boss->target, 5.0s cast, range 6 circle
    Decay = 32857, // VisitantVoidskipper1->self, 13.0s cast, range 60 circle
    Voidblood = 33172, // VisitantTaurus->location, 9.0s cast, range 6 circle
    VoidSlash = 33173, // VisitantBlackguard2->self, 11.0s cast, range 8+R 90-degree cone
    VoidcluserVisual = 32932 // Voidcluster->self, no cast, single-target
}

sealed class Voidblood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Voidblood, 6f);
sealed class VoidSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidSlash, new AOEShapeCone(9.7f, 45f.Degrees()));
sealed class EvilMist(BossModule module) : Components.RaidwideCast(module, (uint)AID.EvilMist);
sealed class VoidEvocation(BossModule module) : Components.RaidwideInstant(module, (uint)AID.VoidEvocation, 5.1d)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AlphinaudShield)
            Activation = WorldState.FutureTime(Delay);
    }
}

sealed class Explosion(BossModule module) : Components.CastTowers(module, (uint)AID.Explosion, 5f);
sealed class Dark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dark, 10f);
sealed class Hellsnap(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Hellsnap, 6f);

sealed class StraightSpindle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(6)];
    public static readonly AOEShapeRect rect = new(51.08f, 2.5f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        var aoesLen = max;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < max; ++i)
        {
            aoes[i] = _aoes[i];
        }
        return aoes.AsSpan(0, aoesLen);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.StraightSpindleFast or (uint)AID.StraightSpindleSlow or (uint)AID.StraightSpindleAdds)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            if (_aoes.Count == 6)
                _aoes.Sort((x, y) => x.Activation.CompareTo(y.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.StraightSpindleFast or (uint)AID.StraightSpindleSlow or (uint)AID.StraightSpindleAdds)
            for (var i = 0; i < _aoes.Count; ++i)
            {
                if (_aoes[i].ActorID == caster.InstanceID)
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }
    }
}

sealed class Decay(BossModule module) : Components.CastHint(module, (uint)AID.Decay, "Kill the Voidskipper!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.CastInfo?.Action.ID == WatchedAction)
                e.Priority = 5;
        }
    }
}

sealed class Shield(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5f, true);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AlphinaudShield)
        {
            _aoe = [new(circle, actor.Position, color: Colors.SafeFromAOE)];
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        hints.Add("Go under shield!", !aoe.Check(actor.Position));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VoidEvocation)
        {
            _aoe = [];
        }
    }
}

sealed class ProtectZero(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var zeros = Module.Enemies((uint)OID.Zero);
        Actor? zero = null;
        for (var i = 0; i < zeros.Count; ++i)
        {
            var zer0 = zeros[i];
            if (zer0.FindStatus(2056u) != null)
            {
                zero = zer0;
                break;
            }
        }
        if (zero != null)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = hints.PotentialTargets[i];
                if (e.Actor.TargetID == zero.InstanceID)
                    e.Priority = 5;
            }
        }
    }
}

sealed class AndromaliusStates : StateMachineBuilder
{
    public AndromaliusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidEvocation>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<EvilMist>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Hellsnap>()
            .ActivateOnEnter<Decay>()
            .ActivateOnEnter<Shield>()
            .ActivateOnEnter<Voidblood>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<ProtectZero>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70209u, NameID = 12071u)]
public sealed class Andromalius(WorldState ws, Actor primary) : BossModule(ws, primary, P1Furcas.Furcas.ArenaBounds.Center, P1Furcas.Furcas.ArenaBounds)
{
    private static readonly uint[] trash = [(uint)OID.VisitantTaurus, (uint)OID.VisitantVoidskipper1, (uint)OID.VisitantVoidskipper2,
    (uint)OID.VisitantOgre1, (uint)OID.VisitantOgre2, (uint)OID.VisitantBlackguard1, (uint)OID.VisitantBlackguard2, (uint)OID.Voidcluster];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, trash);
    }
}
