namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class MustardBomb(BossModule module) : Components.UniformStackSpread(module, default, 6f)
{
    public enum Mechanic { None, Tethers, Nisi, Done }

    public Mechanic CurMechanic;
    private Actor? _tetherTarget;
    private BitMask _bombTargets;
    private BitMask _forbidden;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        switch (CurMechanic)
        {
            case Mechanic.Tethers:
                var wantTether = actor.Role == Role.Tank;
                var hasTether = _bombTargets[slot];
                if (wantTether != hasTether)
                    hints.Add(wantTether ? "Grab tether!" : "Pass tether!");
                break;
            case Mechanic.Nisi:
                var wantNisi = !_forbidden[slot];
                var hasNisi = _bombTargets[slot];
                if (wantNisi != hasNisi)
                    hints.Add(wantNisi ? "Grab nisi!" : "Pass nisi!");
                break;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => CurMechanic switch
    {
        Mechanic.Tethers => PlayerPriority.Normal,
        Mechanic.Nisi => _bombTargets[playerSlot] || !_forbidden[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal,
        _ => PlayerPriority.Irrelevant
    };

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurMechanic == Mechanic.Tethers && _tetherTarget != null)
            foreach (var (_, p) in Raid.WithSlot(false, true, true).IncludedInMask(_bombTargets))
                Arena.AddLine(_tetherTarget.Position, p.Position);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.MustardBomb)
        {
            _bombTargets.Set(Raid.FindSlot(source.InstanceID));
            if (CurMechanic == Mechanic.None)
            {
                CurMechanic = Mechanic.Tethers;
                _tetherTarget = WorldState.Actors.Find(tether.Target);
                AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(8.8d));
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.MustardBomb)
            _bombTargets[Raid.FindSlot(source.InstanceID)] = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MustardBomb)
        {
            _bombTargets[Raid.FindSlot(actor.InstanceID)] = true;
            AddSpread(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MustardBomb)
        {
            _bombTargets[Raid.FindSlot(actor.InstanceID)] = false;
            var count = Spreads.Count;
            for (var i = 0; i < count; ++i)
            {
                if (Spreads[i].Target == actor)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MustardBombFirst:
            case (uint)AID.KindlingCauldron:
                CurMechanic = Mechanic.Nisi;
                Spreads.Clear();
                _forbidden[Raid.FindSlot(spell.MainTargetID)] = true;
                break;
            case (uint)AID.MustardBombSecond:
                CurMechanic = Mechanic.Done;
                Spreads.Clear();
                break;
        }
    }
}
