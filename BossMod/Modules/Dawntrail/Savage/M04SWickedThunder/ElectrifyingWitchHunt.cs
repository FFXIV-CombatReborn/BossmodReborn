namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class ElectrifyingWitchHuntBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrifyingWitchHuntBurst, new AOEShapeRect(40f, 8f));

sealed class ElectrifyingWitchHuntSpread(BossModule module) : Components.UniformStackSpread(module, default, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrifyingWitchHunt)
            AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell, 0.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrifyingWitchHuntAOE)
            Spreads.Clear();
    }
}

sealed class ElectrifyingWitchHuntResolve(BossModule module) : Components.GenericStackSpread(module)
{
    public enum Mechanic { None, Near, Far }

    public Mechanic CurMechanic;
    public BitMask ForbidBait;
    private BitMask _baits;
    private DateTime _activation;

    // mask of the 4 players closest to (or farthest from) the arena center
    private BitMask RangeMask(bool farthest)
    {
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        Span<(int slot, float distSq)> sorted = stackalloc (int, float)[len];
        var center = Arena.Center;
        for (var i = 0; i < len; ++i)
        {
            sorted[i] = (party[i].Item1, (party[i].Item2.Position - center).LengthSq());
        }
        sorted.Sort(static (a, b) => a.distSq.CompareTo(b.distSq));
        BitMask mask = default;
        var take = len < 4 ? len : 4;
        for (var i = 0; i < take; ++i)
        {
            mask.Set(sorted[farthest ? len - 1 - i : i].slot);
        }
        return mask;
    }

    public override void Update()
    {
        _baits = CurMechanic switch
        {
            Mechanic.Near => RangeMask(false),
            Mechanic.Far => RangeMask(true),
            _ => default
        };

        Spreads.Clear();
        foreach (var (i, p) in Raid.WithSlot(true, true, true))
        {
            if (ForbidBait[i])
                Spreads.Add(new(p, 5, _activation));
            if (_baits[i])
                Spreads.Add(new(p, 6, _activation));
        }

        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurMechanic != Mechanic.None)
        {
            var wantBait = !ForbidBait[slot];
            var baitNear = CurMechanic == Mechanic.Near;
            hints.Add(wantBait == baitNear ? "Go near!" : "Go far!", _baits[slot] == ForbidBait[slot]);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ForkedLightning:
                ForbidBait.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.Marker:
                CurMechanic = status.Extra switch
                {
                    0x2F6 => Mechanic.Near,
                    0x2F7 => Mechanic.Far,
                    _ => Mechanic.None
                };
                _activation = status.ExpireAt;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ElectrifyingWitchHuntBait:
                CurMechanic = Mechanic.None;
                break;
            case (uint)AID.ForkedLightning:
                ForbidBait = default;
                break;
        }
    }
}
