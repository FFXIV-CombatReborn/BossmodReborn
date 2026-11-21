namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE207DoubleTrouble;

public enum OID : uint
{
    ConjuredCalofisteri = 0x4BB8,
    Helper = 0x233C,
    LitheLock = 0x4BBA, // R1.000, x0 (spawn during fight)
    Entanglement = 0x4BB9, // R4.440, x0 (spawn during fight)
    BlueIcon = 0x4BBB, // R1.000, x0 (spawn during fight)
    RedIcon = 0x4BBC, // R1.000, x0 (spawn during fight)
}

public enum SID : uint
{
    Fetters = 5349, // Entanglement->player, extra=0xEC4
}

public enum AID : uint
{
    AutoAttack = 50122, // ConjuredCalofisteri->player, no cast, single-target

    AuraBurstVisual = 47079, // ConjuredCalofisteri->self, 5.0s cast, single-target
    AuraBurst = 47080, // Helper->self, no cast, ???
    AsymmetricCoifChangeRightLeft = 47054, // ConjuredCalofisteri->self, 3.0s cast, single-target - right to left
    AsymmetricCoifChangeLeftRight = 47055, // ConjuredCalofisteri->self, 3.0s cast, single-target - left to right
    DualCutVisual1 = 47058, // ConjuredCalofisteri->self, 2.0s cast, single-target
    DualCutVisual2 = 47059, // ConjuredCalofisteri->self, 2.0s cast, single-target
    DualCutVisual3 = 47061, // ConjuredCalofisteri->self, no cast, single-target
    DualCutVisual4 = 47060, // ConjuredCalofisteri->self, no cast, single-target
    DualCut1 = 50691, // Helper->self, 2.8s cast, range 60 180-degree cone
    DualCut2 = 50692, // Helper->self, 4.8s cast, range 60 180-degree cone
    DashingCutLongTeleport = 47067, // ConjuredCalofisteri->location, 6.0s cast, single-target
    DashingCutTeleport = 47068, // ConjuredCalofisteri->location, 0.5s cast, single-target
    DashingCut1 = 49052, // Helper->location, 6.5s cast, width 10 rect charge
    DashingCut2 = 49053, // Helper->location, 1.0s cast, width 10 rect charge

    Extension = 47069, // ConjuredCalofisteri->self, 3.0s cast, single-target

    HairShearsCast = 47075, // ConjuredCalofisteri->self, 5.0s cast, single-target
    HairShearsVisual = 47599, // Helper->self, no cast, range 60 width 4 cross
    HairShearsCross = 47077, // Helper->self, 5.0s cast, range 60 width 4 cross
    HairShearsCircle = 47076, // Helper->self, 5.0s cast, range 10 circle

    Graft = 47070, // 4BBA->self, 3.0s cast, range 6 circle
    BalefulBlowout = 47071, // ConjuredCalofisteri->self, 5.0s cast, single-target
    MaliciousWeave = 47072, // 4BB9->self, 5.5s cast, range 6 circle
    MaliciousWeave1 = 47078, // 4BB9->self, 1.0s cast, range 6 circle
    GarroteConsume = 47073, // 4BB9->self, 10.0s cast, range 6 circle
    Garrote = 47074, // 4BB9->self, no cast, single-target

    CoifChange = 47057, // ConjuredCalofisteri->self, no cast, single-target
    CoifChange1 = 47056, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray = 47062, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray1 = 47065, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray2 = 47063, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray3 = 47064, // ConjuredCalofisteri->self, no cast, single-target
    RedIconTeleport = 47066, // 4BBC->location, no cast, single-target
}

sealed class AuraBurst(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AuraBurstVisual, (uint)AID.AuraBurst, 0.8d);
sealed class Graft(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Graft, (uint)AID.MaliciousWeave, (uint)AID.MaliciousWeave1], 6f);
sealed class DashingCut(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.DashingCut1, (uint)AID.DashingCut2], 5f);
sealed class HairShearsCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HairShearsCross, new AOEShapeCross(60f, 2f));
sealed class HairShearsCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HairShearsCircle, 10f);

sealed class DualCut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private (WPos, Angle, DateTime)? caster1;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    // TODO: probably should make an early prediction based on coif changes and teleports if possible
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.DualCut1 or (uint)AID.DualCut2)
        {
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell);
            if (caster1 == null)
            {
                caster1 = (pos, rot, act);
            }
            else
            {
                var isFirst = id == (uint)AID.DualCut1;
                var c1 = caster1.Value;
                AddAOE(isFirst ? pos : c1.Item1, isFirst ? rot : c1.Item2, isFirst ? act : c1.Item3, false);
                AddAOE(isFirst ? c1.Item1 : pos, isFirst ? c1.Item2 : rot, isFirst ? c1.Item3 : act, true);
                caster1 = null;
            }
        }
        void AddAOE(WPos position, Angle rotation, DateTime activation, bool isSecond)
        {
            var pos2 = isSecond ? position + 5f * rotation.ToDirection() : position;
            _aoes.Add(new(cone, pos2, rotation, activation, shapeDistance: cone.Distance(pos2, rotation)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.DualCut1 or (uint)AID.DualCut2)
        {
            _aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe2 = ref _aoes.Ref(0);
                var rot = aoe2.Rotation;
                aoe2.Origin -= 5f * rot.ToDirection();
                aoe2.ShapeDistance = cone.Distance(aoe2.Origin, rot);
            }
        }
    }
}

sealed class CE207DoubleTroubleStates : StateMachineBuilder
{
    public CE207DoubleTroubleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AuraBurst>()
            .ActivateOnEnter<DualCut>()
            .ActivateOnEnter<Graft>()
            .ActivateOnEnter<DashingCut>()
            .ActivateOnEnter<HairShearsCross>()
            .ActivateOnEnter<HairShearsCircle>();
    }
}

//TODO: Add AI Hint to move closer to the middle of the cleaves to make dodging easier- can be marked as verified after implemented
[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.ConjuredCalofisteri, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1093u, NameID = 50u)]
public sealed class CE207DoubleTrouble : BossModule
{
    public CE207DoubleTrouble(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private CE207DoubleTrouble(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-215f, -65f), 22f, 128, 15f.Degrees())]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Entanglement));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InCircle(Arena.Center, 22f);
}
