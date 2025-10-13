namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P1CyclonicBreakSpreadStack(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 2, 2)
{
    public DateTime Activation = DateTime.MaxValue;
    private bool _fullHints; // we only need to actually stack/spread after first protean bait

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_fullHints)
        {
            base.AddHints(slot, actor, hints);
        }
        else if (Stacks.Count > 0)
        {
            hints.Add("Prepare to stack", false);
        }
        else if (Spreads.Count > 0)
        {
            hints.Add("Prepare to spread", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by dedicated component

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CyclonicBreakBossStack:
            case (uint)AID.CyclonicBreakImageStack:
                // TODO: this can target either supports or dd
                Activation = Module.CastFinishAt(spell, 2.7d);
                AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Class.IsSupport()), Activation);
                break;
            case (uint)AID.CyclonicBreakBossSpread:
            case (uint)AID.CyclonicBreakImageSpread:
                Activation = Module.CastFinishAt(spell, 2.7d);
                AddSpreads(Raid.WithoutSlot(true, true, true), Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CyclonicBreakSinsmoke:
                Stacks.Clear();
                break;
            case (uint)AID.CyclonicBreakSinsmite:
                Spreads.Clear();
                break;
            case (uint)AID.CyclonicBreakAOEFirst:
                _fullHints = true;
                break;
        }
    }
}

sealed class P1CyclonicBreakProtean(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, P1CyclonicBreakCone.Shape, (uint)AID.CyclonicBreakAOEFirst)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by dedicated component
}

sealed class P1CyclonicBreakCone(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private DateTime _currentBundle;

    public static readonly AOEShapeCone Shape = new(60f, 11.5f.Degrees()); // TODO: verify angle

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by dedicated component

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CyclonicBreakAOEFirst:
                AOEs.Add(new(Shape, caster.Position.Quantized(), spell.Rotation, WorldState.FutureTime(2d)));
                break;
            case (uint)AID.CyclonicBreakAOERest:
                if (WorldState.CurrentTime > _currentBundle)
                {
                    _currentBundle = WorldState.CurrentTime.AddSeconds(1d);
                    ++NumCasts;
                    foreach (ref var aoe in AOEs.AsSpan())
                        aoe.Rotation -= 22.5f.Degrees();
                }
                break;
        }
    }
}

sealed class P1CyclonicBreakAIBait(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P1CyclonicBreakSpreadStack? _spreadStack = module.FindComponent<P1CyclonicBreakSpreadStack>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockspot = _config.P1CyclonicBreakSpots[assignment];
        if (clockspot < 0 || _spreadStack == null || !_spreadStack.Active)
            return; // no assignment
        var assignedDirection = (180f - 45f * clockspot).Degrees();
        // TODO: think about melee vs ranged distance...
        hints.AddForbiddenZone(new SDInvertedRect(Module.PrimaryActor.Position, assignedDirection, 15f, -5f, 1f), _spreadStack.Activation);
    }
}

sealed class P1CyclonicBreakAIDodgeSpreadStack(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P1CyclonicBreakSpreadStack? _spreadStack = module.FindComponent<P1CyclonicBreakSpreadStack>();
    private readonly P1CyclonicBreakCone? _cones = module.FindComponent<P1CyclonicBreakCone>();
    private readonly ArcList _forbiddenDirections = new(module.PrimaryActor.Position, 0);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockspot = _config.P1CyclonicBreakSpots[assignment];
        if (clockspot < 0 || _cones == null || _spreadStack == null || !_spreadStack.Active)
            return;

        _forbiddenDirections.Forbidden.Clear();
        var aoes = CollectionsMarshal.AsSpan(_cones.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            _forbiddenDirections.ForbidArcByLength(aoes[i].Rotation, P1CyclonicBreakCone.Shape.HalfAngle);
        }

        var isSupport = actor.Class.IsSupport();
        var dodgeCCW = _spreadStack.Stacks.Count > 0 ? _config.P1CyclonicBreakStackSupportsCCW == isSupport : isSupport ? _config.P1CyclonicBreakSpreadSupportsCCW : _config.P1CyclonicBreakSpreadDDCCW;
        var assignedDirection = (180f - 45f * clockspot).Degrees();
        var safeAngles = _forbiddenDirections.NextAllowed(assignedDirection, dodgeCCW);
        var (rangeMin, rangeMax) = _spreadStack.Stacks.Count > 0 ? (4, 10) : assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 ? (3, 6) : (7, 15);

        hints.AddForbiddenZone(new SDInvertedDonutSector(_forbiddenDirections.Center, rangeMin, rangeMax, (safeAngles.min + safeAngles.max) * 0.5f, (safeAngles.max - safeAngles.min) * 0.5f), _spreadStack.Activation);

        // micro adjusts if activation is imminent
        if (_spreadStack.Activation < WorldState.FutureTime(0.5f))
        {
            if (_spreadStack.Stacks.Count > 0)
            {
                var closestPartner = Module.Raid.WithoutSlot(false, true, true).Where(p => p.Class.IsSupport() != isSupport).Closest(actor.Position);
                if (closestPartner != null)
                    hints.AddForbiddenZone(new SDInvertedCircle(closestPartner.Position, _spreadStack.StackRadius), _spreadStack.Activation);
            }
            else
            {
                foreach (var p in Raid.WithoutSlot(false, true, true).Exclude(actor))
                    hints.AddForbiddenZone(new SDCircle(p.Position, _spreadStack.SpreadRadius), _spreadStack.Activation);
            }
        }
    }
}

sealed class P1CyclonicBreakAIDodgeRest(BossModule module) : BossComponent(module)
{
    private readonly P1CyclonicBreakCone? _cones = module.FindComponent<P1CyclonicBreakCone>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_cones != null)
        {
            var aoes = CollectionsMarshal.AsSpan(_cones.AOEs);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                hints.AddForbiddenZone(aoe.Shape.Distance(aoe.Origin, aoe.Rotation), aoe.Activation);
            }
        }
    }
}
