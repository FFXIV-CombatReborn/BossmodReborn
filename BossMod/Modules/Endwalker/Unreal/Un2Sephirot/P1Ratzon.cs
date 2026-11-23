namespace BossMod.Endwalker.Unreal.Un2Sephirot;

// TODO: generalize
sealed class P1Ratzon(BossModule module) : BossComponent(module)
{
    private BitMask _greenTargets;
    private BitMask _purpleTargets;

    private const float _greenRadius = 5f;
    private const float _purpleRadius = 10f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((_greenTargets | _purpleTargets).None())
            return;

        var clippedByGreen = Raid.WithSlot(false, true, true).Exclude(slot).IncludedInMask(_greenTargets).InRadius(actor.Position, _greenRadius).Any();
        var clippedByPurple = Raid.WithSlot(false, true, true).Exclude(slot).IncludedInMask(_purpleTargets).InRadius(actor.Position, _purpleRadius).Any();
        hints.Add($"Spread! (debuff: {(_greenTargets[slot] ? "green" : _purpleTargets[slot] ? "purple" : "none")})", clippedByGreen || clippedByPurple);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return (_greenTargets | _purpleTargets)[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (slot, actor) in Raid.WithSlot(false, true, true))
            if (_greenTargets[slot])
                Arena.ZoneCircleOutline(actor.Position, _greenRadius, Colors.Safe, slot == pcSlot ? 2 : 1);
        foreach (var (slot, actor) in Raid.WithSlot(false, true, true))
            if (_purpleTargets[slot])
                Arena.ZoneCircleOutline(actor.Position, _purpleRadius, Colors.Vulnerable, slot == pcSlot ? 2 : 1);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RatzonAOEGreen:
                _greenTargets.Clear(Raid.FindSlot(spell.MainTargetID));
                break;
            case (uint)AID.RatzonAOEPurple:
                _purpleTargets.Clear(Raid.FindSlot(spell.MainTargetID));
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.RatzonGreen:
                _greenTargets.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)IconID.RatzonPurple:
                _purpleTargets.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}
