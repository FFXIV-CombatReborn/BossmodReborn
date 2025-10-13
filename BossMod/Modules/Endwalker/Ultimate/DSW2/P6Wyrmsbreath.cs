namespace BossMod.Endwalker.Ultimate.DSW2;

// baited cones part of the mechanic
abstract class P6Wyrmsbreath(BossModule module, bool allowIntersect) : Components.GenericBaitAway(module, (uint)AID.FlameBreath) // note: cast is arbitrary
{
    public Actor?[] Dragons = [null, null]; // nidhogg & hraesvelgr
    public BitMask Glows;
    private readonly bool _allowIntersect = allowIntersect;
    private readonly Actor?[] _tetheredTo = new Actor?[PartyState.MaxPartySize];
    private BitMask _tooClose;

    private static readonly AOEShapeCone _shape = new(100f, 10f.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var bait = ActiveBaitsOn(actor);
        Bait? b = bait.Count != 0 ? bait[0] : null;
        if (b != null && b.Value.Source == null)
        {
            if (ActiveBaits.Any(b => IsClippedBy(actor, ref b)))
                hints.Add("GTFO from baits!");
        }
        else
        {
            if (_tooClose[slot])
                hints.Add("Stretch the tether!");

            var partner = IgnoredPartner(slot, actor);
            if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(ref b).Any(p => p != partner)))
                hints.Add("Bait away from raid!");
            if (ActiveBaitsNotOn(actor).Any(b => b.Target != partner && IsClippedBy(actor, ref b)))
                hints.Add("GTFO from baited aoe!");
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Glows.Any())
            hints.Add(Glows.Raw == 3ul ? "Tankbuster: shared" : "Tankbuster: solo");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var partner = IgnoredPartner(pcSlot, pc);
        var baits = CollectionsMarshal.AsSpan(ActiveBaitsNotOn(pc));
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var bait = ref baits[i];
            if (bait.Target == partner)
            {
                continue;
            }
            bait.Shape.Draw(Arena, BaitOrigin(ref bait), bait.Rotation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaitsOn(pc));
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var bait = ref baits[i];
            bait.Shape.Outline(Arena, BaitOrigin(ref bait), bait.Rotation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DreadWyrmsbreathNormal:
                Dragons[0] = caster;
                break;
            case (uint)AID.DreadWyrmsbreathGlow:
                Dragons[0] = caster;
                Glows.Set(0);
                break;
            case (uint)AID.GreatWyrmsbreathNormal:
                Dragons[1] = caster;
                break;
            case (uint)AID.GreatWyrmsbreathGlow:
                Dragons[1] = caster;
                Glows.Set(1);
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.FlameBreath or (uint)TetherID.IceBreath or (uint)TetherID.FlameIceBreathNear)
        {
            var slot = Raid.FindSlot(source.InstanceID);
            var boss = WorldState.Actors.Find(tether.Target);
            if (slot >= 0 && boss != null)
            {
                if (_tetheredTo[slot] == null)
                    CurrentBaits.Add(new(boss, source, _shape));
                _tooClose[slot] = tether.ID == (uint)TetherID.FlameIceBreathNear;
                _tetheredTo[slot] = boss;
            }
        }
    }

    private Actor? IgnoredPartner(int slot, Actor actor) => _allowIntersect && _tetheredTo[slot] != null ? Raid.WithSlot(false, true, true).WhereSlot(i => _tetheredTo[i] != null && _tetheredTo[i] != _tetheredTo[slot]).Closest(actor.Position).Item2 : null;
}
sealed class P6Wyrmsbreath1(BossModule module) : P6Wyrmsbreath(module, true);
sealed class P6Wyrmsbreath2(BossModule module) : P6Wyrmsbreath(module, false);

// note: it is actually symmetrical (both tanks get tankbusters), but that is hard to express, so we select one to show arbitrarily (nidhogg)
sealed class P6WyrmsbreathTankbusterShared(BossModule module) : Components.GenericSharedTankbuster(module, (uint)AID.DarkOrb, 6f)
{
    private readonly P6Wyrmsbreath? _main = module.FindComponent<P6Wyrmsbreath>();

    public override void Update()
    {
        Source = Target = null;
        if (_main?.Glows.Raw == 3)
        {
            Source = _main.Dragons[0];
            Target = WorldState.Actors.Find(Source?.TargetID ?? 0);
            Activation = Module.CastFinishAt(Source?.CastInfo);
        }
    }
}

sealed class P6WyrmsbreathTankbusterSolo(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly P6Wyrmsbreath? _main = module.FindComponent<P6Wyrmsbreath>();

    private static readonly AOEShapeCircle _shape = new(15);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_main?.Glows.Raw is 1 or 2)
        {
            var source = _main.Dragons[_main.Glows.Raw == 1 ? 1 : 0];
            var target = WorldState.Actors.Find(source?.TargetID ?? default);
            if (source != null && target != null)
                CurrentBaits.Add(new(source, target, _shape));
        }
    }
}

sealed class P6WyrmsbreathCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly P6Wyrmsbreath? _main = module.FindComponent<P6Wyrmsbreath>();

    private static readonly AOEShapeCone _shape = new(50f, 15f.Degrees()); // TODO: verify angle

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_main?.Glows.Raw is 1 or 2)
        {
            var source = _main.Dragons[_main.Glows.Raw == 1ul ? 0 : 1];
            if (source != null)
                return new AOEInstance[1] { new(_shape, source.Position, source.Rotation) }; // TODO: activation
        }
        return [];
    }
}
