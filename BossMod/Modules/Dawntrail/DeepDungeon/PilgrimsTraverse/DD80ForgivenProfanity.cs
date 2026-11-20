namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.DD80ForgivenProfanity;

public enum OID : uint
{
    ForgivenProfanity = 0x485D, // R5.6
    BallOfLevin = 0x485E, // R1.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45130, // ForgivenProfanity->player, no cast, single-target

    RoaringRingVisual1 = 43465, // ForgivenProfanity->self, 5.2+0,8s cast, single-target
    RoaringRingVisual2 = 43467, // ForgivenProfanity->self, 5.2+0,8s cast, single-target
    RoaringRing = 43468, // Helper->self, 6.0s cast, range 8-40 donut
    PerilousLair = 43472, // Helper->self, 6.0s cast, range 12 circle
    ProfaneWaul = 43473, // Helper->self, 6.0s cast, range 40 180-degree cone
    StalkingStaticRaidwide = 43476, // ForgivenProfanity->self, 3.0s cast, range 40 circle, small amount of damage, can be ignored
    StalkingStaticVisual = 43477, // BallOfLevin->location, 0.7s cast, width 4 rect charge
    StalkingStatic = 43795, // Helper->location, 1.0s cast, width 4 rect charge
    StaticShockVisual = 43478, // BallOfLevin->self, 1.5s cast, range 30 circle
    StaticShock = 44060, // Helper->self, 2.0s cast, range 30 circle
    PerilousLairVisual1 = 43469, // ForgivenProfanity->self, 5.2+0,8s cast, single-target
    PerilousLairVisual2 = 43471, // ForgivenProfanity->self, 5.2+0,8s cast, single-target
    ProwlingDeath = 43475 // ForgivenProfanity->self, 3.0s cast, range 40 circle, applies Nowhere to Run or Shadow of Death
}

public enum TetherID : uint
{
    Lightning = 21 // BallOfLevin->BallOfLevin
}

public enum SID : uint
{
    NowhereToRun = 4519, // none->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
    ShadowOfDeath = 4518 // none->player, extra=0x0
}

sealed class BarefistedDeath(BossModule module) : Components.CastHint(module, (uint)AID.ProwlingDeath, "Watch your incoming debuff!");

sealed class RoaringRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoaringRing, new AOEShapeDonut(8f, 40f));
sealed class PerilousLair(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PerilousLair, 12f);

sealed class StalkingStaticShock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(9)];
    private readonly AOEShapeCircle circle = new(30f);
    private readonly List<Actor> levins = module.Enemies((uint)OID.BallOfLevin);
    private readonly Dictionary<ulong, ulong> _next = [];
    private readonly HashSet<ulong> _targets = [];
    private DateTime _spawnTime;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BallOfLevin && levins.Count == 9)
            _spawnTime = WorldState.CurrentTime;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Lightning)
            return;
        _next[source.InstanceID] = tether.Target;
        _targets.Add(tether.Target);
        if (_next.Count == 8)
            BuildChain();
    }

    private void BuildChain()
    {
        ulong start = default;
        foreach (var src in _next.Keys)
        {
            if (!_targets.Contains(src))
            {
                start = src;
                break;
            }
        }
        var activation = _spawnTime != default ? _spawnTime : WorldState.CurrentTime;
        _aoes.Clear();
        var current = WorldState.Actors.Find(start);
        for (var i = 0; current != null && _next.TryGetValue(current.InstanceID, out var nextID) && i < 8; ++i)
        {
            var next = WorldState.Actors.Find(nextID);
            if (next == null)
                break;
            var pos = current.Position;
            var dir = next.Position.Quantized() - pos;
            var rot = Angle.FromDirection(dir);
            var shape = new AOEShapeRect(dir.Length(), 2f);
            _aoes.Add(new(shape, pos, rot, activation.AddSeconds(7.5d + 0.4d * i), shapeDistance: shape.Distance(pos, rot)));
            current = next;
        }
        if (current != null && _aoes.Count != 0)
        {
            var loc = current.Position.Quantized();
            _aoes.Add(new(circle, loc, default, activation.AddSeconds(11.7d), shapeDistance: circle.Distance(loc, default)));
            _aoes.Ref(0).Color = Colors.Danger;
        }
        _next.Clear();
        _targets.Clear();
        _spawnTime = default;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count != 0 && spell.Action.ID is (uint)AID.StaticShock or (uint)AID.StalkingStatic)
        {
            _aoes.RemoveAt(0);
            if (count > 2)
            {
                _aoes.Ref(0).Color = Colors.Danger;
            }
        }
    }

    public void UpdateAOE() // ProfaneWaul combo might overlap with this making the whole arena the same color, so we need to do an extra update
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            _aoes.Ref(count - 1).Color = Colors.Danger;
        }
    }
}

sealed class ProfaneWaulShadowOfDeath(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] _aoe = [], _aoeInv = [];
    private readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private BitMask doomed;
    private readonly StalkingStaticShock _shock = module.FindComponent<StalkingStaticShock>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => doomed[slot] ? _aoeInv : _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ProfaneWaul)
        {
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell);
            _aoe = [new(cone, pos, rot, act, shapeDistance: cone.Distance(pos, rot))];
            _aoeInv = [new(cone, pos, rot, act, Colors.SafeFromAOE, shapeDistance: cone.InvertedDistance(pos, rot))];
            _shock.UpdateAOE();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ProfaneWaul)
        {
            _aoe = [];
            _aoeInv = [];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ShadowOfDeath)
        {
            doomed.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ShadowOfDeath)
        {
            doomed.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!doomed[slot])
        {
            base.AddHints(slot, actor, hints);
        }
        else if (_aoeInv.Length != 0)
        {
            ref var aoe = ref _aoeInv[0];
            hints.Add("Get hit by cone AOE!", !aoe.Check(actor.Position));
        }
    }
}

sealed class NowhereToRun(BossModule module) : BossComponent(module)
{
    private BitMask debuffed;
    private readonly int[] numStacks = new int[4];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.NowhereToRun)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            debuffed.Set(slot);
            if (slot >= 0)
            {
                numStacks[slot] = status.Extra;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.NowhereToRun)
        {
            debuffed.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!debuffed[slot])
        {
            return;
        }
        var stacks = numStacks[slot];
        hints.Add($"Minimize movement! {stacks}/8 stacks", stacks > 4);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (debuffed[slot] && numStacks[slot] >= 6)
        {
            hints.ForcedMovement = null; // forbid all AI movement in emergency, prefer eating AOEs over instant death
        }
    }
}

sealed class DD80ForgivenProfanityStates : StateMachineBuilder
{
    public DD80ForgivenProfanityStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NowhereToRun>()
            .ActivateOnEnter<RoaringRing>()
            .ActivateOnEnter<PerilousLair>()
            .ActivateOnEnter<StalkingStaticShock>()
            .ActivateOnEnter<ProfaneWaulShadowOfDeath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.ForgivenProfanity, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1039u, NameID = 13968u)]
public sealed class DD80ForgivenProfanity(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, new ArenaBoundsCustom([new Polygon(arenaCenter, 19.5f, 40)]))
{
    private static readonly WPos arenaCenter = new(-600f, -300f);
}
