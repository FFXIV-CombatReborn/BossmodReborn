namespace BossMod.Endwalker.Savage.P8S2;

sealed class Dominion : Components.UniformStackSpread
{
    public int NumDeformations;
    public int NumShifts;
    public List<Actor> Casters = [];
    private BitMask _secondOrder;

    private const float _towerRadius = 3;

    public Dominion(BossModule module) : base(module, 0, 6)
    {
        AddSpreads(Raid.WithoutSlot(false, true, true));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (NumDeformations >= 4)
        {
            hints.Add($"Soak order: {(_secondOrder[slot] ? "2" : "1")}", ShouldSoak(slot) && !InActiveTower(actor));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (NumDeformations >= 4 && ShouldSoak(pcSlot))
            foreach (var tower in ActiveTowers())
                Arena.ZoneCircleOutline(tower.Position, _towerRadius, Colors.Danger, 2);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OrogenicShift)
        {
            Casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OrogenicShift)
        {
            ++NumShifts;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OrogenicDeformation)
        {
            Spreads.Clear();
            _secondOrder.Set(Raid.FindSlot(spell.MainTargetID));
            ++NumDeformations;
        }
    }

    private bool ShouldSoak(int slot) => NumShifts == (_secondOrder[slot] ? 4 : 0);

    // first four towers before the shifts, last four after them
    private ReadOnlySpan<Actor> ActiveTowers()
    {
        if (NumShifts == 0 && Casters.Count >= 4)
            return Casters.SpanRange(0, 4);
        if (NumShifts == 4 && Casters.Count == 8)
            return Casters.SpanRange(4);
        return [];
    }

    private bool InActiveTower(Actor actor) => ActiveTowers().AnyInRadius(actor.Position, _towerRadius);
}
