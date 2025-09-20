namespace BossMod.Global.MaskedCarnivale.Stage30.Act2;

public enum OID : uint
{
    Boss = 0x2C6A, //R=2.0
    IceBoulder = 0x2CC6, // R=2.0
    FireVoidzone = 0x1E8D9B,
    Helper = 0x233C
}

public enum AID : uint
{
    LawOfTheTorch1 = 18838, // Boss->self, 3.0s cast, range 34 20-degree cone
    LawOfTheTorch2 = 18839, // Helper->self, 3.0s cast, range 34 20-degree cone
    Teleport = 18848, // Boss->location, no cast, ???
    SwiftsteelKB = 18842, // Boss->self, 5.0s cast, range 100 circle, knockback 10, away from source
    Swiftsteel1 = 18843, // Helper->location, 8.8s cast, range 4 circle
    Swiftsteel2 = 18844, // Helper->self, 8.8s cast, range 8-20 donut
    SparksteelVisual = 18893, // Boss->self, no cast, single-target
    Sparksteel1 = 18840, // Boss->location, 3.0s cast, range 6 circle, spawns voidzone
    Sparksteel2 = 18841, // Helper->location, 4.0s cast, range 8 circle
    Sparksteel3 = 18897, // Helper->location, 6.0s cast, range 8 circle
    Shattersteel = 19027, // Boss->self, 5.0s cast, range 8 circle
    SphereShatter = 18986 // IceBoulder->self, no cast, range 10 circle
}

sealed class LawOfTheTorch(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LawOfTheTorch1, (uint)AID.LawOfTheTorch2], new AOEShapeCone(34f, 10f.Degrees()));

sealed class SwiftsteelKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SwiftsteelKB, 10f)
{
    private readonly Swiftsteel1 _aoe1 = module.FindComponent<Swiftsteel1>()!;
    private readonly Swiftsteel2 _aoe2 = module.FindComponent<Swiftsteel2>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes1 = _aoe1.ActiveAOEs(slot, actor);
        var len1 = aoes1.Length;
        for (var i = 0; i < len1; ++i)
        {
            ref readonly var aoe = ref aoes1[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len2 = aoes2.Length;
        for (var i = 0; i < len2; ++i)
        {
            ref readonly var aoe = ref aoes2[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class Swiftsteel1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swiftsteel1, 4f);
sealed class Swiftsteel2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swiftsteel2, new AOEShapeDonut(8f, 20f));
sealed class SparksteelVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.Sparksteel1, GetVoidzones, 0.8d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class SparksteelAOE : Components.SimpleAOEGroupsByTimewindow
{
    public SparksteelAOE(BossModule module) : base(module, [(uint)AID.Sparksteel2, (uint)AID.Sparksteel3], 8f, expectedNumCasters: 8)
    {
        MaxDangerColor = 4;
    }
}

sealed class Shattersteel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shattersteel, 5f);
sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private readonly List<AOEInstance> _aoes = new(7);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IceBoulder)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(8.4d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
        {
            _aoes.Clear();
        }
    }
}

sealed class Stage30Act2States : StateMachineBuilder
{
    public Stage30Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LawOfTheTorch>()
            .ActivateOnEnter<Swiftsteel1>()
            .ActivateOnEnter<Swiftsteel2>()
            .ActivateOnEnter<SwiftsteelKB>()
            .ActivateOnEnter<SparksteelVoidzone>()
            .ActivateOnEnter<SparksteelAOE>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<Shattersteel>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 2)]
public sealed class Stage30Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall);