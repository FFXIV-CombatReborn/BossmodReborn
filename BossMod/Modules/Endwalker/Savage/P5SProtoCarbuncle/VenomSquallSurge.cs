namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

sealed class VenomDrops(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VenomDrops, 5);

sealed class VenomSquallSurge(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, Rain, Drops, Pool }

    public int Progress;
    public bool _reverse;

    private const float _radius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (NextMechanic)
        {
            case Mechanic.Rain:
                if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _radius))
                    hints.Add("Spread!");
                break;
            case Mechanic.Pool:
                if (Raid.WithoutSlot(false, true, true).CountInRadius(actor.Position, _radius, Role.Healer) != 1)
                    hints.Add("Stack with healer!");
                break;
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        hints.Add(_reverse ? "Order: stack -> mid -> spread" : "Order: spread -> mid -> stack");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => NextMechanic == Mechanic.Pool && player.Role == Role.Healer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        switch (NextMechanic)
        {
            case Mechanic.Rain: // spreads
                Arena.ZoneCircleOutline(pc.Position, _radius, Colors.Danger);
                break;
            case Mechanic.Drops: // bait
                foreach (var p in Raid.WithoutSlot(false, true, true))
                    Arena.ZoneCircleOutline(p.Position, _radius, Colors.Danger);
                break;
            case Mechanic.Pool: // party stacks
                foreach (var p in Raid.WithoutSlot(false, true, true))
                    if (p.Role == Role.Healer)
                        Arena.ZoneCircleOutline(p.Position, _radius, Colors.Danger);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.VenomSurge:
                _reverse = true;
                break;
            case (uint)AID.VenomDrops:
                if (NextMechanic == Mechanic.Drops)
                    ++Progress;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.VenomRain:
                if (NextMechanic == Mechanic.Rain)
                    ++Progress;
                break;
            case (uint)AID.VenomPool:
                if (NextMechanic == Mechanic.Pool)
                    ++Progress;
                break;
        }
    }

    private Mechanic NextMechanic => Progress switch
    {
        0 => _reverse ? Mechanic.Pool : Mechanic.Rain,
        1 => Mechanic.Drops,
        2 => _reverse ? Mechanic.Rain : Mechanic.Pool,
        _ => Mechanic.None
    };
}
