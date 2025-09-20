namespace BossMod.Endwalker.Alliance.A14Naldthal;

class FortuneFluxOrder(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, AOE, Knockback }

    public List<(WPos source, Mechanic mechanic, DateTime activation, Angle rotation)> Mechanics = new(3);
    public int NumComplete;
    private Actor? _currentTethered;
    private Mechanic _currentMechanic;
    private DateTime activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orderBuilder = new StringBuilder();
        var count = Mechanics.Count;
        for (var i = NumComplete; i < count; ++i)
        {
            if (i > NumComplete)
                orderBuilder.Append(" > ");
            orderBuilder.Append(Mechanics[i].mechanic);
        }

        var order = orderBuilder.ToString();
        if (order.Length != 0)
            hints.Add($"Order: {order}");
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.FiredUp)
        {
            _currentTethered = source;
            TryAdd();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FiredUp1AOE:
            case (uint)AID.FiredUp2AOE:
            case (uint)AID.FiredUp3AOE:
                _currentMechanic = Mechanic.AOE;
                TryAdd();
                break;
            case (uint)AID.FiredUp1Knockback:
            case (uint)AID.FiredUp2Knockback:
            case (uint)AID.FiredUp3Knockback:
                _currentMechanic = Mechanic.Knockback;
                TryAdd();
                break;
            case (uint)AID.FortuneFluxAOE1:
                UpdateActivation(0, Mechanic.AOE, spell);
                break;
            case (uint)AID.FortuneFluxAOE2:
                UpdateActivation(1, Mechanic.AOE, spell);
                break;
            case (uint)AID.FortuneFluxAOE3:
                UpdateActivation(2, Mechanic.AOE, spell);
                break;
            case (uint)AID.FortuneFluxKnockback1:
                UpdateActivation(0, Mechanic.Knockback, spell);
                break;
            case (uint)AID.FortuneFluxKnockback2:
                UpdateActivation(1, Mechanic.Knockback, spell);
                break;
            case (uint)AID.FortuneFluxKnockback3:
                UpdateActivation(2, Mechanic.Knockback, spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FortuneFluxAOE1 or (uint)AID.FortuneFluxAOE2 or (uint)AID.FortuneFluxAOE3 or (uint)AID.FortuneFluxKnockback1
        or (uint)AID.FortuneFluxKnockback2 or (uint)AID.FortuneFluxKnockback3)
            ++NumComplete;
    }

    private void TryAdd()
    {
        if (_currentTethered != null && _currentMechanic != Mechanic.None)
        {
            if (activation == default)
                activation = WorldState.FutureTime(100d);
            Mechanics.Add((_currentTethered.Position, _currentMechanic, activation, _currentTethered.Rotation));
            _currentTethered = null;
            _currentMechanic = Mechanic.None;
        }
    }

    private void UpdateActivation(int order, Mechanic mechanic, ActorCastInfo spell)
    {
        if (order >= Mechanics.Count)
        {
            ReportError($"Unexpected mechanic #{order}, only {Mechanics.Count} in list");
            return;
        }

        ref var m = ref Mechanics.AsSpan()[order];
        if (m.mechanic != mechanic)
        {
            ReportError($"Unexpected mechanic #{order}: started {mechanic}, expected {m.mechanic}");
            m.mechanic = mechanic;
        }

        if (!m.source.AlmostEqual(spell.LocXZ, 0.5f))
        {
            ReportError($"Unexpected mechanic #{order} position: started {spell.LocXZ}, expected {m.source}");
            m.source = spell.LocXZ;
        }

        if (m.activation != activation)
        {
            ReportError($"Several cast-start for #{order}");
        }
        m.activation = Module.CastFinishAt(spell);
    }
}

class FortuneFluxAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly FortuneFluxOrder? _order = module.FindComponent<FortuneFluxOrder>();

    private static readonly AOEShapeCircle _shape = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {

        if (_order == null || _order.Mechanics.Count <= _order.NumComplete)
            return [];
        var count = _order.Mechanics.Count;
        var validCount = 0;
        for (var i = _order.NumComplete; i < count; ++i)
        {
            if (_order.Mechanics[i].mechanic == FortuneFluxOrder.Mechanic.AOE)
                ++validCount;
        }

        if (validCount == 0)
            return [];

        var aoes = new AOEInstance[validCount];
        var index = 0;

        for (var i = _order.NumComplete; i < count; ++i)
        {
            var m = _order.Mechanics[i];
            if (m.mechanic == FortuneFluxOrder.Mechanic.AOE)
                aoes[index++] = new(_shape, m.source, default, m.activation);
        }
        return aoes;
    }
}

class FortuneFluxKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly FortuneFluxOrder? _order = module.FindComponent<FortuneFluxOrder>();
    private static readonly Angle a45 = 45f.Degrees(), a180 = 180f.Degrees();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_order == null || _order.Mechanics.Count <= _order.NumComplete)
            return [];

        var mechanics = _order.Mechanics;
        var countM = mechanics.Count;

        var count = 0;

        for (var i = _order.NumComplete; i < countM; ++i)
        {
            if (mechanics[i].mechanic == FortuneFluxOrder.Mechanic.Knockback)
                ++count;
        }

        if (count == 0)
            return [];

        var sources = new Knockback[count];
        var index = 0;

        for (var i = _order.NumComplete; i < countM; ++i)
        {
            var m = _order.Mechanics[i];
            if (m.mechanic == FortuneFluxOrder.Mechanic.Knockback)
                sources[index++] = new(m.source, 30f, m.activation);
        }

        return sources;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_order == null || _order.Mechanics.Count <= _order.NumComplete)
            return;
        var mechanics = _order.Mechanics;
        var countM = mechanics.Count;
        for (var i = _order.NumComplete; i < countM; ++i)
        {
            var m = _order.Mechanics[i];
            if (m.mechanic == FortuneFluxOrder.Mechanic.Knockback)
            {
                var act = m.activation;
                if (!IsImmune(slot, act))
                {
                    hints.AddForbiddenZone(new SDInvertedCone(m.source, 5f, m.rotation + a180, a45), act);
                    for (var j = _order.NumComplete; j < countM; ++j)
                    {
                        var mj = _order.Mechanics[j];
                        if (mj.mechanic == FortuneFluxOrder.Mechanic.AOE)
                        {
                            hints.AddForbiddenZone(new SDCone(m.source, 100f, Angle.FromDirection(m.source - mj.source), Angle.Asin(20f / (mj.source - m.source).Length())), act.AddSeconds(1d));
                        }
                    }
                    return;
                }
            }
        }
    }
}
