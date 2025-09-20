namespace BossMod.Shadowbringers.Alliance.A20HeavyArtilleryUnit;

public enum OID : uint
{
    Boss = 0x2E83, // R4.0
    Barricade = 0x2E85, // R2.5
    Energy = 0x2E84, // R1.0
}

public enum AID : uint
{
    ManeuverHighPoweredLaser = 20657, // Boss->self, 2.5s cast, range 90 width 8 rect
    EnergyBomb = 20658 // Energy->player, no cast, single-target
}

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0D)
        {
            Arena.Bounds = A20HeavyArtilleryUnit.Arena3b;
        }
        else if (index == 0x0E)
        {
            Arena.Bounds = A20HeavyArtilleryUnit.Arena3c;
        }
    }
}

// some lasers are baited, but there doesn't seem to be any indication in the logs to see which turret is aiming at a specific player
sealed class ManeuverHighPoweredLaser(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.ManeuverHighPoweredLaser], new AOEShapeRect(90f, 4f));

sealed class Energy(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 1f, Length = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _energy = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _energy.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _energy[i];
            aoes[i] = new(capsule, h.Position, h.Rotation);
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Energy)
        {
            _energy.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Energy)
        {
            _energy.Remove(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyBomb)
        {
            _energy.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _energy.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenImminent = new ShapeDistance[count];
        var forbiddenFuture = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _energy[i];
            forbiddenFuture[i] = new SDCapsule(h.Position, h.Rotation, Length, 1.5f);
            forbiddenImminent[i] = new SDCircle(h.Position, Radius);
        }
        hints.AddForbiddenZone(new SDUnion(forbiddenFuture), WorldState.FutureTime(1.1d));
        hints.AddForbiddenZone(new SDUnion(forbiddenImminent));
    }
}

sealed class A20HeavyArtilleryUnitStates : StateMachineBuilder
{
    public A20HeavyArtilleryUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<ManeuverHighPoweredLaser>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.Boss);
                var count = enemies.Count;
                var bounds = module.Arena.Bounds;
                if (bounds == A20HeavyArtilleryUnit.Arena1)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var e = enemies[i];
                        if (!e.IsDeadOrDestroyed && e.PosRot.Z > -262f)
                        {
                            return false;
                        }
                    }
                }
                else if (bounds == A20HeavyArtilleryUnit.Arena2)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var e = enemies[i];
                        if (!e.IsDeadOrDestroyed && e.PosRot.Z > -362f)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < count; ++i)
                    {
                        var e = enemies[i];
                        if (!e.IsDeadOrDestroyed)
                        {
                            return false;
                        }
                    }
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9656, SortOrder = 5)]
public sealed class A20HeavyArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, IsArena1(primary) ? new(200f, -216.25f) : IsArena2(primary) ? new(200f, -316.532f) : arena3a.Center,
IsArena1(primary) ? Arena1 : IsArena2(primary) ? Arena2 : arena3a)
{
    public static bool IsArena1(Actor primary) => primary.PosRot.Z > -262f;
    public static bool IsArena2(Actor primary) => primary.PosRot.Z > -362f;

    public static readonly ArenaBoundsRect Arena1 = new(11.5f, 44.25f);
    public static readonly ArenaBoundsRect Arena2 = new(11.5f, 44.468f);
    private static readonly WPos arena3center = new(200f, -416.5f);
    private static readonly Rectangle[] rectarena3 = [new(arena3center, 11.5f, 44.5f)];
    private static readonly Rectangle barrier1 = new(new(200f, -405f), 11.5f, 1f);
    private static readonly Rectangle barrier2 = new(new(200f, -435f), 11.5f, 1f);
    private static readonly ArenaBoundsCustom arena3a = new(rectarena3, [barrier1, barrier2]);
    public static readonly ArenaBoundsCustom Arena3b = new(rectarena3, [barrier2]);
    public static readonly ArenaBoundsRect Arena3c = new(11.5f, 44.468f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
        Arena.Actors(Enemies((uint)OID.Barricade), Colors.Object);
    }

    protected override bool CheckPull()
    {
        var enemies = Enemies((uint)OID.Barricade);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.IsTargetable)
            {
                return true;
            }
        }
        var artillery = Enemies((uint)OID.Boss);
        var count2 = artillery.Count;
        for (var i = 0; i < count2; ++i)
        {
            var enemy = artillery[i];
            if (enemy.InCombat)
            {
                return true;
            }
        }
        return false;
    }
}
