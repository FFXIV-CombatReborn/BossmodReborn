namespace BossMod.Endwalker.Raid.P4NHesperos;

public enum OID : uint
{
    Boss = 0x3602, // R5.700, x1
    Helper = 0x233C, // R0.500, x38, Helper type
    SceneSetter = 0x35FE, // R1.000, x4, one per corner, casts before setting the scene
    ExplosiveAether = 0x3603, // R1.000, x8, belone bursts orbs
    Actor3849 = 0x3849, // R1.000, x2
    Actor384A = 0x384A, // R1.000, x2
    ActorFE371 = 0xFE371, // R0.500, x1, EventNpc type
    ActorFE372 = 0xFE372, // R0.500, x1, EventNpc type
    ActorFE373 = 0xFE373, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1E8536 = 0x1E8536, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Ability27082 = 27082, // Boss->self, no cast, single-target
    SettingTheScene = 27083, // Boss->self, 4.0s cast, single-target
    SceneSetterVisual = 27084, // SceneSetter->self, 2.0s cast, single-target
    SceneSetterEnd = 27085, // SceneSetter->self, no cast, single-target
    Ability27086 = 27086, // Boss->location, no cast, single-target
    Pinax = 27087, // Boss->self, 5.0s cast, single-target
    BeloneBursts = 27097, // Boss->self, 5.0s cast, single-target
    BeloneCoils = 27101, // Boss->self, 4.0s cast, single-target
    AcidMekhane = 27192, // Helper->players, 5.0s cast, range 5 circle spread
    LavaMekhane = 27193, // Helper->players, 5.0s cast, range 6 circle, not seen yet
    WellMekhane = 27194, // Helper->self, 1.0s cast, range 60 circle, knockback 13 from center
    LevinstrikeMekhane = 27195, // Helper->self, 2.0s cast, range 60 circle, damage falls off with distance from center
    AcidPinax = 27196, // Helper->self, 9.0s cast, single-target, quadrant
    LavaPinax = 27197, // Helper->self, 9.0s cast, single-target, quadrant, not seen yet
    WellPinax = 27198, // Helper->self, 9.0s cast, single-target, quadrant
    LevinstrikePinax = 27199, // Helper->self, 9.0s cast, single-target, quadrant
    Bloodrake = 27200, // Boss->self, 5.0s cast, range 40 circle
    BurstOrbTank = 27201, // ExplosiveAether->player, no cast, single-target
    BurstOrbHealer = 27202, // ExplosiveAether->player, no cast, single-target
    BurstOrbDPS = 27203, // ExplosiveAether->player, no cast, single-target
    BurstTowerNoTank = 27204, // Helper->self, 10.0s cast, range 4 circle
    BurstTowerNoHealer = 27205, // Helper->self, 10.0s cast, range 4 circle
    BurstTowerNoDPS = 27206, // Helper->self, 10.0s cast, range 4 circle
    NortherlyShiftSword = 27210, // Boss->location, 7.0s cast, single-target
    SoutherlyShiftSword = 27211, // Boss->self, 7.0s cast, single-target
    EasterlyShiftSword = 27212, // Boss->self, 7.0s cast, single-target
    WesterlyShiftSword = 27213, // Boss->self, 7.0s cast, single-target
    ShiftingStrikeSword = 27214, // Helper->self, 8.5s cast, range 60 120-degree cone
    HellSkewer = 27215, // Boss->self, 5.0s cast, range 60 width 6 rect
    ElegantEvisceration = 27216, // Boss->players, 5.0s cast, range 5 circle
    Decollation = 27217, // Boss->self, 5.0s cast, range 40 circle
    NortherlyShiftCloak = 28078, // Boss->location, 7.0s cast, single-target
    SoutherlyShiftCloak = 28079, // Boss->self, 7.0s cast, single-target
    EasterlyShiftCloak = 28080, // Boss->self, 7.0s cast, single-target
    WesterlyShiftCloak = 28081, // Boss->self, 7.0s cast, single-target
    ShiftingStrikeCloak = 28082, // Helper->self, 8.7s cast, range 60 circle, knockback
}

public enum SID : uint
{
    OrbRole = 2056, // none->ExplosiveAether, extra=0x13A tank, 0x13B dps, 0x13C healer
    ThriceComeRuin = 2530, // ExplosiveAether->player, extra=stacks
}

public enum IconID : uint
{
    AcidMekhane = 96, // player
    ElegantEvisceration = 218, // player
    ShiftSword = 312, // Helper
    ShiftCloak = 313, // Helper
}

public enum TetherID : uint
{
    Orb = 17, // ExplosiveAether->player
}

sealed class ElegantEvisceration(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ElegantEvisceration, new AOEShapeCircle(5), true);
sealed class Bloodrake(BossModule module) : Components.RaidwideCast(module, (uint)AID.Bloodrake);
sealed class Decollation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Decollation);
sealed class HellSkewer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HellSkewer, new AOEShapeRect(60, 3));
sealed class ShiftingStrikeSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShiftingStrikeSword, new AOEShapeCone(60, 60.Degrees()));
sealed class ShiftingStrikeCloak(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ShiftingStrikeCloak, 23);
sealed class ShiftDirection(BossModule module) : BossComponent(module)
{
    private string? _hint;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_hint != null)
            hints.Add(_hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var hint = spell.Action.ID switch
        {
            (uint)AID.NortherlyShiftSword => "Sword cleave from the north",
            (uint)AID.SoutherlyShiftSword => "Sword cleave from the south",
            (uint)AID.EasterlyShiftSword => "Sword cleave from the east",
            (uint)AID.WesterlyShiftSword => "Sword cleave from the west",
            (uint)AID.NortherlyShiftCloak => "Knockback from the north",
            (uint)AID.SoutherlyShiftCloak => "Knockback from the south",
            (uint)AID.EasterlyShiftCloak => "Knockback from the east",
            (uint)AID.WesterlyShiftCloak => "Knockback from the west",
            _ => null
        };
        if (hint != null)
            _hint = hint;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShiftingStrikeSword or (uint)AID.ShiftingStrikeCloak)
            _hint = null;
    }
}

sealed class AcidMekhane(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AcidMekhane, 5);
sealed class LavaMekhane(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.LavaMekhane, 6);

sealed class PinaxTileCenters(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(2.5f);
    private readonly bool[] _placed = new bool[4];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        for (var i = 0; i < _placed.Length; ++i)
            if (_placed[i])
                _aoesBuffer.Add(new(Shape, TileCenter(i)));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 5 and <= 20 && state is 0x00020001 or 0x02000001 or 0x08000004)
            _placed[(index - 5) & 3] = state == 0x00020001;
    }

    private WPos TileCenter(int corner) => Module.Center + 10 * corner switch
    {
        0 => new WDir(+1, -1),
        1 => new WDir(+1, +1),
        2 => new WDir(-1, +1),
        _ => new WDir(-1, -1)
    };
}

sealed class LevinstrikeMekhane(BossModule module) : Components.GenericAOEs(module, (uint)AID.LevinstrikeMekhane, "Go to a corner!")
{
    private static readonly AOEShapeCircle Shape = new(20);
    private AOEInstance? _aoe;

    private readonly AOEInstance[] _aoeBuffer = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is not AOEInstance aoe)
            return [];
        _aoeBuffer[0] = aoe;
        return _aoeBuffer;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LevinstrikePinax)
            _aoe = new(Shape, Module.Center, default, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LevinstrikeMekhane)
            _aoe = null;
    }
}

sealed class WellMekhane(BossModule module) : Components.GenericKnockback(module, (uint)AID.WellMekhane)
{
    private DateTime _activation;

    private readonly List<Knockback> _kbBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbBuffer.Clear();
        if (_activation != default)
            _kbBuffer.Add(new(Module.Center, 13, _activation));
        return CollectionsMarshal.AsSpan(_kbBuffer);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WellPinax)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WellMekhane)
            _activation = default;
    }
}

sealed class BeloneCoils(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Func<Actor, bool>? banned = spell.Action.ID switch
        {
            (uint)AID.BurstTowerNoTank => a => a.Role == Role.Tank,
            (uint)AID.BurstTowerNoHealer => a => a.Role == Role.Healer,
            (uint)AID.BurstTowerNoDPS => a => a.Role is Role.Melee or Role.Ranged,
            _ => null
        };
        if (banned != null)
            Towers.Add(new(caster.Position, 4, 1, 8, Raid.WithSlot().WhereActor(banned).Mask(), Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BurstTowerNoTank or (uint)AID.BurstTowerNoHealer or (uint)AID.BurstTowerNoDPS)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

sealed class BeloneBursts(BossModule module) : BossComponent(module)
{
    private const float PopRadius = 2.5f;
    private readonly Dictionary<ulong, Role> _orbRoles = [];
    private readonly int[] _ruin = new int[PartyState.MaxPartySize];

    private static Role GroupRole(Role r) => r == Role.Ranged ? Role.Melee : r;
    private bool Lethal(int slot, Actor player, Role orbRole) => _ruin[slot] >= 2 || orbRole == GroupRole(player.Role);
    private readonly List<Actor> _orbs = module.Enemies((uint)OID.ExplosiveAether);
    private readonly List<(Actor orb, Role role)> _liveOrbs = [];

    private List<(Actor orb, Role role)> LiveOrbs
    {
        get
        {
            _liveOrbs.Clear();
            for (var i = 0; i < _orbs.Count; ++i)
            {
                var o = _orbs[i];
                if (!o.IsDead && _orbRoles.TryGetValue(o.InstanceID, out var role))
                    _liveOrbs.Add((o, role));
            }
            return _liveOrbs;
        }
    }

    private bool InWrongOrb(int slot, Actor actor)
    {
        foreach (var o in LiveOrbs)
            if (Lethal(slot, actor, o.role) && o.orb.Position.InCircle(actor.Position, PopRadius))
                return true;
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orbRoles.Count == 0)
            return;
        if (InWrongOrb(slot, actor))
            hints.Add("GTFO from wrong orb!");
        else if (_ruin[slot] < 2)
            hints.Add("Intercept an orb of another role", false);
        else
            hints.Add("Avoid orbs", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (orb, role) in LiveOrbs)
            if (Lethal(slot, actor, role))
                hints.AddForbiddenZone(new SDCircle(orb.Position, PopRadius + 1));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (orb, role) in LiveOrbs)
        {
            var lethal = Lethal(pcSlot, pc, role);
            Arena.Actor(orb, lethal ? Colors.Enemy : Colors.Object, true);
            Arena.ZoneCircleOutline(orb.Position, PopRadius, lethal ? Colors.Danger : Colors.Safe);
            if (WorldState.Actors.Find(orb.Tether.Target) is var target && target != null)
                Arena.AddLine(orb.Position, target.Position, Colors.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.OrbRole:
                _orbRoles[actor.InstanceID] = status.Extra switch
                {
                    0x13A => Role.Tank,
                    0x13C => Role.Healer,
                    _ => Role.Melee
                };
                break;
            case (uint)SID.ThriceComeRuin:
                if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                    _ruin[slot] = status.Extra;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ThriceComeRuin && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _ruin[slot] = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BurstOrbTank or (uint)AID.BurstOrbHealer or (uint)AID.BurstOrbDPS)
            _orbRoles.Remove(caster.InstanceID);
    }
}

sealed class P4NHesperosStates : StateMachineBuilder
{
    public P4NHesperosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElegantEvisceration>()
            .ActivateOnEnter<Bloodrake>()
            .ActivateOnEnter<Decollation>()
            .ActivateOnEnter<HellSkewer>()
            .ActivateOnEnter<ShiftingStrikeSword>()
            .ActivateOnEnter<ShiftingStrikeCloak>()
            .ActivateOnEnter<ShiftDirection>()
            .ActivateOnEnter<AcidMekhane>()
            .ActivateOnEnter<LavaMekhane>()
            .ActivateOnEnter<PinaxTileCenters>()
            .ActivateOnEnter<LevinstrikeMekhane>()
            .ActivateOnEnter<WellMekhane>()
            .ActivateOnEnter<BeloneCoils>()
            .ActivateOnEnter<BeloneBursts>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 800, NameID = 10742)]
public sealed class P4NHesperos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
