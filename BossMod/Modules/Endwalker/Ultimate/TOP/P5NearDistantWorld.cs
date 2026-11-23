namespace BossMod.Endwalker.Ultimate.TOP;

abstract class NearDistantWorld(BossModule module) : Components.GenericStackSpread(module, true)
{
    public int NumNearJumpsDone;
    public int NumDistantJumpsDone;
    public Actor? NearWorld;
    public Actor? DistantWorld;
    private BitMask _completedJumps;
    private BitMask _targets;
    private BitMask _risky;
    private DateTime _firstActivation;

    public override void Update()
    {
        Spreads.Clear();
        _risky.Reset();
        _targets = _completedJumps;
        AddChain(NearWorld, NumNearJumpsDone, true);
        AddChain(DistantWorld, NumDistantJumpsDone, false);

        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_risky[slot])
            hints.Add("Avoid baiting jump!");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.HelloNearWorld:
                NearWorld = actor;
                _firstActivation = status.ExpireAt;
                break;
            case (uint)SID.HelloDistantWorld:
                DistantWorld = actor;
                _firstActivation = status.ExpireAt;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HelloNearWorld:
            case (uint)AID.HelloNearWorldJump:
                ++NumNearJumpsDone;
                var nearSlot = Raid.FindSlot(spell.MainTargetID);
                _completedJumps.Set(nearSlot);
                NearWorld = Raid[nearSlot];
                break;
            case (uint)AID.HelloDistantWorld:
            case (uint)AID.HelloDistantWorldJump:
                ++NumDistantJumpsDone;
                var distantSlot = Raid.FindSlot(spell.MainTargetID);
                _completedJumps.Set(distantSlot);
                DistantWorld = Raid[distantSlot];
                break;
        }
    }

    protected void Reset(Actor? near, Actor? distant, DateTime activation)
    {
        NumNearJumpsDone = NumDistantJumpsDone = 0;
        NearWorld = near;
        DistantWorld = distant;
        _completedJumps.Reset();
        _firstActivation = activation;
    }

    private void AddChain(Actor? start, int numDone, bool close)
    {
        if (numDone == 0)
        {
            if (start != null)
                AddSpread(start, 8f, 0);
        }
        if (numDone <= 1 && start != null)
        {
            start = close ? Raid.WithoutSlot(false, true, true).ClosestTo(start.Position, start) : Raid.WithoutSlot(false, true, true).FarthestFrom(start.Position, start);
            if (start != null)
                AddSpread(start, 4f, 1);
        }
        if (numDone <= 2 && start != null)
        {
            start = close ? Raid.WithoutSlot(false, true, true).ClosestTo(start.Position, start) : Raid.WithoutSlot(false, true, true).FarthestFrom(start.Position, start);
            if (start != null)
                AddSpread(start, 4f, 2);
        }
    }

    private void AddSpread(Actor target, float radius, int order)
    {
        Spreads.Add(new(target, radius, _firstActivation.AddSeconds(order)));
        var slot = Raid.FindSlot(target.InstanceID);
        if (_targets[slot])
            _risky.Set(slot);
        else
            _targets.Set(slot);
    }
}

sealed class P5NearDistantWorld(BossModule module) : NearDistantWorld(module) { }
