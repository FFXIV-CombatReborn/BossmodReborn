namespace BossMod.Endwalker.Savage.P12S1Athena;

// note: no idea how pair targets are selected, assume same role as POV
abstract class SuperchainTheory(BossModule module) : BossComponent(module)
{
    public enum Shape { Unknown, Circle, Donut, Spread, Pairs }
    public readonly struct Chain(Actor origin, Actor moving, Shape shape, DateTime activation)
    {
        public readonly Actor Origin = origin;
        public readonly Actor Moving = moving;
        public readonly Shape Shape = shape;
        public readonly DateTime Activation = activation;
    }

    public List<Chain> Chains = [];
    public int NumCasts;
    private readonly List<Actor> _pendingTethers = []; // unfortunately, sometimes tether targets are created after tether events - recheck such tethers every frame

    private static readonly AOEShapeCircle _shapeCircle = new(7f);
    private static readonly AOEShapeDonut _shapeDonut = new(6f, 70f);
    private static readonly AOEShapeCone _shapeSpread = new(100f, 15f.Degrees()); // TODO: verify angle
    private static readonly AOEShapeCone _shapePair = new(100f, 20f.Degrees()); // TODO: verify angle

    public List<Chain> ImminentChains()
    {
        var result = new List<Chain>();

        if (Chains == null || Chains.Count == 0)
            return result;

        var threshold = Chains[0].Activation.AddSeconds(1d);

        var count = Chains.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chains[i];
            if (c.Activation < threshold)
            {
                result.Add(c);
            }
            else
            {
                break;
            }
        }

        return result;
    }

    public abstract float ActivationDelay(float distance);

    public override void Update()
    {
        var count = _pendingTethers.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var source = _pendingTethers[i];
            var shape = source.Tether.ID switch
            {
                (uint)TetherID.SuperchainCircle => Shape.Circle,
                (uint)TetherID.SuperchainDonut => Shape.Donut,
                (uint)TetherID.SuperchainSpread => Shape.Spread,
                (uint)TetherID.SuperchainPairs => Shape.Pairs,
                _ => Shape.Unknown
            };

            if (shape == Shape.Unknown)
            {
                // irrelevant, remove
                _pendingTethers.RemoveAt(i);
            }
            else if (WorldState.Actors.Find(source.Tether.Target) is var origin && origin != null)
            {
                Chains.Add(new(origin, source, shape, WorldState.FutureTime(ActivationDelay((source.Position - origin.Position).Length()))));
                Chains.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                _pendingTethers.RemoveAt(i);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var chains = ImminentChains();
        var count = chains.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chains[i];
            switch (c.Shape)
            {
                case Shape.Circle:
                    if (_shapeCircle.Check(actor.Position, c.Origin))
                        hints.Add("GTFO from aoe!");
                    break;
                case Shape.Donut:
                    if (_shapeDonut.Check(actor.Position, c.Origin))
                        hints.Add("GTFO from aoe!");
                    break;
                case Shape.Spread:
                    var posOrigin = c.Origin.Position;
                    var party = Raid.WithoutSlot(false, true, true);
                    var len = party.Length;
                    var actorID = actor.InstanceID;
                    var anyInside = false;
                    var angle = Angle.FromDirection(actor.Position - posOrigin);
                    for (var j = 0; j < len; ++j)
                    {
                        var p = party[i];
                        if (p.InstanceID == actorID)
                        {
                            continue;
                        }
                        if (_shapeSpread.Check(p.Position, posOrigin, angle))
                        {
                            anyInside = true;
                            break;
                        }
                    }
                    hints.Add("Spread!", anyInside);
                    break;
                case Shape.Pairs:
                    var actorIsSupport = actor.Class.IsSupport();
                    int sameRole = 0, diffRole = 0;
                    var posOrigin2 = c.Origin.Position;
                    var party2 = Raid.WithoutSlot(false, true, true);
                    var len2 = party2.Length;
                    var actorID2 = actor.InstanceID;
                    var angle2 = Angle.FromDirection(actor.Position - posOrigin2);
                    for (var j = 0; j < len2; ++j)
                    {
                        ref readonly var p = ref party2[i];
                        if (p.InstanceID == actorID2)
                        {
                            continue;
                        }
                        if (_shapeSpread.Check(p.Position, posOrigin2, angle2))
                        {
                            if (p.Class.IsSupport() == actorIsSupport)
                            {
                                ++sameRole;
                            }
                            else
                            {
                                ++diffRole;
                            }
                        }
                    }
                    hints.Add("Stack in pairs!", sameRole != 0 || diffRole != 1);
                    break;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var chains = ImminentChains();
        var count = chains.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chains[i];
            switch (c.Shape)
            {
                case Shape.Circle:
                    _shapeCircle.Draw(Arena, c.Origin);
                    break;
                case Shape.Donut:
                    _shapeDonut.Draw(Arena, c.Origin);
                    break;
                case Shape.Spread:
                    var posOrigin = c.Origin.Position;
                    var party = Raid.WithoutSlot(false, true, true);
                    var len = party.Length;
                    var actorID = pc.InstanceID;
                    for (var j = 0; j < len; ++j)
                    {
                        var p = party[i];
                        if (p.InstanceID == actorID)
                        {
                            continue;
                        }
                        _shapeSpread.Draw(Arena, posOrigin, Angle.FromDirection(p.Position - posOrigin));
                    }
                    break;
                case Shape.Pairs:
                    var pcIsSupport = pc.Class.IsSupport();
                    var posOrigin2 = c.Origin.Position;
                    var party2 = Raid.WithoutSlot(false, true, true);
                    var len2 = party2.Length;
                    var actorID2 = pc.InstanceID;
                    for (var j = 0; j < len2; ++j)
                    {
                        var p = party2[i];
                        if (p.InstanceID != actorID2 && p.Class.IsSupport() == pcIsSupport)
                        {
                            _shapePair.Draw(Arena, posOrigin2, Angle.FromDirection(p.Position - posOrigin2));
                        }
                    }
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var chains = ImminentChains();
        var count = chains.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Chains[i];
            var posOrigin = c.Origin.Position;
            var angle = Angle.FromDirection(pc.Position - posOrigin);
            switch (c.Shape)
            {
                case Shape.Spread:
                    _shapeSpread.Outline(Arena, posOrigin, angle);
                    break;
                case Shape.Pairs:
                    _shapePair.Outline(Arena, posOrigin, angle, Colors.Safe);
                    break;
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        _pendingTethers.Add(source);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.SuperchainBurst => Shape.Circle,
            (uint)AID.SuperchainCoil => Shape.Donut,
            (uint)AID.SuperchainRadiation => Shape.Spread,
            (uint)AID.SuperchainEmission => Shape.Pairs,
            _ => Shape.Unknown
        };
        if (shape != Shape.Unknown)
        {
            var count = Chains.Count;
            for (var i = 0; i < count; ++i)
            {
                var c = Chains[i];
                var posOrigin = c.Origin.Position;
                if (c.Shape == shape && posOrigin.AlmostEqual(caster.Position, 1f) && posOrigin.AlmostEqual(c.Moving.Position, 3f))
                {
                    ++NumCasts;
                    Chains.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class SuperchainTheory1(BossModule module) : SuperchainTheory(module)
{
    public override float ActivationDelay(float distance)
    {
        return distance switch
        {
            < 7f => 10.7f, // first circle/donut + spread/pair are at distance 6
            < 16f => 12.7f, // second circle + donut are at distance 15
            < 19f => 14.6f, // third circle/donut is at distance 18
            _ => 16.6f, // fourth circle/donut is at distance 24
        };
    }
}

sealed class SuperchainTheory2A(BossModule module) : SuperchainTheory(module)
{
    public override float ActivationDelay(float distance)
    {
        return distance switch
        {
            < 10f => 11.9f, // first 2 circles + pairs are at distance 9
            < 18f => 14.3f, // second donut is at distance 16.5
            < 25f => 16.9f, // third circle is at distance 24
            _ => 20.3f, // fourth circle + spread/pairs are at distance 34.5
        };
    }
}

sealed class SuperchainTheory2B(BossModule module) : SuperchainTheory(module)
{
    public override float ActivationDelay(float distance)
    {
        return distance switch
        {
            < 10f => 11.7f, // first circle + donut are at distance 9
            < 20f => 15.1f, // second circle + spread/pairs area at distance 18
            _ => 19.6f, // third 2 circles + spread/pairs are at distance 33
        };
    }
}
