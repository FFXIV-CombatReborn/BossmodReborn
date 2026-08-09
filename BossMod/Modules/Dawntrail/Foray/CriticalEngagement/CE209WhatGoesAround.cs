using BossMod.Dawntrail.Foray.CriticalEngagement;

namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE209WhatGoesAround;

public enum OID : uint
{
    Boss = 0x4BC1, // R3.0, BNpcName 14512, undead mage
    AncientExplorerSpirit = 0x4BC2, // R1.0, untargetable circle casters
    AncientPirateSpirit = 0x4BC3, // R1.0, untargetable cross casters
    Controller = 0x4C75, // arena controller at (224, -860)
    Helper = 0x233C
}

public enum AID : uint
{
    ArenaControl = 0xB845, // controller->self, no cast, persistent encounter control pulse
    Necromancy = 0xB846, // boss->self, 3.0s cast, summons spirits
    SpiritExplosionCircle = 0xB847, // explorer spirit->self, 2.0s cast, range 8 circle
    SpiritExplosionCross = 0xB848, // pirate spirit->self, 4.0s cast, range 80 width 7 cross
    MarchOfTheDead = 0xB849, // boss->self, 3.0s cast, visual
    GrudgeRelease = 0xB84A, // spirit->self, 5.0s cast, range 50 width 5 rect
    DeployMagicCircle = 0xB84B, // boss->self, 3.0s cast, visual
    GloomCurrent = 0xB84C, // helper->self, 7.0s cast, range 70 width 12 rect, lane centered on the helper
    Gloom = 0xB84D, // boss->self, 5.0s cast, range 50 width 50 rect
    DarkIV = 0xB84E, // boss->self, 5.0s cast, raidwide visual
    DarkIVHit = 0xB84F, // helpers->players, raidwide damage
    AutoAttack = 0xC649
}

// Every avoidable hit in the recording exposes a real cast-start packet from the actor that owns
// the shape. This includes the staggered 8y grid, the large crosses and all three Gloom Current
// lanes, so no position or rotation needs to be inferred from the boss visuals.
sealed class WhatGoesAroundAOEs(BossModule module) : ReplayValidatedCastAOEs(module)
{
    private static readonly AOEShapeCircle SpiritCircle = new(8f);
    private static readonly AOEShapeCross SpiritCross = new(80f, 3.5f);
    private static readonly AOEShapeRect Grudge = new(50f, 2.5f);
    // Replay hits land up to ~26y on both sides of the casting helper (projections -16.2..+25.6 on
    // the lane axis, lateral offsets within 6y), and the packet rotation points opposite to the
    // visual flow. Draw each lane as a symmetric line through the helper so all three lanes of a
    // wave cover the arena correctly regardless of the packet rotation sign.
    private static readonly AOEShapeRect Current = new(35f, 6f, 35f);
    private static readonly AOEShapeRect Gloom = new(50f, 25f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // Replay has three distinct B84C helpers in every wave. The lines leave safe pockets near
        // the rim, but forbidden zones alone give the pathfinder no reason to leave a currently
        // safe central pocket early. Prefer the outer two yalms while the triple-line wave is
        // visible; the three actual line shapes still decide which part of the rim is safe.
        var currentVisible = false;
        foreach (ref readonly var aoe in ActiveAOEs(slot, actor))
        {
            if (ReferenceEquals(aoe.Shape, Current))
            {
                currentVisible = true;
                break;
            }
        }
        if (currentVisible)
        {
            var center = Arena.Center;
            hints.GoalZones.Add(position => (position - center).LengthSq() is >= 289f and <= 361f ? 5f : 0f);
        }
    }

    protected override AOEConfig? ConfigFor(uint actionID) => actionID switch
    {
        (uint)AID.SpiritExplosionCircle => new(SpiritCircle),
        (uint)AID.SpiritExplosionCross => new(SpiritCross),
        (uint)AID.GrudgeRelease => new(Grudge),
        (uint)AID.GloomCurrent => new(Current),
        (uint)AID.Gloom => new(Gloom),
        _ => null
    };
}

// B84F is split across three helpers. The boss cast is the stable advance warning.
sealed class DarkIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkIV);

sealed class ElectricBoundary(BossModule module) : Components.GenericAOEs(module)
{
    // ARR BFD0 deaths cluster at ~24.4y from center. Visual strip is 4.5y wide (2.25 half-width);
    // AI pathfinding keeps a 1.5y extra margin on each side so it never routes into the fence.
    private static readonly AOEShapeRect Shape = new(24.5f, 2.25f, 24.5f);
    private static readonly AOEShapeRect AIShape = new(24.5f, 3.75f, 24.5f);
    private readonly AOEInstance[] _aoes = Build(module.Arena.Center);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in _aoes)
            hints.AddForbiddenZone(AIShape.Distance(aoe.Origin, aoe.Rotation));
    }

    private static AOEInstance[] Build(WPos center)
    {
        var result = new AOEInstance[4];
        for (var i = 0; i < result.Length; ++i)
        {
            var normal = (i * 90f).Degrees().ToDirection();
            var rotation = Angle.FromDirection(normal.OrthoL());
            var origin = center + 23.75f * normal;
            result[i] = new(Shape, origin, rotation, color: Colors.Danger, risky: false, shapeDistance: Shape.Distance(origin, rotation));
        }
        return result;
    }
}

sealed class WhatGoesAroundStates : StateMachineBuilder
{
    public WhatGoesAroundStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricBoundary>()
            .ActivateOnEnter<WhatGoesAroundAOEs>()
            .ActivateOnEnter<DarkIV>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(WhatGoesAroundStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "KanoNoUta",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 57u,
    SortOrder = 8)]
public sealed class WhatGoesAround(WorldState ws, Actor primary) : BossModule(ws, primary, new(224f, -860f), new ArenaBoundsSquare(24.5f));
