namespace BossMod.Stormblood.Dungeon.D13TheBurn.D131Hedetet;

public enum OID : uint
{
    Boss = 0x2419, // R4.2
    DimCrystal = 0x241A, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport1 = 33212, // Boss->location, no cast, single-target
    Teleport2 = 12694, // Boss->location, no cast, single-target

    CrystalNeedle = 12691, // Boss->player, 3.0s cast, single-target
    Hailfire = 12692, // Boss->self/players, 6.0s cast, range 40+R width 4 rect
    ShardstrikeVisual = 12693, // Boss->self, 5.0s cast, single-target
    Shardstrike = 12697, // Helper->players, no cast, range 5 circle
    Shardfall = 12689, // Boss->self, 5.0s cast, range 40 circle
    ResonantFrequency = 12696, // DimCrystal->self, 3.0s cast, range 6 circle
    Dissonance = 12690, // Boss->self, 5.0s cast, range 5-40 donut
    CrystallineFracture = 12695 // DimCrystal->self, 3.0s cast, range 3 circle
}

public enum IconID : uint
{
    Spreadmarker = 96 // player
}

class Shardstrike(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.Shardstrike, 5f, 5.8f);
class ShardstrikeCrystals(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Shardstrike _st = module.FindComponent<Shardstrike>()!;
    private static readonly AOEShapeCircle circle = new(6.6f); // for non players hitbox must not be clipped

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_st.Stacks.Count == 0)
            return [];

        var enemies = Module.Enemies((uint)OID.DimCrystal);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var aoes = new List<AOEInstance>(count);

        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                aoes[i] = new(circle, z.Position, color: Colors.FutureVulnerable);
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_st.Stacks.Count != 0)
            hints.Add("Avoid clipping crystals!");
    }
}

class Hailfire(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _target;
    private const float Length = 44.2f;
    private readonly List<RectangleSE> rects = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_target != null)
        {
            var primary = Module.PrimaryActor;
            return new AOEInstance[1] { _target == actor
                ? new(new AOEShapeCustom(rects, invertForbiddenZone: true), Arena.Center, color: Colors.SafeFromAOE)
                : new(new AOEShapeCustom([new RectangleSE(primary.Position, primary.Position + Length * primary.DirectionTo(_target), 2f)], rects), Arena.Center) };
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hailfire)
        {
            var enemies = Module.Enemies((uint)OID.DimCrystal);
            var count = enemies.Count;
            var boss = Module.PrimaryActor;
            for (var i = 0; i < count; ++i)
            {
                var c = enemies[i];
                if (!c.IsDead)
                {
                    var dir = boss.DirectionTo(c);
                    rects.Add(new(c.Position + 0.1f * dir, c.Position + Length * dir, 1.6f));
                }
            }
            _target = WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hailfire)
        {
            rects.Clear();
            _target = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == actor)
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var isRisky = true;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(actor.Position))
                {
                    isRisky = false;
                    break;
                }
            }
            hints.Add("Hide behind crystal!", isRisky);
        }
        else
            base.AddHints(slot, actor, hints);
    }
}

class CrystalNeedle(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CrystalNeedle);
class Shardfall(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.Shardfall, 40f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.DimCrystal);
        var count = boulders.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (!b.IsDead)
                actors.Add(b);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}
class CrystallineFracture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrystallineFracture, 3f);
class ResonantFrequency(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ResonantFrequency, 6f);
class Dissonance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dissonance, new AOEShapeDonut(5f, 40f));

class D131HedetetStates : StateMachineBuilder
{
    public D131HedetetStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shardstrike>()
            .ActivateOnEnter<ShardstrikeCrystals>()
            .ActivateOnEnter<Hailfire>()
            .ActivateOnEnter<CrystalNeedle>()
            .ActivateOnEnter<Shardfall>()
            .ActivateOnEnter<CrystallineFracture>()
            .ActivateOnEnter<ResonantFrequency>()
            .ActivateOnEnter<Dissonance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7667)]
public class D131Hedetet(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(174f, 178f), 19.5f)], [new Rectangle(new(174f, 197.6f), 20f, 1f), new Rectangle(new(174f, 158.3f), 20f, 1f)]);
}
