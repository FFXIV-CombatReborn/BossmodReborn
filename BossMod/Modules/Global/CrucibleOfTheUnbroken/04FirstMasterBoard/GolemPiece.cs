namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.GolemPiece;

public enum OID : uint
{
    GolemPiece = 0x4CCB,
    GolemPiece1 = 0x4CCC, // R4.000, x1
    GolemPieceHeart = 0x4D5A, // R2.200, x0 (spawn during fight), Part type
    SkyRock = 0x1EC0D2, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackStone = 50930, // GolemPiece/4CCC->player, no cast, single-target

    EarthenRingBoss = 48752, // GolemPiece->self, 6.2+0.8s cast, single-target
    EarthenRing = 48753, // Helper->self, 7.0s cast, range 5-50 donut
    RockWall = 48748, // GolemPiece/4CCC->self, 3.0+1.0s cast, single-target
    Shockwave = 48750, // Helper->self, 4.0s cast, range 5 width 5 rect
    Rockslide = 48751, // Helper->self, no cast, range 30 width 10 rect
    PlaincrackerBoss = 48754, // 4CCC->self, 6.0+1.0s cast, single-target
    Plaincracker = 48755, // Helper->self, 7.0s cast, range 20 circle
    Obliterate = 50649, // 4CCC->self, 5.0s cast, range 60 circle
    OutcropBoss = 48767, // 4CCC/GolemPiece->self, 4.2+0.8s cast, single-target
    Outcrop = 48768, // Helper->self, 5.0s cast, range 40 60.000-degree cone
    Stoneshower = 48756, // 4CCC/GolemPiece->self, 3.0s cast, single-target
    StoneshowerCircle = 48757, // Helper->self, 1.0s cast, range 8 circle
    StoneshowerDonut = 48758, // Helper->self, 1.0s cast, range 3-11 donut
    SelfDestruct = 48766, // 4D5A->self, 20.0s cast, range 100 circle

    // Swap spells
    PlaincrackerSwap = 48760, // GolemPiece->self, 6.0+1.0s cast, single-target
    Plaincracker1 = 48764, // 4CCC->self, no cast, single-target
    PlaincrackerShort = 48765, // Helper->self, 1.0s cast, range 20 circle

    EarthenRingSwap = 48759, // GolemPiece->self, 6.2+0.8s cast, single-target
    EarthenRing1 = 48762, // GolemPiece1->self, 0.5s cast, single-target
    EarthenRingShort = 48763, // Helper->self, 1.0s cast, range 5-50 donut

    // Most likely to do with them swapping / turning into enrage heart
    GolemDeath = 48761, // Helper->4CCC/GolemPiece, no cast, single-target
    Unknown1 = 50687, // 4CCC/GolemPiece->self, no cast, single-target
}

public enum TetherID : uint
{
    SwapTether = 431 // GolemPiece->4CCC
}

sealed class EarthenRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthenRing, new AOEShapeDonut(5f, 50f));
sealed class Plaincracker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plaincracker, 20f);
sealed class Outcrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Outcrop, new AOEShapeCone(40f, 30f.Degrees()));
sealed class SelfDestruct(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDestruct, "Enrage");

sealed class Shockwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shockwave, new AOEShapeRect(5f, 2.5f))
{
    // Used to track if the mechanic started - needed as the boss can die while casting it, if the cast goes through and the boss dies, the mechanic
    // will still play out
    private bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GolemDeath && !active)
        {
            if (Casters.Count > 0)
            {
                Casters.Clear();
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (state)
        {
            case 0x00020001u:
            case 0x00200010u:
                active = true;
                break;
            case 0x00400004u:
            case 0x00080004u:
                Casters.Clear();
                active = false;
                break;
        }
    }
}

sealed class Rockslide(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect shape = new(15f, 5f, 15f);

    // Used to track if the mechanic started - needed as the boss can die while casting it, if the cast goes through and the boss dies, the mechanic
    // will still play out
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GolemDeath && !active)
        {
            if (aoes.Count > 0)
            {
                aoes.Clear();
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (aoes.Count == 0)
        {
            if (state == 0x00200010u)
            { // Outside is bad - so we just display everything right away
                var pos1 = new WPos(505f, 0f);
                var pos2 = new WPos(535f, 0f);
                var rot = 180f.Degrees();
                aoes.Add(new(shape, pos1, rot, shapeDistance: shape.Distance(pos1, rot)));
                aoes.Add(new(shape, pos2, rot, shapeDistance: shape.Distance(pos2, rot)));
                active = true;
            }
            else if (state == 0x00020001u)
            { // Inside is bad
                var pos = new WPos(520f, 0f);
                var rot = 180f.Degrees();
                aoes.Add(new(shape, pos, rot, shapeDistance: shape.Distance(pos, rot)));
                active = true;
            }
        }
        else if (state is 0x00400004u or 0x00080004u)
        {
            aoes.Clear();
            active = false;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class PlaincrackerSwap(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle circle = new(20f);
    private readonly AOEShapeDonut donut = new(5.0f, 50.0f);
    private Actor? swapSource; // Used to track is the main source dies before the cast finishes
    private Actor? targetSource;
    private const float riskyWindow = 6.0f;
    private enum SpellType { NONE, CIRCLE, DONUT }
    private SpellType aoeType = SpellType.NONE;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.SwapTether)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null)
            {
                return;
            }

            swapSource = source;
            targetSource = target;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PlaincrackerSwap)
        {
            aoeType = SpellType.CIRCLE;
            InitIfReady();
            return;
        }

        if (spell.Action.ID == (uint)AID.EarthenRingSwap)
        {
            aoeType = SpellType.DONUT;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (swapSource == null || targetSource == null || aoeType == SpellType.NONE)
        {
            return;
        }

        if (aoeType == SpellType.CIRCLE)
        {
            aoes.Add(new(circle, targetSource.Position, targetSource.Rotation, WorldState.FutureTime(12.7f)));
            return;
        }

        if (aoeType == SpellType.DONUT)
        {
            aoes.Add(new(donut, targetSource.Position, targetSource.Rotation, WorldState.FutureTime(12.7f)));
        }
    }

    // If the source of the cast dies before the cast finishes then the swapped boss will not cast PlainCracker
    public override void OnActorDeath(Actor actor)
    {
        if (swapSource == null)
        {
            return;
        }

        if (actor.InstanceID == swapSource.InstanceID)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PlaincrackerShort or (uint)AID.EarthenRingShort)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
                targetSource = null;
                swapSource = null;
                aoeType = SpellType.NONE;
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

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var time = WorldState.CurrentTime;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            aoe.Risky = aoe.Activation.AddSeconds(-riskyWindow) <= time;
        }

        return incomingAOEs;
    }
}

sealed class SkyRock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle circle = new(8f);
    private readonly AOEShapeDonut donut = new(3f, 11f);
    private const float riskyWindow = 5.0f;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is var oid && oid == (uint)OID.SkyRock && state == 0x00010002u)
        {
            aoes.Add(new(circle, actor.Position, actor.Rotation, WorldState.FutureTime(10.0f)));
        }
        else if (oid == (uint)OID.SkyRock && state == 0x00400080u)
        {
            aoes.Add(new(donut, actor.Position, actor.Rotation, WorldState.FutureTime(10.0f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.StoneshowerCircle or (uint)AID.StoneshowerDonut)
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

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var time = WorldState.CurrentTime;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            aoe.Risky = aoe.Activation.AddSeconds(-riskyWindow) <= time;
        }

        return incomingAOEs;
    }
}

sealed class Obliterate(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Obliterate, 17f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return;
        }

        var knockbacks = CollectionsMarshal.AsSpan(Casters);
        ref var knockback = ref knockbacks[0];

        if (IsImmune(slot, knockback.Activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOrigin(Arena.Center, knockback.Origin, knockback.Distance, 19.0f, 14.0f), knockback.Activation);
    }
}

sealed class GolemPieceStates : StateMachineBuilder
{
    public GolemPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EarthenRing>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<Outcrop>()
            .ActivateOnEnter<PlaincrackerSwap>()
            .ActivateOnEnter<SkyRock>()
            .ActivateOnEnter<SelfDestruct>()
            .Raw.Update = () => AllDeadOrDestroyed(GolemPiece.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.GolemPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14617u, SortOrder = 7)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Obliterate's knockback")]
public sealed class GolemPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, 0f), new ArenaBoundsRect(20.0f, 15.0f))
{
    public static readonly uint[] Bosses = [(uint)OID.GolemPiece, (uint)OID.GolemPiece1, (uint)OID.GolemPieceHeart];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Bosses);
    }

    private readonly string[] _prePullHints = [
        "This boss has an enrage - When you have killed both golems, it will turn into a heart which you have to kill within 20.0 seconds",
    ];

    public override string[] PrePullHints => _prePullHints;
}
