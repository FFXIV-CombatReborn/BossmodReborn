namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D122Arkas;

public enum OID : uint
{
    Boss = 0x3EEA, //R=5.1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BattleCry1 = 34605, // Boss->self, 5.0s cast, range 40 circle
    BattleCry2 = 33364, // Boss->self, 5.0s cast, range 40 circle
    ElectricEruption = 33615, // Boss->self, 5.0s cast, range 40 circle

    Electrify = 33367, // Helper->location, 5.5s cast, range 10 circle

    LightningClaw1 = 33366, // Boss->location, no cast, range 6 circle
    LightningClaw2 = 34712, // Boss->player, 5.0s cast, single-target 
    LightningLeapA = 33358, // Boss->location, 4.0s cast, single-target
    LightningLeapB = 33359, // Boss->location, 5.0s cast, single-target
    LightningLeap1 = 33360, // Helper->location, 6.0s cast, range 10 circle
    LightningLeap2 = 34713, // Helper->location, 5.0s cast, range 10 circle

    LightningRampageA = 34318, // Boss->location, 4.0s cast, single-target
    LightningRampageB = 34319, // Boss->location, 2.0s cast, single-target
    LightningRampageC = 34320, // Boss->location, 2.0s cast, single-target
    LightningRampage1 = 34321, // Helper->location, 5.0s cast, range 10 circle
    LightningRampage2 = 34714, // Helper->location, 5.0s cast, range 10 circle

    RipperClaw = 33368, // Boss->player, 5.0s cast, single-target
    Shock = 33365, // Helper->location, 3.5s cast, range 6 circle
    SpinningClaw = 33362, // Boss->self, 3.5s cast, range 10 circle
    ForkedFissures = 33361, // Helper->location, 1.0s cast, width 4 rect charge
    SpunLightning = 33363 // Helper->self, 3.5s cast, range 30 width 8 rect
}

public enum IconID : uint
{
    Stackmarker = 161 // player
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(10f, 15f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = D122Arkas.SmallerBounds;
                Arena.Center = D122Arkas.SmallerBounds.Center;
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = D122Arkas.DefaultBounds;
                Arena.Center = D122Arkas.DefaultBounds.Center;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BattleCry2)
        {
            _aoe = [new(donut, D122Arkas.ArenaCenter, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }
}

sealed class SpunLightning(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpunLightning, new AOEShapeRect(30f, 4f));
sealed class LightningClaw(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.LightningClaw2, 6, 5.2f, 4, 4);

sealed class ForkedFissures(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly (WPos[] Start, WPos[] End) pattern0x01 = (
        [
            new(430.357f, -445.557f), new(434.507f, -440.159f), new(430.357f, -445.557f), new(424.561f, -449.554f),
            new(425.887f, -446.107f), new(423.516f, -434.294f), new(430.346f, -445.616f), new(419.902f, -439.485f),
            new(430.357f, -445.557f), new(430.404f, -445.54f), new(429.973f, -432.501f), new(427.648f, -437.101f),
            new(430.357f, -445.557f), new(427.648f, -438.04f), new(424.713f, -449.483f), new(418.756f, -446.251f)
        ],
        [
            new(439.2f, -444.602f), new(435.416f, -429.313f), new(429.618f, -454.246f), new(423.24f, -454.887f),
            new(419.303f, -439.078f), new(417.441f, -427.054f), new(424.704f, -444.846f), new(410.757f, -435.294f),
            new(424.643f, -449.577f), new(427.451f, -437.247f), new(424.796f, -425.132f), new(423.148f, -433.951f),
            new(434.867f, -439.109f), new(431.693f, -426.627f), new(418.051f, -446.036f), new(411.063f, -445.579f)
        ]);
    private static readonly (WPos[] Start, WPos[] End) pattern0x02 = (
        [
            new(430.635f, -434.592f), new(430.708f, -434.484f), new(434.518f, -440.005f), new(424.457f, -445.105f),
            new(430.834f, -434.374f), new(431.156f, -439.05f), new(430.599f, -434.383f), new(434.571f, -440.454f),
            new(423.033f, -437.371f), new(422.069f, -437.329f), new(419.287f, -441.464f), new(430.513f, -434.548f),
            new(417.501f, -435.027f), new(431.252f, -446.301f), new(430.458f, -434.36f), new(425.28f, -430.49f)
        ],
        [
            new(439.139f, -435.325f), new(422.232f, -437.583f), new(431.083f, -446.983f), new(420.279f, -454.215f),
            new(429.831f, -440.299f), new(424.063f, -445.762f), new(434.379f, -440.269f), new(439.811f, -441.733f),
            new(411.612f, -433.28f), new(418.936f, -441.794f), new(412.07f, -447.532f), new(424.308f, -430.228f),
            new(410.025f, -440.269f), new(430.594f, -453.88f), new(429.587f, -425.834f), new(414.298f, -429.557f)
        ]);
    private static readonly (WPos[] Start, WPos[] End) pattern0x03 = (
        [
            new(419.343f, -434.343f), new(416.325f, -437.829f), new(419.304f, -434.353f), new(427.97f, -434.253f),
            new(430.244f, -447.772f), new(422.523f, -438.223f), new(427.408f, -441.363f), new(419.274f, -434.245f),
            new(422.582f, -432.152f), new(416.35f, -442.222f), new(423.09f, -445.755f), new(419.412f, -434.285f),
            new(419.294f, -434.309f), new(416.47f, -442.448f), new(430.633f, -433.69f), new(431.389f, -439.114f)
        ],
        [
            new(410.880f, -435.019f), new(416.312f, -442.557f), new(417.441f, -427.085f), new(437.949f, -432.547f),
            new(436.942f, -448.875f), new(428.031f, -442.039f), new(431.571f, -448.63f), new(430.9f, -426.261f),
            new(428.916f, -434.379f), new(411.032f, -445.457f), new(426.413f, -454.917f), new(422.934f, -438.59f),
            new(416.037f, -438.926f), new(423.941f, -446.738f), new(432.364f, -440.177f), new(439.475f, -443.809f)
        ]);
    private static readonly (WPos[] Start, WPos[] End) pattern0x04 = (
        [
            new(419.293f, -445.661f), new(422.919f, -448.675f), new(419.359f, -445.715f), new(419.333f, -437.25f),
            new(432.791f, -434.82f), new(423.239f, -442.489f), new(426.377f, -437.596f), new(419.335f, -445.663f),
            new(417.162f, -442.421f), new(427.274f, -448.618f), new(430.839f, -441.877f), new(419.292f, -445.596f),
            new(427.482f, -448.548f), new(419.101f, -434.242f), new(424.274f, -433.427f), new(419.326f, -445.681f)
        ],
        [
            new(420.035f, -454.124f), new(427.42f, -448.692f), new(412.039f, -447.562f), new(417.533f, -427.085f),
            new(433.860f, -427.97f), new(426.993f, -437.034f), new(433.646f, -433.433f), new(411.276f, -434.165f),
            new(419.394f, -436.118f), new(430.442f, -453.971f), new(439.872f, -438.59f), new(423.667f, -442.039f),
            new(431.815f, -441.032f), new(425.437f, -432.547f), new(428.824f, -425.528f), new(424.002f, -448.966f)
        ]);

    private readonly List<AOEInstance> _aoes = new(32);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 16 ? 16 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state is 0x00200010u or 0x00020001u)
        {
            var pattern = index switch
            {
                0x01 => pattern0x01,
                0x02 => pattern0x02,
                0x03 => pattern0x03,
                0x04 => pattern0x04,
                _ => default
            };
            if (pattern != default)
            {
                var starts = pattern.Start;
                var ends = pattern.End;
                for (var i = 15; i >= 0; --i)
                {
                    var start = starts[i];
                    var end = ends[i];
                    _aoes.Add(new(new AOEShapeRect((start - end).Length(), 2f), start, Angle.FromDirection(end - start), WorldState.FutureTime(6d)));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ForkedFissures)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class ElectrifyLightningLeapRampageSpinningClaw(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Electrify, (uint)AID.LightningLeap1,
(uint)AID.LightningLeap2, (uint)AID.LightningRampage1, (uint)AID.LightningRampage2, (uint)AID.SpinningClaw], 10f);

sealed class RipperClaw(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RipperClaw);
sealed class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 6f);
sealed class BattleCryElectricEruption(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.BattleCry1, (uint)AID.BattleCry2, (uint)AID.ElectricEruption]);

sealed class D122ArkasStates : StateMachineBuilder
{
    public D122ArkasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<LightningClaw>()
            .ActivateOnEnter<SpunLightning>()
            .ActivateOnEnter<ForkedFissures>()
            .ActivateOnEnter<ElectrifyLightningLeapRampageSpinningClaw>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<BattleCryElectricEruption>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 822, NameID = 12337, SortOrder = 6)]
public sealed class D122Arkas(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(425f, -440f);
    public static readonly ArenaBoundsCustom DefaultBounds = new([new Polygon(ArenaCenter, 14.5f, 48)], [new Rectangle(new(425f, -424f), 20f, 2.4f), new Rectangle(new(425f, -455f), 10f, 1.25f)]);
    public static readonly ArenaBoundsCustom SmallerBounds = new([new Polygon(ArenaCenter, 10f, 48)]);
}
