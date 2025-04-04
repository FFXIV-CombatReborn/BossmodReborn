using BossMod.Global.MaskedCarnivale.Stage01;

namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

class WavelengthAlphaBeta(BossModule module) : BossComponent(module)
{
    private class PlayerInfo
    {
        public Actor Actor;
        public DateTime Expiration;  // rounded for matching
        public DateTime ExpirationReal;
        public uint StatusID;
        public int Order;  // 1-4, where 1 is the shortest time
    }

    private readonly PlayerInfo[] playersBySlot = new PlayerInfo[8];
    private int numCasts;
    private bool orderDetermined;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (numCasts == 8)
        {
            var player = playersBySlot[slot];
            if (player == null)
                return;

            var partner = FindPartner(slot, player);
            if (partner != null)
            {
                var remaining = Math.Max(0d, (partner.ExpirationReal - WorldState.CurrentTime).TotalSeconds);
                var check = remaining < 5d;
                hints.Add($"Order: {player.Order}");
                hints.Add($"Stack with: {partner.Actor.Name} (#{player.Order}) in {remaining:f0}s", check);

                // Check for incorrect stacks
                bool inRisk = false;
                for (var i = 0; i < 8; ++i)
                {
                    var other = playersBySlot[i];
                    if (other == null || i == slot || other == partner)
                        continue;

                    var otherRemaining = Math.Max(0d, (other.ExpirationReal - WorldState.CurrentTime).TotalSeconds);
                    if (otherRemaining < 5d && actor.Position.InCircle(other.Actor.Position, 2f))
                    {
                        inRisk = true;
                    }
                }

                if (inRisk)
                    hints.Add("GTFO from incorrect stacks!");
            }
            else
            {
                hints.Add("No viable partner found!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (numCasts == 8)
        {
            var player = playersBySlot[pcSlot];
            if (player == null)
            {
                return;
            }

            var partner = FindPartner(pcSlot, player);
            for (var i = 0; i < 8; ++i)
            {
                var other = playersBySlot[i];
                if (other == null || i == pcSlot)
                    continue;

                var remaining = Math.Max(0d, (other.ExpirationReal - WorldState.CurrentTime).TotalSeconds);
                if (remaining >= 5d)
                    continue;

                // partner
                if (partner != null && other == partner)
                {
                    var color = pc.Position.InCircle(other.Actor.Position, 2f) ? Colors.Safe : Colors.Danger;
                    Arena.AddCircle(other.Actor.Position, 2f, color);
                }

                // intersecting wrong player
                if (pc.Position.InCircle(other.Actor.Position, 2f))
                {
                    Arena.AddCircle(other.Actor.Position, 2f, Colors.Danger);
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.WavelengthAlpha or (uint)SID.WavelengthBeta)
        {
            var slot = WorldState.Party.FindSlot(actor.InstanceID);
            if (slot < 0)
                return;

            playersBySlot[slot] = new PlayerInfo
            {
                Actor = actor,
                Expiration = status.ExpireAt.Round(TimeSpan.FromSeconds(1d)),
                ExpirationReal = status.ExpireAt,
                StatusID = status.ID,
                Order = 0
            };
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.WavelengthAlpha or (uint)SID.WavelengthBeta)
        {
            var slot = WorldState.Party.FindSlot(actor.InstanceID);
            if (slot < 0)
                return;

            if (playersBySlot[slot] != null)
            {
                playersBySlot[slot].ExpirationReal = DateTime.MinValue;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GetDownBait)
        {
            ++numCasts;

            // determine debuff order after all 8 casts
            if (numCasts == 8 && !orderDetermined)
            {
                DetermineOrder();
                orderDetermined = true;
            }
        }
    }

    private void DetermineOrder()
    {
        // group players by debuff
        var alphaPlayers = new List<PlayerInfo>();
        var betaPlayers = new List<PlayerInfo>();

        foreach (var player in playersBySlot)
        {
            if (player == null)
                continue;

            if (player.StatusID == (uint)SID.WavelengthAlpha)
            {
                alphaPlayers.Add(player);
            }
            else
            {
                betaPlayers.Add(player);
            }
        }

        // sort each debuff group by expiration time
        alphaPlayers.Sort((a, b) => a.Expiration.CompareTo(b.Expiration));
        betaPlayers.Sort((a, b) => a.Expiration.CompareTo(b.Expiration));

        // assign order from 1-4
        for (int index = 0; index < alphaPlayers.Count; index++)
        {
            alphaPlayers[index].Order = index + 1;
        }

        for (int index = 0; index < betaPlayers.Count; index++)
        {
            betaPlayers[index].Order = index + 1;
        }
    }

    private PlayerInfo FindPartner(int slot, PlayerInfo player)
    {
        PlayerInfo bestMatch = null;
        TimeSpan smallestDiff = TimeSpan.MaxValue;

        // look for the player with opposite debuff and closest expiration time
        foreach (var other in playersBySlot)
        {
            if (other == null || other.Actor.InstanceID == player.Actor.InstanceID) 
                continue;

            // skip dead players or those with expired debuffs
            if (!other.Actor.IsTargetable || other.ExpirationReal <= WorldState.CurrentTime) 
                continue;

            // check if opposite debuff
            bool isOpposite = (player.StatusID == (uint)SID.WavelengthAlpha && other.StatusID == (uint)SID.WavelengthBeta) ||
                             (player.StatusID == (uint)SID.WavelengthBeta && other.StatusID == (uint)SID.WavelengthAlpha);

            if (isOpposite)
            {
                // check if times are close
                var timeDiff = player.Expiration - other.Expiration;
                if (timeDiff < TimeSpan.Zero)
                {
                    timeDiff = -timeDiff;
                }

                // the correct debuff never seems to be more than 3 seconds apart
                if (timeDiff < smallestDiff && timeDiff.TotalSeconds < 3.0f)
                {
                    smallestDiff = timeDiff;
                    bestMatch = other;
                }
            }
        }

        return bestMatch;
    }
}
