namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D082JackInThePot;

public enum OID : uint
{
    Boss = 0x41CA, // R4.16
    SpectralSamovar = 0x41CB, // R2.88
    StrayPhantagenitrix = 0x41D2, // R2.1
    TeacupHelper = 0x41D5, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 37169, // Boss->player, no cast, single-target

    TroublingTeacups = 36716, // Boss->self, 3.0s cast, single-target, Spawns teacups
    TeaAwhirl = 36717, // Boss->self, 6.0s cast, single-target, ghost(s) tether teacup and enters, teacups spin then possesed teacup explodes in AOE
    TricksomeTreat = 36720, // StrayPhantagenitrix->self, 3.0s cast, range 19 circle, TeaAwhirl AOE

    ToilingTeapots = 36722, // Boss->self, 3.0s cast, single-target, spawns 13 teacups

    Puppet = 36721, // StrayPhantagenitrix->location, 4.0s cast, single-target
    PipingPour = 36723, // SpectralSamovar->location, 2.0s cast, single-target, spreading AOE

    MadTeaParty = 36724, // Helper->self, no cast, range 0 circle, DOT applied to players in puddles

    LastDrop = 36726, // Boss->player, 5.0s cast, single-target, tankbuster

    SordidSteam = 36725 // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum TetherID : uint
{
    CupTether = 276 // UnknownActor->StrayPhantagenitrix
}

public enum SID : uint
{
    AreaOfInfluenceUp = 1909 // none->Helper, extra=0x1/0x2/0x3/0x4
}

sealed class PipingPour(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DD && _aoes.Count != 0 && actor.OID == (uint)OID.SpectralSamovar)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AreaOfInfluenceUp && status.Extra == 0x1)
            _aoes.Add(new(circle, actor.Position.Quantized()));
    }
}

sealed class TeaAwhirl : Components.GenericAOEs
{
    private static readonly AOEShapeCircle circle = new(19f);
    private readonly List<Actor> _cups = new(2);
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly Dictionary<uint, Action> cupPositions;

    public TeaAwhirl(BossModule module) : base(module)
    {
        cupPositions = new Dictionary<uint, Action>
        {
            { 0x02000100u, () => HandleActivation(11.5d,
                [
                    (new(17f, -163), new(17f, -177f), [new(3.5f, -161.5f), new(30.5f, -178.5f)]),
                    (new(17f, -153), new(10f, -170f), [new(25.5f, -156.5f), new(20.5f, -178.5f)]),
                    (new(17f, -153), new(17f, -177f), [new(20.5f, -178.5f), new(3.5f, -161.5f)]),
                    (new(34f, -170), null, [new(8.5f, -173.5f)]),
                    (new(default, -170), null, [new(25.5f, -166.5f)])
                ])
            },
            { 0x10000800u, () => HandleActivation(14.5d,
                [
                    (new(default, -170f), new(34f, -170f), [new(8.5f, -156.5f), new(25.5f, -183.5f)]),
                    (new(default, -170f), new(17f, -187f), [new(3.5f, -178.5f), new(8.5f, -156.5f)]),
                    (new(17f, -187f), new(17f, -153f), [new(30.5f, -161.5f), new(3.5f, -178.5f)])
                ])
            },
            { 0x00100001u, () => AddAOEs(WorldState.FutureTime(16d), [_cups[0].Position, _cups[1].Position]) },
            { 0x00400020u, () => HandleActivation(19f,
                [
                    (new(default, -170f), new(17f, -163f), [new(5f, -165f), new(22f, -182f)]),
                    (new(17f, -177f), new(17f, -153f), [new(5f, -175f), new(29f, -175f)])
                ])
            }
        };
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.CupTether)
            _cups.Add(source);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is 0x23 or 0x01)
        {
            if (cupPositions.TryGetValue(state, out var action))
                action.Invoke();
        }
    }

    private void HandleActivation(double futureTime, List<(WPos pos1, WPos? pos2, WPos[] positions)> cups)
    {
        var activation = WorldState.FutureTime(futureTime);
        foreach (var (pos1, pos2, positions) in cups)
            if (CheckPositions(pos1, pos2))
            {
                AddAOEs(activation, positions);
                return;
            }
    }

    private bool CheckPositions(WPos pos1, WPos? pos2) => pos2 != null ? _cups.Any(x => x.Position == pos1) && _cups.Any(x => x.Position == pos2) : _cups.Any(x => x.Position == pos1);

    private void AddAOEs(DateTime activation, WPos[] positions)
    {
        var len = positions.Length;
        for (var i = 0; i < len; ++i)
            _aoes.Add(new(circle, positions[i], default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TricksomeTreat)
        {
            _aoes.Clear();
            _cups.Clear();
        }
    }
}

sealed class SordidSteam(BossModule module) : Components.RaidwideCast(module, (uint)AID.SordidSteam);
sealed class LastDrop(BossModule module) : Components.SingleTargetCast(module, (uint)AID.LastDrop);

class D082JackInThePotStates : StateMachineBuilder
{
    public D082JackInThePotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TeaAwhirl>()
            .ActivateOnEnter<PipingPour>()
            .ActivateOnEnter<SordidSteam>()
            .ActivateOnEnter<LastDrop>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12760)]
public sealed class D082JackInThePot(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(17f, -170f), 19.5f)], [new Rectangle(new(17f, -150.15f), 20, 1.25f), new Rectangle(new(17f, -189.5f), 20f, 1.25f)]);
}
