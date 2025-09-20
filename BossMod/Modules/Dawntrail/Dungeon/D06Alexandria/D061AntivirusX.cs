namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D061AntivirusX;

public enum OID : uint
{
    Boss = 0x4173, // R8.0
    ElectricCharge = 0x18D6, // R0.5
    InterferonC = 0x4175, // R1.0
    InterferonR = 0x4174, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36388, // Boss->player, no cast, single-target

    ImmuneResponseVisualSmall = 36378, // Boss->self, 5.0s cast, single-target
    ImmuneResponseSmall = 36379, // Helper->self, 6.0s cast, range 40 120-degree cone, frontal AOE cone
    ImmuneResponseVisualBig = 36380, // Boss->self, 5.0s cast, single-target
    ImmuneResponseBig = 36381, // Helper->self, 6.0s cast, range 40 240-degree cone, side and back AOE cone

    PathocrossPurge = 36383, // InterferonC->self, 1.0s cast, range 40 width 6 cross
    PathocircuitPurge = 36382, // InterferonR->self, 1.0s cast, range 4-40 donut

    QuarantineVisual = 36384, // Boss->self, 3.0s cast, single-target
    Quarantine = 36386, // Helper->players, no cast, range 6 circle, stack
    Disinfection = 36385, // Helper->player, no cast, range 6 circle, tankbuster cleave

    Cytolysis = 36387 // Boss->self, 5.0s cast, range 40 circle
}

public enum IconID : uint
{
    Disinfection = 344, // player
    Quarantine = 62 // player
}

sealed class ImmuneResponseArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom rect = new([new Rectangle(D061AntivirusX.ArenaCenter, 23f, 18f)], [new Rectangle(D061AntivirusX.ArenaCenter, 20f, 15f)]);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ImmuneResponseVisualSmall && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(rect, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x03 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsRect(20f, 15f);
            _aoe = [];
        }
    }
}

sealed class PathoCircuitCrossPurge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(4f, 40f);
    private static readonly AOEShapeCross cross = new(40f, 3f);
    private static readonly AOEShapeCone coneSmall = new(40f, 60f.Degrees());
    private static readonly AOEShapeCone coneBig = new(40f, 120f.Degrees());
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                ref readonly var aoe0 = ref aoes[0];
                var isDonuts = aoe0.Shape == donut && aoe.Shape == donut;
                var isConeWithDelay = (aoe.Shape == coneBig || aoe.Shape == coneSmall) && (aoe.Activation - aoe0.Activation).TotalSeconds > 2d;
                var isCross = aoe0.Shape == cross;
                var isFrontDonutAndConeSmall = aoe.Origin == Arena.Center && aoe.Shape == donut && aoe0.Shape == coneSmall;
                var isRisky = !isDonuts && !isConeWithDelay && !isFrontDonutAndConeSmall || isCross;
                aoe.Risky = isRisky;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ImmuneResponseBig or (uint)AID.ImmuneResponseSmall)
        {
            var coneType = spell.Action.ID == (int)AID.ImmuneResponseBig ? coneBig : coneSmall;
            AddAOE(new(coneType, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.InterferonR or (uint)OID.InterferonC)
        {
            AOEShape shape = actor.OID == (int)OID.InterferonR ? donut : cross;
            var activationTime = _aoes.Count == 0 ? WorldState.FutureTime(9.9d) : _aoes.Ref(0).Activation.AddSeconds(2.5d * _aoes.Count);
            AddAOE(new(shape, actor.Position, default, activationTime)); // intentionally not using quantized values here
        }
    }

    private void AddAOE(AOEInstance aoe)
    {
        _aoes.Add(aoe);
        _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.PathocrossPurge:
                case (uint)AID.PathocircuitPurge:
                case (uint)AID.ImmuneResponseBig:
                case (uint)AID.ImmuneResponseSmall:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}

sealed class Cytolysis(BossModule module) : Components.RaidwideCast(module, (uint)AID.Cytolysis);

sealed class Quarantine(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Quarantine, (uint)AID.Quarantine, 6f, 5.1d, 3, 3);

sealed class Disinfection(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Disinfection, (uint)AID.Disinfection, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly Quarantine _stack = module.FindComponent<Quarantine>()!;
    private BitMask mask;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count != 0)
        {
            ref var b = ref CurrentBaits.Ref(0);
            if (b.Activation.AddSeconds(-2d) >= WorldState.CurrentTime)
            {
                if (b.Target == actor)
                {
                    hints.GoalZones.Add(hints.GoalSingleTarget(new WPos(852f, 808f), 3f, 1f));
                }
                else
                { }
            }
            else
            {
                base.AddAIHints(slot, actor, assignment, hints);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID && Raid.FindSlot(targetID) is var slot)
        {
            if (_stack.Stacks.Count != 0)
            {
                _stack.Stacks.Ref(0).ForbiddenPlayers.Set(slot);
            }
            else
            {
                mask.Set(slot);
            }
        }
        else if (iconID == (uint)IconID.Quarantine && mask != default)
        {
            if (_stack.Stacks.Count != 0)
            {
                _stack.Stacks.Ref(0).ForbiddenPlayers = mask;
                mask = default;
            }
        }
        base.OnEventIcon(actor, iconID, targetID);
    }
}

sealed class D061AntivirusXStates : StateMachineBuilder
{
    public D061AntivirusXStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ImmuneResponseArenaChange>()
            .ActivateOnEnter<PathoCircuitCrossPurge>()
            .ActivateOnEnter<Quarantine>()
            .ActivateOnEnter<Disinfection>()
            .ActivateOnEnter<Cytolysis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12844)]
public sealed class D061AntivirusX(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(22.5f, 17.5f))
{
    public static readonly WPos ArenaCenter = new(852f, 823f);
}
