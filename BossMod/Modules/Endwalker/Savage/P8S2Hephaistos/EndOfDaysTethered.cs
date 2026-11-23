namespace BossMod.Endwalker.Savage.P8S2;

sealed class EndOfDaysTethered(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor source, Actor target)> _tethers = []; // enemy -> player

    private readonly AOEShapeRect _shape = new(60f, 5f);

    public bool Active => _tethers.Count > 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        Actor? tetheredCaster = null;
        var inOtherBait = false;
        for (var i = 0; i < _tethers.Count; ++i)
        {
            var t = _tethers[i];
            if (t.target == actor)
                tetheredCaster ??= t.source;
            else
                inOtherBait |= _shape.Check(actor.Position, t.source);
        }
        if (tetheredCaster == null)
            return; // non-tethered players shouldn't need to worry about this mechanic

        if (Raid.WithoutSlot(false, true, true).AnyInShape(_shape, tetheredCaster.Position, tetheredCaster.Rotation, actor))
            hints.Add("Bait away from raid!");
        if (inOtherBait)
            hints.Add("Move away from other baits!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
            _shape.Draw(Arena, t.source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
            Arena.AddLine(t.source.Position, t.target.Position, Colors.Danger);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.IllusoryHephaistosMovable)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tethers.Add((source, target));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EndOfDaysMovable)
            _tethers.RemoveAll(e => e.source == caster);
    }
}
