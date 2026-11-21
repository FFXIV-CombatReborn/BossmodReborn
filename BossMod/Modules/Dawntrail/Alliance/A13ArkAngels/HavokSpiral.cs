namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class HavokSpiral(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private readonly List<Angle> _rotation = [with(3)];

    private static readonly AOEShapeCone _shape = new(30f, 15f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is not ((uint)IconID.RotateCW or (uint)IconID.RotateCCW))
        {
            return;
        }
        _increment = (iconID == (uint)IconID.RotateCW ? -30f : 30f).Degrees();
        _activation = WorldState.FutureTime(5.5d);
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (_rotation.Count == 3 && _increment != default)
        {
            for (var i = 0; i < 3; ++i)
                Sequences.Add(new(_shape, Arena.Center.Quantized(), _rotation[i], _increment, _activation, 1.2d, 8));
            _rotation.Clear();
            _increment = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HavocSpiralFirst)
        {
            _rotation.Add(spell.Rotation);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HavocSpiralFirst or (uint)AID.HavocSpiralRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

sealed class SpiralFinish(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SpiralFinishAOE, 16f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 9f), c.Activation);
        }
    }
}
