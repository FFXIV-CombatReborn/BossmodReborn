namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE64FeelingTheBurn;

public enum OID : uint
{
    Boss = 0x31A0, // R3.64
    Escort1 = 0x31A1, // R2.8
    Escort2 = 0x32FD, // R2.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBoss = 24281, // Boss->player, no cast, single-target
    AutoAttackEscort = 24319, // Escort2->player, no cast, single-target

    ReadOrdersCoordinatedAssault = 23604, // Boss->self, 19.0s cast, single-target, visual
    DiveFormation = 23606, // Escort1->self, 5.0s cast, range 60 width 6 rect aoe
    AntiPersonnelMissile1 = 23609, // Boss->self, 10.0s cast, single-target, visual (3 impact pairs)
    AntiPersonnelMissile2 = 23618, // Boss->self, 12.0s cast, single-target, visual (4 impact pairs)
    BallisticImpact = 23610, // Helper->self, no cast, range 24 width 24 rect aoe
    ReadOrdersShotsFired = 23611, // Boss->self, 3.0s cast, single-target, visual
    ChainCannonEscort = 23612, // Escort2->self, 1.0s cast, range 60 width 5 rect visual
    ChainCannonEscortAOE = 23613, // Helper->self, no cast, range 60 width 5 rect 'voidzone'
    ChainCannonBoss = 24658, // Boss->self, 3.0s cast, range 60 width 6 rect, visual
    ChainCannonBossAOE = 24659, // Helper->self, no cast, range 60 width 6 rect
    SurfaceMissile = 23614, // Boss->self, 3.0s cast, single-target, visual
    SurfaceMissileAOE = 23615, // Helper->location, 3.0s cast, range 6 circle puddle
    SuppressiveMagitekRays = 23616, // Boss->self, 5.0s cast, single-target, visual
    SuppressiveMagitekRaysAOE = 23617, // Helper->self, 5.5s cast, ???, raidwide
    Analysis = 23607, // Boss->self, 3.0s cast, single-target, visual
    PreciseStrike = 23619 // Escort1->self, 5.0s cast, range 60 width 6 rect aoe (should orient correctly to avoid vuln)
}

public enum SID : uint
{
    Tracking = 2056, // none->Escort2, extra=0x87
    FrontUnseen = 2644, // Boss->player, extra=0x120
    BackUnseen = 1709 // Boss->player, extra=0xE8
}

public enum IconID : uint
{
    BallisticImpact = 261 // Helper
}

sealed class DiveFormation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiveFormation, new AOEShapeRect(60f, 3f));

sealed class AntiPersonnelMissile(BossModule module) : Components.GenericAOEs(module, (uint)AID.BallisticImpact)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect _shape = new(12f, 12f, 12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // TODO: activation time (icon pairs are ~3s apart, but explosion pairs are ~2.6s apart; first explosion is ~2.1s after visual cast end)
        if (iconID == (uint)IconID.BallisticImpact)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(11d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == WatchedAction)
            _aoes.RemoveAt(0);
    }
}

sealed class ChainCannonEscort(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = new(6);
    public static readonly AOEShapeRect Rect = new(60f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(6);
    private DateTime activation;
    private readonly List<Actor> participants = [];
    private bool trackingactive;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var caster = _casters[i];
            if (IsTrackingPlayer(caster, pc))
                Rect.Outline(Arena, _casters[i]);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        var count = _casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var caster = _casters[i];
            if (!IsTrackingPlayer(caster, pc))
                Rect.Draw(Arena, caster);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Tracking)
        {
            _casters.Add(actor);
            trackingactive = true;
            if (activation == default)
            {
                activation = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonEscort)
        {
            _casters.Clear();
            _aoes.Add(new(Rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
        else if (spell.Action.ID == (uint)AID.SurfaceMissile)
        {
            trackingactive = false; // tracking stops 4-5 seconds before debuff disappears
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonEscortAOE)
        {
            var count = _aoes.Count;
            var pos = caster.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 0.1f))
                {
                    if (++aoe.ActorID == 6ul)
                    {
                        _aoes.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }

    private static bool IsTrackingPlayer(Actor caster, Actor actor) => caster.TargetID == actor.InstanceID;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var count = _casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var caster = _casters[i];
            if (!IsTrackingPlayer(caster, actor) || !trackingactive)
                hints.AddForbiddenZone(Rect.Distance(caster.Position, caster.Rotation), activation);
            else
            {
                var countP = participants.Count;
                if (countP == 0)
                {
                    foreach (var a in Module.WorldState.Actors.Actors.Values)
                        if (a.OID == default && a.Position.InSquare(Arena.Center, 24f))
                            participants.Add(a);
                }
                for (var j = 0; j < countP; ++j)
                {
                    var a = participants[j];
                    if (a == actor)
                        continue;
                    hints.AddForbiddenZone(new SDCone(caster.Position, 60f, caster.AngleTo(a), Angle.Asin(Rect.HalfWidth / (a.Position - caster.Position).Length())), activation);
                }
            }
        }
    }
}

sealed class ChainCannonBoss(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonBoss)
        {
            _aoe = [new(ChainCannonEscort.Rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonBossAOE)
        {
            if (++NumCasts >= 4)
            {
                _aoe = [];
                NumCasts = 0;
            }
            else
            {
                _aoe = [new(ChainCannonEscort.Rect, caster.Position.Quantized(), caster.Rotation, WorldState.FutureTime(1d))];
            }
        }
    }
}

sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissileAOE, 6f);
sealed class SuppressiveMagitekRays(BossModule module) : Components.RaidwideCast(module, (uint)AID.SuppressiveMagitekRays);
sealed class Analysis(BossModule module) : Components.CastHint(module, (uint)AID.Analysis, "Face open weakpoint to charging adds");
sealed class PreciseStrike(BossModule module) : Components.CastWeakpoint(module, (uint)AID.PreciseStrike, new AOEShapeRect(60f, 3f), (uint)SID.FrontUnseen, (uint)SID.BackUnseen, default, default);

sealed class CE64FeelingTheBurnStates : StateMachineBuilder
{
    public CE64FeelingTheBurnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiveFormation>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<ChainCannonEscort>()
            .ActivateOnEnter<ChainCannonBoss>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<SuppressiveMagitekRays>()
            .ActivateOnEnter<Analysis>()
            .ActivateOnEnter<PreciseStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 18)] // bnpcname=9945
public sealed class CE64FeelingTheBurn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240f, -230f), new ArenaBoundsSquare(24f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.Escort2));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 24f);
}
