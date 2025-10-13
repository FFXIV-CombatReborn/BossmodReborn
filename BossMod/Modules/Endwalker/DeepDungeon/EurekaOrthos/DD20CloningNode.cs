namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD20CloningNode;

public enum OID : uint
{
    Boss = 0x3E77, // R6.0
    ClonedShieldDragon = 0x3E78, // R3.445
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 32817, // Boss->player, no cast, single-target
    Teleport = 32554, // ClonedShieldDragon->location, no cast, single-target

    OffensiveCommand = 32543, // Boss->self, 5.0s cast, single-target
    FlameBreathVisual1 = 32544, // ClonedShieldDragon->self, 2.0+1,5s cast, single-target
    FlameBreathVisual2 = 32553, // ClonedShieldDragon->self, no cast, single-target
    FlameBreathVisual3 = 32552, // ClonedShieldDragon->self, no cast, single-target
    FlameBreath = 32864, // Helper->self, 3.5s cast, range 50 30-degree cone
    PiercingLaser = 32547, // Boss->self, 2.5s cast, range 40 width 5 rect
    OrderRelay = 32545 // Boss->self, 8.0s cast, single-target
}

sealed class FlameBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeCone cone = new(50f, 15f.Degrees());
    private const float IntercardinalDistance = 22.627417f;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
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

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FlameBreathVisual3)
        {
            var activation = WorldState.FutureTime(9.6d);

            if ((caster.Position - Arena.Center).LengthSq() > 625f)
            {
                _aoes.Add(new(cone, CalculatePosition(caster).Quantized(), caster.Rotation, activation));
            }
            else
            {
                _aoes.Add(new(cone, RoundPosition(caster.Position).Quantized(), caster.Rotation, activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.FlameBreath)
        {
            _aoes.RemoveAt(0);
        }
    }

    private static WPos CalculatePosition(Actor caster)
    {
        var isIntercardinal = IsCasterIntercardinal(caster);
        var distance = isIntercardinal ? IntercardinalDistance : 22f;
        var position = caster.Position + distance * caster.Rotation.ToDirection();
        var pos = RoundPosition(position);
        // the top left add is slightly off for some reason
        return pos == new WPos(-315f, -315f) ? new(-315.5f, -315.5f) : pos;

        static bool IsCasterIntercardinal(Actor caster)
        {
            for (var i = 0; i < 4; ++i)
            {
                if (caster.Rotation.AlmostEqual(Angle.AnglesIntercardinals[i], Angle.DegToRad))
                {
                    return true;
                }
            }
            return false;
        }
    }

    private static WPos RoundPosition(WPos position) => new(MathF.Round(position.X * 2f) * 0.5f, MathF.Round(position.Z * 2f) * 0.5f);
}

sealed class PiercingLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PiercingLaser, new AOEShapeRect(40f, 2.5f));

sealed class DD20CloningNodeStates : StateMachineBuilder
{
    public DD20CloningNodeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlameBreath>()
            .ActivateOnEnter<PiercingLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 898, NameID = 12261)]
public sealed class DD20CloningNode(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(19.5f));
