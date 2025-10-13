namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P2OptimizedSagittariusArrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimizedSagittariusArrow, new AOEShapeRect(100f, 5f));

sealed class P2OptimizedBladedance : Components.BaitAwayTethers
{
    public P2OptimizedBladedance(BossModule module) : base(module, new AOEShapeCone(100f, 45f.Degrees()), (uint)TetherID.OptimizedBladedance, (uint)AID.OptimizedBladedanceAOE)
    {
        ForbiddenPlayers = Raid.WithSlot(true, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
    }
}

sealed class P2BeyondDefense(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 3)
{
    public enum Mechanic { None, Spread, Stack }

    public Mechanic CurMechanic;
    private Actor? _source;
    private DateTime _activation;
    private BitMask _forbiddenStack;

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();
        if (_source != null)
        {
            switch (CurMechanic)
            {
                case Mechanic.Spread:
                    AddSpreads(Raid.WithoutSlot(false, true, true).SortedByRange(_source.Position).Take(2), _activation);
                    break;
                case Mechanic.Stack:
                    if (Raid.WithoutSlot(false, true, true).Closest(_source.Position) is var target && target != null)
                        AddStack(target, _activation, _forbiddenStack);
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_source, Colors.Object, true);
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SyntheticShield:
                _source = caster;
                break;
            case (uint)AID.BeyondDefense:
                _source = caster;
                CurMechanic = Mechanic.Spread;
                _activation = Module.CastFinishAt(spell, 0.2d);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BeyondDefenseAOE:
                foreach (var t in spell.Targets)
                    _forbiddenStack[Raid.FindSlot(t.ID)] = true;
                CurMechanic = Mechanic.Stack;
                _activation = WorldState.FutureTime(3.2d);
                break;
            case (uint)AID.PilePitch:
                CurMechanic = Mechanic.None;
                break;
        }
    }
}

sealed class P2CosmoMemory(BossModule module) : Components.CastCounter(module, (uint)AID.CosmoMemoryAOE);

sealed class P2OptimizedPassageOfArms(BossModule module) : BossComponent(module)
{
    public Actor? _invincible;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var e = hints.FindEnemy(_invincible);
        e?.Priority = AIHints.Enemy.PriorityInvincible;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Invincibility && actor.OID == (uint)OID.OmegaM)
            _invincible = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Invincibility && _invincible == actor)
            _invincible = null;
    }
}
