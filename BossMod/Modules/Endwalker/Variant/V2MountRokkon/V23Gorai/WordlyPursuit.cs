namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V23Gorai;

sealed class WorldlyPursuit(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross cross = new(60f, 10f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = spell.Action.ID switch
        {
            (uint)AID.WorldlyPursuitFirstCW => -22.5f.Degrees(),
            (uint)AID.WorldlyPursuitFirstCCW => 22.5f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            ImminentColor = Colors.AOE;
            Sequences.Add(new(cross, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 3.7d, 5, 1));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WorldlyPursuitFirstCCW or (uint)AID.WorldlyPursuitFirstCW or (uint)AID.WorldlyPursuitRest)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}
