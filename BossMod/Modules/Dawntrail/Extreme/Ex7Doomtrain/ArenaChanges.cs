namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

// cars are 50y apart along Z, car N is centered at (100, 100 + 50 * (N - 1))
// cars 4 and 6 have raised platforms that are reached via portals, these are not cut out of the bounds:
// players on the ground get temporary obstacles instead, AOEs that only hit one level use GroundShape / AirShape
sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public uint Car = 1u;
    public override bool KeepOnPhaseChange => true;

    private static readonly AOEShapeRect defaultShape = new(15f, 10f, 15f);
    public AOEShape GroundShape = defaultShape;
    public AOEShape AirShape = defaultShape;
    public readonly List<(WPos Entrance, WPos Exit)> Portals = [with(2)];
    private readonly List<(WPos From, WPos To)> platforms = [with(2)]; // center lines of raised platforms, width 5
    private BitMask car5Crates = new(0xFFFFul); // 4x4 grid, ordered left to right, top to bottom
    private bool car6Crates = true;

    public static WPos CarCenter(uint car) => new(100f, 100f + 50f * (car - 1u));

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x04 when state == 0x00020001u:
                SetCar(2u);
                break;
            case 0x05 when state == 0x00020001u:
                SetCar(3u);
                break;
            case >= 0x10 and <= 0x1F when state == 0x00100001u: // car 5 crate disappears
                car5Crates.Clear(index - 0x10);
                if (Car == 5u)
                {
                    SetCar(5u);
                }
                break;
            case 0x20 when state == 0x00100001u: // car 6 crates disappear
                car6Crates = false;
                if (Car == 6u)
                {
                    SetCar(6u);
                }
                break;
        }
    }

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.Aether)
        {
            // Intermission
            var center = new WPos(-400f, -400f);
            Arena.Center = center;
            Arena.Bounds = new ArenaBoundsCustom([new Polygon(center, 14.495f, 64)]);
        }
        else if (actor.OID == (uint)OID.Doomtrain && Car == 3u)
        {
            // Return from intermission, back to car 3
            SetCar(3u);
        }
    }

    public void NextCar() => SetCar(Car + 1u);

    public void SetCar(uint car)
    {
        Car = car;
        var center = CarCenter(car);
        Portals.Clear();
        platforms.Clear();
        GroundShape = AirShape = defaultShape;
        Rectangle[] carRect = [new(center, 10.5f, 15f)];
        switch (car)
        {
            case 2u:
                Arena.Bounds = new ArenaBoundsCustom(carRect,
                    [new Square(new(107.575f, 147.575f), 2.5f), new Square(new(92.575f, 157.425f), 2.5f),
                    new Square(new(97.5f, 147.5f), 2.01f), new Square(new(102.5f, 157.5f), 2.01f)], AdjustForHitboxInwards: true);
                break;
            case 4u:
                // double fence south of the center, platforms east (short) and west (long)
                Arena.Bounds = new ArenaBoundsCustom(carRect, [new Rectangle(new(100f, 252.5f), 5f, 2f)], AdjustForHitboxInwards: true);
                SetPlatforms(center, [new Rectangle(new(107.5f, 250f), 2.5f, 5f), new Rectangle(new(92.5f, 257.25f), 2.5f, 7.25f)]);
                platforms.Add((new(107.5f, 245f), new(107.5f, 255f)));
                platforms.Add((new(92.5f, 250f), new(92.5f, 264.5f)));
                Portals.Add((new(103.5f, 247.5f), new(107.5f, 247.5f)));
                Portals.Add((new(96.5f, 260f), new(92.5f, 260f)));
                break;
            case 5u:
                var numCrates = car5Crates.NumSetBits();
                if (numCrates == 0)
                {
                    Arena.Bounds = new ArenaBoundsRect(10f, 14.5f);
                    break;
                }
                var crates = new Shape[numCrates];
                var index = 0;
                for (var i = 0; i < 16; ++i)
                {
                    if (car5Crates[i])
                    {
                        crates[index++] = new Square(new(92.5f + 5f * (i & 3), 292.5f + 5f * (i >> 2)), 2.5f);
                    }
                }
                Arena.Bounds = new ArenaBoundsCustom(carRect, crates, AdjustForHitboxInwards: true);
                break;
            case 6u:
                // fence south east of the center, platform west, crates in the north east and south east corners until they despawn
                Shape[] obstacles = car6Crates
                    ? [new Square(new(102.5f, 352.5f), 2.01f), new Rectangle(new(107.5f, 340f), 2.5f, 5f), new Rectangle(new(105f, 362.5f), 5f, 2.5f)]
                    : [new Square(new(102.5f, 352.5f), 2.01f)];
                Arena.Bounds = new ArenaBoundsCustom(carRect, obstacles, AdjustForHitboxInwards: true);
                SetPlatforms(center, [new Rectangle(new(92.5f, 354.75f), 2.5f, 9.75f)]);
                platforms.Add((new(92.5f, 345f), new(92.5f, 364.5f)));
                Portals.Add((new(96.5f, 350f), new(92.5f, 350f)));
                break;
            default: // cars 1 (only relevant after a module reload) and 3
                Arena.Bounds = new ArenaBoundsRect(10f, 14.5f);
                break;
        }
        Arena.Center = center;
    }

    private void SetPlatforms(WPos center, Rectangle[] shapes)
    {
        AirShape = new AOEShapeCustom(center, shapes);
        GroundShape = new AOEShapeCustom(center, [new Rectangle(center, 10f, 15f)], shapes);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (platforms.Count != 0)
        {
            AirShape.Outline(Arena, Arena.Center, default, Colors.Border);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Portals.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircle(Portals[i].Entrance, 1f, Colors.SafeFromAOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countP = Portals.Count;
        if (countP == 0)
        {
            return;
        }
        for (var i = 0; i < countP; ++i)
        {
            var p = Portals[i];
            hints.Teleporters.Add(new(p.Entrance, p.Exit, 1.5f, false));
        }
        if (actor.PosRot.Y < 4f) // platforms can't be walked onto from the ground
        {
            var count = platforms.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = platforms[i];
                hints.TemporaryObstacles.Add(new SDRect(p.From, p.To, 2.5f));
            }
        }
    }
}
