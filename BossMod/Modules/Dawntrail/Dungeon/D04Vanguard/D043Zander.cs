namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D043Zander;

public enum OID : uint
{
    Boss = 0x411E, // R2.1
    BossP2 = 0x41BA, // R2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/BossP2->player, no cast, single-target

    BurstVisual1 = 39240, // Helper->self, 10.5s cast, range 20 width 40 rect
    BurstVisual2 = 39241, // Helper->self, 11.5s cast, range 20 width 40 rect
    Burst1 = 36575, // Helper->self, 10.0s cast, range 20 width 40 rect
    Burst2 = 36591, // Helper->self, 11.0s cast, range 20 width 40 rect

    Electrothermia = 36594, // Boss->self, 5.0s cast, range 60 circle, raidwide
    SaberRush = 36595, // Boss->player, 5.0s cast, single-target
    Screech = 36596, // BossP2->self, 5.0s cast, range 60 circle, raidwide
    ShadeShot = 36597, // BossP2->player, 5.0s cast, single-target

    SlitherbaneForeguardRect = 36589, // BossP2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneForeguardCone = 36592, // Helper->self, 4.5s cast, range 20 180-degree cone

    SlitherbaneRearguardRect = 36590, // Boss2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneRearguardCone = 36593, // Helper->self, 4.5s cast, range 20 180-degree cone

    SoulbaneSaber = 36574, // Boss->self, 3.0s cast, range 20 width 4 rect
    SoulbaneShock = 37922, // Helper->player, 5.0s cast, range 5 circle
    Syntheslean = 37198, // BossP2->self, 4.0s cast, range 19 90-degree cone

    SyntheslitherVisual1 = 36579, // BossP2->location, 4.0s cast, single-target
    SyntheslitherVisual2 = 36584, // BossP2->location, 4.0s cast, single-target
    Syntheslither1 = 36580, // Helper->self, 5.0s cast, range 19 90-degree cone
    Syntheslither2 = 36581, // Helper->self, 5.6s cast, range 19 90-degree cone
    Syntheslither3 = 36582, // Helper->self, 6.2s cast, range 19 90-degree cone
    Syntheslither4 = 36583, // Helper->self, 6.8s cast, range 19 90-degree cone
    Syntheslither5 = 36585, // Helper->self, 5.0s cast, range 19 90-degree cone
    Syntheslither6 = 36586, // Helper->self, 5.6s cast, range 19 90-degree cone
    Syntheslither7 = 36587, // Helper->self, 6.2s cast, range 19 90-degree cone
    Syntheslither8 = 36588, // Helper->self, 6.8s cast, range 19 90-degree cone

    PhaseChangeVisual1 = 36576, // Boss->self, no cast, single-target
    PhaseChangeVisual2 = 36577, // Boss->self, no cast, single-target
    PhaseChangeVisual3 = 36578 // Boss->self, no cast, single-target
}

sealed class ElectrothermiaArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(17f, 20f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrothermia && Arena.Bounds.Radius > 17f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.5d))];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = D043Zander.DefaultBounds;
            Arena.Center = D043Zander.DefaultBounds.Center;
            _aoe = [];
        }
    }
}

sealed class SlitherbaneBurstCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Angle a180 = 180f.Degrees();
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());
    private static readonly AOEShapeRect rect = new(20f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = true;
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
            ref var aoe1 = ref aoes[1];
            if (aoe0.Rotation.AlmostEqual(aoe1.Rotation + a180, Angle.DegToRad))
            {
                aoe1.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.SlitherbaneRearguardCone:
            case (uint)AID.SlitherbaneForeguardCone:
                AddAOE(cone);
                break;
            case (uint)AID.Burst2:
                AddAOE(rect);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.SlitherbaneRearguardCone:
                case (uint)AID.SlitherbaneForeguardCone:
                case (uint)AID.Burst2:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}

sealed class Electrothermia(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electrothermia);
sealed class Screech(BossModule module) : Components.RaidwideCast(module, (uint)AID.Screech);
sealed class Burst1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst1, new AOEShapeRect(20f, 20f));
sealed class SaberRush(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.SaberRush)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.PhaseChangeVisual1) // tankbuster will be cancelled on phase change
        {
            Targets.Clear();
        }
    }
}

sealed class ShadeShot(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ShadeShot);
sealed class SoulbaneShock(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SoulbaneShock, 5f);

sealed class SlitherbaneSoulbaneSaber(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SlitherbaneForeguardRect, (uint)AID.SlitherbaneRearguardRect, (uint)AID.SoulbaneSaber], new AOEShapeRect(20f, 2f));
sealed class Syntheslither(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Syntheslean, (uint)AID.Syntheslither1, (uint)AID.Syntheslither2,
(uint)AID.Syntheslither3, (uint)AID.Syntheslither4, (uint)AID.Syntheslither5, (uint)AID.Syntheslither6, (uint)AID.Syntheslither7, (uint)AID.Syntheslither8], new AOEShapeCone(19f, 45f.Degrees()));

sealed class D043ZanderStates : StateMachineBuilder
{
    public D043ZanderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrothermiaArenaChange>()
            .ActivateOnEnter<Electrothermia>()
            .ActivateOnEnter<Screech>()
            .ActivateOnEnter<Burst1>()
            .ActivateOnEnter<SaberRush>()
            .ActivateOnEnter<ShadeShot>()
            .ActivateOnEnter<SlitherbaneSoulbaneSaber>()
            .ActivateOnEnter<SlitherbaneBurstCombo>()
            .ActivateOnEnter<SoulbaneShock>()
            .ActivateOnEnter<Syntheslither>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12752, SortOrder = 7)]
public sealed class D043Zander(WorldState ws, Actor primary) : BossModule(ws, primary, StartingBounds.Center, StartingBounds)
{
    private static readonly WPos ArenaCenter = new(90f, -430f);
    public static readonly ArenaBoundsCustom StartingBounds = new([new Polygon(ArenaCenter, 19.5f, 40)], [new Rectangle(new(90f, -410f), 20f, 0.85f)]);
    public static readonly ArenaBoundsCustom DefaultBounds = new([new Polygon(ArenaCenter, 17f, 40)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BossP2));
    }
}
