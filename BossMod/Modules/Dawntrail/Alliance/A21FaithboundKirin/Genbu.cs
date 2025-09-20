namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class MoontideFont(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.MoontideFont1, (uint)AID.MoontideFont2], 9f);

sealed class MidwinterMarchNorthernCurrent(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(12f), new AOEShapeDonut(12f, 60f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MidwinterMarch)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.MidwinterMarch => 0,
                (uint)AID.NorthernCurrent => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(5d));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // extra ai hint: stay close to the edge of the first aoe
        if (Sequences.Count != 0)
        {
            ref var aoe = ref Sequences.Ref(0);
            if (aoe.NumCastsDone == 0)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 15f), aoe.NextActivation);
            }
        }
    }
}
