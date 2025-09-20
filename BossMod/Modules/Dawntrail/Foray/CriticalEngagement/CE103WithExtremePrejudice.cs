namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE103WithExtremePrejudice;

public enum OID : uint
{
    Boss = 0x46E1, // R4.92
    VassalVessel = 0x46E2, // R2.75
    CommandUrn = 0x4739, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 37821, // Boss->player, no cast, single-target
    AutoAttackAdd = 43116, // VassalVessel->player, no cast, single-target

    Summon = 41416, // Boss->self, 3.0s cast, single-target
    Assail = 41417, // Boss->self, 3.0s cast, single-target
    StoneSwell1 = 41420, // VassalVessel->self, 1.0s cast, range 16 circle
    StoneSwell2 = 39470, // VassalVessel->self, 1.0s cast, range 16 circle
    Rockslide1 = 41421, // VassalVessel->self, 1.0s cast, range 40 width 10 cross
    Rockslide2 = 39471, // VassalVessel->self, 1.0s cast, range 40 width 10 cross
    AethericBurstVisual = 41424, // Boss->self, 5.0s cast, single-target, raidwide
    AethericBurst = 41425, // Helper->self, 5.0s cast, ???
    Destruct = 41422, // Boss->self, 3.0s cast, single-target
    SelfDestructVisual = 41423, // VassalVessel->self, no cast, single-target
    SelfDestruct = 39094 // Helper->self, no cast, ???
}

public enum TetherID : uint
{
    StoneSwell1 = 303, // CommandUrn/CrescentGolem3->CrescentGolem3
    Rockslide = 304, // CommandUrn/CrescentGolem3->CrescentGolem3
    StoneSwell2 = 306 // CrescentGolem3->CommandUrn
}

sealed class AethericBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.AethericBurst);

sealed class RockSlideStoneSwell(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeCircle circle = new(16f);
    private static readonly AOEShapeCross cross = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        var deadline1 = aoe0.Activation.AddSeconds(4.8d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline1)
            {
                break;
            }
            ++index;
        }
        if (index > 1 && aoes[index - 1].Activation > aoe0.Activation.AddSeconds(1d))
        {
            aoe0.Color = Colors.Danger;
        }
        return aoes[..index];
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var id = tether.ID;
        AOEShape? shape = id switch
        {
            (uint)TetherID.StoneSwell1 => circle,
            (uint)TetherID.Rockslide => cross,
            _ => null
        };
        if (shape != null)
        {
            var tid = tether.Target;
            if (WorldState.Actors.Find(tid) is Actor t)
            {
                AddAOE(shape, t.Position, t.Rotation, tid);
            }
            return;
        }
        if (id == (uint)TetherID.StoneSwell2)
        {
            AddAOE(circle, source.Position, default, source.InstanceID);
        }
        void AddAOE(AOEShape shape, WPos position, Angle rotation, ulong cid) => _aoes.Add(new(shape, position.Quantized(), rotation, WorldState.FutureTime(6.1d), actorID: cid));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.StoneSwell1 or (uint)AID.StoneSwell2 or (uint)AID.Rockslide1 or (uint)AID.Rockslide2)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class CE103WithExtremePrejudiceStates : StateMachineBuilder
{
    public CE103WithExtremePrejudiceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AethericBurst>()
            .ActivateOnEnter<RockSlideStoneSwell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 43)]
public sealed class CE103WithExtremePrejudice(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-352f, -608f), 19.5f, 32)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.VassalVessel));
        Arena.Actor(PrimaryActor);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VassalVessel => 1,
                _ => 0
            };
        }
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
