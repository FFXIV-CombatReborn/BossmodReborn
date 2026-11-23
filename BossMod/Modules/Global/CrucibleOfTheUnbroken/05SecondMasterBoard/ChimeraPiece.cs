namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.ChimeraPiece;

public enum OID : uint
{
    ChimeraPiece = 0x4CFC,
    IcePuddle = 0x1EC102, // R0.500, x0 (spawn during fight), EventObj type
    BallLightning = 0x4CFD, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49680, // ChimeraPiece->player, no cast, single-target
    ToxicTantrum = 49332, // ChimeraPiece->self, 5.0s cast, range 60 circle
    TheRamsKeeperTeleport = 49319, // ChimeraPiece->location, 4.0+0.5s cast, single-target
    TheRamsKeeper = 49320, // Helper->location, 4.5s cast, range 6 circle

    TheRamsRoarBoss2 = 49309, // ChimeraPiece->self, 6.0+1.0s cast, single-target
    TheRamsRoarBoss3 = 49310, // ChimeraPiece->self, 6.0+1.0s cast, single-target
    TheRamsRoarBoss = 49311, // ChimeraPiece->self, 6.0+1.0s cast, single-target
    TheRamsRoar = 49315, // Helper->self, 7.0s cast, range 12 circle

    TheDragonsRoarBoss = 49312, // ChimeraPiece->self, 6.0s cast, single-target
    TheDragonsRoarBoss2 = 49313, // ChimeraPiece->self, 6.0s cast, single-target
    TheDragonsRoarBoss3 = 49314, // ChimeraPiece->self, 6.0s cast, single-target
    TheDragonsRoar = 49316, // Helper->self, 7.0s cast, range 8-40 donut

    TheLionsBreath = 49304, // Helper->self, 7.0s cast, range 60 240.000-degree cone
    TheRamsBreath = 49300, // Helper->self, 7.0s cast, range 60 240.000-degree cone
    TheDragonsBreath = 49302, // Helper->self, 7.0s cast, range 60 240.000-degree cone

    RushingRoarTeleport = 49322, // ChimeraPiece->player, no cast, single-target
    // Fire (boss - forward)
    RushingRoarTheLionsBreathBossStart = 49325, // ChimeraPiece->self, 5.9s cast, single-target
    RushingRoarTheLionsBreathBossEnd = 49330, // ChimeraPiece->self, 0.5s cast, single-target
    RushingRoarTheLionsBreath = 49331, // Helper->self, 1.5s cast, range 60 240.000-degree cone
    // lightning (right head)
    RushingRoarTheDragonsBreathBossStart = 49324, // ChimeraPiece->self, 5.9s cast, single-target
    RushingRoarTheDragonsBreathBoss = 49328, // ChimeraPiece->self, 0.5s cast, single-target
    RushingRoarTheDragonsBreath = 49329, // Helper->self, 1.5s cast, range 60 240.000-degree cone
    // Ice (left head)
    RushingRoarTheRamsBreathStart = 49323, // ChimeraPiece->self, 5.9s cast, single-target
    RushingRoarTheRamsBreathBoss = 49326, // ChimeraPiece->self, 0.5s cast, single-target
    RushingRoarTheRamsBreath = 49327, // Helper->self, 1.5s cast, range 60 240.000-degree cone

    // BallLightning
    Cacophony = 49317, // ChimeraPiece->self, 3.0s cast, single-target
    ChaoticChorus = 49318, // 4CFD->self, no cast, range 6 circle
}

public enum SID : uint
{
    Poison = 5140, // ChimeraPiece->player, extra=0x0
    Paralysis = 5382, // 4CFD->player, extra=0x0
    Frostbite = 5500, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    TheRamsKeeper = 669, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_arrow01f = 57, // ChimeraPiece->player
    _Gen_Tether_chn_dark001f = 1, // ChimeraPiece->player
}

sealed class ToxicTantrum(BossModule module) : Components.RaidwideCast(module, (uint)AID.ToxicTantrum, "Raidwide + applies poison");
sealed class TheRamsRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsRoar, 12f);
sealed class TheDragonsRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsRoar, new AOEShapeDonut(8f, 40f));
sealed class TheLionsBreath(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TheLionsBreath, (uint)AID.TheRamsBreath,
        (uint)AID.TheDragonsBreath], new AOEShapeCone(60f, 120f.Degrees()));

sealed class TheRamsKeeperBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(18f), (uint)IconID.TheRamsKeeper, centerAtTarget: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheRamsKeeper)
        {
            CurrentBaits.Clear();
        }
    }
}
sealed class TheRamsKeeper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsKeeper, 6f);

sealed class IcePuddle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> puddles = [];
    private readonly AOEShapeCircle shape = new(18f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.IcePuddle)
        {
            puddles.Add(new(shape, actor.Position, actorID: actor.InstanceID));
        }
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == (uint)OID.IcePuddle && state == 4)
        {
            var count = puddles.Count;
            var id = actor.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (puddles[i].ActorID == id)
                {
                    puddles.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(puddles);
}

sealed class BallLightning(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] aoes = [];
    private readonly List<Actor> puddles = [];
    private readonly AOEShapeCapsule shape = new(6f, 1f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.BallLightning)
        {
            puddles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.BallLightning)
        {
            puddles.Remove(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChaoticChorus)
        {
            if (puddles.Count > 0)
            {
                puddles.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return aoes;
    }

    public override void Update()
    {
        var count = puddles.Count;
        if (aoes.Length != count)
            aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var puddle = puddles[i];
            aoes[i] = new(shape, puddle.Position, puddle.Rotation, color: Colors.Danger);
        }
    }
}

sealed class RushingRoarTether(BossModule module) : Components.StretchTetherDuo(module, 16f, 5.8d);
sealed class RushingRoar(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(60f, 120f.Degrees());
    private Angle? rotation;
    private bool aoeLocked = true;
    private WPos startPosition = default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RushingRoarTheLionsBreathBossStart:
                rotation = default;
                break;
            case (uint)AID.RushingRoarTheDragonsBreathBossStart:
                rotation = 120.0f.Degrees();
                break;
            case (uint)AID.RushingRoarTheRamsBreathStart:
                rotation = -120.0f.Degrees();
                break;
            case (uint)AID.RushingRoarTheLionsBreath:
            case (uint)AID.RushingRoarTheDragonsBreath:
            case (uint)AID.RushingRoarTheRamsBreath:
                aoeLocked = true;

                // Case: there are no aoes, so we just display it normally - this shouldn't happen, but just for safety
                if (aoes.Count == 0)
                {
                    aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                    return;
                }

                // Case: there is an aoe stored, but we should update it position to ensure it is accurate and potentially not a couple of pixels off
                if (aoes.Count > 0)
                {
                    ref var aoe = ref aoes.Ref(0);
                    aoe.Origin = spell.LocXZ;
                    aoe.Rotation = spell.Rotation;
                    aoe.Activation = Module.CastFinishAt(spell);
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.RushingRoarTheLionsBreath or (uint)AID.RushingRoarTheDragonsBreath or (uint)AID.RushingRoarTheRamsBreath)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
                rotation = null;
                aoeLocked = true;
            }
        }
        else if (id == (uint)AID.RushingRoarTeleport)
        {
            aoeLocked = false;
            startPosition = Module.PrimaryActor.Position;
        }
    }

    public override void Update()
    {
        if (aoeLocked || rotation == null)
        {
            return;
        }

        if ((Module.PrimaryActor.Position - startPosition).LengthSq() > 0.5f && Module.PrimaryActor.LastFrameMovementVec4 == default)
        {
            aoes.Add(new(shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + rotation.Value));
            aoeLocked = true;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class ChimeraPieceStates : StateMachineBuilder
{
    public ChimeraPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ToxicTantrum>()
            .ActivateOnEnter<TheDragonsRoar>()
            .ActivateOnEnter<TheRamsRoar>()
            .ActivateOnEnter<TheLionsBreath>()
            .ActivateOnEnter<TheRamsKeeperBait>()
            .ActivateOnEnter<TheRamsKeeper>()
            .ActivateOnEnter<IcePuddle>()
            .ActivateOnEnter<BallLightning>()
            .ActivateOnEnter<RushingRoarTether>()
            .ActivateOnEnter<RushingRoar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.ChimeraPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14663u, SortOrder = 8)]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Poison, Frostbite and Ball Lightning's Paralysis")]
public sealed class ChimeraPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    private readonly string[] _prePullHints = [
        "Toxic Tantrum: Raidwide + poison, can be esuna’d",
    ];

    public override string[] PrePullHints => _prePullHints;
}
