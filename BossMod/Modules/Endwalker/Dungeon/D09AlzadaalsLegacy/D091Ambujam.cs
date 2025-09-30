namespace BossMod.Endwalker.Dungeon.D09AlzadaalsLegacy.D091Ambujam;

public enum OID : uint
{
    Boss = 0x3879, // R=9.0
    CyanTentacle = 0x387B, // R2.400, x1
    ScarletTentacle = 0x387A, // R2.400, x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BigWave = 28512, // Boss->self, 5.0s cast, range 40 circle

    CorrosiveFountain = 29556, // Helper->self, 7.0s cast, range 8 circle, knockback 10, away from source
    ToxicFountainVisual = 29466, // Boss->self, 4.0s cast, single-target
    ToxicFountain = 29467, // Helper->self, 7.0s cast, range 8 circle

    TentacleDig1 = 28501, // Boss->self, 3.0s cast, single-target
    TentacleDig2 = 28505, // Boss->self, 3.0s cast, single-target

    CorrosiveVenomVisual = 29157, // CyanTentacle->self, no cast, single-target
    CorrosiveVenom = 29158, // Helper->self, 2.5s cast, range 21 circle, knockback 10, away from source
    ToxinShowerVisual = 28507, // ScarletTentacle->self, no cast, single-target
    ToxinShower = 28508, // Helper->self, 2.5s cast, range 21 circle

    ModelStateChange1 = 28502, // Boss->self, no cast, single-target
    ModelStateChange2 = 28506 // Boss->self, no cast, single-target
}

sealed class ToxinShowerCorrosiveVenom(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(21f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnMapEffect(byte index, uint state)
    {
        WPos? pos = (index, state) switch
        {
            (0x11, 0x00080004u) => new(117f, -97f),
            (0x11, 0x00800004u) => new(131f, -83f),
            (0x11, 0x00200004u) => new(131f, -97f),
            (0x11, 0x02000004u) => new(117f, -83f),

            (0x10, 0x00200004u) => new(109f, -90f),
            (0x10, 0x02000004u) => new(139f, -90f),
            (0x10, 0x00080004u) => new(124f, -75f),
            (0x10, 0x00800004u) => new(124f, -105f),
            _ => null
        };

        if (pos is WPos p)
        {
            _aoes.Add(new(circle, p.Quantized(), default, WorldState.FutureTime(10.5d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.CorrosiveVenom or (uint)AID.ToxinShower)
        {
            _aoes.RemoveAt(0);
        }
    }
}
sealed class ToxicCorrosiveFountain(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ToxicFountain, (uint)AID.CorrosiveFountain], 8f, 10, 13);

sealed class BigWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.BigWave);

sealed class D091AmbujamStates : StateMachineBuilder
{
    public D091AmbujamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ToxicCorrosiveFountain>()
            .ActivateOnEnter<ToxinShowerCorrosiveVenom>()
            .ActivateOnEnter<BigWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11241)]
public sealed class D091Ambujam(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly WPos[] vertices = [new(130.52f, -108.34f), new(134.72f, -106.35f), new(135.26f, -105.95f), new(139.06f, -102.49f), new(142.38f, -97.38f),
    new(143.58f, -89.97f), new(142.79f, -84.64f), new(142.60f, -84.01f), new(140.33f, -79.22f), new(136.35f, -74.84f),
    new(129.83f, -71.42f), new(118.02f, -71.42f), new(111.47f, -74.97f), new(107.83f, -78.98f), new(105.23f, -84.47f),
    new(104.52f, -90.06f), new(105.90f, -97.34f), new(106.21f, -97.99f), new(109.00f, -102.54f), new(112.96f, -106.12f),
    new(113.55f, -106.48f), new(117.17f, -108.19f), new(130.52f, -108.34f)];
    private static readonly ArenaBoundsCustom DefaultBounds = new([new PolygonCustom(vertices)]);
}
