namespace BossMod.Dawntrail.Hunt.RankA.UrnaVariabilis;

public enum OID : uint
{
    Boss = 0x416F // R3.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ProximityPlasma1 = 39106, // Boss->self, 5.0s cast, range 20 circle
    ProximityPlasma2 = 39113, // Boss->self, 1.0s cast, range 20 circle
    RingLightning1 = 39107, // Boss->self, 5.0s cast, range 8-60 donut
    RingLightning2 = 39114, // Boss->self, 1.0s cast, range 8-60 donut
    Magnetron = 39108, // Boss->self, 3.0s cast, range 40 circle, applies positive/negative charges, knockback or attract distance 10
    Magnetoplasma1 = 39109, // Boss->self, 6.0s cast, range 60 circle, circle, positive charge
    Magnetoplasma2 = 39111, // Boss->self, 6.0s cast, range 60 circle, circle, negative charge
    Magnetoring1 = 39110, // Boss->self, 6.0s cast, range 60 circle, donut, positive charge
    Magnetoring2 = 39112, // Boss->self, 6.0s cast, range 60 circle, donut, negative charge
    ThunderousShower = 39115, // Boss->player, 5.0s cast, range 6 circle stack
    Electrowave = 39116 // Boss->self, 4.0s cast, range 60 circle
}

public enum SID : uint
{
    PositiveCharge = 4071, // Boss->player, extra=0x0
    NegativeCharge = 4072 // Boss->player, extra=0x0
}

public enum IconID : uint
{
    PositiveChargeBoss = 291, // Boss
    NegativeChargeBoss = 290 // Boss
}

sealed class Magnetism(BossModule module) : Components.GenericKnockback(module)
{
    private BitMask positiveCharge;
    private BitMask negativeCharge;
    private DateTime activation;
    private bool bossCharge; // false if negative, true if positive
    private bool shape; // false if circle, true if donut
    private MagnetismCircleDonut? _aoe;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (activation != default)
        {
            if (positiveCharge[slot])
                return new Knockback[1] { new(Module.PrimaryActor.Position, 10f, activation, kind: bossCharge ? Kind.AwayFromOrigin : Kind.TowardsOrigin) };
            if (negativeCharge[slot])
                return new Knockback[1] { new(Module.PrimaryActor.Position, 10f, activation, kind: bossCharge ? Kind.TowardsOrigin : Kind.AwayFromOrigin) };
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Magnetoplasma1:
            case (uint)AID.Magnetoring1:
                bossCharge = true;
                shape = spell.Action.ID == (uint)AID.Magnetoring1;
                activation = Module.CastFinishAt(spell);
                break;
            case (uint)AID.Magnetoplasma2:
            case (uint)AID.Magnetoring2:
                bossCharge = false;
                shape = spell.Action.ID == (uint)AID.Magnetoring2;
                activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Magnetoplasma1 or (uint)AID.Magnetoplasma2 or (uint)AID.Magnetoring1 or (uint)AID.Magnetoring2)
        {
            activation = default;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _aoe ??= Module.FindComponent<MagnetismCircleDonut>();
        if (_aoe!.AOE.Length != 0)
        {
            ref var aoe = ref _aoe.AOE[0];
            if (aoe.Check(pos))
                return true;
        }
        return false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PositiveCharge:
                positiveCharge.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.NegativeCharge:
                negativeCharge.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PositiveCharge:
                positiveCharge.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.NegativeCharge:
                negativeCharge.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (activation == default)
            return;

        var active = ActiveKnockbacks(slot, actor);
        if (active.Length == 0 || IsImmune(slot, active[0].Activation))
            return;

        var isPositive = positiveCharge[slot];
        var isNegative = negativeCharge[slot];
        var isPull = !bossCharge && isPositive || bossCharge && isNegative;
        var isKnockback = bossCharge && isPositive || !bossCharge && isNegative;
        var pos = Module.PrimaryActor.Position;

        ShapeDistance? forbidden = shape
            ? isPull ? new SDInvertedCircle(pos, 18f)
            : isKnockback ? new SDInvertedCircle(pos, 1f)
            : null
            : isPull ? new SDCircle(pos, 30f)
            : isKnockback ? new SDCircle(pos, 10f)
            : null;

        if (forbidden != null)
        {
            hints.AddForbiddenZone(forbidden, activation);
        }
    }
}

sealed class MagnetismCircleDonut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Magnetism _kb = module.FindComponent<Magnetism>()!;
    private static readonly AOEShapeDonut donut = new(8f, 60f);
    private static readonly AOEShapeCircle circle = new(20f);
    public AOEInstance[] AOE = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, double delay = default) => AOE = [new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay))];
        switch (spell.Action.ID)
        {
            case (uint)AID.Magnetoplasma1:
            case (uint)AID.Magnetoplasma2:
                AddAOE(circle, 2.5d);
                break;
            case (uint)AID.ProximityPlasma1:
                AddAOE(circle);
                break;
            case (uint)AID.Magnetoring1:
            case (uint)AID.Magnetoring2:
                AddAOE(donut, 2.5d);
                break;
            case (uint)AID.RingLightning1:
                AddAOE(donut);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ProximityPlasma1 or (uint)AID.ProximityPlasma2 or (uint)AID.RingLightning1 or (uint)AID.RingLightning2)
        {
            AOE = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kb = _kb.ActiveKnockbacks(slot, actor);
        var len = kb.Length;
        if (len == 0 || len != 0 && _kb.IsImmune(slot, kb[0].Activation))
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class ThunderousShower(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThunderousShower, 6f, 8);
sealed class ElectrowaveMagnetron(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Electrowave, (uint)AID.Magnetron]);

sealed class UrnaVariabilisStates : StateMachineBuilder
{
    public UrnaVariabilisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrowaveMagnetron>()
            .ActivateOnEnter<Magnetism>()
            .ActivateOnEnter<MagnetismCircleDonut>()
            .ActivateOnEnter<ThunderousShower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13158)]
public sealed class UrnaVariabilis(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
