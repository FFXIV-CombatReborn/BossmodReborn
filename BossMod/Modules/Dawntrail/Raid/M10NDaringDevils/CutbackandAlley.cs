namespace BossMod.Dawntrail.Raid.M10NDaringDevils;

sealed class CutbackBlazeBait(BossModule module) : Components.BaitAwayIcon(
    module,
    new AOEShapeCone(60f, 30f.Degrees()),                 // tune angle if needed
    (uint)IconID.CutbackBlazeBait,
    (uint)AID.CutbackBlaze1,
    activationDelay: 5.0d,
    centerAtTarget: false)
{
    public static readonly AOEShapeCone Cone = new(60f, 30f.Degrees());
}

sealed class CutbackBlazePersistent(BossModule module) : TrackedVoidzones(module, CutbackBlazeBait.Cone, (uint)OID.CutbackBlazeVoidzone)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.CutbackBlaze1)
        {
            Predict(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }
}
sealed class AlleyOopInfernoSpread(BossModule module)
    : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno1, 5f);

sealed class AlleyOopInfernoPuddles(BossModule module) : TrackedVoidzones(module, new AOEShapeCircle(5f), (uint)OID.InfernoVoidzone)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.AlleyOopInferno1)
        {
            var targets = spell.Targets;
            var count = targets.Count;
            for (var i = 0; i < count; ++i)
            {
                if (WorldState.Actors.Find(targets[i].ID) is Actor target)
                {
                    Predict(target.Position, default, WorldState.CurrentTime);
                }
            }
        }
    }
}

sealed class AlleyOopMaelstromSequential(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Tracked> _tracked = [];
    private readonly List<AOEInstance> _active = [];
    private static readonly AOEShapeCone Shape30 = new(60f, 15f.Degrees());
    private static readonly AOEShapeCone Shape15 = new(60f, 7.5f.Degrees());

    private struct Tracked(AOEInstance aoe, uint aid)
    {
        public AOEInstance AOE = aoe;
        public uint AID = aid;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();

        var have30 = false;
        foreach (var t in _tracked)
        {
            if (t.AID == (uint)AID.AlleyOopMaelstrom)
            {
                have30 = true;
                break;
            }
        }

        var want = have30 ? (uint)AID.AlleyOopMaelstrom : (uint)AID.AlleyOopMaelstrom2;

        foreach (var t in _tracked)
        {
            if (t.AID == want)
                _active.Add(t.AOE);
        }

        return CollectionsMarshal.AsSpan(_active);
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
        {
            Remove(caster.InstanceID, spell.Action.ID);
        }
    }

    private void Remove(ulong actorID, uint aid = default)
    {
        for (var i = _tracked.Count - 1; i >= 0; --i)
        {
            var t = _tracked[i];
            if (t.AOE.ActorID == actorID && (aid == default || t.AID == aid))
            {
                _tracked.RemoveAt(i);
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AlleyOopMaelstrom => Shape30,   // 46495
            (uint)AID.AlleyOopMaelstrom2 => Shape15,  // 46496
            _ => null
        };
        if (shape == null)
            return;

        var activation = WorldState.FutureTime(spell.NPCRemainingTime);
        var aoe = new AOEInstance(shape, caster.Position, spell.Rotation, activation, Colors.AOE, true, caster.InstanceID);
        _tracked.Add(new(aoe, spell.Action.ID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopMaelstrom or (uint)AID.AlleyOopMaelstrom2)
            Remove(caster.InstanceID, spell.Action.ID);
    }

    public override void OnActorDestroyed(Actor actor) => Remove(actor.InstanceID);
}
