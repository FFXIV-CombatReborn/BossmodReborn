namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 1 wreath of thorns
// note: there should be two tethered helpers for aoes on activation
sealed class WreathOfThorns1(BossModule module) : BossComponent(module)
{
    public enum State { FirstAOEs, Towers, LastAOEs, Done }

    public State CurState = State.FirstAOEs;
    private readonly List<Actor> _relevantHelpers = []; // 2 aoes -> 8 towers -> 2 aoes

    private ReadOnlySpan<Actor> FirstAOEs => _relevantHelpers.SpanRange(0, 2);
    private ReadOnlySpan<Actor> Towers => _relevantHelpers.SpanRange(2, 8);
    private ReadOnlySpan<Actor> LastAOEs => _relevantHelpers.SpanRange(10);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (CurState)
        {
            case State.FirstAOEs:
                if (FirstAOEs.AnyInRadius(actor.Position, P4S2.WreathAOERadius))
                {
                    hints.Add("GTFO from AOE!");
                }
                break;
            case State.Towers:
                {
                    var soakedTower = Towers.FirstInRadius(actor.Position, P4S2.WreathTowerRadius);
                    if (soakedTower == null)
                    {
                        hints.Add("Soak the tower!");
                    }
                    else if (Raid.WithoutSlot(false, true, true).CountInRadius(soakedTower.Position, P4S2.WreathTowerRadius) > 1) // actor is inside, so more than one means extra soakers
                    {
                        hints.Add("Multiple soakers for the tower!");
                    }
                }
                break;
            case State.LastAOEs:
                if (LastAOEs.AnyInRadius(actor.Position, P4S2.WreathAOERadius))
                {
                    hints.Add("GTFO from AOE!");
                }
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (CurState is State.FirstAOEs or State.LastAOEs)
            foreach (var aoe in CurState == State.FirstAOEs ? FirstAOEs : LastAOEs)
                Arena.ZoneCircle(aoe.Position, P4S2.WreathAOERadius, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurState == State.Towers)
        {
            foreach (var tower in Towers)
                Arena.ZoneCircleOutline(tower.Position, P4S2.WreathTowerRadius, Colors.Safe);
            foreach (var player in Raid.WithoutSlot(false, true, true))
                Arena.Actor(player, Colors.PlayerGeneric);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper && tether.ID == (uint)TetherID.WreathOfThorns)
            _relevantHelpers.Add(source);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.FirstAOEs && spell.Action.ID == (uint)AID.AkanthaiExplodeAOE)
            CurState = State.Towers;
        else if (CurState == State.Towers && spell.Action.ID == (uint)AID.AkanthaiExplodeTower)
            CurState = State.LastAOEs;
        else if (CurState == State.LastAOEs && spell.Action.ID == (uint)AID.AkanthaiExplodeAOE)
            CurState = State.Done;
    }
}
