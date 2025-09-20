namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

sealed class FirebreatheRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;

    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FirebreatheRotationVisual)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell, 0.7f);
            InitIfReady(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FirebreatheRotation)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -90f.Degrees(),
            (uint)IconID.RotateCCW => 90f.Degrees(),
            _ => default
        };
        InitIfReady(actor);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(cone, source.Position.Quantized(), _rotation, _increment, _activation, 2f, 5));
            _rotation = default;
            _increment = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // stay close to the middle
        if (Sequences.Count != 0)
            hints.AddForbiddenZone(new SDInvertedCircle(Module.PrimaryActor.Position, 5f));
    }
}
