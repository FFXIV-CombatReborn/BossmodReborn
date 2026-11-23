namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to trail of condemnation mechanic
sealed class TrailOfCondemnation(BossModule module) : BossComponent(module)
{
    public bool Done;
    private readonly bool _isCenter = (module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.TrailOfCondemnationCenter;
    private const float _aoeRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.Position == Arena.Center)
            return;
        if (_isCenter)
        {
            if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _aoeRadius))
            {
                hints.Add("Spread!");
            }
        }
        else
        {
            // note: sparks either target all tanks & healers or all dds - so correct pairings are always dd+tank/healer
            var numStacked = 0;
            var goodPair = false;
            foreach (var pair in Raid.WithoutSlot(false, true, true))
            {
                if (!(pair != actor && pair.Position.InCircle(actor.Position, _aoeRadius)))
                    continue;
                ++numStacked;
                goodPair = (actor.Role == Role.Tank || actor.Role == Role.Healer) != (pair.Role == Role.Tank || pair.Role == Role.Healer);
            }
            if (numStacked != 1)
            {
                hints.Add("Stack in pairs!");
            }
            else if (!goodPair)
            {
                hints.Add("Incorrect pairing!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw all raid members, to simplify positioning
        foreach (var player in Raid.WithoutSlot(false, true, true))
        {
            if (player == pc)
            {
                continue;
            }
            var inRange = player.Position.InCircle(pc.Position, _aoeRadius);
            Arena.Actor(player, inRange ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: inRange ? true : null);
        }

        // draw circle around pc
        Arena.ZoneCircleOutline(pc.Position, _aoeRadius, Colors.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FlareOfCondemnation or (uint)AID.SparksOfCondemnation)
            Done = true;
    }
}
