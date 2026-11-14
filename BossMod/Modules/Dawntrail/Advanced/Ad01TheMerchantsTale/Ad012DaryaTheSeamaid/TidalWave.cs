namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad012DaryaTheSeamaid;

sealed class SeaShackles(BossModule module) : Components.StretchTetherDuo(module, 25f, 30d, (uint)TetherID.TetherBad, (uint)TetherID.TetherGood)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        // add tether for non-"source" player
        var (player, target) = DetermineTetherSides(source, tether);
        if (player != null && target != null)
        {
            var activation = default(DateTime);
            var count = ActivationDelayOnActor.Count;
            for (var i = 0; i < count; ++i)
            {
                if (ActivationDelayOnActor[i].Item1 == target)
                {
                    activation = ActivationDelayOnActor[i].Item2;
                    break;
                }
            }
            if (activation == default)
            {
                activation = WorldState.FutureTime(ActivationDelay);
                ActivationDelayOnActor.Add((target, activation));
            }
            CurrentBaits.Add(new(player, target, Shape ?? new AOEShapeCircle(default), activation));
            TetherOnActor.Add((target, tether.ID));
        }
    }
    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        base.OnUntethered(source, tether);
        var (player, target) = DetermineTetherSides(source, tether);
        if (player != null && target != null)
        {
            for (var i = CurrentBaits.Count - 1; i >= 0; --i)
            {
                var b = CurrentBaits[i];
                if (b.Source == player && b.Target == target)
                {
                    CurrentBaits.RemoveAt(i);
                }
            }
            if (WorldState.Actors.Find(tether.Target) is Actor tetherTarget)
            {
                TetherOnActor.Remove((tetherTarget, tether.ID));
            }
        }
    }
}
sealed class AquaSpear(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AquaSpear)
        {
            _aoes.Add(new(new AOEShapeRect(4f, 4f, 4f), caster.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AquaSpearVoidzone)
        {
            _aoes.Clear();
        }
    }
}

sealed class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWave, 25f, kind: Kind.DirForward)
{
    // kb roughly 3 squares away
    // TODO: add forbidden zones so player doesn't go outside arena
    // add hints for knockbacks where player goes into or over puddle
    // without kb immune safe spots are behind squares on edges
    private readonly List<WPos> _puddles = [];
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.AquaSpear)
        {
            _puddles.Add(caster.Position);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AquaSpearVoidzone)
        {
            _puddles.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // want to avoid getting knocked back into and knocked back over puddles
        var puddles = CollectionsMarshal.AsSpan(_puddles);
        var len = puddles.Length;
        if (len == 0)
            return;

        if (Casters.Count == 0)
            return;

        // only ever 1 caster at a time
        var kb = ActiveKnockbacks(slot, actor)[0];
        var origin = kb.Origin;
        var dir = (origin - Arena.Center).Normalized();
        for (var i = 0; i < len; ++i)
        {
            var puddle = puddles[i];
            hints.AddForbiddenZone(new SDRect(puddle, dir, 30f, 4f, 4f));
        }
    }
}
