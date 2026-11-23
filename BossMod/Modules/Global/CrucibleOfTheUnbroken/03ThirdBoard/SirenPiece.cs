namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.SirenPiece;

public enum OID : uint
{
    SirenPiece = 0x4CA1,
    CrawlingPiece = 0x4CA4, // R0.750, x0 (spawn during fight)
    ShamblingPiece = 0x4CA3, // R0.750, x0 (spawn during fight)
    SweetSong = 0x4CA2, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50395, // SirenPiece->players, no cast, range 9 ?-degree cone
    AutoAttackShambling = 49682, // 4CA3->player, no cast, single-target
    Teleport = 48564, // SirenPiece->location, no cast, single-target

    Summon = 48567, // SirenPiece->self, 3.0s cast, single-target
    SongOfTorment = 48563, // SirenPiece->player, 5.0s cast, single-target
    UnmooringMelody = 48565, // SirenPiece->self, no cast, single-target
    UnmooringMelody1 = 48566, // Helper->self, no cast, range 50 ?-degree cone
    FeralLungeBoss = 48569, // SirenPiece->self, 3.8+0.2s cast, single-target
    FeralLunge = 48570, // Helper->self, 4.0s cast, range 50 width 16 rect
    DeadMansDirgeOuterBoss = 48572, // SirenPiece->self, 5.6+1.4s cast, single-target
    DeadMansDirgeOuter = 48573, // Helper->self, 7.0s cast, range 12 circle
    DeadMansDirgeInnerBoss = 48574, // SirenPiece->self, 6.2+0.8s cast, single-target
    DeadMansDirgeInner = 48575, // Helper->self, 7.0s cast, range 3-43 donut
    DistantTune = 48576, // SirenPiece->self, 3.0s cast, single-target
    Burst = 48577, // 4CA2->self, 1.0s cast, range 9 circle
    InvitingVerse = 48571, // SirenPiece->self, 5.0s cast, range 40 circle
    DeathThroes = 48568, // CrawlingPiece->player, no cast, single-target
    Wallop = 50662, // ShamblingPiece->self, 2.5s cast, range 5 width 3 rect
}

public enum SID : uint
{
    WitsEnd = 5424, // Helper->player, extra=0x1
    Bind = 5710, // CrawlingPiece->player, extra=0x0
    Bleeding = 3077, // none->player, extra=0x0
    Bleeding1 = 3078, // none->player, extra=0x0
    ForcedMarch = 1257, // SirenPiece->player, extra=0x4
    ForwardMarch = 2161, // SirenPiece->player, extra=0x0
    LeftFace = 2163, // SirenPiece->player, extra=0x0
    RightFace = 2164, // SirenPiece->player, extra=0x0
    AboutFace = 2162 // SirenPiece->player, extra=0x0
}

public enum IconID : uint
{
    TankBuster = 218 // player->self
}

public enum TetherID : uint
{
    TargetTether = 17 // 4CA4->player - from the CrawlingPiece
}

sealed class SongOfTorment(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SongOfTorment);
sealed class FeralLunge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeralLunge, new AOEShapeRect(50f, 8f));
sealed class DeadMansDirgeOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadMansDirgeOuter, 12f);
sealed class DeadMansDirgeInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadMansDirgeInner, new AOEShapeDonut(3f, 43f));
sealed class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(5f, 1.5f));

sealed class CrawlingPiece(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> crawlers = module.Enemies((uint)OID.CrawlingPiece);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Bind) != null)
        {
            hints.Add("Kill the crawler holding you!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus((uint)SID.Bind) != null)
        {
            return;
        }

        var count = crawlers.Count;
        for (var i = 0; i < count; ++i)
        {
            var crawler = crawlers[i];
            if (!crawler.IsDeadOrDestroyed)
            {
                hints.AddForbiddenZone(new SDCapsule(crawler.Position, crawler.Rotation, 5f, 2f), DateTime.MaxValue);
            }
        }
    }
}

sealed class UnmooringMelody(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(50f, 45f.Degrees());

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Teleport)
        {
            aoes.Add(new(shape, spell.TargetXZ, Angle.FromDirection(Arena.Center - spell.TargetXZ)));
        }

        if (spell.Action.ID == (uint)AID.UnmooringMelody1)
        {
            NumCasts++;

            if (NumCasts == 12)
            {
                aoes.Clear();
                NumCasts = 0;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(9f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SweetSong)
        {
            aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(5.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            var index = IndexOfClosestAOE(aoes, caster.Position);
            if (index >= 0)
            {
                aoes.RemoveAt(index);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 6 ? 6 : count;
        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i < 3 ? Colors.Danger : Colors.AOE;
        }

        return nextAOEs[..max];
    }
}

sealed class InvitingVerse(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace,
    (uint)SID.RightFace)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }

        var move0 = state.PendingMoves[0];
        var outwards = Angle.FromDirection(actor.Position - Arena.Center) - move0.dir;
        hints.ForbiddenDirections.Add((outwards, 90.0f.Degrees(), move0.activation));
    }
}

sealed class SirenPieceStates : StateMachineBuilder
{
    public SirenPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrawlingPiece>()
            .ActivateOnEnter<SongOfTorment>()
            .ActivateOnEnter<UnmooringMelody>()
            .ActivateOnEnter<FeralLunge>()
            .ActivateOnEnter<DeadMansDirgeOuter>()
            .ActivateOnEnter<DeadMansDirgeInner>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<InvitingVerse>()
            .ActivateOnEnter<Wallop>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.SirenPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14583u, SortOrder = 6)]
public sealed class SirenPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CrawlingPiece => 3,
                (uint)OID.ShamblingPiece => 2,
                (uint)OID.SirenPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CrawlingPiece));
        Arena.Actors(Enemies((uint)OID.ShamblingPiece));
    }
}
