namespace BossMod.Endwalker.Savage.P9SKokytos;

sealed class DualspellFire(BossModule module) : Components.GenericStackSpread(module)
{
    private bool _active;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_active)
            hints.Add("Pairs");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DualspellIceFire or (uint)AID.TwoMindsIceFire)
            _active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var radius = spell.Action.ID switch
        {
            (uint)AID.DualspellVisualIce => 6f,
            (uint)AID.DualspellVisualFire => 12f,
            _ => 0f
        };
        if (_active && radius != 0)
        {
            // assume always targets supports
            foreach (var p in Raid.WithoutSlot(true, true, true))
                if (p.Class.IsSupport())
                    Stacks.Add(new(p, radius, 2, 2, WorldState.FutureTime(4.5f)));
        }
    }
}

sealed class DualspellLightning(BossModule module) : Components.GenericBaitAway(module)
{
    private bool _active;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_active)
            hints.Add("Spread");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DualspellIceLightning or (uint)AID.TwoMindsIceLightning)
            _active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var halfWidth = spell.Action.ID switch
        {
            (uint)AID.DualspellVisualIce => 4,
            (uint)AID.DualspellVisualLightning => 8,
            _ => 0
        };
        if (_active && halfWidth != 0)
        {
            var shape = new AOEShapeRect(40f, halfWidth);
            foreach (var p in Raid.WithoutSlot(true, true, true))
                CurrentBaits.Add(new(Module.PrimaryActor, p, shape));
        }
    }
}

sealed class DualspellIce(BossModule module) : Components.GenericAOEs(module)
{
    public enum Mechanic { None, In, Out };

    private Mechanic _curMechanic;
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_curMechanic != Mechanic.None)
        {
            hints.Add(_curMechanic.ToString());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DualspellVisualIce:
                SetMechanic(Mechanic.In);
                break;
            case (uint)AID.DualspellVisualFire:
            case (uint)AID.DualspellVisualLightning:
                SetMechanic(Mechanic.Out);
                break;
            case (uint)AID.DualspellBlizzardOut:
            case (uint)AID.DualspellBlizzardIn:
                ++NumCasts;
                break;
        }
    }

    private void SetMechanic(Mechanic mechanic)
    {
        _curMechanic = mechanic;
        _aoe = [new(new AOEShapeDonut(mechanic == Mechanic.In ? 8f : 14, 40f), Module.PrimaryActor.Position, default, WorldState.FutureTime(4.5d))];
    }
}
