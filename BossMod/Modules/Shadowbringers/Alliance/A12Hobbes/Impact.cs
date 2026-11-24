namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

sealed class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, 15f);

sealed class Towerfall(BossModule module) : Components.GenericAOEs(module, (uint)AID.Towerfall)
{
    private static readonly AOEShapeRect rect = new(20f, 4f);
    private readonly List<AOEInstance> _aoes = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == WatchedAction)
        {
            ++NumCasts;
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(caster.Position, 1f) && aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
            ReportError($"missing prediction for {spell.Rotation}");
            return;
        }

        var deg = id switch
        {
            (uint)AID.ImpactRotation1 => 0f,
            (uint)AID.ImpactRotation2 => 67.5f,
            (uint)AID.ImpactRotation3 => 45f,
            (uint)AID.ImpactRotation4 => 22.5f,
            _ => -1f
        };

        if (deg >= 0f)
        {
            var rotation = caster.Rotation + deg.Degrees();
            var pos = caster.Position;
            var act = WorldState.FutureTime(3d);
            for (var i = 0; i < 4; ++i)
            {
                _aoes.Add(new(rect, pos, rotation, act));
                rotation += 90f.Degrees();
            }
        }
    }
}

sealed class ConvenientSelfDestruction(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(5f), (uint)TetherID.ConvenientSelfDestruction, (uint)AID.ConvenientSelfDestruction, centerAtTarget: true)
{
    public DateTime Activation;

    private const double PanicTime = 1.2d;

    private bool Panic => WorldState.FutureTime(PanicTime) > Activation;

    private readonly List<Bait> _platformBaits = [];

    private List<Bait> NonTankBaitsOnPlatform(Actor actor)
    {
        var result = _platformBaits;
        result.Clear();
        var plat = A12Hobbes.PlatformCenters[A12Hobbes.PlatformIndex(actor.Position)];
        var count = CurrentBaits.Count;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            if (!b.Source.IsDead && b.Target.Role != Role.Tank && b.Source.Position.InCircle(plat, A12Hobbes.PlatformRadius))
            {
                result.Add(b);
            }
        }
        return result;
    }

    private bool HaveLiveTank()
    {
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            if (Raid[i] is Actor p && !p.IsDead && p.Role == Role.Tank)
            {
                return true;
            }
        }
        return false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.Impact)
        {
            Activation = WorldState.FutureTime(9.5d);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Panic)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (actor.Role == Role.Tank)
        {
            var baits = NonTankBaitsOnPlatform(actor);
            var count = baits.Count;
            if (count != 0)
            {
                var lines = new ShapeDistance[count];
                for (var i = 0; i < count; ++i)
                {
                    var b = baits[i];
                    lines[i] = new SDRect(b.Source.Position, b.Target.Position, 1f);
                }
                hints.AddForbiddenZone(new SDInvertedUnion(lines), Activation);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Panic)
        {
            if (actor.Role == Role.Tank)
            {
                if (NonTankBaitsOnPlatform(actor).Count != 0)
                {
                    hints.Add("Take tethers from party!");
                    return;
                }
            }
            else if (IsBaitTarget(actor) && HaveLiveTank())
            {
                hints.Add("Pass tether to tank!");
                return;
            }
        }
        base.AddHints(slot, actor, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => pc.Role != Role.Tank && player.Role == Role.Tank && playerSlot < PartyState.MaxPartySize && IsBaitTarget(pc)
            ? PlayerPriority.Critical
            : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}
