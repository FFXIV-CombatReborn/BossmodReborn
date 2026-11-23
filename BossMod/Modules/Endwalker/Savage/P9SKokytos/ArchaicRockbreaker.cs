namespace BossMod.Endwalker.Savage.P9SKokytos;

sealed class ArchaicRockbreakerCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArchaicRockbreakerCenter, 6f);

sealed class ArchaicRockbreakerShockwave : Components.GenericKnockback
{
    private readonly ArenaChanges arena;
    private Knockback[] _kb = [];

    public ArchaicRockbreakerShockwave(BossModule module) : base(module, (uint)AID.ArchaicRockbreakerShockwave)
    {
        _activation = module.WorldState.FutureTime(6.5d);

        SafeWall[] CreateRotatedWalls()
        {
            var walls = new SafeWall[4];
            for (var i = 0; i < 4; ++i)
            {
                var wall = WallsCard[i];
                walls[i] = RotatedSafeWall(ref wall);
            }
            return walls;
        }
        var center = Arena.Center;
        SafeWall RotatedSafeWall(ref SafeWall wall)
        {
            var rotatedStart = WPos.RotateAroundOrigin(45f, center, wall.Vertex1);
            var rotatedEnd = WPos.RotateAroundOrigin(45f, center, wall.Vertex2);
            return new(rotatedStart, rotatedEnd);
        }
        WallsIntercard = CreateRotatedWalls();

        arena = module.FindComponent<ArenaChanges>()!;
    }
    private readonly DateTime _activation;

    private readonly SafeWall[] WallsCard = [new(new(93f, 117.5f), new(108f, 117.5f)), new(new(82.5f, 93f), new(82.5f, 108f)),
    new(new(117.5f, 93f), new(117.5f, 108f)), new(new(93f, 82.5f), new(108f, 82.5f))];
    private readonly SafeWall[] WallsIntercard;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void Update()
    {
        if (_kb.Length == 0 && arena.Cardinal is bool cardinal)
        {
            if (cardinal)
            {
                _kb = [new(Arena.Center, 21f, _activation, safeWalls: WallsCard, ignoreImmunes: true)];
            }
            else
            {
                _kb = [new(Arena.Center, 21f, _activation, safeWalls: WallsIntercard, ignoreImmunes: true)];
            }
        }
    }
}

sealed class ArchaicRockbreakerPairs : Components.UniformStackSpread
{
    public ArchaicRockbreakerPairs(BossModule module) : base(module, 6f, default, 2)
    {
        foreach (var p in Raid.WithoutSlot(true, true, true))
            if (p.Class.IsSupport())
                AddStack(p, WorldState.FutureTime(7.8f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArchaicRockbreakerPairs)
            Stacks.Clear();
    }
}

sealed class ArchaicRockbreakerLine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArchaicRockbreakerLine, 8f, maxCasts: 8);

sealed class ArchaicRockbreakerCombination(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeCircle _shapeOut = new(12f);
    private readonly AOEShapeDonut _shapeIn = new(8f, 20f);
    private readonly AOEShapeCone _shapeCleave = new(40f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var safespots = SafeSpots();
        var count = safespots.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
            movementHints.Add(actor.Position, safespots[i], Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (inOutShape, offset) = spell.Action.ID switch
        {
            (uint)AID.FrontCombinationOut => (_shapeOut, default),
            (uint)AID.FrontCombinationIn => (_shapeIn, default),
            (uint)AID.RearCombinationOut => (_shapeOut, 180f.Degrees()),
            (uint)AID.RearCombinationIn => (_shapeIn, 180f.Degrees()),
            _ => ((AOEShape?)null, new Angle())
        };
        if (inOutShape != null)
        {
            _aoes.Add(new(inOutShape, Module.PrimaryActor.Position, default, WorldState.FutureTime(6.9d)));
            _aoes.Add(new(_shapeCleave, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + offset, WorldState.FutureTime(10d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.InsideRoundhouseAOE:
                PopAOE();
                _aoes.Add(new(_shapeIn, Module.PrimaryActor.Position, default, WorldState.FutureTime(6d)));
                break;
            case (uint)AID.OutsideRoundhouseAOE:
                PopAOE();
                _aoes.Add(new(_shapeOut, Module.PrimaryActor.Position, default, WorldState.FutureTime(6d)));
                break;
            case (uint)AID.SwingingKickFrontAOE:
            case (uint)AID.SwingingKickRearAOE:
                PopAOE();
                break;
        }
    }

    private void PopAOE()
    {
        ++NumCasts;
        if (_aoes.Count != 0)
            _aoes.RemoveAt(0);
    }

    private List<WPos> SafeSpots()
    {
        if (NumCasts == 0 && _aoes.Count > 0 && _aoes[0].Shape == _shapeOut && Module.FindComponent<ArchaicRockbreakerLine>() is var forbidden && forbidden?.NumCasts == 0)
        {
            var safespots = new ArcList(_aoes[0].Origin, _shapeOut.Radius + 0.25f);
            var spots = new List<WPos>();
            foreach (var f in forbidden.ActiveCasters)
                safespots.ForbidCircle(f.Origin, 8);
            if (safespots.Forbidden.Segments.Count > 0)
            {
                foreach (var a in safespots.Allowed(default))
                {
                    var mid = ((a.min.Rad + a.max.Rad) * 0.5f).Radians();
                    spots.Add(safespots.Center + safespots.Radius * mid.ToDirection());
                }
            }
            return spots;
        }
        return [];
    }
}

sealed class ArchaicDemolish(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArchaicDemolish)
            AddStacks(Raid.WithoutSlot(true, true, true).Where(a => a.Role == Role.Healer), Module.CastFinishAt(spell, 1.2d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArchaicDemolishAOE)
            Stacks.Clear();
    }
}
