using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

// Normal 魔之塔 Boss4: Index. 封印武器、居合斩/风斩、预言现象、元素扇区与击退。
sealed class IndexAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle Harp = new(15f);
    private static readonly AOEShapeCircle Bow = new(11f);
    private static readonly AOEShapeCone Iainuki = new(30f, 30f.Degrees());
    private static readonly AOEShapeCircle Starfall = new(10f);
    private static readonly AOEShapeDonut Cleansing = new(5f, 15f);

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.RomeosBallad => new(Harp),
        (uint)AID.Aim => new(Bow),
        (uint)AID.Iainuki or (uint)AID.WindSlash => new(Iainuki),
        (uint)AID.Starfall => new(Starfall),
        (uint)AID.Cleansing => new(Cleansing),
        _ => null
    };
}

// Elementary Chemistry is resolved by three outer helpers, each covering the full
// platform immediately inside it. The helper cast rotation is not reliable in ARR,
// so derive the direction from the helper position to the mechanic center instead.
sealed class ElementaryChemistryPlatforms(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(15f, 7.5f);
    private readonly List<AOEInstance> _aoes = [with(3)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.RemoveAll(a => WorldState.CurrentTime > a.Activation.AddSeconds(1d));
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.UnknownWeaponskill2 || spell.EventHappened)
            return;

        var origin = caster.Position;
        var rotation = Angle.FromDirection(IndexArenaBounds.MechanicCenter - origin);
        var activation = Module.CastFinishAt(spell);
        _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
        _aoes.Add(new(Shape, origin, rotation, activation, actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnknownWeaponskill2)
            _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UnknownWeaponskill2)
            _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
    }
}

// Fire/Ice/Thunder sector EventObjects define one 60-degree sector and its opposite.
// Balls rotate clockwise to their element (7s base + 1s/30 degrees); ARR places ring hits 6.8s after spawn.
sealed class ElementalSectors(BossModule module) : Components.GenericAOEs(module)
{
    private enum Element { Fire, Ice, Thunder }
    private enum Mechanic { Ball, Ring }
    private sealed record Pending(Element Element, Mechanic Mechanic, ulong SourceID, DateTime Activation, AOEInstance First, AOEInstance Second);

    private static readonly AOEShapeCone Shape = new(30f, 30f.Degrees());
    private readonly Dictionary<Element, Angle> _sectorRotations = [];
    private readonly List<Pending> _pending = [with(12)];
    private readonly List<AOEInstance> _displayed = [with(24)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Prune();
        _displayed.Clear();
        var ordered = _pending.OrderBy(p => p.Activation).ToArray();
        var riskyDeadline = ordered.Length > 0 ? ordered[0].Activation.AddSeconds(0.5d) : DateTime.MinValue;
        foreach (var p in ordered)
        {
            var risky = p.Activation <= riskyDeadline;
            var first = p.First;
            var second = p.Second;
            first.Risky = second.Risky = risky;
            first.Color = second.Color = risky ? Colors.Danger : Colors.AOE;
            _displayed.Add(first);
            _displayed.Add(second);
        }
        return CollectionsMarshal.AsSpan(_displayed);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (SectorElement(actor.OID) is { } sector)
        {
            _sectorRotations[sector] = actor.Rotation;
            return;
        }

        if (RingElement(actor.OID) is { } ring)
        {
            Schedule(ring, Mechanic.Ring, actor.InstanceID, WorldState.FutureTime(7.0d));
            return;
        }

        if (BallElement(actor.OID) is not { } ball || !_sectorRotations.TryGetValue(ball, out var destination))
            return;

        var source = Angle.FromDirection(actor.Position - IndexArenaBounds.MechanicCenter);
        var delta = (source - destination).Normalized().Rad;
        if (delta < 0f)
            delta += MathF.PI;
        if (delta >= MathF.PI)
            delta -= MathF.PI;
        Schedule(ball, Mechanic.Ball, actor.InstanceID, WorldState.FutureTime(7d + delta * Angle.RadToDeg / 30f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var element = spell.Action.ID switch
        {
            (uint)AID.FireIV => Element.Fire,
            (uint)AID.BlizzardIV => Element.Ice,
            (uint)AID.ThunderIV => Element.Thunder,
            _ => (Element?)null
        };
        if (element is not { } e)
            return;

        var now = WorldState.CurrentTime;
        var next = _pending.Where(p => p.Element == e && p.Activation <= now.AddSeconds(1d)).MinBy(p => p.Activation);
        if (next != null)
            _pending.Remove(next);
    }

    public override void Update() => Prune();

    private void Schedule(Element element, Mechanic mechanic, ulong sourceID, DateTime activation)
    {
        if (!_sectorRotations.TryGetValue(element, out var rotation))
            return;
        if (_pending.Any(p => p.Element == element && p.Mechanic == mechanic && Math.Abs((p.Activation - activation).TotalSeconds) < 0.5d))
            return;
        _pending.RemoveAll(p => p.SourceID == sourceID);
        _pending.Add(new(element, mechanic, sourceID, activation,
            new(Shape, IndexArenaBounds.MechanicCenter, rotation, activation, actorID: sourceID),
            new(Shape, IndexArenaBounds.MechanicCenter, rotation + 180f.Degrees(), activation, actorID: sourceID)));
    }

    private void Prune() => _pending.RemoveAll(p => WorldState.CurrentTime > p.Activation.AddSeconds(1d));

    private static Element? SectorElement(uint oid) => oid switch
    {
        (uint)OID.FireSector => Element.Fire,
        (uint)OID.IceSector => Element.Ice,
        (uint)OID.ThunderSector => Element.Thunder,
        _ => null
    };

    private static Element? RingElement(uint oid) => oid switch
    {
        (uint)OID.FireRing => Element.Fire,
        (uint)OID.IceRing => Element.Ice,
        (uint)OID.ThunderRing => Element.Thunder,
        _ => null
    };

    private static Element? BallElement(uint oid) => oid switch
    {
        (uint)OID.BallOfFire => Element.Fire,
        (uint)OID.SwirlingOrb => Element.Ice,
        (uint)OID.BallOfLevin => Element.Thunder,
        _ => null
    };
}

// ARR has two helper casts per lance; deduplicate them by location so AI receives three real sources.
sealed class PropulsiveShockwave(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCircle AffectedArea = new(15f);
    private readonly List<Knockback> _sources = [with(3)];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Shockwave || spell.EventHappened)
            return;
        var activation = Module.CastFinishAt(spell);
        if (_sources.Any(s => (s.Origin - caster.Position).LengthSq() < 1f && Math.Abs((s.Activation - activation).TotalSeconds) < 1d))
            return;
        _sources.Add(new(caster.Position, 10f, activation, AffectedArea, actorID: caster.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
            _sources.RemoveAll(s => s.ActorID == caster.InstanceID);
    }

    public override void Update()
    {
        _sources.RemoveAll(s => WorldState.CurrentTime > s.Activation.AddSeconds(1d));
        base.Update();
    }
}

sealed class AllConsumingFlames(BossModule module) : Components.SpreadFromIcon(module,
    (uint)IconID.Icon_loc06sp_05ak1, (uint)AID.AllConsumingFlames, 6f, 5.1d);

// 老四场地：中央大六边形内有会掉落的六边形空洞，外侧六条边各接一块方台。
// 坐标来自运行时鼠标取点；Y 的小误差按平台基准忽略。
static class IndexArenaBounds
{
    // ARR NPC spawn (opcode 0x006F, OID 0x4B5F) confirms X=0, Z=-628.
    // Keep this separate from ArenaBoundsCustom.Center: the asymmetric three-platform
    // opening has a bounding-box center north of the actual mechanic origin.
    public static readonly WPos MechanicCenter = new(0f, -628f);

    // measured outer hexagon, clockwise when viewed from above
    public static readonly WPos[] LargeHexagon =
    [
        new(-7.41f, -615.00f), new(7.38f, -614.87f), new(15.03f, -628.07f),
        new(7.47f, -640.96f), new(-7.45f, -641.02f), new(-15.20f, -627.75f)
    ];

    // The captured points are noisy because they were taken from the mouse cursor;
    // the reliable extrema describe a regular six-sided hole around the center.
    public static readonly WPos[] InnerHole =
    [
        new(-3.00f, -622.92f), new(3.00f, -622.92f), new(6.00f, -628.00f),
        new(3.00f, -633.02f), new(-3.00f, -633.02f), new(-6.00f, -628.00f)
    ];

    // measured north platform; the other five are rotations around MechanicCenter
    public static readonly WPos[] NorthPlatform =
    [
        new(-7.43f, -600.03f), new(7.40f, -600.03f),
        new(7.38f, -614.87f), new(-7.41f, -615.00f)
    ];

    public static readonly WPos[][] Platforms = BuildPlatforms();
    // Opening state is upper-left, upper-right and south in the player's arena view.
    // The opposite parity ([1,3,5]) is the upper/south-left/south-right set that was
    // incorrectly used before and made the initial outline appear mirrored.
    public static readonly int[] OpeningPlatformIndices = [0, 2, 4];
    public static readonly int[] CompletePlatformIndices = [0, 1, 2, 3, 4, 5];
    public static readonly ArenaBoundsCustom OpeningBounds = BuildBounds(OpeningPlatformIndices);
    public static readonly ArenaBoundsCustom CompleteBounds = BuildBounds(CompletePlatformIndices);

    private static WPos[][] BuildPlatforms()
    {
        var result = new WPos[6][];
        for (var i = 0; i < result.Length; ++i)
        {
            var rotation = (60f * i).Degrees();
            result[i] = new WPos[NorthPlatform.Length];
            for (var v = 0; v < NorthPlatform.Length; ++v)
                result[i][v] = MechanicCenter + (NorthPlatform[v] - MechanicCenter).Rotate(rotation);

            // Mouse-captured platform points are slightly noisy. Reuse the exact
            // hexagon vertices for the two inner corners so the platform joins do not
            // leave gaps when ArenaBoundsCustom unions the polygons.
            result[i][2] = LargeHexagon[(i + 1) % 6];
            result[i][3] = LargeHexagon[i];
        }
        return result;
    }

    private static ArenaBoundsCustom BuildBounds(int[] platformIndices)
    {
        var union = new Shape[1 + platformIndices.Length];
        union[0] = new PolygonCustom(LargeHexagon);
        for (var i = 0; i < platformIndices.Length; ++i)
            union[i + 1] = new PolygonCustom(Platforms[platformIndices[i]]);

        return new ArenaBoundsCustom(union, [new PolygonCustom(InnerHole)]);
    }
}

sealed class IndexArenaOutline(BossModule module) : BossComponent(module)
{
    private int[] _activePlatformIndices = IndexArenaBounds.OpeningPlatformIndices;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // ArenaBounds draws the union's outer border. These are the internal platform
        // seams, emitted once per currently existing platform to keep every square
        // visibly closed without duplicating its outer outline.
        foreach (var i in _activePlatformIndices)
            Arena.AddLine(IndexArenaBounds.LargeHexagon[(i + 1) % 6], IndexArenaBounds.LargeHexagon[i], Colors.Border, 2f);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0)
            return;

        switch (state)
        {
            case 0x00020001u:
                _activePlatformIndices = IndexArenaBounds.CompletePlatformIndices;
                ApplyBounds(IndexArenaBounds.CompleteBounds);
                break;
            case 0x00080004u:
                _activePlatformIndices = IndexArenaBounds.OpeningPlatformIndices;
                ApplyBounds(IndexArenaBounds.OpeningBounds);
                break;
        }
    }

    private void ApplyBounds(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    Contributors = "KanoNoUta (based on gynorhino)",
    PrimaryActorOID = (uint)OID.Index,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14717u,
    SortOrder = 4,
    Category = BossModuleInfo.Category.Foray,
    Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class Index : BossModule
{
    public Index(WorldState ws, Actor primary) : base(ws, primary, IndexArenaBounds.OpeningBounds.Center, IndexArenaBounds.OpeningBounds)
        => Service.Logger.Information($"[FT] {GetType().Name} created (oid={primary.OID:X})");

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
}
