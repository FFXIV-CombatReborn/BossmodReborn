namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

sealed class DeadMansExpress(BossModule module) : Components.GenericKnockback(module, (uint)AID.DeadMansExpress)
{
    private Knockback[] _kb = [];
    private readonly ArenaChanges arena = module.FindComponent<ArenaChanges>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            SafeWall[]? walls = arena.Car switch
            {
                1u => [new(new(104.51f, 94.99f), new(100.48991f, 94.99f)), new(new(99.50995f, 104.9898f), new(95.49f, 104.9898f)),
                    new(new(90f, 114.5f), new(110f, 114.5f))],
                2u => [new(new(95.49f, 144.99001f), new(99.50995f, 144.99001f)), new(new(100.48991f, 154.98981f), new(104.51f, 154.98981f)),
                    new(new(110.075f, 144.575f), new(105.075f, 144.575f)), new(new(95.075f, 154.425f), new(90.075f, 154.425f)),
                    new(new(90f, 164.5f), new(110f, 164.5f))],
                _ => null // no measured walls for later cars, stop at the arena polygon instead
            };
            StopAfterWall = walls == null;
            _kb = [new(spell.LocXZ, 30f, Module.CastFinishAt(spell), kind: Kind.DirForward, safeWalls: walls)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = [];
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => false;
}

sealed class DeadMansBlastpipe(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    private readonly ArenaChanges arena = module.FindComponent<ArenaChanges>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeadMansWindpipe)
        {
            var shape = new AOEShapeRect(10f, 10f);
            // the blastpipe covers the northernmost 10y of the current car (cars are 50y apart)
            WPos origin = new(99.992f, 84.978f + 50f * (arena.Car - 1u));
            var rotation = Angle.AnglesCardinals[1];
            AOE = [new(shape, origin, rotation, Module.CastFinishAt(spell, 2d), shapeDistance: shape.Distance(origin, rotation))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeadMansBlastpipe)
        {
            AOE = [];
            ++NumCasts;
        }
    }
}

sealed class DeadMansWindpipe(BossModule module) : Components.GenericKnockback(module, (uint)AID.DeadMansWindpipe)
{
    private Knockback[] _kb = [];
    private readonly DeadMansBlastpipe _aoe = module.FindComponent<DeadMansBlastpipe>()!;
    private readonly ArenaChanges arena = module.FindComponent<ArenaChanges>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            SafeWall[]? walls = arena.Car switch
            {
                1u => [new(new(104.51f, 100.01f), new(100.49f, 100.01f)), new(new(95.49f, 110.01f), new(99.51f, 110.01f)),
                    new(new(110f, 85.5f), new(90f, 85.5f))],
                2u => [new(new(110.075f, 150.575f), new(105.075f, 150.575f)), new(new(95.075f, 160.425f), new(90.075f, 160.425f)),
                    new(new(95.49f, 150.01f), new(99.51f, 150.00998f)), new(new(100.49f, 160.01f), new(104.51f, 160.01f)),
                    new(new(110f, 135.5f), new(90f, 135.5f))],
                _ => null // no measured walls for later cars, stop at the arena polygon instead
            };
            StopAfterWall = walls == null;
            _kb = [new(spell.LocXZ, 28f, Module.CastFinishAt(spell), kind: Kind.DirBackward, minDistance: -10f, safeWalls: walls)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = [];
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        return _aoe.AOE.Length != 0 && _aoe.AOE[0].Check(pos);
    }
}
