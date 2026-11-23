namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.MindFlayerPiece;

public enum OID : uint
{
    MindflayerPiece = 0x4CDD,
    MyconidPiece = 0x4CDE, // R0.600, x0 (spawn during fight)
    WaterPuddle = 0x1E9998, // R0.500, x0 (spawn during fight), EventObj type
    ShroombedPuddle = 0x4CDF, // R6.000, x0 (spawn during fight)
    ArcaneSphere = 0x4CE1, // R1.000-1.860, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackThunder = 48623, // MindflayerPiece->player, no cast, single-target
    VoidWaterIIIBoss = 49199, // MindflayerPiece->self, 5.0s cast, single-target
    VoidWaterIII = 49200, // Helper->players, no cast, range 8 circle
    VoidThunderIIIBoss = 49205, // MindflayerPiece->self, 5.0s cast, single-target
    VoidThunderIIITB = 49206, // Helper->player, no cast, range 6 circle
    VoidThunderIIICross = 49207, // Helper->self, 4.0s cast, range 50 width 10 cross
    VoidThunderIIICross1 = 50939, // Helper->self, 4.0s cast, range 50 width 10 cross
    ArcaneUtterance = 49201, // MindflayerPiece->self, 5.0s cast, single-target
    ArcaneEnhancement = 49202, // MindflayerPiece->self, 5.0s cast, single-target
    DarkCurrentSmall = 49203, // 4CE1->self, 2.0s cast, range 100 width 4 rect
    DarkCurrentBig = 49204, // 4CE1->self, 2.0s cast, range 100 width 10 rect
    VoidParalyzeIII = 49208, // MindflayerPiece->self, 7.0s cast, range 60 circle

    // MyconidPiece
    AutoAttackMyconidPiece = 49682, // 4CDE->player, no cast, single-target
    SporeSpill = 49196, // Helper->self, 1.0s cast, range 6 circle

    Unknown = 49197, // 4CDF->self, no cast, range 6 circle - most likely spawning the puddle
}

public enum SID : uint
{
    WaterResistanceDown = 5021, // Helper->player, extra=0x1/0x2
    LightningResistanceDownII = 4456, // none->player, extra=0x0
    SustainedDamage = 3795, // none->4CE1, extra=0x1
    Paralysis = 5382, // MindflayerPiece->player, extra=0x0
}

public enum IconID : uint
{
    VoidWaterIIIIcon = 135, // player/4A04/4A15->self
    VoidThunderIIITankBuster = 344, // player->self
}

public enum TetherID : uint
{
    ArcaneEnhancementTether = 426, // 4CE1->MindflayerPiece
}

sealed class VoidThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidThunderIIITankBuster, (uint)AID.VoidThunderIIITB, 6f, 5.1d);
sealed class VoidThunderIIICross(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VoidThunderIIICross, (uint)AID.VoidThunderIIICross1],
    new AOEShapeCross(50f, 5f));
sealed class VoidParalyzeIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidParalyzeIII, "Raidwide + Applies Paralysis");

sealed class VoidWaterIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidWaterIIIIcon, (uint)AID.VoidWaterIII, 8f, 5.1d);

sealed class WaterPuddles : Components.PersistentInvertibleVoidzone
{
    private readonly VoidThunderIII? buster;

    public WaterPuddles(BossModule module) : base(module, 8f, GetVoidzones)
    {
        InvertResolveAt = WorldState.CurrentTime;
        buster = module.FindComponent<VoidThunderIII>();
    }

    private DateTime BusterActivation(Actor actor)
    {
        if (buster == null)
        {
            return default;
        }

        var spreads = CollectionsMarshal.AsSpan(buster.Spreads);
        var len = spreads.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var spread = ref spreads[i];
            if (spread.Target == actor)
            {
                return spread.Activation;
            }
        }
        return default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Sources(Module).Any())
        {
            return;
        }

        if (BusterActivation(actor) != default)
        {
            hints.Add("Bait the buster away from puddles!");
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var activation = BusterActivation(actor);
        if (activation == default)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        foreach (var puddle in Sources(Module))
        {
            hints.AddForbiddenZone(new SDCircle(puddle.Position, Shape.Radius), activation);
        }
    }

    public static Actor[] GetVoidzones(BossModule module)
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

sealed class SporeSpill(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(6f);

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.MyconidPiece)
        {
            aoes.Add(new(shape, actor.Position, actor.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SporeSpill)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class ShroombedPuddles(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ShroombedPuddle);
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

sealed class ArcaneEnhancement(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private readonly AOEShapeRect smallRect = new(100f, 2f, 100f);
    private readonly AOEShapeRect bigRect = new(100f, 5f, 100f);
    private bool active; // Used to wait showing the aoes until the tethers are sent out

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphere)
        {
            var loc = actor.Position.Quantized();
            var rot = actor.Rotation;
            // activation seems to have like 1s variation, using the lowest I found
            _aoes.Add(new(smallRect, loc, rot, WorldState.FutureTime(16.7d), actorID: actor.InstanceID, shapeDistance: smallRect.Distance(loc, rot)));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ArcaneEnhancementTether)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = source.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe.Shape = bigRect;
                    aoe.ShapeDistance = bigRect.Distance(aoe.Origin, aoe.Rotation);
                    active = true;
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DarkCurrentSmall or (uint)AID.DarkCurrentBig)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    active = count > 1;
                    return;
                }
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!active)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }
}

sealed class MindflayerPieceStates : StateMachineBuilder
{
    public MindflayerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidWaterIII>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<VoidThunderIIICross>()
            .ActivateOnEnter<VoidParalyzeIII>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<ShroombedPuddles>()
            .ActivateOnEnter<ArcaneEnhancement>()
            .ActivateOnEnter<SporeSpill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.MindflayerPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14633u, SortOrder = 2)]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Paralysis from Void Paralyze III")]
public sealed class MindflayerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var puddles = WaterPuddles.GetVoidzones(this);

        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.MyconidPiece)
            {
                var inPuddle = InsidePuddle(e.Actor, puddles);
                e.Priority = inPuddle ? 2 : AIHints.Enemy.PriorityForbidden;
                if (!inPuddle && e.Actor.TargetID != actor.InstanceID)
                {
                    e.PreferProvoking = true;
                }
            }
            else
            {
                e.Priority = e.Actor.OID == (uint)OID.MindflayerPiece ? 1 : 0;
            }
        }
    }

    private static bool InsidePuddle(Actor actor, Actor[] puddles)
    {
        foreach (var puddle in puddles)
        {
            if (actor.Position.InCircle(puddle.Position, 6.0f))
            {
                return true;
            }
        }

        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MyconidPiece), Colors.Vulnerable);
    }

    private readonly string[] _prePullHints = [
        "Kill the MyconidPiece inside the water puddles to solve the mechanic correctly"
    ];

    public override string[] PrePullHints => _prePullHints;
}
