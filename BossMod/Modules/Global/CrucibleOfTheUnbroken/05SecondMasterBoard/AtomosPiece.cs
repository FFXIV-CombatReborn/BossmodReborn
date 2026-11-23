namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.AtomosPiece;

public enum OID : uint
{
    AtomosPiece = 0x4CE6,
    RedGremlinPiece = 0x4CE7, // R1.500, x0 (spawn during fight)
    GremlinPiece = 0x4CE8, // R1.000, x0 (spawn during fight)
    VodorigaPiece = 0x4CEC, // R1.440, x0 (spawn during fight)
    BavaroisPiece = 0x4CEA, // R2.400, x0 (spawn during fight)
    PuddingPiece = 0x4CE9, // R2.400, x0 (spawn during fight)
    FlanPiece = 0x4CEB, // R2.400, x0 (spawn during fight)
    MenacingDahakPiece = 0x4CED, // R3.750, x0 (spawn during fight)
    WaterPuddle = 0x1EAC2A, // R0.500, x0 (spawn during fight), EventObj type
    AethericChargeBig = 0x4CEF, // R2.000, x0 (spawn during fight)
    AethericChargeSmall = 0x4CEE, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50941, // AtomosPiece->player, no cast, single-target

    ManaManifestationBoss = 49232, // AtomosPiece->self, 3.0s cast, single-target
    ManaManifestation = 49233, // Helper->location, 4.0s cast, range 8 circle
    TeleportHelper = 49234, // Helper->location, no cast, single-target
    Shockwave = 49238, // AtomosPiece->self, 6.0s cast, range 40 circle
    DarkIIBoss = 49239, // AtomosPiece->self, 6.3+0.7s cast, single-target
    DarkII = 49240, // Helper->self, 7.0s cast, range 60 120.000-degree cone
    ForsakenFount = 49235, // AtomosPiece->self, 5.0s cast, range 60 circle

    // RedGremlinPiece /  GremlinPiece
    AutoAttackGremlin = 50398, // 4CE8/4CE7->player, no cast, single-target
    BadMouth = 49241, // 4CE7->player, 3.0s cast, single-target

    // VodorigaPiece
    TerrorEye = 49247, // 4CEC->location, 3.0s cast, range 6 circle

    // BavaroisPiece
    AutoAttackWater = 50748, // 4CEA->player, no cast, single-target
    WaterIII = 49242, // 4CEA->self, 2.0s cast, range 60 circle

    // PuddingPiece
    AutoAttackFire = 51241, // 4CE9->player, no cast, single-target
    FireIII = 49243, // 4CE9->player, 8.0s cast, range 6 circle

    // FlanPiece
    AutoAttackThunder = 51242, // 4CEB->player, no cast, single-target
    ThunderIIIActor = 49244, // 4CEB->self, 8.0s cast, single-target
    ThunderIII = 49245, // Helper->location, 8.0s cast, range 5 circle
    ThunderIIISpread = 49246, // Helper->players, no cast, range 5 circle

    // AethericCharge
    ExplosionBig = 49237, // 4CEF->self, 2.0s cast, range 12 circle
    ExplosionSmall = 49236, // 4CEE->self, 2.0s cast, range 6 circle

    // MenacingDahakPiece
    AutoAttackMenacingDahak = 49680, // 4CED->player, no cast, single-target
    TailDrive = 49249, // 4CED->self, 5.0s cast, range 30 90.000-degree cone
    RottenBreath = 49248, // 4CED->self, 5.0s cast, range 28 120.000-degree cone
}

public enum SID : uint
{
    PhysicalVulnerabilityUp = 126, // 4CE7->player, extra=0x1
    FireResistanceUp = 5551, // none->player, extra=0x0
    LightningResistanceDown = 5552, // none->player, extra=0x0
}

public enum IconID : uint
{
    FireIIIStack = 100, // player->self
    ThunderIIISpread = 704, // player->self
}

sealed class ManaManifestation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManaManifestation, 8f);
sealed class TerrorEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrorEye, 6f);
sealed class FireIII(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FireIIIStack, (uint)AID.FireIII, 6f, 8.1d, minStackSize: 1);
sealed class ThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderIII, 5f);
sealed class ThunderIIISpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ThunderIIISpread, (uint)AID.ThunderIIISpread, 5f, 8.1d);
sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeCone(60f, 60f.Degrees()));
sealed class ForsakenFount(BossModule module) : Components.RaidwideCast(module, (uint)AID.ForsakenFount);
sealed class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(30f, 45f.Degrees()));
sealed class RottenBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenBreath, new AOEShapeCone(28f, 60f.Degrees()));

sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 16f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }

        var knockback = Casters[0];
        var activation = Casters[0].Activation;

        if (IsImmune(slot, activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, 16f, 19f), activation);
    }
}

sealed class WaterIII(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WaterIII, 20f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }

        var knockback = Casters[0];
        var activation = Casters[0].Activation;

        if (IsImmune(slot, activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, knockback.Origin, 20f, 20f), activation);
    }
}

sealed class WaterPuddles(BossModule module) : Components.PersistentInvertibleVoidzone(module, 5f, GetVoidzones)
{
    private readonly FireIII? fireIII = module.FindComponent<FireIII>();

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? Colors.SafeFromAOE : Colors.Danger;
        foreach (var s in Sources(Module))
        {
            Shape.Draw(Arena, s.Position, s.Rotation, color);
        }
    }

    // Since the waterPuddle may be killed late or after the fire puddle, we have to make the voidzone base on if there is a stack or not
    public override void Update()
    {
        base.Update();

        if (fireIII == null || !fireIII.Active)
        {
            InvertResolveAt = default;
            return;
        }

        InvertResolveAt = WorldState.CurrentTime;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Sources(Module).Any())
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.WaterPuddle);
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

sealed class AethericChargeBig(BossModule module) : Components.Voidzone(module, 12f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AethericChargeBig);
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

sealed class AethericChargeSmall(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.AethericChargeSmall);
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

sealed class AtomosPieceStates : StateMachineBuilder
{
    public AtomosPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManaManifestation>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<ThunderIIISpread>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<ForsakenFount>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<RottenBreath>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<AethericChargeBig>()
            .ActivateOnEnter<AethericChargeSmall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.AtomosPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14642u, SortOrder = 5)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Shockwave and Water III knockbacks")]
public sealed class AtomosPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BavaroisPiece => 8,
                (uint)OID.PuddingPiece => 7,
                (uint)OID.FlanPiece => 6,
                (uint)OID.MenacingDahakPiece => 5,
                (uint)OID.RedGremlinPiece => 4,
                (uint)OID.GremlinPiece => 3,
                (uint)OID.VodorigaPiece => 2,
                (uint)OID.AtomosPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.RedGremlinPiece));
        Arena.Actors(Enemies((uint)OID.GremlinPiece));
        Arena.Actors(Enemies((uint)OID.VodorigaPiece));
        Arena.Actors(Enemies((uint)OID.BavaroisPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.PuddingPiece));
        Arena.Actors(Enemies((uint)OID.MenacingDahakPiece));
    }

    private readonly string[] _prePullHints = [
        "Fight kill priority: BavaroisPiece -> PuddingPiece/FlanPiece/MenacingDahakPiece -> Gremlins -> VodorigaPiece -> Boss",
        "BavaroisPiece will cast a knockback when killed and spawn 9 puddles"
    ];

    public override string[] PrePullHints => _prePullHints;
}
