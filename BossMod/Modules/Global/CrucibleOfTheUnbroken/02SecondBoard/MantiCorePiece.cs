namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.MantiCorePiece;

public enum OID : uint
{
    ManticorePiece = 0x4C53,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49680, // ManticorePiece->player, no cast, single-target
    Teleport = 48126, // ManticorePiece->location, no cast, single-target

    ArmAndHammerLeftGlow = 48124, // ManticorePiece->self, 5.0+0.6s cast, single-target
    ArmAndHammerLeft = 48125, // Helper->self, 5.6s cast, range 30 90-degree cone
    ArmAndHammerRightGlow = 48122, // ManticorePiece->self, 5.0+0.6s cast, single-target
    ArmAndHammerRight = 48123, // Helper->self, 5.6s cast, range 30 90-degree cone
    DeadlyHold = 48138, // ManticorePiece->player, 5.0s cast, single-target

    HammerleapTeleport = 48135, // ManticorePiece->location, 7.0+1.1s cast, single-target
    Hammerleap = 48137, // Helper->self, 8.1s cast, range 30 circle

    HeadsAndTailsFrontVisual = 48139, // ManticorePiece->self, 3.5+0.4s cast, single-target
    HeadsAndTailsFront = 48140, // Helper->self, 3.9s cast, range 40 180.000-degree cone
    HeadsAndTailsBackVisual = 48141, // ManticorePiece->self, 2.0+0.4s cast, single-target
    HeadsAndTailsBack = 48142, // Helper->self, 2.4s cast, range 40 180.000-degree cone

    TailsAndHeadsBackVisual = 50410, // ManticorePiece->self, 3.5+0.4s cast, single-target
    TailsAndHeadsBack = 50411, // Helper->self, 3.9s cast, range 40 180.000-degree cone
    TailsAndHeadsFrontVisual = 50412, // ManticorePiece->self, 2.0+0.4s cast, single-target
    TailsAndHeadsFront = 50413, // Helper->self, 2.4s cast, range 40 180.000-degree cone

    ChargeAndHammer = 48127, // ManticorePiece->self, 10.0s cast, single-target
    WildChargeVisual = 48128, // Helper->location, 1.5s cast, width 8 rect charge
    WildChargeTeleport = 48129, // ManticorePiece->location, no cast, single-target
    WildCharge = 48130, // Helper->location, 1.1s cast, width 8 rect charge
    ArmAndHammerVisual = 48133, // ManticorePiece->self, no cast, single-target
    ArmAndHammer1 = 48134, // Helper->self, 0.6s cast, range 30 ?-degree cone
    ArmAndHammerVisual1 = 48131, // ManticorePiece->self, no cast, single-target
    ArmAndHammer2 = 48132, // Helper->self, 0.6s cast, range 30 ?-degree cone
}

public enum SID : uint
{
    LeftHandGlow = 2193, // none->ManticorePiece, extra=0x413
    RightHandGlow = 2056, // none->ManticorePiece, extra=0x414
}

public enum IconID : uint
{
    TankBuster = 218, // player->self
}

sealed class ArmAndHammer(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ArmAndHammerLeft, (uint)AID.ArmAndHammerRight],
    new AOEShapeCone(30.0f, 90.0f.Degrees()));
sealed class DeadlyHold(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DeadlyHold);
sealed class Hammerleap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hammerleap, 30.0f);

sealed class WildCharge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly List<ActorStatus> armGlows = [];
    private WPos lastTarget;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.WildChargeVisual)
        {
            return;
        }

        // Case: start of path is from boss location, all other starts is from the last aoe end position
        var origin = aoes.Count == 0 ? Module.PrimaryActor.Position : lastTarget;
        var target = spell.LocXZ;
        var direction = target - origin;
        var shape = new AOEShapeRect(direction.Length(), 4.0f);
        aoes.Add(new(shape, origin, Angle.FromDirection(direction)));
        lastTarget = target;

        // Once the path has been fully created, create the two arm aoes that will happen later
        if (aoes.Count == 4)
        {
            foreach (var glows in armGlows)
            {
                var side = glows.ID == (uint)SID.LeftHandGlow ? 90.0f.Degrees() : -90.0f.Degrees();
                aoes.Add(new(new AOEShapeCone(30.0f, 90.0f.Degrees()), lastTarget, Angle.FromDirection(direction) + side));
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.LeftHandGlow or (uint)SID.RightHandGlow)
        {
            armGlows.Add(status);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.LeftHandGlow or (uint)SID.RightHandGlow)
        {
            if (armGlows.Count > 0)
            {
                armGlows.RemoveAt(0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WildCharge or (uint)AID.ArmAndHammer1 or (uint)AID.ArmAndHammer2)
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
        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return nextAOEs[..max];
    }
}

sealed class HeadsAndTails(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40.0f, 90.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HeadsAndTailsFront or (uint)AID.TailsAndHeadsBack)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation + 180.0f.Degrees(), WorldState.FutureTime(7.7d), risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HeadsAndTailsFront or (uint)AID.HeadsAndTailsBack or (uint)AID.TailsAndHeadsBack or (uint)AID.TailsAndHeadsFront)
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
        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return nextAOEs[..max];
    }
}

sealed class ManticorePieceStates : StateMachineBuilder
{
    public ManticorePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArmAndHammer>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Hammerleap>()
            .ActivateOnEnter<HeadsAndTails>()
            .ActivateOnEnter<WildCharge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.ManticorePiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14545u, SortOrder = 1)]
public sealed class ManticorePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
