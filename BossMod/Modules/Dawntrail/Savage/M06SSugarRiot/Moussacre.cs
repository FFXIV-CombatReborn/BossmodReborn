namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class Moussacre(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private DateTime _activation;

    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());

    private (Actor actor, float distSq)[] _distances = new (Actor, float)[PartyState.MaxPartySize];

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_activation != default)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            if (_distances.Length < len)
                _distances = new (Actor, float)[len];
            var distances = _distances;
            var source = Module.PrimaryActor;
            var sourcePos = source.Position;

            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                var distSq = (p.Position - sourcePos).LengthSq();
                distances[i] = (p, distSq);
            }

            var targets = Math.Min(4, len);
            for (var i = 0; i < targets; ++i)
            {
                var selIdx = i;
                for (var j = i + 1; j < len; ++j)
                {
                    if (distances[j].distSq < distances[selIdx].distSq)
                        selIdx = j;
                }

                if (selIdx != i)
                {
                    (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
                }

                CurrentBaits.Add(new(source, distances[i].actor, cone, _activation));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MoussacreVisual)
        {
            _activation = Module.CastFinishAt(spell, 0.7f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Moussacre)
        {
            ++NumCasts;
            CurrentBaits.Clear();
            _activation = default;
        }
    }
}
