namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreathCast, 14f)
{
    // on visual cast since there are x2 instances of actual knockback
    // if happening during hissing reprise, handling drawing/hints there so arrows follow order
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(new(Arena.Center, Distance, Module.CastFinishAt(spell), Shape, spell.Rotation, KnockbackKind, 0, [], caster.InstanceID, IgnoreImmunes));
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var hissing = Module.FindComponent<HissingReprise>();
        if (hissing == null || hissing.ActiveKnockbacks(pcSlot, pc).Length == 0)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var hissing = Module.FindComponent<HissingReprise>();
        if (hissing == null || hissing.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var hissing = Module.FindComponent<HissingReprise>();
        if (hissing == null || hissing.ActiveKnockbacks(slot, actor).Length == 0)
        {
            var kbs = ActiveKnockbacks(slot, actor);
            if (kbs.Length != 0)
            {
                var kb = kbs[0];
                var act = kb.Activation;
                if (!IsImmune(slot, act))
                {
                    // slightly larger to avoid sus knockback
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, kb.Origin, 19f, 20f), act);
                }
            }
        }
    }
}
sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempestVisual)
{
    // don't show raidwide hint twice
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Casters.Count == 0)
        {
            base.OnCastStarted(caster, spell);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Casters.Count != 0)
        {
            base.OnCastFinished(caster, spell);
        }
    }
}
sealed class TwoTerrors(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors, new AOEShapeRect(40f, 5f));

sealed class LightningIcePoison(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PoisonBreath or (uint)AID.IceCluster or (uint)AID.LightningCluster)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            AOEShapeCircle shape = spell.Action.ID switch
            {
                (uint)AID.PoisonBreath => new(18f),
                _ => new(15f)
            };
            aoes.Add(new(shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(origin, rotation)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PoisonBreath or (uint)AID.IceCluster or (uint)AID.LightningCluster)
        {
            var count = aoes.Count;
            var id = caster.InstanceID;
            var aoespan = CollectionsMarshal.AsSpan(aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoespan[i].ActorID == id)
                {
                    aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var knockbacks = Module.FindComponent<HissingReprise>();
        if (knockbacks == null || knockbacks.ActiveKnockbacks(slot, actor).Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            if (aoes.Count != 0)
            {
                // avoid standing on edge to look less sus
                hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, 0f.Degrees(), 18f, 18f, 18f));
            }
        }
    }
}

sealed class BlazeLoop(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        aoe0.Color = Colors.Danger;
        aoe0.Risky = true;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Blaze1 or (uint)AID.Blaze2 or (uint)AID.Blaze3)
        {
            var act = Module.CastFinishAt(spell);
            _aoes.Add(new(new AOEShapeCircle(5f), spell.LocXZ, activation: act, risky: false));
            _aoes.Add(new(new AOEShapeDonut(5f, 60f), spell.LocXZ, activation: act.AddSeconds(2.5d), risky: false));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Blaze1:
            case (uint)AID.Blaze2:
            case (uint)AID.Blaze3:
            case (uint)AID.Blazeloop:
                _aoes.RemoveAt(0);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // try to position near origin for better dodging
        var active = ActiveAOEs(slot, actor);
        var count = active.Length;
        if (count != 0)
        {
            var aoe = active[0];
            hints.AddForbiddenZone(new AOEShapeDonut(7f, 60f), aoe.Origin, activation: aoe.Activation);
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class ArcaneBeacon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcaneBeacon, new AOEShapeRect(60f, 2.5f), 8);
sealed class Archaeofury1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Archaeofury1, 6f);
sealed class Archaeofury2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Archaeofury2, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.GreenHead, Contributors = "Equilius + gynorhino", GroupType = BossModuleInfo.GroupType.TheForkedTowerMagicNormal, GroupID = 1093u, NameID = 14489u)]
public sealed class FTMN1TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f))
{
    private Actor? _blueHead;
    public Actor? BlueHead()
    {
        return _blueHead;
    }

    protected override void UpdatePreModuleActivation()
    {
        _blueHead ??= GetActor((uint)OID.BlueHead);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_blueHead);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InSquare(Arena.Center, 20f);
}
