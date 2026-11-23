namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.AdministratorPiece;

public enum OID : uint
{
    AdministratorPiece = 0x4CC4,
    BugPiece = 0x4CC5, // R0.960, x0 (spawn during fight)
    DronePiece = 0x4CC8, // R1.600, x0 (spawn during fight)
    FootmanPiece = 0x4CC6, // R1.050, x0 (spawn during fight)
    CobraPiece = 0x4CCA, // R1.200, x0 (spawn during fight)
    DreadnaughtPiece = 0x4CC7, // R2.700, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50542, // AdministratorPiece->player, no cast, single-target
    AutoAttackFootman = 49682, // 4CC6->player, no cast, single-target
    AutoAttackCobraPiece = 49680, // 4CCA->player, no cast, single-target
    AutoAttackIncinerate = 48742, // 4CC7->self, no cast, range 6 ?-degree cone
    AutoCannons = 48735, // 4CC8->self, 3.5s cast, range 40 width 5 rect
    Object199 = 48743, // AdministratorPiece->self, 5.0s cast, range 60 circle
    Headspin = 48736, // 4CC6->self, 1.5s cast, range 5 circle
    FlameThrowerBoss = 48746, // AdministratorPiece->self, 6.2+0.8s cast, single-target
    FlameThrower = 48747, // Helper->self, 7.0s cast, range 40 120.000-degree cone
    NoxiousNova = 48738, // 4CCA->self, 8.0s cast, range 60 circle
    Confound = 48740, // AdministratorPiece->self, 8.0s cast, range 60 circle
    PiercingLaserBoss = 48744, // AdministratorPiece->self, 6.0s cast, range 80 circle
    PiercingLaser = 48745, // AdministratorPiece->self, no cast, range 40 width 5 rect

    Unknown = 48734, // 4CC5->AdministratorPiece, no cast, single-target - Most likely the BugPiece skill when they reach the boss
}

public enum SID : uint
{
    DamageUp = 2550, // 4CC5->AdministratorPiece, extra=0x1
    Rehabilitation = 989, // 4CC5->AdministratorPiece, extra=0x1
    Confused = 1283, // AdministratorPiece->player, extra=0x0
    Poison = 5140, // CobraPiece->player, extra=0x0
    Bind = 2518, // AdministratorPiece->player, extra=0x0
    FireResistanceDown = 5336, // 4CC7->player, extra=0x1
}

public enum IconID : uint
{
    StackShareLaser = 524, // AdministratorPiece->player
}

public enum TetherID : uint
{
    BugPieceTether = 399, // 4CC5->AdministratorPiece
    TargetTether = 17, // 4CC6->player
    PiercingLaserTether = 54, // AdministratorPiece->player
}

sealed class AutoCannons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AutoCannons, new AOEShapeRect(40f, 2.5f));
sealed class Object199(BossModule module) : Components.RaidwideCast(module, (uint)AID.Object199);
sealed class Headspin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Headspin, 5f);
sealed class FlameThrower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlameThrower, new AOEShapeCone(40f, 60f.Degrees()));
sealed class NoxiousNova(BossModule module) : Components.RaidwideCast(module, (uint)AID.NoxiousNova, "Applies poison!");
sealed class Confound(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Confound, showNameInHint: true);

sealed class PiercingLaser(BossModule module) : Components.GenericWildCharge(module, 2.5f, (uint)AID.PiercingLaserBoss, 40f)
{
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StackShareLaser)
        {
            var targetPlayer = WorldState.Actors.Find(targetID);
            if (targetPlayer == null)
            {
                return;
            }

            target = targetPlayer;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PiercingLaser)
        {
            target = null;
            Source = null;
        }
    }

    private void InitIfReady()
    {
        if (target == null || Source == null)
        {
            return;
        }

        foreach (var (slot, player) in Raid.WithSlot(false, true, true))
        {
            PlayerRoles[slot] = player.InstanceID == target.InstanceID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}

sealed class AdministratorPieceStates : StateMachineBuilder
{
    public AdministratorPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoCannons>()
            .ActivateOnEnter<Object199>()
            .ActivateOnEnter<Headspin>()
            .ActivateOnEnter<FlameThrower>()
            .ActivateOnEnter<NoxiousNova>()
            .ActivateOnEnter<Confound>()
            .ActivateOnEnter<PiercingLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.AdministratorPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14610u, SortOrder = 6)]
[PreferredKinship(BST.Kinship.Soul, "Interrupt Confound")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Poison from Cobra Piece")]
public sealed class AdministratorPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BugPiece => 6,
                (uint)OID.CobraPiece => 5,
                (uint)OID.DreadnaughtPiece => 4,
                (uint)OID.FootmanPiece => 3,
                (uint)OID.DronePiece => 2,
                (uint)OID.AdministratorPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BugPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.CobraPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.DronePiece));
        Arena.Actors(Enemies((uint)OID.FootmanPiece));
        Arena.Actors(Enemies((uint)OID.DreadnaughtPiece));
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: BugPiece/CobraPiece (purple) -> DreadnaughtPiece -> DronePiece/FootmanPiece -> Boss",
        "Interrupt the Confound spell or you will get confused for 12 seconds and most likely die - This can be skipped if you kill the boss fast enough"
    ];

    public override string[] PrePullHints => _prePullHints;
}
