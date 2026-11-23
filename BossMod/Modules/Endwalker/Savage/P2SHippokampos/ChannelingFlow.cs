namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to channeling [over]flow mechanics
sealed class ChannelingFlow(BossModule module) : BossComponent(module)
{
    public int NumStunned;
    private readonly (WDir, DateTime)[] _arrows = new (WDir, DateTime)[PartyState.MaxPartySize];

    private const float _typhoonHalfWidth = 2.5f;

    public bool SlotActive(int slot)
    {
        var (dir, expire) = _arrows[slot];
        return dir != new WDir() && (expire - WorldState.CurrentTime).TotalSeconds < 13d;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        Actor? partner = null;
        if (SlotActive(slot))
        {
            int numPartners = 0, numClipped = 0;
            var partnerDir = -_arrows[slot].Item1;
            float minDistance = 50;
            foreach (var (otherSlot, otherActor) in Raid.WithSlot(false, true, true))
            {
                if (otherSlot == slot || !otherActor.Position.InRect(actor.Position, _arrows[slot].Item1, 50f, default, _typhoonHalfWidth))
                    continue;
                if (_arrows[otherSlot].Item1 == partnerDir)
                {
                    minDistance = Math.Min(minDistance, partnerDir.Dot(actor.Position - otherActor.Position));
                    ++numPartners;
                    partner = otherActor;
                }
                else
                {
                    ++numClipped;
                }
            }

            if (numPartners == 0)
                hints.Add("Aim to hit partner!");
            if (numPartners > 1 || numClipped > 0)
                hints.Add("Avoid clipping irrelevant players!");
            if (minDistance < 20) // TODO: verify min range
                hints.Add("Too close to partner!");
        }

        foreach (var (s, p) in Raid.WithSlot(false, true, true))
        {
            if (SlotActive(s) && p != actor && p != partner && actor.Position.InRect(p.Position, _arrows[s].Item1, 50f, default, _typhoonHalfWidth))
            {
                hints.Add("GTFO from imminent flow!");
                break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (s, player) in Raid.WithSlot(false, true, true))
        {
            if (SlotActive(s))
                Arena.ZoneRect(player.Position, _arrows[s].Item1, 50f, default, _typhoonHalfWidth, Colors.AOE);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.MarkFlowN:
                SetArrow(actor, new(0, -1), status.ExpireAt);
                break;
            case (uint)SID.MarkFlowS:
                SetArrow(actor, new(0, +1), status.ExpireAt);
                break;
            case (uint)SID.MarkFlowW:
                SetArrow(actor, new(-1, 0), status.ExpireAt);
                break;
            case (uint)SID.MarkFlowE:
                SetArrow(actor, new(+1, 0), status.ExpireAt);
                break;
            case (uint)SID.Stun:
                ++NumStunned;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.MarkFlowN:
            case (uint)SID.MarkFlowS:
            case (uint)SID.MarkFlowW:
            case (uint)SID.MarkFlowE:
                SetArrow(actor, default, default);
                break;
            case (uint)SID.Stun:
                --NumStunned;
                break;
        }
    }

    private void SetArrow(Actor actor, WDir dir, DateTime expire)
    {
        var slot = WorldState.Party.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _arrows[slot] = (dir, expire);
    }

}
