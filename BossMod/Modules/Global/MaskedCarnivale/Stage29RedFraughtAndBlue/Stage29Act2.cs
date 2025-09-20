namespace BossMod.Global.MaskedCarnivale.Stage29.Act2;

public enum OID : uint
{
    Boss = 0x2C5D, //R=3.0
    FireTornado = 0x2C60, // R=4.0
    LeftHand = 0x2C5F, // R=1.3
    RightHand = 0x2C5E, // R=1.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 6499, // Boss->player, no cast, single-target
    AutoAttack2 = 18980, // LeftHand->player, no cast, single-target
    AutoAttack3 = 18976, // RightHand->player, no cast, single-target

    Visual = 18975, // Boss->self, no cast, single-target
    ProteanWave1 = 18971, // Boss->self, 3.0s cast, range 39 30-degree cone, knockback 40, away from source (only first wave)
    ProteanWave2 = 18972, // Boss->self, no cast, range 39 30-degree cone, hits player position, but can be dodged. angle snapshot sometime between cast event of wave 1 and wave 2, not sure if its possible to predict accurately
    ProteanWave3 = 18984, // FireTornado->self, 3.0s cast, range 50 30-degree cone
    Throttle = 18964, // Boss->player, no cast, range 5 circle
    FerrofluidKB = 18963, // Boss->self, 5.0s cast, range 80 circle, knockback 6, away from source
    FluidConvection = 18974, // Boss->self, no cast, range 10-40 donut
    FerrofluidAttract = 18962, // Boss->self, 5.0s cast, range 80 circle, pull 6, between centers
    FluidDynamic = 18973, // Boss->self, no cast, range 6 circle
    FluidBallVisual = 18968, // Boss->self, 3.0s cast, single-target
    FluidBall = 18969, // Helper->location, 3.0s cast, range 5 circle
    WateryGrasp = 19028, // Boss->self, 5.0s cast, single-target, calls left/right hand, happens at 70% max hp
    FluidStrike1 = 18981, // LeftHand->self, no cast, range 12 90-degree cone
    FluidStrike2 = 18977, // RightHand->self, no cast, range 12 90-degree cone
    BigSplashFirst = 18965, // Boss->self, 8.0s cast, range 80 circle, knockback 25, away from source, use diamondback to survive
    BigSplashRepeat = 18966, // Boss->self, no cast, range 80 circle, knockback 25, away from source
    Cascade = 19022, // Boss->self, 4.0s cast, range 80 circle, raidwide, summons water tornados
    Unwind = 18985, // FireTornado->self, 5.0s cast, range 80 circle, damage fall of AOE, about 10 distance is fine
    FluidSwing = 18961, // Boss->player, 4.0s cast, range 11 30-degree cone, interruptible, knockback 50, source forward
    Palmistry = 18982, // LeftHand->player, 3.0s cast, single-target, drains 5000 MP
    WashAwayFirst = 18978, // RightHand->self, 5.0s cast, range 80 circle, multiple raidwides, add should be killed before cast finishes
    WashAwayRest = 18979 // RightHand->self, no cast, range 80 circle
}

public enum SID : uint
{
    Throttle = 700, // Boss->player, extra=0x0, player dies when debuff runs out if not cleansed before
    Stun = 149 // Boss->player, extra=0x0
}

sealed class BigSplash(BossModule module) : Components.RaidwideCast(module, (uint)AID.BigSplashFirst, "Diamondback! (Multiple raidwides + knockbacks)");
sealed class BigSplashKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BigSplashFirst, 25f);
sealed class Cascade(BossModule module) : Components.RaidwideCast(module, (uint)AID.Cascade, "Raidwide + Tornados spawn");
sealed class WateryGrasp(BossModule module) : Components.CastHint(module, (uint)AID.WateryGrasp, "Spawns hands. Focus left hand first.");
sealed class Throttle(BossModule module) : Components.CastHint(module, (uint)AID.Throttle, "Prepare to use Excuviation to remove debuff");
sealed class FluidSwing(BossModule module) : Components.CastInterruptHint(module, (uint)AID.FluidSwing);
sealed class FluidSwingKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FluidSwing, 50f, kind: Kind.DirForward);

sealed class ProteanWave(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ProteanWave1, (uint)AID.ProteanWave3], new AOEShapeCone(39f, 15f.Degrees()));

sealed class KnockbackPull(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private readonly FluidConvectionDynamic _aoe = module.FindComponent<FluidConvectionDynamic>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOE.Length != 0)
        {
            ref var aoe = ref _aoe.AOE[0];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSource(Kind kind)
            => _kb = [new(spell.LocXZ, 6f, Module.CastFinishAt(spell), kind: kind)];
        if (spell.Action.ID == (uint)AID.FerrofluidKB)
        {
            AddSource(Kind.AwayFromOrigin);
        }
        else if (spell.Action.ID == (uint)AID.FerrofluidAttract)
        {
            AddSource(Kind.TowardsOrigin);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FerrofluidKB or (uint)AID.FerrofluidAttract)
        {
            _kb = [];
        }
    }
}

sealed class Unwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Unwind, 10f);
sealed class FluidBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FluidBall, 5f);

sealed class FluidConvectionDynamic(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(10f, 40f);
    private static readonly AOEShapeCircle circle = new(6f);
    public AOEInstance[] AOE = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.FerrofluidKB => donut,
            (uint)AID.FerrofluidAttract => circle,
            _ => null
        };
        if (shape != null)
        {
            AOE = [new(shape, spell.LocXZ, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FluidConvection or (uint)AID.FluidDynamic)
        {
            AOE = [];
        }
    }
}

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var leftHands = Module.Enemies((uint)OID.LeftHand);
        var countL = leftHands.Count;
        if (countL != 0)
        {
            var left = leftHands[0];
            if (!left.IsDead)
            {
                hints.Add($"{left.Name} will drain all your MP, kill it fast!");
                return;
            }
        }
        var rightHands = Module.Enemies((uint)OID.RightHand);
        var countR = rightHands.Count;
        if (countR != 0)
        {
            var right = rightHands[0];
            if (!right.IsDead)
                hints.Add($"{right.Name} will do multiple raidwides, kill it fast!");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Throttle) != null)
            hints.Add("You were doomed! Cleanse it with Exuviation."); // it is called throttle, but works exactly like any cleansable doom
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Throttle on you which needs to be\ncleansed with Excuviation. It will also spawn two hands which need to be\nkilled asap. Focus the left hand first because it will drain all your MP.");
    }
}

sealed class Stage29Act2States : StateMachineBuilder
{
    public Stage29Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<FluidConvectionDynamic>()
            .ActivateOnEnter<FluidSwingKnockback>()
            .ActivateOnEnter<BigSplash>()
            .ActivateOnEnter<BigSplashKB>()
            .ActivateOnEnter<Cascade>()
            .ActivateOnEnter<WateryGrasp>()
            .ActivateOnEnter<Throttle>()
            .ActivateOnEnter<ProteanWave>()
            .ActivateOnEnter<KnockbackPull>()
            .ActivateOnEnter<FluidBall>()
            .ActivateOnEnter<Unwind>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 698u, NameID = 9241u, SortOrder = 2)]
public sealed class Stage29Act2 : BossModule
{
    public Stage29Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LeftHand));
        Arena.Actors(Enemies((uint)OID.RightHand));
    }
}
