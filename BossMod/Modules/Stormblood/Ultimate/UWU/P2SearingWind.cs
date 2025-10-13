namespace BossMod.Stormblood.Ultimate.UWU;

class P2SearingWind(BossModule module) : Components.UniformStackSpread(module, default, 14f, includeDeadTargets: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfernoHowl && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            AddSpread(target, WorldState.FutureTime(8d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SearingWind)
        {
            var index = Enumerable.Range(0, Spreads.Count).MinBy(i => (spell.TargetXZ - Spreads[i].Target.Position).LengthSq());
            if (index < Spreads.Count)
            {
                ref var spread = ref Spreads.Ref(index);
                var status = spread.Target.FindStatus((uint)SID.SearingWind);
                if (status == null || (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds < 6d)
                {
                    Spreads.RemoveAt(index);
                }
                else
                {
                    spread.Activation = WorldState.FutureTime(6d);
                }
            }
        }
    }
}
