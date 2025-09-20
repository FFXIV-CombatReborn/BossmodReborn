namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE110FlameOfDusk;

public enum OID : uint
{
    Boss = 0x46D1, // R4.0
    HinkypunkClone = 0x46D3, // R4.0
    AvianHusk = 0x46D2, // R4.0
    Deathwall = 0x1EBD55, // R0.5
    DeathwallHelper = 0x4863, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Deathwall = 41382, // DeathwallHelper->self, no cast, range 20n-25 donut

    LamplightVisual = 41381, // Boss->self, 5.0s cast, single-target, raidwide
    Lamplight = 41744, // Helper->self, 5.8s cast, ???
    DreadDive = 41380, // Boss->player, 5.0s cast, single-target, tankbuster

    BlowoutTelegraph = 41379, // Helper->self, 2.5s cast, range 50 circle
    ShadesNestTelegraph = 41374, // Helper->self, 2.5s cast, range 7-50 donut
    ShadesCrossingTelegraph = 41377, // Helper->self, 2.5s cast, range 50 width 15 cross
    BlowoutVisual = 41378, // Boss/HinkypunkClone->self, 2.0s cast, single-target
    Blowout = 41397, // Helper->self, 2.8s cast, ???, knockback 20, away from source
    ShadesNestVisual = 41373, // Boss/HinkypunkClone->self, 2.0s cast, single-target
    ShadesNestBossVisual = 41372, // Boss->self, 5.0+0,8s cast, single-target
    ShadesNestBoss = 42032, // Helper->self, 5.8s cast, range 7-50 donut
    ShadesNestHusk = 42033, // Helper->self, 2.8s cast, range 7-50 donut
    ShadesCrossingVisual = 41376, // Boss/HinkypunkClone->self, 2.0s cast, single-target
    ShadesCrossingBossVisual = 41375, // Boss->self, 5.0+0,8s cast, single-target
    ShadesCrossingBoss = 42034, // Helper->self, 5.8s cast, range 50 width 15 cross
    ShadesCrossingHusk = 42035, // Helper->self, 2.8s cast, range 50 width 15 cross

    Molt1 = 41368, // Boss->self, 13.0s cast, single-target
    Molt2 = 41369, // HinkypunkClone->self, 13.0s cast, single-target

    FlockOfSouls = 41370, // Boss->self, 5.0s cast, single-target
    BossDeath = 41396, // Boss->self, no cast, single-target
    ActivateHusk = 41371 // Helper->AvianHusk/Boss, no cast, single-target
}

sealed class DreadDive(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.DreadDive);
sealed class Lamplight(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.LamplightVisual, (uint)AID.Lamplight, 0.9f);

sealed class MoltAOEs(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private readonly List<(WPos position, AOEShape? shape, bool knockback, DateTime activation)> pendingMechanics = new(4);
    private static readonly AOEShapeDonut donut = new(7f, 50f);
    private static readonly AOEShapeCross cross = new(50f, 7.5f);
    private bool first = true;
    private readonly MoltKB _kb = module.FindComponent<MoltKB>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? CollectionsMarshal.AsSpan(AOEs)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (AOEShape?, bool?) shape = spell.Action.ID switch
        {
            (uint)AID.ShadesCrossingTelegraph => (cross, false),
            (uint)AID.ShadesNestTelegraph => (donut, false),
            (uint)AID.BlowoutTelegraph => (null, true),
            (uint)AID.ShadesCrossingBoss => (cross, null),
            (uint)AID.ShadesNestBoss => (donut, null),
            _ => default
        };
        if (shape != default)
        {
            if (shape.Item2 != null)
            {
                var delay = (pendingMechanics.Count, first) switch
                {
                    (0, true) => 16.1f,
                    (1, true) => 18.4f,
                    (2, true) => 20.8f,
                    (3, true) => 23.2f,
                    (0, false) => 14.9f,
                    (1, false) => 15.3f,
                    (2, false) => 22.1f,
                    (3, false) => 22.5f,
                    _ => default
                };
                pendingMechanics.Add((caster.Position, shape.Item1, shape.Item2.Value, Module.CastFinishAt(spell, delay)));
                if (pendingMechanics.Count == 4)
                {
                    first = false;
                }
            }
            else
            {
                AOEs.Add(new(shape.Item1!, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ActivateHusk && pendingMechanics.Count != 0)
        {
            var pos = WorldState.Actors.Find(spell.MainTargetID)?.Position;
            if (pos is WPos position)
            {
                (WPos position, AOEShape? shape, bool knockback, DateTime activation) mech = default;
                var count = pendingMechanics.Count;
                var mechs = CollectionsMarshal.AsSpan(pendingMechanics);
                var cpos = caster.Position;
                for (var i = 0; i < count; ++i) // update moved actor positions
                {
                    ref var mch = ref mechs[i];
                    if (mch.position.AlmostEqual(cpos, 1f))
                    {
                        mch.position = position;
                        if (mech == default) // get first mech for matching actor position
                        {
                            mech = mch;
                        }
                    }
                }
                if (!mech.knockback)
                {
                    var husks = Module.Enemies((uint)OID.AvianHusk);
                    var countH = husks.Count;

                    for (var i = 0; i < countH; ++i)
                    {
                        var husk = husks[i];
                        if (husk.Position.AlmostEqual(position, 1f))
                        {
                            AOEs.Add(new(mech.shape!, position.Quantized(), husk.Rotation, mech.activation));
                            if (AOEs.Count == 2)
                            {
                                var aoes = CollectionsMarshal.AsSpan(AOEs);
                                ref var aoe1 = ref aoes[0];
                                ref var aoe2 = ref aoes[1];
                                if (aoe1.Activation > aoe2.Activation)
                                {
                                    (aoe1, aoe2) = (aoe2, aoe1);
                                }
                            }
                            RemovePendingMechanic(position);
                            return;
                        }
                    }
                }
                else
                {
                    _kb.AddKnockback(position.Quantized(), mech.activation);
                    RemovePendingMechanic(position);
                }
                void RemovePendingMechanic(WPos position)
                {
                    var count = pendingMechanics.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        if (pendingMechanics[i].position.AlmostEqual(position, 1f))
                        {
                            pendingMechanics.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.ShadesCrossingHusk or (uint)AID.ShadesNestHusk or (uint)AID.ShadesCrossingBoss or (uint)AID.ShadesNestBoss)
        {
            AOEs.RemoveAt(0);
        }
    }
}

sealed class MoltKB(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    private MoltAOEs? _aoe;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public void AddKnockback(WPos position, DateTime activation) => _kb = [new(position, 20f, activation)];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Blowout)
        {
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                // circle intentionally slightly smaller to prevent sus knockback
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, kb.Origin, 20f, 18f), act);
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_kb.Length != 0)
        {
            _aoe ??= Module.FindComponent<MoltAOEs>();
            if (_aoe!.AOEs.Count == 0)
            {
                return;
            }
            ref var aoe = ref _aoe.AOEs.Ref(0);
            ref var kb = ref _kb[0];
            if (aoe.Activation > kb.Activation)
            {
                hints.Add("Order: Knockback -> AOE");
            }
            else
            {
                hints.Add("Order: AOE -> Knockback");
            }
        }
    }
}

sealed class CE110FlameOfDuskStates : StateMachineBuilder
{
    public CE110FlameOfDuskStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MoltKB>()
            .ActivateOnEnter<MoltAOEs>()
            .ActivateOnEnter<DreadDive>()
            .ActivateOnEnter<Lamplight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 47)]
public sealed class CE110FlameOfDusk(WorldState ws, Actor primary) : BossModule(ws, primary, new WPos(-570f, -160f).Quantized(), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
