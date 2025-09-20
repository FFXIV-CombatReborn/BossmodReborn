namespace BossMod.Stormblood.Raid.O8NKefka;

public enum OID : uint
{
    Boss = 0x2153, // R2.700, x1
    GravenImage = 0x18D6, // R0.500, x39, mixed types
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Hyperdrive = 10542, // Boss->player, 4.0s cast, single-target

    BlizzardBlitzVisual = 10519, // Boss->self, 5.0s cast, single-target
    BlizzardBlitzCircleFake1 = 10515, // GravenImage->self, 5.0s cast, range 10 circle
    BlizzardBlitzCircleFake2 = 10517, // GravenImage->self, 5.0s cast, range 40 circle
    BlizzardBlitzDonut1 = 10516, // GravenImage->self, 5.0s cast, range 10-40 donut
    BlizzardBlitzDonut2 = 10521, // GravenImage->self, 5.0s cast, range 10-40 donut
    BlizzardBlitzCircle1 = 10518, // GravenImage->self, 5.0s cast, range 10 circle
    BlizzardBlitzCircle2 = 10520, // GravenImage->self, 5.0s cast, range 10 circle

    ThrummingThunderVisual = 10522, // Boss->self, 5.0s cast, single-target
    ThrummingThunderFake = 10523, // GravenImage->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunder1 = 10524, // GravenImage->self, 5.0s cast, range 40+R width 10 rect
    ThrummingThunder2 = 10525, // GravenImage->self, 5.0s cast, range 40+R width 10 rect

    FlagrantFireVisual = 10526, // Boss->self, 4.2s cast, single-target
    FlagrantFireSpread = 10527, // GravenImage->players, 5.2s cast, range 5 circle
    FlagrantFireStack = 10528, // GravenImage->players, 5.2s cast, range 6 circle

    TimelyTeleportVisual1 = 10529, // Boss->self, 4.0s cast, single-target
    TimelyTeleportVisual2 = 10530, // GravenImage->self, 4.0s cast, range 6 circle
    RevoltingRuin = 10531, // Boss->self, no cast, range 100+R 120-degree cone
    AeroAssault = 10532, // Boss->self, 3.0s cast, range 100 circle
    GravenImage = 10533, // Boss->self, 5.0s cast, single-target
    Shockwave = 10535, // GravenImage->self, 13.0s cast, range 100+R width 40 rect
    WaveCannon = 10536, // GravenImage->self, 3.0s cast, range 70+R width 6 rect

    GravitationalWave = 10537, // GravenImage->self, 6.0s cast, range 100+R 180-degree cone, west half cleave
    IntemperateWill = 10538, // GravenImage->self, 6.0s cast, range 100+R 180-degree cone, east half cleave
    AveMaria = 10539, // GravenImage->self, 6.0s cast, range 100 circle, reverse gaze, must look at tower
    IndolentWill = 10540, // GravenImage->self, 6.0s cast, range 100 circle, gaze

    UltimaUpsurge = 10541, // Boss->self, 4.0s cast, range 100 circle
}

class Hyperdrive(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Hyperdrive);
class BlizzardBlitzDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlizzardBlitzDonut1, (uint)AID.BlizzardBlitzDonut2], new AOEShapeDonut(10f, 40f));
class BlizzardBlitzCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlizzardBlitzCircle1, (uint)AID.BlizzardBlitzCircle2], 10f);

class FlagrantFireSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FlagrantFireSpread, 5f);
class FlagrantFireStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FlagrantFireStack, 6f, 8, 8);

class ThrummingThunder(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThrummingThunder1, (uint)AID.ThrummingThunder2], new AOEShapeRect(40, 5f));

class UltimaUpsurge(BossModule module) : Components.RaidwideCast(module, (uint)AID.UltimaUpsurge);

class AeroAssault(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AeroAssault, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, c.Origin, 10f, 19f), c.Activation);
            }
        }
    }
}

class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 15f, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInCircleFixedDirection(Arena.Center, 15f * c.Direction.ToDirection(), 19f), act);
            }
        }
    }
}

class WaveCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaveCannon, new AOEShapeRect(70f, 3f));

class GravitationalWaveIntemperateWill(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GravitationalWave, (uint)AID.IntemperateWill], new AOEShapeCone(100f, 90f.Degrees()));
class AveMaria(BossModule module) : Components.CastGaze(module, (uint)AID.AveMaria, true);
class IndolentWill(BossModule module) : Components.CastGaze(module, (uint)AID.IndolentWill);

class RevoltingRuin(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(102.7f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TimelyTeleportVisual2 && !caster.Position.AlmostEqual(Module.PrimaryActor.Position, 1f))
        {
            _aoe = [new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 3.1d))];
        }
        else if (spell.Action.ID == (uint)AID.AeroAssault) // sometimes it does AeroAssault directly and skipping Revolting Ruin
        {
            _aoe = [];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RevoltingRuin)
        {
            _aoe = [];
        }
    }
}

class O8NKefkaStates : StateMachineBuilder
{
    public O8NKefkaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hyperdrive>()
            .ActivateOnEnter<BlizzardBlitzDonut>()
            .ActivateOnEnter<BlizzardBlitzCircle>()
            .ActivateOnEnter<FlagrantFireSpread>()
            .ActivateOnEnter<FlagrantFireStack>()
            .ActivateOnEnter<ThrummingThunder>()
            .ActivateOnEnter<UltimaUpsurge>()
            .ActivateOnEnter<RevoltingRuin>()
            .ActivateOnEnter<AeroAssault>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<GravitationalWaveIntemperateWill>()
            .ActivateOnEnter<AveMaria>()
            .ActivateOnEnter<IndolentWill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS, Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 289, NameID = 7131)]
public class O8NKefka(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f));
