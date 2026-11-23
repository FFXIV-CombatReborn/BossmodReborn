namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to hell's sting mechanic (part of curtain call sequence)
sealed class HellsSting(BossModule module) : BossComponent(module)
{
    public int NumCasts;

    private readonly AOEShapeCone _cone = new(50f, 15f.Degrees());
    private readonly List<Angle> _directions = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts >= _directions.Count * 2)
            return;

        var offset = ConeOffset;
        for (var i = 0; i < _directions.Count; ++i)
        {
            if (actor.Position.InCone(Module.PrimaryActor.Position, _directions[i] + offset, _cone.HalfAngle))
            {
                hints.Add("GTFO from cone!");
                break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (NumCasts >= _directions.Count * 2)
            return;

        var offset = ConeOffset;
        for (var i = 0; i < _directions.Count; ++i)
            _cone.Draw(Arena, Module.PrimaryActor.Position, _directions[i] + offset);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HellsStingAOE1)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HellsStingAOE1 or AID.HellsStingAOE2)
            ++NumCasts;
    }

    // second set of cones is rotated by half a cone
    private Angle ConeOffset => NumCasts < _directions.Count ? default : 22.5f.Degrees();
}
