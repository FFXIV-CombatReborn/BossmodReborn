namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class BackUpDance(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private DateTime _activation;
    private readonly List<Actor> sources = new(4);
    private readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_activation != default)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var count = sources.Count;
            var len = party.Length;
            for (var i = 0; i < count; ++i)
            {
                var source = sources[i];
                Actor? closest = null;
                var closestDistSq = float.MaxValue;

                for (var j = 0; j < len; ++j)
                {
                    var player = party[j];
                    var distSq = (player.Position - source.Position).LengthSq();
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closest = player;
                    }
                }

                if (closest != null)
                {
                    CurrentBaits.Add(new(source, closest, cone, _activation));
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BackUpDanceVisual)
        {
            sources.Add(caster);
            _activation = Module.CastFinishAt(spell, 0.6d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BackUpDance)
        {
            ++NumCasts;
            CurrentBaits.Clear();
            sources.Clear();
            _activation = default;
        }
    }
}
