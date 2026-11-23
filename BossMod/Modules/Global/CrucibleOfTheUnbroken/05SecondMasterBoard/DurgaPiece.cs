namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.DurgaPiece;

public enum OID : uint
{
    DurgaPiece = 0x4CF6,
    ElectricPillar = 0x4EA8, // R1.000, x4
    Missile = 0x4CF7, // R1.300, x8
    SpinnerRookPiece = 0x4CF8, // R0.750, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49270, // DurgaPiece->player, no cast, single-target
    ElectricAbsorption = 49263, // DurgaPiece->self, 5.0s cast, single-target
    Summon = 49269, // DurgaPiece->self, 3.0s cast, single-target
    GroundingJoltBoss = 49264, // DurgaPiece->self, 4.5+0.7s cast, single-target
    GroundingJoltBig = 49265, // Helper->self, 5.2s cast, range 20 circle
    GroundingJoltSmall = 49266, // Helper->self, 5.2s cast, range 6 circle
    AtomicRay = 49272, // DurgaPiece->self, 13.0s cast, range 60 circle
    DiffusionRayBoss = 49275, // DurgaPiece->self, 4.0+1.0s cast, single-target
    DiffusionRay = 49276, // Helper->self, 5.0s cast, range 30 120.000-degree cone
    Missile = 49267, // DurgaPiece->self, 3.0s cast, single-target
    Missile1 = 49512, // DurgaPiece->self, no cast, single-target
    VoyageActor = 49268, // 4CF7->self, 1.0s cast, single-target
    Voyage = 50688, // Helper->self, 1.5s cast, range 100 width 4 rect
    ThermobaricChargeBoss = 49273, // DurgaPiece->self, 4.2+0.8s cast, single-target
    ThermobaricCharge = 49274, // Helper->self, 2.0s cast, range 55 circle

    // SpinnerRookPiece
    AetherCharge = 49271, // 4CF8->DurgaPiece, 3.0s cast, single-target
}

public enum SID : uint
{
    Gen = 2056, // DurgaPiece->4CF7/DurgaPiece, extra=0x476/0x23E/0xD1
    Gen1 = 2193, // DurgaPiece->DurgaPiece, extra=0x48C
    Bleeding = 3077, // none->player, extra=0x0
    Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint
{
    ThermobaricChargeLockOn = 23, // player->self
}

public enum TetherID : uint
{
    ElectricPillarNETether = 416, // 4EA8->DurgaPiece
    ElectricPillarSETether = 417, // 4EA8->DurgaPiece
    ElectricPillarNWTether = 418, // 4EA8->DurgaPiece
    ElectricPillarSWTether = 419, // 4EA8->DurgaPiece
}

sealed class AtomicRay(BossModule module) : Components.RaidwideCast(module, (uint)AID.AtomicRay, "Raidwide - Pet cover does not work");
sealed class DiffusionRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffusionRay, new AOEShapeCone(30f, 60f.Degrees()));

// Due to the boss rotating as he starts these casts, it impossible to tell any earlier than just when the cast is started
sealed class GroundingJoltBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundingJoltBig, 20f);
sealed class GroundingJoltSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundingJoltSmall, 6f);

sealed class Missile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Voyage, new AOEShapeRect(100f, 2f));
sealed class MissileBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect shape = new(100f, 2f);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Missile && id == 4565)
        {
            var party = Raid.WithoutSlot();
            var len = party.Length;

            for (var i = 0; i < len; i++)
            {
                if (party[i].Type == ActorType.Pet)
                {
                    continue;
                }

                CurrentBaits.Add(new(actor, party[i], shape));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Voyage)
        {
            if (CurrentBaits.Count > 0)
            {
                CurrentBaits.RemoveAt(0);
            }
        }
    }
}

sealed class ThermobaricChargeBait(BossModule module) : Components.BaitAwayIcon(module, 2f, (uint)IconID.ThermobaricChargeLockOn, (uint)AID.ThermobaricChargeBoss)
{
    private readonly WPos[] positions = [
        new(138.0f, -18.0f), // NE
        new(138.0f, 18.0f), // SE
        new(102.0f, -18.0f), // NW
        new(102.0f, 18.0f) // SW
    ];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count == 0)
        {
            return;
        }

        for (var i = 0; i < 4; ++i)
        {
            Arena.AddCircleUnfilled(positions[i], 1f, Colors.Safe);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (CurrentBaits.Count == 0)
        {
            return;
        }

        hints.Add("Drop the bait at a corner of the map!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor))
        {
            return;
        }

        var bestPosition = positions[0];
        var playerPosition = actor.Position;
        var bestDistance = (playerPosition - bestPosition).LengthSq();

        for (var i = 1; i < 4; ++i)
        {
            var point = positions[i];
            var distance = (playerPosition - point).LengthSq();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPosition = point;
            }
        }

        hints.AddForbiddenZone(new SDInvertedCircle(bestPosition, 1f));
    }
}
sealed class ThermobaricChargeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ThermobaricCharge, 40f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }

        ref readonly var knockback = ref Casters.Ref(0);
        var activation = knockback.Activation;

        if (IsImmune(slot, activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOrigin(Arena.Center, knockback.Origin, 39f, 20f, 20f), activation);
    }
}

sealed class DurgaPieceStates : StateMachineBuilder
{
    public DurgaPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<DiffusionRay>()
            .ActivateOnEnter<GroundingJoltBig>()
            .ActivateOnEnter<GroundingJoltSmall>()
            .ActivateOnEnter<Missile>()
            .ActivateOnEnter<MissileBait>()
            .ActivateOnEnter<ThermobaricChargeBait>()
            .ActivateOnEnter<ThermobaricChargeKnockback>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.DurgaPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14657u, SortOrder = 6)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Thermobaric Charge's knockback")]
public sealed class DurgaPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.SpinnerRookPiece => 2,
                (uint)OID.DurgaPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SpinnerRookPiece));
    }

    private readonly string[] _prePullHints = [
        "Atomic Ray: Heavy raidwide damage - Pet cover will not take damage for you",
    ];

    public override string[] PrePullHints => _prePullHints;
}
