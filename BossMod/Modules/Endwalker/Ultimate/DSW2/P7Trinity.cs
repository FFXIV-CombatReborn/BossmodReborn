namespace BossMod.Endwalker.Ultimate.DSW2;

// TODO: improve...
sealed class P7Trinity(DSW2 module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly Actor? _source = module.BossP7();

    private readonly AOEShapeCircle _shape = new(3);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null)
        {
            var party = Raid.WithoutSlot(false, true, true);
            Actor? closest = null;
            var closestDistSq = float.MaxValue;
            var sourcePos = _source.Position;
            for (var i = 0; i < party.Length; ++i)
            {
                var target = party[i];
                if (target.Role == Role.Tank)
                {
                    CurrentBaits.Add(new(_source, target, _shape));
                    continue;
                }
                var distSq = (target.Position - sourcePos).LengthSq();
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = target;
                }
            }
            if (closest != null)
                CurrentBaits.Add(new(_source, closest, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TrinityAOE1 or (uint)AID.TrinityAOE2 or (uint)AID.TrinityAOE3)
        {
            ++NumCasts;
        }
    }
}
