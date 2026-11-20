namespace BossMod.Dawntrail.Savage.M01SBlackCat;

sealed class RainingCatsTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(100f, 35f.Degrees()), (uint)TetherID.RainingCats, (uint)AID.RainingCatsTether) // TODO: verify angle
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Module.Raid.FindSlot(t.ID));
        }
    }
}

sealed class RainingCatsStack(BossModule module) : Components.UniformStackSpread(module, 4f, default, 3)
{
    private readonly RainingCatsTether? _tether = module.FindComponent<RainingCatsTether>();
    private DateTime _activation;

    public override void Update()
    {
        Stacks.Clear();
        if (_activation != default)
        {
            var tetherTargets = new BitMask();
            if (_tether != null)
                foreach (var t in _tether.CurrentBaits)
                    tetherTargets.Set(Module.Raid.FindSlot(t.Target.InstanceID));

            Actor? closest = null, farthest = null;
            float minDistSq = float.MaxValue, maxDistSq = float.MinValue;
            var origin = Module.PrimaryActor.Position;
            var party = Module.Raid.WithoutSlot(false, true, true);
            for (var i = 0; i < party.Length; ++i)
            {
                var p = party[i];
                var distSq = (p.Position - origin).LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = p;
                }
                if (distSq > maxDistSq)
                {
                    maxDistSq = distSq;
                    farthest = p;
                }
            }
            if (closest != null)
                AddStack(closest, _activation, tetherTargets);
            if (farthest != null)
                AddStack(farthest, _activation, tetherTargets);
        }
        base.Update();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RainingCatsFirst or (uint)AID.RainingCatsMid or (uint)AID.RainingCatsLast)
            _activation = Module.CastFinishAt(spell, 0.8f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RainingCatsStack)
            _activation = default;
    }
}
