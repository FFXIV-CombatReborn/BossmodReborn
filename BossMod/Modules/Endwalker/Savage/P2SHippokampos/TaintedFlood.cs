namespace BossMod.Endwalker.Savage.P2SHippokampos;

// note: if activated together with ChannelingFlow, it does not target next flow arrows
sealed class TaintedFlood : Components.CastCounter
{
    private BitMask _ignoredTargets;

    private const float _radius = 6f;

    public TaintedFlood(BossModule module) : base(module, (uint)AID.TaintedFloodAOE)
    {
        var flow = module.FindComponent<ChannelingFlow>();
        if (flow != null)
        {
            _ignoredTargets = Raid.WithSlot(false, true, true).WhereSlot(flow.SlotActive).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;

        if (_ignoredTargets[slot])
        {
            // player is not a target of flood, so just make sure he is not clipped by others
            foreach (var (s, p) in Raid.WithSlot(false, true, true))
            {
                if (!_ignoredTargets[s] && p.Position.InCircle(actor.Position, _radius))
                {
                    hints.Add("GTFO from flood!");
                    break;
                }
            }
        }
        else
        {
            // player is target of flood => make sure no one is in range
            if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _radius))
                hints.Add("Spread!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts > 0)
            return;

        if (_ignoredTargets[pcSlot])
        {
            foreach ((var slot, var actor) in Raid.WithSlot(false, true, true))
            {
                if (_ignoredTargets[slot])
                {
                    continue;
                }
                Arena.Actor(actor, Colors.Danger, drawWorld: true);
                Arena.ZoneCircleOutline(actor.Position, _radius, Colors.Danger);
            }
        }
        else
        {
            Arena.ZoneCircleOutline(pc.Position, _radius, Colors.Danger);
            foreach (var player in Raid.WithoutSlot(false, true, true))
            {
                if (player == pc)
                {
                    continue;
                }
                Arena.Actor(player, player.Position.InCircle(pc.Position, _radius) ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: player.Position.InCircle(pc.Position, _radius) ? true : null);
            }
        }
    }
}
