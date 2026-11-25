namespace BossMod.Shadowbringers.Raid.E11Fatebreaker;

public enum OID : uint
{
    Boss = 0x3070, // R5.004, Fatebreaker
    Helper = 0x233C, // R0.500
    FatebreakersImage = 0x3071, // R5.004
    BlastingZoneHelper = 0x3076, // R0.500
    DemiGukumatz = 0x3073, // R3.780, snake
    HaloOfFlame = 0x3074, // R1.000
    HaloOfLevin = 0x3075, // R1.000
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    BossMove = 22792, // Boss->location, no cast

    BurntStrikeFire = 22060, // Boss->self, 8.0s cast, range 80 width 10 rect
    Blastburn = 22061, // Helper->self, 10.0s cast, range 80 width 50 rect (knockback from line)
    BurntStrikeLightning = 22062, // Boss->self, 8.0s cast, range 80 width 10 rect
    Burnout = 22063, // Helper->self, 9.7s cast, range 80 width 20 rect
    BurntStrikeLight = 22064, // Boss->self, 8.0s cast, range 80 width 10 rect
    ShiningBlade = 22065, // Helper->location, 3.0s cast, range 6 circle

    BoundOfFaithFire = 19224, // Boss->self, 8.0s cast, single-target
    BoundOfFaithLightning = 19225, // Boss->self, 8.0s cast, single-target
    BoundOfFaithLight = 19227, // Boss->self, 8.0s cast, single-target
    FloatingFetters = 22772, // Boss->player, no cast, single-target
    SolemnChargeFire = 22066, // Boss->player, no cast, single-target
    Sinsmoke = 22067, // Helper->player, no cast, range 6 circle
    SolemnChargeLightning = 22068, // Boss->player, no cast, single-target
    Sinsmite = 22069, // Helper->player, no cast, single-target
    SolemnChargeLight = 22070, // Boss->player, no cast, single-target
    Sinsight = 22071, // Helper->player, no cast, range 6 circle
    MortalBurnMark = 22072, // Helper->self, no cast, range 40 circle

    BurnishedGlory = 22096, // Boss->self, 5.0s cast, range 40 circle
    PowderMark = 22094, // Boss->player, 5.0s cast, single-target
    BurnMark = 22095, // Helper->self, no cast, range 10 circle

    TurnOfTheHeavensFire = 22073, // Boss->self, 8.0s cast, single-target
    TurnOfTheHeavensLightning = 22074, // Boss->self, 8.0s cast, single-target
    BrightfireFlame = 22076, // HaloOfFlame->self, 8.0s cast, range 10 circle
    BrightfireLevin = 22075, // HaloOfLevin->self, 8.0s cast, range 5 circle

    ShiftingSky = 22079, // Boss->self, 8.0s cast, single-target
    ShiftingSkyKB = 22091, // Helper->self, 4.0s cast, range 40+R circle
    AgelessSerpent = 22092, // DemiGukumatz->self, no cast, range 30+R circle (knockback)
    ResoundingCrack = 22093, // DemiGukumatz->self, 5.0s cast, range 40 270-degree cone
    CloneBurntStrikeFire = 22083, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect
    CloneBlastburn = 22084, // Helper->self, 10.0s cast, range 80 width 50 rect
    CloneBurntStrikeLightning = 22085, // FatebreakersImage->self, 8.0s cast, range 80 width 10 rect
    CloneBurnout = 22086, // Helper->self, 9.7s cast, range 80 width 20 rect

    PrismaticDeception = 22077, // Boss->self, 6.0s cast, single-target
    BlastingZoneVis = 22097, // BlastingZoneHelper->self, 14.0s cast, single-target
    BlastingZone = 22078, // FatebreakersImage->self, no cast, range 50 width 16 rect
}

public enum SID : uint
{
    PowderMark = 2451, // Boss->player
    Invisible = 1621, // none->FatebreakersImage
    BlastingZone = 1678, // none->FatebreakersImage
}

public enum IconID : uint
{
    Sinsmoke = 100, // player stack
    Sinsight = 101, // player spread
}

public enum TetherID : uint
{
    BoundOfFaithFire = 5, // Boss->player
    BoundOfFaithLightning = 6, // Boss->player
    BoundOfFaithLight = 2, // Boss->player
}

sealed class BurnishedGlory(BossModule module) : Components.RaidwideCast(module, (uint)AID.BurnishedGlory);
sealed class PowderMark(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PowderMark);

sealed class BurntStrike(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BurntStrikeFire, (uint)AID.BurntStrikeLightning, (uint)AID.BurntStrikeLight, (uint)AID.CloneBurntStrikeFire, (uint)AID.CloneBurntStrikeLightning], new AOEShapeRect(80, 5));

sealed class Burnout(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Burnout, (uint)AID.CloneBurnout], new AOEShapeRect(80, 10));
sealed class ShiningBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShiningBlade, 6);

sealed class Blastburn(BossModule module) : Components.GenericKnockback(module)
{
    public const float Distance = 12f;
    private readonly List<(Actor Caster, WPos Origin, Angle Rotation, DateTime Activation)> _sources = [];
    private readonly List<Knockback> _kbs = [];

    private static WPos NearestOnLine(WPos p, WPos origin, Angle rotation)
    {
        var dir = rotation.ToDirection();
        return origin + (p - origin).Dot(dir) * dir;
    }

    // knocked away from the nearest point of each line
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbs.Clear();
        for (var i = 0; i < _sources.Count; ++i)
        {
            var s = _sources[i];
            _kbs.Add(new(NearestOnLine(actor.Position, s.Origin, s.Rotation), Distance, s.Activation));
        }
        return CollectionsMarshal.AsSpan(_kbs);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Blastburn or (uint)AID.CloneBlastburn)
            _sources.Add((caster, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Blastburn or (uint)AID.CloneBlastburn)
        {
            for (var i = _sources.Count - 1; i >= 0; --i)
            {
                if (_sources[i].Caster == caster)
                    _sources.RemoveAt(i);
            }
        }
    }
}

sealed class Sinsmoke(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Sinsmoke, (uint)AID.Sinsmoke, 6, 8);
sealed class Sinsight(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Sinsight, (uint)AID.Sinsight, 6, 8);

sealed class BurnMark(BossModule module) : Components.UniformStackSpread(module, 0, 10)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.PowderMark)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BurnMark)
            Spreads.Clear();
    }
}

sealed class BrightfireFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrightfireFlame, 10);
sealed class BrightfireLevin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrightfireLevin, 5);

sealed class ShiftingSkyKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ShiftingSkyKB, 15);
sealed class ResoundingCrack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ResoundingCrack, new AOEShapeCone(40, 135.Degrees()));

sealed class PrismaticDeception(BossModule module) : Components.CastHint(module, (uint)AID.PrismaticDeception, "Walk near the edge to find the clones.");

sealed class BlastingZone(BossModule module) : Components.GenericAOEs(module, (uint)AID.BlastingZone)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(50, 8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.BlastingZoneVis || _aoes.Count > 0)
            return;

        var act = Module.CastFinishAt(spell);
        foreach (var image in Module.Enemies((uint)OID.FatebreakersImage))
        {
            if (image.FindStatus((uint)SID.BlastingZone) != null || image.FindStatus((uint)SID.Invisible) != null)
            {
                // images face outward, lines fire across the arena toward center
                var dir = Angle.FromDirection(Module.Center - image.Position);
                _aoes.Add(new(_shape, image.Position, dir, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

sealed class E11FatebreakerStates : StateMachineBuilder
{
    public E11FatebreakerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurnishedGlory>()
            .ActivateOnEnter<PowderMark>()
            .ActivateOnEnter<BurnMark>()
            .ActivateOnEnter<BurntStrike>()
            .ActivateOnEnter<Burnout>()
            .ActivateOnEnter<Blastburn>()
            .ActivateOnEnter<ShiningBlade>()
            .ActivateOnEnter<Sinsmoke>()
            .ActivateOnEnter<Sinsight>()
            .ActivateOnEnter<BrightfireFlame>()
            .ActivateOnEnter<BrightfireLevin>()
            .ActivateOnEnter<ShiftingSkyKnockback>()
            .ActivateOnEnter<ResoundingCrack>()
            .ActivateOnEnter<PrismaticDeception>()
            .ActivateOnEnter<BlastingZone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 751, NameID = 9707)]
public sealed class E11Fatebreaker(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FatebreakersImage), Colors.Object);
        Arena.Actors(Enemies((uint)OID.DemiGukumatz));
    }
}
