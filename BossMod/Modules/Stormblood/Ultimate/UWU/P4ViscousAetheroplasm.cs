namespace BossMod.Stormblood.Ultimate.UWU;

class P4ViscousAetheroplasmApply(BossModule module) : Components.Cleave(module, (uint)AID.ViscousAetheroplasmApply, new AOEShapeCircle(2f), [(uint)OID.UltimaWeapon], originAtTarget: true);

// TODO: if aetheroplasm target is the same as homing laser target, assume it is being soaked solo; consider merging these two components
class P4ViscousAetheroplasmResolve(BossModule module) : Components.UniformStackSpread(module, 4f, default, 7)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HomingLasers)
        {
            // update avoid target to homing laser target
            BitMask avoid = default;
            avoid.Set(Raid.FindSlot(spell.TargetID));
            foreach (ref var s in Stacks.AsSpan())
                s.ForbiddenPlayers = avoid;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ViscousAetheroplasmApply:
                var target = WorldState.Actors.Find(spell.MainTargetID);
                if (target != null)
                    AddStack(target, default, Raid.WithSlot(true, true, true).WhereActor(a => a.InstanceID != spell.MainTargetID && a.Role == Role.Tank).Mask());
                break;
            case (uint)AID.ViscousAetheroplasmResolve:
                Stacks.Clear();
                break;
            case (uint)AID.HomingLasers:
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers.Reset();
                break;
        }
    }
}

class P5ViscousAetheroplasmTriple(BossModule module) : Components.UniformStackSpread(module, 4f, default, 8)
{
    public int NumCasts;
    private readonly List<(Actor target, DateTime resolve)> _aetheroplasms = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ViscousAetheroplasm)
        {
            _aetheroplasms.Add((actor, status.ExpireAt));
            _aetheroplasms.Sort(static (a, b) => a.resolve.CompareTo(b.resolve));
            UpdateStackTargets();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ViscousAetheroplasmResolve)
        {
            ++NumCasts;
            _aetheroplasms.RemoveAll(a => a.target.InstanceID == spell.MainTargetID);
            UpdateStackTargets();
        }
    }

    private void UpdateStackTargets()
    {
        Stacks.Clear();
        if (_aetheroplasms.Count != 0)
            AddStack(_aetheroplasms[0].target, _aetheroplasms[0].resolve);
    }
}
