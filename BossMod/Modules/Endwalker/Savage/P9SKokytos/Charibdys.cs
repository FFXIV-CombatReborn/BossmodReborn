namespace BossMod.Endwalker.Savage.P9SKokytos;

class Charibdys(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, (uint)AID.CharybdisAOE, m => m.Enemies((uint)OID.Charybdis).Where(v => v.EventState != 7), 0.6f);

class Comet(BossModule module) : Components.Adds(module, (uint)OID.Comet)
{
    public static bool IsActive(Actor c) => c.ModelState.AnimState2 == 1;
    public static bool IsFinished(Actor c) => c.IsDead || c.CastInfo != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in Actors.Where(a => !IsFinished(a)))
        {
            Arena.Actor(c, IsActive(c) ? Colors.Enemy : Colors.Object, true);
        }
    }
}

class CometImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CometImpact, 10); // TODO: verify falloff
class CometBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CometBurstLong, 10);

class BeastlyBile(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4)
{
    public int NumCasts;
    private readonly Comet? _comet = module.FindComponent<Comet>();
    private DateTime _activation = module.WorldState.FutureTime(15); // assuming component is activated after proximity
    private BitMask _forbiddenPlayers;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_comet != null && IsStackTarget(actor))
        {
            if (_comet.Actors.Any(c => !Comet.IsActive(c) && c.Position.InCircle(actor.Position, StackRadius)))
                hints.Add("GTFO from normal comets!");
            if (!_comet.Actors.Any(c => Comet.IsActive(c) && c.Position.InCircle(actor.Position, StackRadius)))
                hints.Add("Bait to glowing comet!");
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        var target = NumCasts < 2 ? Raid.WithoutSlot(false, true, true).Farthest(Module.PrimaryActor.Position) : null;
        if (target != null)
            AddStack(target, _activation, _forbiddenPlayers);
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BeastlyBileAOE)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(6);
            foreach (var t in spell.Targets)
                _forbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

class Thunderbolt(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ThunderboltAOE)
{
    private readonly Comet? _comet = module.FindComponent<Comet>();

    private readonly AOEShapeCone _shape = new(40, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var p in Raid.WithoutSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position).Take(4))
            CurrentBaits.Add(new(Module.PrimaryActor, p, _shape));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var bait = ref baits[i];
            if (!bait.Source.IsDead && bait.Target == actor)
            {
                var comets = _comet?.Actors;
                var count = comets?.Count;
                for (var j = 0; j < count; ++j)
                {
                    if (IsClippedBy(comets![i], ref bait))
                    {
                        hints.Add("Aim away from comets!");
                        return;
                    }
                }
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

class EclipticMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, (uint)AID.EclipticMeteorAOE, 60, safeInsideHitbox: false)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.EclipticMeteor)
        {
            Modify(actor.Position, Module.Enemies((uint)OID.Comet).Where(c => c != actor && !Comet.IsFinished(c)).Select(c => (c.Position, c.HitboxRadius)));
        }
    }
}
