namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

sealed class RingARingOExplosions(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> ActiveBombs = [];
    private readonly List<Actor> _bombs = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(12f);

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = ActiveBombs.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(_shape, ActiveBombs[i].Position, default, _activation);
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void Update()
    {
        var count = _bombs.Count;
        if (count == 6 && ActiveBombs.Count == 0)
        {
            Actor? glowingBomb = null;
            for (var i = 0; i < count; ++i)
            {
                var bomb = _bombs[i];
                if (bomb.ModelState.AnimState1 == 1)
                {
                    glowingBomb = bomb;
                    break;
                }
            }
            if (glowingBomb != null)
            {
                var cur = glowingBomb;
                do
                {
                    ActiveBombs.Add(cur);
                    cur = WorldState.Actors.Find(cur.Tether.Target);
                } while (cur != null && cur != glowingBomb);
                _activation = WorldState.FutureTime(17.4d);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NBomb or (uint)OID.SBomb)
            _bombs.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBombBurst or (uint)AID.SBombBurst)
        {
            ++NumCasts;
            ActiveBombs.Remove(caster);
        }
    }
}
