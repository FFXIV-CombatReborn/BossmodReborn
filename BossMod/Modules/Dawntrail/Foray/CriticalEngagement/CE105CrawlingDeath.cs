namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE105CrawlingDeath;

public enum OID : uint
{
    Boss = 0x46C4, // R3.6
    Deathwall = 0x483F, // R0.5
    PhantomClaw = 0x46C5, // R2.925
    Clawmarks1 = 0x46C6, // R1.0-1.7
    Clawmarks2 = 0x46C7, // R1.0
    Clawmarks3 = 0x46C8, // R1.0
    ClawsOnFloor = 0x1EBCF0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Deathwall = 41308, // Deathwall->self, no cast, ???

    DirtyNails = 41332, // Boss->player, 5.0s cast, single-target, tankbuster
    Visual = 41312, // PhantomClaw->self, no cast, single-target
    Clawmarks = 41309, // Boss->self, 5.0s cast, single-target
    ThreefoldMarks = 41310, // Boss->self, 5.0s cast, single-target
    ManifoldMarks = 41311, // Boss->self, 5.0s cast, single-target
    LethalNails1 = 41315, // Clawmarks2/Clawmarks3/Clawmarks1->self, 2.0s cast, range 60 width 7 rect
    LethalNails2 = 41316, // Clawmarks1/Clawmarks2->self, 4.0s cast, range 60 width 7 rect
    LethalNails3 = 41317, // Clawmarks1->self, 6.0s cast, range 60 width 7 rect
    HorizontalCrosshatch1 = 41324, // Boss->self, 5.0s cast, single-target
    HorizontalCrosshatch2 = 41331, // Boss->self, 7.5s cast, single-target
    VerticalCrosshatch1 = 41323, // Boss->self, 5.0s cast, single-target
    VerticalCrosshatch2 = 41330, // Boss->self, 7.5s cast, single-target
    RakingScratch = 41325, // Helper->self, no cast, range 50 90-degree cone
    SkulkingOrders1 = 41326, // Boss->self, 7.0s cast, single-target
    SkulkingOrders2 = 41329, // Boss->self, 7.0s cast, single-target
    ClawingShadowVisual = 41328, // Helper->self, 1.0s cast, range 50 90-degree cone
    ClawingShadow = 41327, // PhantomClaw->self, no cast, range 50 90-degree cone
    TheGripOfPoisonVisual = 41333, // Boss->self, 4.0s cast, single-target
    TheGripOfPoison = 41334 // Helper->self, no cast, ???
}

sealed class TheGripOfPoison(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.TheGripOfPoisonVisual, (uint)AID.TheGripOfPoison, 0.9f, "Raidwide + bleed");
sealed class DirtyNails(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DirtyNails);

sealed class Clawmarks(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(16);
    private static readonly AOEShapeRect rect = new(60f, 3.5f);
    private uint lastOID;
    private int currentWave;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        var id = actor.OID;
        if (id is (uint)OID.Clawmarks1 or (uint)OID.Clawmarks2 or (uint)OID.Clawmarks3)
        {
            if (lastOID != id)
            {
                if (lastOID == default)
                {
                    lastOID = id;
                }
                else
                {
                    lastOID = id;
                    ++currentWave;
                }
            }
            var activation = currentWave switch
            {
                0 => 6.7d,
                1 => 8.7d,
                2 => 10.8d,
                _ => default
            };
            if (activation != default)
            {
                AOEs.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(activation), actorID: actor.InstanceID));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Clawmarks or (uint)AID.ThreefoldMarks or (uint)AID.ManifoldMarks)
        {
            lastOID = default;
            currentWave = default;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.LethalNails1 or (uint)AID.LethalNails2 or (uint)AID.LethalNails3)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class ClawingShadow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    public static readonly AOEShapeCone Cone = new(50f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 1 ? CollectionsMarshal.AsSpan(_aoes)[..2] : CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020u && actor.OID == (uint)OID.ClawsOnFloor)
        {
            _aoes.Add(new(Cone, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(_aoes.Count < 3 ? 8.1d : 10.6d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ClawingShadow)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class Crosshatch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly Clawmarks _aoe = module.FindComponent<Clawmarks>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _aoe.AOEs.Count != 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < 2; ++i)
        {
            aoes[i].Risky = true;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.HorizontalCrosshatch1 or (uint)AID.HorizontalCrosshatch2 or (uint)AID.VerticalCrosshatch1 or (uint)AID.VerticalCrosshatch2)
        {
            Angle[] angles = [-90.004f.Degrees(), 89.999f.Degrees(), -0.003f.Degrees(), 180f.Degrees()];
            if (id is (uint)AID.VerticalCrosshatch1 or (uint)AID.VerticalCrosshatch2)
            {
                angles.Reverse();
            }
            for (var i = 0; i < 4; ++i)
            {
                _aoes.Add(new(ClawingShadow.Cone, spell.LocXZ, angles[i], Module.CastFinishAt(spell, i < 2 ? default : 2f), i < 2 ? Colors.Danger : default, i < 2));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.RakingScratch)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
        if (count > 2)
        {
            var aoe = _aoes.Ref(0);
            // stay close to the middle if there is more than one aoe left
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 5f), aoe.Activation);
        }
    }
}

sealed class CE105CrawlingDeathStates : StateMachineBuilder
{
    public CE105CrawlingDeathStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Clawmarks>()
            .ActivateOnEnter<ClawingShadow>()
            .ActivateOnEnter<Crosshatch>()
            .ActivateOnEnter<TheGripOfPoison>()
            .ActivateOnEnter<DirtyNails>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 36)]
public sealed class CE105CrawlingDeath(WorldState ws, Actor primary) : BossModule(ws, primary, new(681, 534f), new ArenaBoundsSquare(21f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InSquare(Arena.Center, 21f);
}
