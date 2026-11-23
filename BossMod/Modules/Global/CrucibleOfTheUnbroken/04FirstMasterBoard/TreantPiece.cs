namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.TreantPiece;

public enum OID : uint
{
    TreantPiece = 0x4CCD,
    BossPuddle = 0x1EABFA, // R0.500, x1, EventObj type
    SlugPiece = 0x4CCE, // R0.800, x0 (spawn during fight)
    SaplingPiece = 0x4CCF, // R0.600-2.400, x0 (spawn during fight)
    BilokoPiece = 0x4CD1, // R2.280, x0 (spawn during fight)
    DiremitePiece = 0x4CD0, // R2.400, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackAero = 48624, // TreantPiece->player, no cast, single-target
    RustlingBreezeBoss = 48777, // TreantPiece->self, 6.2+0.8s cast, single-target
    RustlingBreezeBoss1 = 48776, // TreantPiece->self, 6.1+0.9s cast, single-target
    RustlingBreezeLeftRight = 48779, // Helper->self, 7.0s cast, range 60 150.000-degree cone
    RustlingBreezeLeftRight1 = 48780, // Helper->self, 7.0s cast, range 60 150.000-degree cone
    RustlingBreezeMiddle = 48778, // Helper->self, 7.0s cast, range 60 90.000-degree cone
    FallingLeaves = 48769, // TreantPiece->self, 3.0s cast, single-target
    ArborealStormBoss = 48770, // TreantPiece->self, 4.3+0.7s cast, single-target
    ArborealStorm1 = 48771, // Helper->self, 5.0s cast, range 12 circle
    ArborealStorm2 = 48772, // Helper->self, 7.0s cast, range ?-18 donut
    ArborealStorm3 = 48773, // Helper->self, 9.0s cast, range ?-24 donut
    ArborealStorm4 = 48774, // Helper->self, 11.0s cast, range ?-30 donut
    ArborealStorm5 = 48775, // Helper->self, 13.0s cast, range ?-36 donut
    AcornBombBoss = 50518, // TreantPiece->self, 5.0s cast, single-target
    AcornBomb = 48781, // TreantPiece->players, no cast, range 3 circle

    // SlugPiece
    AutoAttackSlugPiece = 49682, // 4CCE/4CD0->player, no cast, single-target
    AqueousDischarge = 48783, // 4CCE->self, 2.5s cast, range 5 circle

    // SaplingPiece
    AutoAttackStone = 48625, // 4CCF->player, no cast, single-target
    GrabAndGrow = 48786, // 4CCF->TreantPiece, no cast, single-target

    // BilokoPiece
    NaturalNurture = 48782, // 4CD1->TreantPiece, 9.0s cast, single-target

    // DiremitePiece
    Silkscreen = 48785, // 4CD0->self, 3.5s cast, range 40 width 4 rect
}

public enum SID : uint
{
    Rehabilitation = 989, // 4CCF->4CCF, extra=0x1/0x2/0x3/0x4/0x5
    Growing = 390, // 4CCF->4CCF, extra=0x1/0x2/0x3/0x4
    FullGrown = 384, // 4CCF->4CCF, extra=0x1
    Sludge = 3071, // none->player, extra=0x0
    Sludge1 = 3072, // none->player, extra=0x0
}

public enum IconID : uint
{
    AcornBombIcon = 712, // player->self
}

public enum TetherID : uint
{
    SaplingPieceTether = 57, // 4CCF->TreantPiece
    BilokoPieceTether = 17, // 4CD1->TreantPiece
}

sealed class RustlingBreezeLeftRight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RustlingBreezeLeftRight,
        (uint)AID.RustlingBreezeLeftRight1], new AOEShapeCone(60f, 75f.Degrees()));
sealed class RustlingBreezeMiddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RustlingBreezeMiddle, new AOEShapeCone(60f, 45f.Degrees()));
sealed class AqueousDischarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AqueousDischarge, 5f);
sealed class Silkscreen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Silkscreen, new AOEShapeRect(40f, 2f));
sealed class AcornBomb(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AcornBombIcon, (uint)AID.AcornBomb, 3f, 5.1d);

sealed class BossPuddle(BossModule module) : Components.Voidzone(module, 10f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.BossPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class ArborealStorm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ArborealStorm1:
                aoes.Add(new(new AOEShapeCircle(12.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                SortHelpers.SortAOEByActivation(aoes);
                break;
            case (uint)AID.ArborealStorm2:
                aoes.Add(new(new AOEShapeDonut(12.0f, 18.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                SortHelpers.SortAOEByActivation(aoes);
                break;
            case (uint)AID.ArborealStorm3:
                aoes.Add(new(new AOEShapeDonut(18.0f, 24.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                SortHelpers.SortAOEByActivation(aoes);
                break;
            case (uint)AID.ArborealStorm4:
                aoes.Add(new(new AOEShapeDonut(24.0f, 30.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                SortHelpers.SortAOEByActivation(aoes);
                break;
            case (uint)AID.ArborealStorm5:
                aoes.Add(new(new AOEShapeDonut(30.0f, 36.0f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                SortHelpers.SortAOEByActivation(aoes);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ArborealStorm1 or (uint)AID.ArborealStorm2 or (uint)AID.ArborealStorm3 or (uint)AID.ArborealStorm4
            or (uint)AID.ArborealStorm5)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
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

        var max = count > 2 ? 2 : count;
        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return incomingAOEs[..max];
    }
}

sealed class TreantPieceStates : StateMachineBuilder
{
    public TreantPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustlingBreezeLeftRight>()
            .ActivateOnEnter<RustlingBreezeMiddle>()
            .ActivateOnEnter<AqueousDischarge>()
            .ActivateOnEnter<ArborealStorm>()
            .ActivateOnEnter<Silkscreen>()
            .ActivateOnEnter<AcornBomb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.TreantPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14618u, SortOrder = 8)]
public sealed class TreantPiece : BossModule
{
    public TreantPiece(WorldState ws, Actor primary) : base(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
    {
        ActivateComponent<BossPuddle>();
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.SaplingPiece => 5,
                (uint)OID.BilokoPiece => 4,
                (uint)OID.DiremitePiece => 3,
                (uint)OID.SlugPiece => 2,
                (uint)OID.TreantPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SaplingPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.BilokoPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.DiremitePiece));
        Arena.Actors(Enemies((uint)OID.SlugPiece));
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: SaplingPiece -> BilokoPiece -> DiremitePiece/SlugPiece -> TreantPiece",
    ];

    public override string[] PrePullHints => _prePullHints;
}
