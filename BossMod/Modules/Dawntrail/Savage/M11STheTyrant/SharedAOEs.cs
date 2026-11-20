#pragma warning disable IDE0028
namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class Cometite : Components.SimpleAOEs
{
    public Cometite(BossModule module) : base(module, (uint)AID.Cometite, 6f, 24)
    {
        MaxDangerColor = 8;
    }
}
sealed class MajesticMeteorStorm : Components.SimpleAOEs
{
    public MajesticMeteorStorm(BossModule module) : base(module, (uint)AID.MajesticMeteorStorm, 6f, 36)
    {
        MaxDangerColor = 6;
    }
}
sealed class MammothMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MammothMeteor, 18f);
sealed class AtomicImpact(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AtomicImpactIcon, (uint)AID.AtomicImpact, 5f, 6);
sealed class Comet(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CometIcon, (uint)AID.Comet, 6f, 5d);
sealed class CrushingComet(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.CrushingComet, 6f);
sealed class EyeOfTheHurricane(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.EyeOfTheHurricane, 6f, 2, 2);
sealed class Explosion(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Explosion1, (uint)AID.Explosion2], new AOEShapeRect(60f, 5f));
sealed class FireAndFury(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FireAndFuryFront, (uint)AID.FireAndFuryBack], new AOEShapeCone(60f, 45.Degrees()));
sealed class GreatWallOfFire(BossModule module) : Components.BaitAwayCast(module, (uint)AID.GreatWallOfFire, new AOEShapeRect(60f, 3f), false, true, true);
sealed class GreatWallOfFireExplosion(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GreatWallOfFire1, (uint)AID.GreatWallOfFire2, (uint)AID.GreatWallOfFireExplosion], new AOEShapeRect(60f, 3f));
sealed class FearsomeFireball(BossModule module) : Components.LineStack(module, (uint)IconID.FearsomeFireballIcon, (uint)AID.FearsomeFireball1, 5d, 60f, 4f, 4, 6, 1, true, null);
sealed class OneAndOnly(BossModule module) : Components.RaidwideCast(module, (uint)AID.OneAndOnly1, "Raidwide");
sealed class Shockwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shockwave, new AOEShapeCircle(1f)); //Just want to track casts; we actually use this AID in TripleTyrannhilation
sealed class Flatliner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flatliner, new AOEShapeRect(30f, 5f, 30f));
sealed class FlatlinerKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Flatliner1, 15f, true, 1, new AOEShapeCircle(60f));
sealed class ArcadionAvalanche(BossModule module) :
    Components.SimpleAOEGroups(module, [(uint)AID.ArcadionAvalanche_Pick1, (uint)AID.ArcadionAvalanche_Pick2,
    (uint)AID.ArcadionAvalanche_Pick3, (uint)AID.ArcadionAvalanche_Pick4], new AOEShapeRect(40f, 20f));
sealed class ArcadionAvalancheSmash(BossModule module) :
    Components.SimpleAOEGroups(module, [(uint)AID.ArcadionAvalanche_Smash1, (uint)AID.ArcadionAvalanche_Smash2,
    (uint)AID.ArcadionAvalanche_Smash3, (uint)AID.ArcadionAvalanche_Smash4], new AOEShapeRect(40f, 20f), riskyWithSecondsLeft: 5);
sealed class OrbitalOmen : Components.SimpleAOEs
{
    public OrbitalOmen(BossModule module) : base(module, (uint)AID.OrbitalOmen_Lines, new AOEShapeRect(60f, 5f), 4)
    {
        MaxDangerColor = 2;
    }
}
sealed class AtomicImpactVoidZones(BossModule module) :
    Components.Voidzone(module, 5f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AtomicImpactVoidZones);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }
        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}
sealed class DanceOfDomination(BossModule module) : Components.RaidwideCastsDelay(module, [(uint)AID.DanceOfDominationTrophy], [(uint)AID.DanceOfDomination, (uint)AID.DanceOfDomination1, (uint)AID.DanceOfDomination2, (uint)AID.DanceOfDomination3], 6f, "Multi-hit Raidwide");
sealed class Charybdistopia(BossModule module) : Components.CastHint(module, (uint)AID.Charybdistopia, "Set hp to 1");
sealed class MassiveMeteor(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MassiveMeteorIcon, (uint)AID.MassiveMeteorHit, 6f, 5, 4, 4, 5);
sealed class MaelstromVoidZones(BossModule module) : Components.GenericAOEs(module)
{
    // Adjust radius to match the real danger zone
    private static readonly AOEShapeCircle VoidZone = new(4f);

    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var maelstroms = Module.Enemies((uint)OID.Maelstrom);
        var count = maelstroms.Count;
        for (var i = 0; i < count; ++i)
        {
            _aoes.Add(new(VoidZone, maelstroms[i].Position, default, default, Colors.Danger));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }
}
sealed class MaelstromGustCones(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone GustCone = new(60f, 45f.Degrees());

    private readonly List<Actor> _maelstroms = new(4);
    private readonly List<AOEInstance> _aoes = new(8);
    public bool _resolved;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PowerfulGust)
            _resolved = true;
    }

    private readonly List<Actor> _sortedParty = new(PartyState.MaxPartySize);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_resolved)
            return default;

        _maelstroms.Clear();

        // gather maelstroms from the module's enemy list (previously scanned every actor in the world each frame)
        _maelstroms.AddRange(Module.Enemies((uint)OID.Maelstrom));

        if (_maelstroms.Count < 4)
            return default;

        _aoes.Clear();

        // For each maelstrom, find two closest players
        var party = Raid.WithoutSlot(false, true, true);
        foreach (var m in _maelstroms)
        {
            var origin = m.Position;
            _sortedParty.Clear();
            _sortedParty.AddRange(party);
            _sortedParty.Sort((a, b) => (a.Position - origin).LengthSq().CompareTo((b.Position - origin).LengthSq()));
            var n = Math.Min(2, _sortedParty.Count);
            for (var i = 0; i < n; ++i)
            {
                var dir = Angle.FromDirection(_sortedParty[i].Position - origin);
                _aoes.Add(new AOEInstance(GustCone, origin, dir));
            }
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }
}

sealed class MaelstromBaitSafeSpots(BossModule module) : BossComponent(module)
{
    private readonly PartyRolesConfig _roles = Service.Config.Get<PartyRolesConfig>();
    private MaelstromGustCones? _cones;

    private const float Distance = 6.5f;
    private const float Radius = 1f;

    public override void Update()
    {
        _cones ??= Module.FindComponent<MaelstromGustCones>();
    }

    public override void DrawArenaForeground(int slot, Actor pc)
    {
        if (pc == null || _cones == null || _cones._resolved)
            return;

        var assignments = _roles.AssignmentsPerSlot(WorldState.Party);
        if (assignments.Length == 0 || (uint)slot >= (uint)assignments.Length)
            return;

        var role = assignments[slot];

        var count = 0;
        foreach (var a in WorldState.Actors)
            if (a.OID == (uint)OID.Maelstrom)
                ++count;

        if (count < 4)
            return;

        foreach (var a in WorldState.Actors)
        {
            if (a.OID != (uint)OID.Maelstrom)
                continue;

            var pos = a.Position;
            var delta = pos - Module.Center;

            var dx = delta.X;
            var dy = delta.Z;

            WPos spot = default;

            // WEST
            if (Math.Abs(dx) > Math.Abs(dy) && dx < 0f)
            {
                if (role == PartyRolesConfig.Assignment.H1)
                    spot = Offset(pos, (-135f).Degrees());
                else if (role == PartyRolesConfig.Assignment.M1)
                    spot = Offset(pos, (-45f).Degrees());
            }
            // EAST
            else if (Math.Abs(dx) > Math.Abs(dy) && dx > 0f)
            {
                if (role == PartyRolesConfig.Assignment.R2)
                    spot = Offset(pos, 135f.Degrees());
                else if (role == PartyRolesConfig.Assignment.H2)
                    spot = Offset(pos, 45f.Degrees());
            }
            // NORTH
            else if (dy < 0f)
            {
                if (role == PartyRolesConfig.Assignment.R1)
                    spot = Offset(pos, (-135f).Degrees());
                else if (role == PartyRolesConfig.Assignment.MT)
                    spot = Offset(pos, 135f.Degrees());
            }
            // SOUTH
            else
            {
                if (role == PartyRolesConfig.Assignment.M2)
                    spot = Offset(pos, 45f.Degrees());
                else if (role == PartyRolesConfig.Assignment.OT)
                    spot = Offset(pos, (-45f).Degrees());
            }

            if (spot != default)
            {
                Arena.ZoneCircleOutline(spot, Radius, Colors.Safe);
                Arena.AddLine(pc.Position, spot, Colors.Safe);
                return;
            }
        }
    }

    private static WPos Offset(WPos origin, Angle angle)
        => origin + Distance * angle.ToDirection();
}

sealed class AtomicImpactBaitPath(BossModule module) : BossComponent(module)
{
    private int _baitSlot1 = -1;
    private int _baitSlot2 = -1;

    private int _baitCount1;
    private int _baitCount2;

    private bool _northIsWest = true;
    private bool _quadrantLocked;

    private readonly MammothMeteor? _meteors = module.FindComponent<MammothMeteor>();

    private static readonly WPos[] NorthPathWest =
    [
        new(93f, 81f),
        new(97f, 81f),
        new(103f, 81f),
        new(107f, 81f),
        new(102f, 88f),
        new(98f, 88f)
    ];

    private static readonly WPos[] NorthPathEast =
    [
        new(111f, 81f),
        new(107f, 81f),
        new(101f, 81f),
        new(97f, 81f),
        new(102f, 88f),
        new(106f, 88f)
    ];

    private static readonly WPos[] SouthPathWest =
    [
        new(93f, 119f),
        new(97f, 119f),
        new(103f, 119f),
        new(107f, 119f),
        new(102f, 112f),
        new(98f, 112f)
    ];

    private static readonly WPos[] SouthPathEast =
    [
        new(111f, 119f),
        new(107f, 119f),
        new(101f, 119f),
        new(97f, 119f),
        new(102f, 112f),
        new(106f, 112f)
    ];

    public override void Update()
    {
        TryLockQuadrant();
    }

    private void TryLockQuadrant()
    {
        if (_quadrantLocked || _meteors == null)
            return;

        var aoes = _meteors.ActiveCasters;
        if (aoes.Length < 2)
            return;

        var nw = false;

        for (var i = 0; i < aoes.Length; ++i)
        {
            ref readonly var aoe = ref aoes[i];

            if (aoe.Origin.Z < 100f && aoe.Origin.X < 100f)
            {
                nw = true;
                break;
            }
        }

        _northIsWest = !nw;
        _quadrantLocked = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.AtomicImpactIcon)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        if (slot == _baitSlot1 || slot == _baitSlot2)
            return;

        if (_baitSlot1 < 0)
            _baitSlot1 = slot;
        else
            _baitSlot2 = slot;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.AtomicImpact)
            return;

        var slot = Raid.FindSlot(spell.MainTargetID);

        if (slot == _baitSlot1)
            ++_baitCount1;
        else if (slot == _baitSlot2)
            ++_baitCount2;
    }

    private WPos NextTarget(int slot, Actor actor)
    {
        var north = actor.Position.Z < 100f;
        var west = _northIsWest;

        var index = slot == _baitSlot1 ? _baitCount1 : _baitCount2;

        if (index >= 6)
            return Module.Arena.Center;

        if (north)
            return west ? NorthPathWest[index] : NorthPathEast[index];
        else
            return west ? SouthPathWest[index] : SouthPathEast[index];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot != _baitSlot1 && slot != _baitSlot2)
            return;

        var target = NextTarget(slot, actor);
        hints.GoalZones.Add(AIHints.GoalProximity(target, 1f, 1000f));
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints hints)
    {
        if (slot != _baitSlot1 && slot != _baitSlot2)
            return;

        var target = NextTarget(slot, actor);
        hints.Add(actor.Position, target, Colors.Safe);
    }

    public override void DrawArenaForeground(int slot, Actor actor)
    {
        if (slot != _baitSlot1 && slot != _baitSlot2)
            return;

        var target = NextTarget(slot, actor);
        Module.Arena.ZoneCircleOutline(target, 1.2f, Colors.Safe);
    }
}