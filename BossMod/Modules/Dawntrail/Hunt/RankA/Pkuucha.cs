namespace BossMod.Dawntrail.Hunt.RankA.Pkuucha;

public enum OID : uint
{
    Boss = 0x4580 // R4.34
}

public enum AID : uint
{
    AutoAttack = 872, //  Boss->player, no cast, single-target

    MesmerizingMarch1 = 39863, //  Boss->self, 4.0s cast, range 12 circle
    MesmerizingMarch2 = 39755, //  Boss->self, 1.5s cast, range 12 circle
    StirringSamba1 = 39864, //  Boss->self, 4.0s cast, range 40 90-degree cone
    StirringSamba2 = 39756, //  Boss->self, 1.0s cast, range 40 90-degree cone
    GlidingSwoop = 39757, //  Boss->self, 3.5s cast, range 18 width 16 rect
    MarchingSamba = 39797, //  Boss->self, 5.0s cast, single-target
    PeckingFlurryFirst = 39760, //  Boss->self, 5.0s cast, range 40 circle
    PeckingFlurryRest = 39761, // Boss->self, no cast, range 40 circle
    DeadlySwoop = 39799 // Boss->player, no cast, single-target, deadly ability if caught in samba mechanic
}

sealed class GlidingSwoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlidingSwoop, new AOEShapeRect(18f, 8f));
sealed class PeckingFlurry(BossModule module) : Components.RaidwideCast(module, (uint)AID.PeckingFlurryFirst, "Raidwide (3x)");

sealed class MesmerizingMarchStirringSamba(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle circle = new(12f);
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, float delay = default) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, delay)));
        switch (spell.Action.ID)
        {
            case (uint)AID.MesmerizingMarch1:
                AddAOE(circle);
                break;
            case (uint)AID.StirringSamba1:
                AddAOE(cone);
                break;
            case (uint)AID.MarchingSamba:
                AddAOE(circle, 1.7f);
                AddAOE(cone, 5.7f);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.MesmerizingMarch1 or (uint)AID.MesmerizingMarch2 or (uint)AID.StirringSamba1 or (uint)AID.StirringSamba2)
            _aoes.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // stay close to the middle
        hints.AddForbiddenZone(new SDInvertedCircle(Module.PrimaryActor.Position, 14f));
    }
}

sealed class PkuuchaStates : StateMachineBuilder
{
    public PkuuchaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GlidingSwoop>()
            .ActivateOnEnter<MesmerizingMarchStirringSamba>()
            .ActivateOnEnter<PeckingFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13443)]
public sealed class Pkuucha(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
