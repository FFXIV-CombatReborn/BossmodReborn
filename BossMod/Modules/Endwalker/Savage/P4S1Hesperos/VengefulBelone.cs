namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to vengeful belone mechanic
sealed class VengefulBelone(BossModule module) : BossComponent(module)
{
    private readonly Dictionary<ulong, Role> _orbTargets = [];
    private int _orbsExploded;
    private readonly int[] _playerRuinCount = new int[8];
    private readonly Role[] _playerActingRole = new Role[8];

    private const float _burstRadius = 8f;

    private Role OrbTarget(ulong instanceID) => _orbTargets.GetValueOrDefault(instanceID, Role.None);

    private bool NearLethalOrb(int slot, Actor actor)
    {
        var orbs = Module.Enemies((uint)OID.Orb);
        for (var i = 0; i < orbs.Count; ++i)
        {
            var orb = orbs[i];
            if (orb.Position.InCircle(actor.Position, _burstRadius) && IsOrbLethal(slot, actor, OrbTarget(orb.InstanceID)))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
            return; // inactive

        var ruinCount = _playerRuinCount[slot];
        if (ruinCount > 2 || ruinCount == 2 && _playerActingRole[slot] != Role.None)
        {
            hints.Add("Failed orbs...");
        }

        if (NearLethalOrb(slot, actor))
        {
            hints.Add("GTFO from wrong orb!");
        }

        if (ruinCount < 2)
        {
            // TODO: stack check...
            hints.Add($"Pop next orb {ruinCount + 1}/2!", false);
        }
        else if (ruinCount == 2 && _playerActingRole[slot] == Role.None)
        {
            hints.Add($"Avoid orbs", false);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_orbTargets.Count == 0 || _orbsExploded == _orbTargets.Count)
            return;

        var orbs = Module.Enemies((uint)OID.Orb);
        foreach (var orb in orbs)
        {
            var orbRole = OrbTarget(orb.InstanceID);
            if (orbRole == Role.None)
                continue; // this orb has already exploded

            var lethal = IsOrbLethal(pcSlot, pc, orbRole);
            Arena.Actor(orb, lethal ? Colors.Enemy : Colors.Danger, true);

            var target = WorldState.Actors.Find(orb.Tether.Target);
            if (target != null)
            {
                Arena.AddLine(orb.Position, target.Position, Colors.Danger);
            }

            int goodInRange = 0, badInRange = 0;
            foreach ((var i, var player) in Raid.WithSlot(false, true, true))
            {
                if (!(player.Position.InCircle(orb.Position, _burstRadius)))
                    continue;
                if (IsOrbLethal(i, player, orbRole))
                    ++badInRange;
                else
                    ++goodInRange;
            }

            var goodToExplode = goodInRange == 2 && badInRange == 0;
            Arena.ZoneCircleOutline(orb.Position, _burstRadius, goodToExplode ? Colors.Safe : Colors.Danger);
        }

        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            var nearLethalOrb = NearLethalOrb(i, player);
            Arena.Actor(player, nearLethalOrb ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: nearLethalOrb ? true : null);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.OrbRole:
                _orbTargets[actor.InstanceID] = OrbRoleFromStatusParam(status.Extra);
                break;
            case (uint)SID.ThriceComeRuin:
                ModifyRuinStacks(actor, status.Extra);
                break;
            case (uint)SID.ActingDPS:
                ModifyActingRole(actor, Role.Melee);
                break;
            case (uint)SID.ActingHealer:
                ModifyActingRole(actor, Role.Healer);
                break;
            case (uint)SID.ActingTank:
                ModifyActingRole(actor, Role.Tank);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ThriceComeRuin:
                ModifyRuinStacks(actor, 0);
                break;
            case (uint)SID.ActingDPS:
            case (uint)SID.ActingHealer:
            case (uint)SID.ActingTank:
                ModifyActingRole(actor, Role.None);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BeloneBurstsAOETank or (uint)AID.BeloneBurstsAOEHealer or (uint)AID.BeloneBurstsAOEDPS)
        {
            _orbTargets[caster.InstanceID] = Role.None;
            ++_orbsExploded;
        }
    }

    private Role OrbRoleFromStatusParam(uint param)
    {
        return param switch
        {
            0x13A => Role.Tank,
            0x13B => Role.Melee,
            0x13C => Role.Healer,
            _ => Role.None
        };
    }

    private bool IsOrbLethal(int slot, Actor player, Role orbRole)
    {
        var ruinCount = _playerRuinCount[slot];
        if (ruinCount >= 2)
            return true; // any orb is now lethal

        var actingRole = _playerActingRole[slot];
        if (ruinCount == 1 && actingRole != Role.None)
            return orbRole != actingRole; // player must clear acting debuff, or he will die

        var playerRole = player.Role == Role.Ranged ? Role.Melee : player.Role;
        return orbRole == playerRole;
    }

    private void ModifyRuinStacks(Actor actor, ushort count)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerRuinCount[slot] = count;
    }

    private void ModifyActingRole(Actor actor, Role role)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerActingRole[slot] = role;
    }
}
