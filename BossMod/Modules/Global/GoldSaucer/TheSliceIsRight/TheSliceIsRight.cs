namespace BossMod.Global.GoldSaucer.SliceIsRight;

public enum OID : uint
{
    Boss = 0x25AB, //R=1.8
    Daigoro = 0x25AC, //R=2.5
    Bamboo = 0x25AD, //R=0.5
    Helper = 0x1EAEDF,
    VisualOutsideArena = 0x2BC8,
    HelperCupPhase1 = 0x1EAEB7,
    HelperCupPhase2 = 0x1EAEB6,
    HelperCupPhase3 = 0x1EAE9D,
    HelperSingleRect = 0x1EAE99,
    HelperDoubleRect = 0x1EAE9A,
    HelperCircle = 0x1EAE9B,
    Pileofgold = 0x1EAE9C
}

public enum AID : uint
{
    YojimboVisual1 = 19070, // Boss->self, no cast, single-target
    YojimboVisual2 = 18331, // Boss->self, no cast, single-tat
    YojimboVisual3 = 18329, // Boss->self, no cast, single-target
    YojimboVisual4 = 18332, // Boss->self, no cast, single-target
    YojimboVisual5 = 18328, // Boss->location, no cast, single-target
    YojimboVisual6 = 18326, // Boss->self, 3.0s cast, single-target
    YojimboVisual7 = 18339, // Boss->self, 3.0s cast, single-target
    YojimboVisual8 = 18340, // Boss->self, no cast, single-target
    YojimboVisual9 = 19026, // Boss->self, no cast, single-target
    VisualOutsideArena = 18338, // VisualOutsideArena->self, no cast, single-target

    BambooSplit = 18333, // Bamboo->self, 0.7s cast, range 28 width 5 rect
    BambooCircleFall = 18334, // Bamboo->self, 0.7s cast, range 11 circle 
    BambooSpawn = 18327, // Bamboo->self, no cast, range 3 circle
    FirstGilJump = 18335, // Daigoro->location, 2.5s cast, width 7 rect charge
    NextGilJump = 18336, // Daigoro->location, 1.5s cast, width 7 rect charge
    BadCup = 18337 // Daigoro->self, 1.0s cast, range 15+R 120-degree cone
}

sealed class BambooSplits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rect = new(28f, 2.5f);
    private static readonly AOEShapeCircle circle = new(11f);
    private static readonly AOEShapeCircle bamboospawn = new(3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.HelperCircle or (uint)OID.HelperDoubleRect or (uint)OID.HelperSingleRect)
        {
            _aoes.Add(new(bamboospawn, actor.Position.Quantized(), default, WorldState.FutureTime(2.7d)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002)
        {
            void AddAOE(AOEShape shape, Angle offset) => _aoes.Add(new(shape, actor.Position.Quantized(), actor.Rotation + offset, WorldState.FutureTime(7d)));
            switch (actor.OID)
            {
                case (uint)OID.HelperCircle:
                    AddAOE(circle, default);
                    break;
                case (uint)OID.HelperSingleRect:
                    AddAOE(rect, 90f.Degrees());
                    break;
                case (uint)OID.HelperDoubleRect:
                    AddAOE(rect, 90f.Degrees());
                    AddAOE(rect, -90f.Degrees());
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            void RemoveAOE(AOEShape shape)
            {
                var pos = caster.Position.Quantized();
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Origin == pos && aoe.Shape == shape)
                    {
                        _aoes.RemoveAt(i);
                        return;
                    }
                }
            }
            switch (spell.Action.ID)
            {
                case (uint)AID.BambooSpawn:
                    RemoveAOE(bamboospawn);
                    break;
                case (uint)AID.BambooSplit:
                    RemoveAOE(rect);
                    break;
                case (uint)AID.BambooCircleFall:
                    RemoveAOE(circle);
                    break;
            }
        }
    }
}

sealed class DaigoroGilJump(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.FirstGilJump, (uint)AID.NextGilJump], 3.5f, extraLengthFront: 5f);

sealed class TheSliceIsRightStates : StateMachineBuilder
{
    public TheSliceIsRightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BambooSplits>()
            .ActivateOnEnter<DaigoroGilJump>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || !module.Arena.InBounds(module.Raid.Player()!.Position);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.GoldSaucer, GroupID = 181, NameID = 9066)]
public sealed class TheSliceIsRight(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(70.5f, -36f), 15f * CosPI.Pi28th, 28)]);
    protected override bool CheckPull() => Arena.InBounds(Raid.Player()!.Position); // only activate module if player is taking part in the event
}
