namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class SeaShackles(BossModule module) : Components.StretchTetherDuo(module, minimumDistance: 16f, activationDelay: 28d, tetherIDBad: (uint)TetherID.Far, tetherIDGood: (uint)TetherID.Safe)
{
    public bool Active => CurrentBaits.Count != 0;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (!IsShackle(tether.ID) || WorldState.Actors.Find(tether.Target) is not Actor partner || Raid.FindSlot(source.InstanceID) < 0 || Raid.FindSlot(partner.InstanceID) < 0)
        {
            return;
        }

        var activation = default(DateTime);
        var count = ActivationDelayOnActor.Count;
        for (var i = 0; i < count; ++i)
        {
            if (ActivationDelayOnActor[i].Item1 == source)
            {
                activation = ActivationDelayOnActor[i].Item2;
                break;
            }
        }
        if (activation == default)
        {
            activation = WorldState.FutureTime(ActivationDelay);
            ActivationDelayOnActor.Add((source, activation));
        }

        CurrentBaits.Add(new(partner, source, Shape ?? new AOEShapeCircle(default), activation));
        TetherOnActor.Add((source, tether.ID));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (!IsShackle(tether.ID))
        {
            return;
        }
        var partnerID = tether.Target;
        for (var i = CurrentBaits.Count - 1; i >= 0; --i)
        {
            var b = CurrentBaits[i];
            if (b.Target == source && b.Source.InstanceID == partnerID)
            {
                CurrentBaits.RemoveAt(i);
            }
        }
        var tetherID = tether.ID;
        for (var i = TetherOnActor.Count - 1; i >= 0; --i)
        {
            var t = TetherOnActor[i];
            if (t.Item1 == source && t.Item2 == tetherID)
            {
                TetherOnActor.RemoveAt(i);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurrentBaits.Count <= 1)
        {
            return;
        }
        base.DrawArenaForeground(pcSlot, pc);
        if (FindBait(pc, out var partner, out var tetherID))
        {
            Arena.AddLine(pc.Position, partner.Position, tetherID == (uint)TetherID.Safe ? Colors.Safe : Colors.Danger);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (CurrentBaits.Count <= 1)
        {
            return PlayerPriority.Irrelevant;
        }
        if (player == pc)
        {
            return PlayerPriority.Normal;
        }
        return FindBait(pc, out var partner, out _) && partner == player ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count <= 1 || !FindBait(actor, out _, out var tetherID))
        {
            return;
        }
        switch (tetherID)
        {
            case (uint)TetherID.Safe:
                hints.Add("Tether distance is correct", false);
                break;
            case (uint)TetherID.Far:
                hints.Add("Move closer to tether partner!");
                break;
            case (uint)TetherID.Near:
                hints.Add("Move away from tether partner!");
                break;
        }
    }

    private static bool IsShackle(uint tetherID) => tetherID is (uint)TetherID.Safe or (uint)TetherID.Far or (uint)TetherID.Near;

    private bool FindBait(Actor actor, out Actor partner, out uint tetherID)
    {
        var count = CurrentBaits.Count;
        for (var i = 0; i < count; ++i)
        {
            var b = CurrentBaits[i];
            if (b.Source == actor || b.Target == actor)
            {
                partner = b.Target == actor ? b.Source : b.Target;
                tetherID = default;
                var tethers = TetherOnActor.Count;
                for (var j = 0; j < tethers; ++j)
                {
                    var t = TetherOnActor[j];
                    if (t.Item1 == b.Target || t.Item1 == b.Source)
                    {
                        tetherID = t.Item2;
                        break;
                    }
                }
                return true;
            }
        }
        partner = actor;
        tetherID = default;
        return false;
    }
}

sealed class HydrobulletStack(BossModule module) : Components.UniformStackSpread(module, 15f, default, 2, 2)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HydrobulletTarget)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrobullet2)
        {
            Stacks.Clear();
        }
    }
}
