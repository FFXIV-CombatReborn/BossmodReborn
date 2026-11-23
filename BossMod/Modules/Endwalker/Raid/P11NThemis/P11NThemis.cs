namespace BossMod.Endwalker.Raid.P11NThemis;

public enum OID : uint
{
    Boss = 0x3EEC, // R11.000, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    IllusoryThemis = 0x3EED, // R11.000, x4
    ArcaneSphere = 0x3EEE, // R1.000, x4
    ArcaneSphere1 = 0x3EEF, // R1.000, x4
    ArcaneCylinder = 0x3EF1, // R1.000, x3
    ActorFF38E = 0xFF38E, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    DivisiveRulingLight = 33203, // Boss->self, 4.3+1.9s cast, single-target, line then it widens
    DivisiveRulingDark = 33204, // Boss->self, 4.3+1.9s cast, single-target, line then the two side lanes
    DivisiveRulingAOE = 33205, // Helper->self, 6.2s cast, range 46 width 16 rect
    DivineRuination = 33206, // Helper->self, 8.8s cast, range 46 width 26 rect
    RipplesOfGloomR = 33207, // Helper->self, 8.8s cast, range 46 width 16 rect
    RipplesOfGloomL = 33208, // Helper->self, 8.8s cast, range 46 width 16 rect
    UpheldRulingLightAOE = 33209, // Boss->players, no cast, range 6 circle, stack
    UpheldRulingDarkAOE = 33210, // Boss->player, no cast, range 13 circle, tankbuster
    LightburstAOE = 33213, // Helper->self, 5.0s cast, range 13 circle
    DarkPerimeterAOE = 33214, // Helper->self, 6.0s cast, range 8-50 donut
    DarkAndLight = 33219, // Boss->self, 4.0s cast, single-target
    SigilsOfDiscord = 33220, // Boss->self, 4.0s cast, single-target
    KatakrisisLight = 33221, // Helper->player, no cast, single-target, discord left unbalanced
    KatakrisisDark = 33222, // Helper->player, no cast, single-target
    EmissarysWill = 33223, // Boss->self, 4.0s cast, range 50 circle
    ShadowedMessengers = 33236, // Boss->self, 4.0s cast, single-target
    DivisiveRulingCloneLight = 33237, // IllusoryThemis->self, 7.7+0.5s cast, single-target
    DivisiveRulingCloneDark = 33238, // IllusoryThemis->self, 7.7+0.5s cast, single-target
    DivisiveRulingCloneAOE = 33239, // Helper->self, 8.2s cast, range 46 width 16 rect
    DivineRuinationClone = 33240, // Helper->self, 11.4s cast, range 46 width 26 rect
    RipplesOfGloomCloneR = 33241, // Helper->self, 11.4s cast, range 46 width 16 rect
    RipplesOfGloomCloneL = 33242, // Helper->self, 11.4s cast, range 46 width 16 rect
    Styx = 33243, // Boss->players, 5.0s cast, range 6 circle, stack, then 4 more hits
    StyxRepeat = 33244, // Boss->players, no cast, range 6 circle
    BlindingLight = 33248, // Boss->self, 3.0s cast, single-target
    BlindingLightAOE = 33249, // Helper->player, 5.2s cast, range 6 circle spread
    Eunomia = 33250, // Boss->self, 5.0s cast, range 50 circle
    Dike = 33251, // Boss->self, 5.0s cast, single-target
    DikeAOE1 = 33252, // Helper->player, 5.2s cast, single-target
    DikeAOE2 = 33253, // Helper->player, 5.2s cast, single-target
    DismissalRulingLight = 33272, // Boss->self, 5.5+0.5s cast, single-target
    DismissalRulingDark = 33273, // Boss->self, 5.5+0.5s cast, single-target
    DismissalRulingLightAOE = 33274, // Helper->self, 6.0s cast, range 40 circle, knockback
    DismissalRulingDarkAOE = 33275, // Helper->self, 6.0s cast, range 40 circle, knockback
    AutoAttack = 33331, // Boss->player, no cast, single-target
    Ability33333 = 33333, // Boss->location, no cast, single-target
    InnerLight = 34547, // Helper->self, 9.5s cast, range 13 circle
    OuterDark = 34548, // Helper->self, 9.5s cast, range 10-50 donut
    Dineis = 34597, // Boss->self, 3.0s cast, single-target
    DineisAOE = 34598, // Helper->location, 4.0s cast, range 4 circle
    KatakrisisLight2 = 34704, // Helper->player, no cast, single-target
    UpheldRulingLight = 34766, // Boss->self, 5.8+0.2s cast, single-target
    UpheldRulingDark = 34767, // Boss->self, 5.8+0.2s cast, single-target
}

public enum SID : uint
{
    LightsAccord = 3553, // none->player
    DarksAccord = 3554, // none->player
    LightsDiscord = 3555, // none->player, balanced by standing on the west half
    DarksDiscord = 3556, // none->player, balanced by standing on the east half
}

public enum IconID : uint
{
    Dike = 218, // player
    Styx = 305, // player
    UpheldRulingLight = 318, // player
    UpheldRulingDark = 463, // player
    BlindingLight = 466, // player
}

public enum TetherID : uint
{
    UpheldRulingDark = 192, // Boss->player
    UpheldRulingLight = 193, // Boss->player
}

sealed class EmissarysWill(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmissarysWill);
sealed class Eunomia(BossModule module) : Components.RaidwideCast(module, (uint)AID.Eunomia, "Raidwide + bleed");
sealed class DikeAOE1(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DikeAOE1);
sealed class DikeAOE2(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DikeAOE2);
class DismissalRuling(BossModule module, uint aid) : Components.SimpleKnockbacks(module, aid, 11f)
{
    private const float MaxLanding = 18.5f;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if ((pos - Arena.Center).Length() > MaxLanding)
        {
            return true;
        }

        return Module.FindComponent<DarkAndLight>()?.Sigil(slot) is var (c, r) && (pos - c).Length() > r - 0.5f;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0)
            return;
        if (Module.FindComponent<DarkAndLight>()?.Sigil(slot) is var (c, _))
        {
            var spot = c + 5 * (Module.Center - c).Normalized();
            hints.GoalZones.Add(p => Math.Max(0f, 1f - (p - spot).Length() / 4f) * 5f);
        }
        else if (PullToCenter)
        {
            var center = Module.Center;
            hints.GoalZones.Add(p => Math.Max(0f, 1f - (p - center).Length() / 8f) * 5f);
        }
    }

    protected virtual bool PullToCenter => false;
}
sealed class DismissalRulingLight(BossModule module) : DismissalRuling(module, (uint)AID.DismissalRulingLightAOE);

sealed class DismissalRulingDark(BossModule module) : DismissalRuling(module, (uint)AID.DismissalRulingDarkAOE)
{
    protected override bool PullToCenter => true;
}
sealed class InnerLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InnerLight, 13);
sealed class OuterDark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OuterDark, new AOEShapeDonut(8, 50));
sealed class Lightburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightburstAOE, 13);
sealed class DarkPerimeter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkPerimeterAOE, new AOEShapeDonut(8, 50));
sealed class Dineis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DineisAOE, 4);
sealed class BlindingLight(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BlindingLightAOE, 6);

sealed class DivisiveRuling(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _narrow = new(46, 8);
    private static readonly AOEShapeRect _wide = new(46, 13);
    private readonly List<AOEInstance> _aoes = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var deadline = _aoes[0].Activation.AddSeconds(1d);
        for (var i = 0; i < count; ++i)
        {
            var a = _aoes[i];
            if (a.Activation < deadline || a.Shape == _wide)
                _aoesBuffer.Add(a);
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.DivisiveRulingAOE or (uint)AID.DivisiveRulingCloneAOE or (uint)AID.RipplesOfGloomR or (uint)AID.RipplesOfGloomL or (uint)AID.RipplesOfGloomCloneR or (uint)AID.RipplesOfGloomCloneL => _narrow,
            (uint)AID.DivineRuination or (uint)AID.DivineRuinationClone => _wide,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DivisiveRulingAOE or (uint)AID.DivisiveRulingCloneAOE or (uint)AID.RipplesOfGloomR or (uint)AID.RipplesOfGloomL or (uint)AID.RipplesOfGloomCloneR or (uint)AID.RipplesOfGloomCloneL or (uint)AID.DivineRuination or (uint)AID.DivineRuinationClone && _aoes.Count > 0)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

sealed class UpheldRulingDark(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UpheldRulingDarkAOE, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCircle _shape = new(13);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UpheldRulingDark && WorldState.Actors.Find(caster.Tether.Target) is var target && target != null)
            CurrentBaits.Add(new(caster, target, _shape, Module.CastFinishAt(spell, 0.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.UpheldRulingDarkAOE)
            CurrentBaits.Clear();
    }
}

sealed class UpheldRulingLight(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UpheldRulingLight && WorldState.Actors.Find(caster.Tether.Target) is var target && target != null)
            AddStack(target, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UpheldRulingLightAOE)
            Stacks.Clear();
    }
}

sealed class Styx(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4)
{
    private int _hits;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Styx && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            _hits = 0;
            AddStack(target, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Styx or (uint)AID.StyxRepeat && ++_hits >= 5)
            Stacks.Clear();
    }
}

sealed class DarkAndLight(BossModule module) : BossComponent(module)
{
    private const float SafeMargin = 1.5f;
    private static readonly (WPos center, float radius) _lightSigil = (new(88, 100), 8);
    private static readonly (WPos center, float radius) _darkSigil = (new(112, 100), 8);
    private readonly int[] _side = new int[PartyState.MaxPartySize];
    private bool _active;

    private static (WPos center, float radius) SigilAt(int side) => side < 0 ? _lightSigil : _darkSigil;
    public (WPos center, float radius)? Sigil(int slot) => _active && slot is >= 0 and < PartyState.MaxPartySize && _side[slot] != 0 ? SigilAt(_side[slot]) : null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active || _side[slot] == 0)
            return;
        var (center, radius) = SigilAt(_side[slot]);
        var inside = actor.Position.InCircle(center, radius - SafeMargin);
        hints.Add(_side[slot] < 0 ? "Stand in the west (light) circle!" : "Stand in the east (dark) circle!", !inside);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_active && _side[slot] != 0)
        {
            var (center, radius) = SigilAt(_side[slot]);
            hints.AddForbiddenZone(new SDInvertedCircle(center, radius - SafeMargin));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;
        if (_side[pcSlot] != 0)
        {
            var (center, radius) = SigilAt(_side[pcSlot]);
            Arena.ZoneCircleOutline(center, radius, Colors.Safe, 2);
        }
        else
        {
            Arena.ZoneCircleOutline(_lightSigil.center, _lightSigil.radius, Colors.Object);
            Arena.ZoneCircleOutline(_darkSigil.center, _darkSigil.radius, Colors.Object);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var side = status.ID switch
        {
            (uint)SID.LightsDiscord or (uint)SID.LightsAccord => -1,
            (uint)SID.DarksDiscord or (uint)SID.DarksAccord => 1,
            _ => 0
        };
        if (side != 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _side[slot] = side;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 1 && state is 0x00020001 or 0x00080004)
        {
            _active = state == 0x00020001;
            if (!_active)
                Array.Fill(_side, 0);
        }
    }
}

sealed class P11NThemisStates : StateMachineBuilder
{
    public P11NThemisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmissarysWill>()
            .ActivateOnEnter<Eunomia>()
            .ActivateOnEnter<DikeAOE1>()
            .ActivateOnEnter<DikeAOE2>()
            .ActivateOnEnter<DismissalRulingLight>()
            .ActivateOnEnter<DismissalRulingDark>()
            .ActivateOnEnter<InnerLight>()
            .ActivateOnEnter<OuterDark>()
            .ActivateOnEnter<Lightburst>()
            .ActivateOnEnter<DarkPerimeter>()
            .ActivateOnEnter<Dineis>()
            .ActivateOnEnter<BlindingLight>()
            .ActivateOnEnter<DivisiveRuling>()
            .ActivateOnEnter<UpheldRulingDark>()
            .ActivateOnEnter<UpheldRulingLight>()
            .ActivateOnEnter<Styx>()
            .ActivateOnEnter<DarkAndLight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 940, NameID = 12388)]
public sealed class P11NThemis(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
