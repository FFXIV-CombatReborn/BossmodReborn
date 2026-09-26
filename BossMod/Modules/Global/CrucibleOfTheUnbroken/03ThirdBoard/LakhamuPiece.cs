namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.LakhamuPiece;

public enum OID : uint
{
    LakhamuPiece = 0x4C9E,
    GolemPiece = 0x4C9F, // R2.200, x0 (spawn during fight)
    SandSphere = 0x4CA0, // R1.800, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackStone = 50792, // LakhamuPiece->player, no cast, single-target
    AutoAttackGolem = 50398, // 4C9F->player, no cast, single-target
    EarthrenderBoss = 48559, // LakhamuPiece->self, 4.0s cast, single-target
    Earthrender = 48560, // Helper->location, 3.0s cast, range 6 circle
    RockslideGolem = 48554, // 4C9F->self, 6.0s cast, single-target
    Rockslide = 48555, // Helper->self, 7.0s cast, range 45 width 10 rect
    SandTempest = 48561, // LakhamuPiece->self, 5.0s cast, range 60 circle
    Burst = 48562, // 4CA0->self, 3.0s cast, range 12 circle
    EarthShakerBoss = 48557, // LakhamuPiece->self, 4.0+0.2s cast, single-target
    EarthShaker = 48558, // Helper->self, no cast, range 60 50-degree cone // TODO degree is a guess
    LandslipBoss = 48553, // LakhamuPiece->self, 7.5+1.0s cast, single-target // Actors facing direction will do a rect and the direction they look is the knockback
    Landslip = 48556, // Helper->self, 8.0s cast, range 45 width 10 rect
}

public enum SID : uint
{
    EarthResistanceDown = 5025, // Helper->player, extra=0x1
    Blind = 5389, // LakhamuPiece->player, extra=0x0
}

public enum IconID : uint
{
    EarthShake = 40, // player/4A09/49F0->self
}

sealed class Earthrender(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthrender, 6.0f);
sealed class SandTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.SandTempest, "Applies blind debuff");

// Eventcast is triggered but the bait actually resolves a little bit later, so a timer of 1.0 second is added
sealed class EarthShaker : Components.BaitAwayIcon {
    private const double additionalActivationTime = 1d;
    private DateTime? eventCastActivation;

    public EarthShaker(BossModule module) : base(module, new AOEShapeCone(60f, 75f.Degrees()), (uint)IconID.EarthShake, (uint)AID.EarthShaker, 3.3d) {
        AllowPetTargets = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (CurrentBaits.Count != 0 && spell.Action.ID == WatchedAction) {
            eventCastActivation = WorldState.CurrentTime;
        }
    }

    public override void Update() {
        base.Update();

        if (eventCastActivation == null || CurrentBaits.Count == 0) {
            return;
        }

        if (eventCastActivation.Value.AddSeconds(additionalActivationTime) > WorldState.CurrentTime) {
            return;
        }

        CurrentBaits.Clear();
    }
}

sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(12f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SandSphere)
        {
            aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(5.9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class Rockslide(BossModule module) : Components.GenericAOEs(module, (uint)AID.Rockslide)
{
    public readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect shape = new(45f, 5f);
    private bool active = false;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.GolemPiece)
        {
            aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(9.7d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Landslip)
        {
            active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Rockslide)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }

        if (spell.Action.ID == (uint)AID.Landslip)
        {
            active = false;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }
}

sealed class Landslip(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Landslip, 20.0f, shape: new AOEShapeRect(45.0f, 5.0f), maxCasts: 1,
    kind: Kind.DirForward)
{
    private readonly Rockslide? rockslide = module.FindComponent<Rockslide>();

    // Grid map
    private const float tileSize = 10f;
    private const float tileHalf = tileSize * 0.5f;
    private const int tiles = tilesFromCenter * 2;
    private const int tilesFromCenter = 2;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (rockslide == null || rockslide.aoes.Count == 0 || Casters.Count == 0)
        {
            return;
        }

        for (var y = 0; y < tiles; y++)
        {
            for (var x = 0; x < tiles; x++)
            {
                // Checking each tile
                var tile = Arena.Center + new WDir((x - tilesFromCenter + 0.5f) * tileSize, (y - tilesFromCenter + 0.5f) * tileSize); // center of tile

                // Checking the knockback against the tile
                foreach (var knockback in Casters)
                {
                    if (knockback.Shape == null || !knockback.Shape.Check(tile, knockback.Origin, knockback.Direction))
                    {
                        continue;
                    }

                    var destination = tile + knockback.Distance * knockback.Direction.ToDirection();
                    if (!Arena.InBounds(destination))
                    {
                        continue;
                    }

                    var tileSafe = true; // We have to check every rockslide cast since they can overlap
                    foreach (var aoe in rockslide.aoes)
                    {
                        if (aoe.Check(destination))
                        {
                            tileSafe = false;
                            break;
                        }
                    }

                    if (tileSafe)
                    {
                        Arena.ZoneRect(tile, new WDir(0, 1), tileHalf, tileHalf, tileHalf, Colors.SafeFromAOE);
                    }
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (rockslide == null || rockslide.aoes.Count == 0 || Casters.Count == 0)
        {
            return;
        }

        var knockbacks = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < knockbacks.Length; ++i)
        {
            ref var knockback = ref knockbacks[i];
            if (knockback.Shape == null || IsImmune(slot, knockback.Activation))
            {
                continue;
            }

            var direction = knockback.Distance * knockback.Direction.ToDirection();
            hints.AddForbiddenZone(new SDIntersection([
                    knockback.Shape.Distance(knockback.Origin, knockback.Direction),
                    new SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(Arena.Center, direction,  19.0f, [.. rockslide.aoes], rockslide.aoes.Count)]),
                    knockback.Activation);
        }
    }
}

sealed class LakhamuPieceStates : StateMachineBuilder
{
    public LakhamuPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<SandTempest>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Earthrender>()
            .ActivateOnEnter<EarthShaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.LakhamuPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14580u, SortOrder = 5)]
public sealed class LakhamuPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GolemPiece => 2,
                (uint)OID.LakhamuPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GolemPiece));
    }
}
