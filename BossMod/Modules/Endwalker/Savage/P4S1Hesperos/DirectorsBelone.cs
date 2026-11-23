namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to director's belone (debuffs) mechanic
// note that forbidden targets are selected either from bloodrake tethers (first instance of mechanic) or from tower types (second instance of mechanic)
sealed class DirectorsBelone(BossModule module) : BossComponent(module)
{
    private bool _assigned;
    private BitMask _debuffForbidden;
    private BitMask _debuffTargets;
    private BitMask _debuffImmune;

    private const float _debuffPassRange = 3; // not sure about this...

    public override void Update()
    {
        if (!_assigned)
        {
            var coils = Module.FindComponent<BeloneCoils>();
            if (coils == null)
            {
                // assign from bloodrake tethers
                _debuffForbidden = Raid.WithSlot(false, true, true).Tethered(TetherID.Bloodrake).Mask();
                _assigned = true;
            }
            else if (coils.ActiveSoakers != BeloneCoils.Soaker.Unknown)
            {
                // assign from coils (note that it happens with some delay)
                _debuffForbidden = Raid.WithSlot(false, true, true).WhereActor(coils.IsValidSoaker).Mask();
                _assigned = true;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_debuffForbidden.None())
            return;

        if (!_debuffForbidden[slot])
        {
            // we should be grabbing debuff
            if (_debuffTargets.None())
            {
                // debuffs not assigned yet => spread and prepare to grab
                var stacked = Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _debuffPassRange);
                hints.Add("Debuffs: spread and prepare to handle!", stacked);
            }
            else if (_debuffImmune[slot])
            {
                hints.Add("Debuffs: failed to handle");
            }
            else if (_debuffTargets[slot])
            {
                hints.Add("Debuffs: OK", false);
            }
            else
            {
                hints.Add("Debuffs: grab!");
            }
        }
        else
        {
            // we should be passing debuff
            if (_debuffTargets.None())
            {
                var badStack = false;
                foreach (var (s, p) in Raid.WithSlot(false, true, true))
                {
                    if (s != slot && _debuffForbidden[s] && !p.Position.InCircle(actor.Position, _debuffPassRange))
                    {
                        badStack = true;
                        break;
                    }
                }
                hints.Add("Debuffs: stack and prepare to pass!", badStack);
            }
            else if (_debuffTargets[slot])
            {
                hints.Add("Debuffs: pass!");
            }
            else
            {
                hints.Add("Debuffs: avoid", false);
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var forbidden = _debuffForbidden.Any() ? Raid[_debuffForbidden.LowestSetBit()] : null;
        if (forbidden != null)
        {
            hints.Add($"Stack: {(forbidden.Role is Role.Tank or Role.Healer ? "Tanks/Healers" : "DD")}");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_debuffTargets.None())
            return;

        var failingPlayers = _debuffForbidden & _debuffTargets;
        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            var isfailing = failingPlayers[i];
            Arena.Actor(player, isfailing ? Colors.Danger : Colors.PlayerGeneric, drawWorld: isfailing ? true : null);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.RoleCall:
                _debuffTargets.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.Miscast:
                _debuffImmune.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.RoleCall:
                _debuffTargets.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.Miscast:
                _debuffImmune.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CursedCasting1 or (uint)AID.CursedCasting2)
            _debuffForbidden.Reset();
    }
}
