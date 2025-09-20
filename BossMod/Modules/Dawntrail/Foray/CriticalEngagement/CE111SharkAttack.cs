namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE111SharkAttack;

public enum OID : uint
{
    Boss = 0x4704, // R2.47
    PetalodusProgeny1 = 0x4705, // R1.95
    PetalodusProgeny2 = 0x47BC, // R1.95
    JumpMarkerRed = 0x4706, // R1.0
    JumpMarkerBlue = 0x4707, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/PetalodusProgeny1->player, no cast, single-target
    Teleport1 = 41693, // PetalodusProgeny2->location, no cast, single-target
    Teleport2 = 41683, // PetalodusProgeny1->location, no cast, single-target
    Teleport3 = 41682, // JumpMarkerRed->location, no cast, single-target

    MarineMayhemVisual = 41689, // Boss->self, 5.0s cast, single-target, raidwide
    MarineMayhem = 41691, // Helper->self, no cast, ???
    PelagicCleaverVisual = 41685, // PetalodusProgeny2->location, 8.3s cast, single-target
    PelagicCleaver = 41970, // Helper->self, 9.0s cast, range 50 60-degree cone
    Hydrocleave = 43149, // Boss->self, 5.0s cast, range 50 60-degree cone

    Dive = 41676, // Boss->location, 3.0s cast, single-target
    TidalGuillotineVisual = 41677, // Boss->location, 7.0s cast, single-target
    TidalGuillotineTeleport1 = 43148, // Boss/PetalodusProgeny2->location, 0.5s cast, single-target
    TidalGuillotineTeleport2 = 41679, // Boss->location, 0.5s cast, single-target
    TidalGuillotineTeleport3 = 41678, // PetalodusProgeny2->location, 7.0s cast, single-target
    TidalGuillotineTeleport4 = 41680, // PetalodusProgeny2->location, 0.5s cast, single-target
    TidalGuillotine1 = 41723, // Helper->location, 7.9s cast, range 20 circle
    TidalGuillotine2 = 41722, // Helper->location, 1.2s cast, range 20 circle

    OpenWaterVisual = 41686, // Boss->self, 5.0s cast, single-target
    OpenWaterVisualFirst = 41687, // Helper->self, 5.0s cast, range 4 circle, visual
    OpenWaterVisualRest = 41688, // Helper->self, no cast, range 4 circle, visual
    OpenWater = 43151 // Helper->self, no cast, range 4 circle, damage
}

sealed class TidalGuillotine(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TidalGuillotine1)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Teleport3)
        {
            _aoes.Add(new(circle, spell.TargetXZ, default, WorldState.FutureTime(_aoes.Count == 1 ? 8.7d : 9.9d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.TidalGuillotine1 or (uint)AID.TidalGuillotine2)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class MarineMayhem(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.MarineMayhemVisual, (uint)AID.MarineMayhem, 1.2f);

sealed class PelagicCleaverHydrocleave : Components.SimpleAOEGroups
{
    public PelagicCleaverHydrocleave(BossModule module) : base(module, [(uint)AID.Hydrocleave, (uint)AID.PelagicCleaver], new AOEShapeCone(50f, 30f.Degrees()))
    {
        MaxDangerColor = 1;
    }
}

abstract class OpenWater(BossModule module, int maxCasts, float timeToMove, Angle increment, bool isInner) : Components.GenericAOEs(module)
{
    private readonly int MaxCasts = maxCasts;
    private readonly float TimeToMove = timeToMove;
    private readonly List<AOEInstance> _aoes = new(maxCasts);
    private static readonly AOEShapeCircle circleIn = new(4f), circleOut = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 5 ? 5 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var maxC = Math.Min(max, count - NumCasts);

        if (NumCasts < MaxCasts - 1)
            aoes[NumCasts].Color = Colors.Danger;
        return aoes.Slice(NumCasts, maxC);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OpenWaterVisualFirst)
        {
            var dir = caster.Position - CE111SharkAttack.ArenaCenter;
            var inside = dir.LengthSq() < 225f;
            if (isInner != inside)
                return;
            var inc = (dir.OrthoR().Dot(caster.Rotation.ToDirection()) < 0f ? 1f : -1f) * increment;
            var pos = caster.Position;
            var shape = isInner ? circleIn : circleOut;
            for (var i = 0; i < MaxCasts; ++i)
            {
                var rotate = (pos - CE111SharkAttack.ArenaCenter).Rotate(inc * i) + CE111SharkAttack.ArenaCenter;
                _aoes.Add(new(shape, rotate.Quantized(), default, Module.CastFinishAt(spell, TimeToMove * i)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OpenWater)
        {
            var inside = (caster.Position - CE111SharkAttack.ArenaCenter).LengthSq() < 225f;
            if (isInner != inside)
                return;
            if (++NumCasts == MaxCasts)
            {
                _aoes.Clear();
                NumCasts = 0;
            }
        }
    }
}

// for some reason the inner open water is on an almost perfect circle with 22.5° steps
// but the outer exaflare seems to have huge deviations of about 0.5° or more resulting in noticeable differences
// giving outer exaflares a radius of 5 to compensate for the uncertainity
sealed class OpenWaterInside(BossModule module) : OpenWater(module, 35, 1.2f, 22.5f.Degrees(), true);
sealed class OpenWaterOutside(BossModule module) : OpenWater(module, 59, 0.8f, 12.5f.Degrees(), false);

sealed class CE111SharkAttackStates : StateMachineBuilder
{
    public CE111SharkAttackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<OpenWaterInside>()
            .ActivateOnEnter<OpenWaterOutside>()
            .ActivateOnEnter<PelagicCleaverHydrocleave>()
            .ActivateOnEnter<MarineMayhem>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 41)]
public sealed class CE111SharkAttack(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(-117f, -850f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 19.5f, 64)], [new Rectangle(new(-117f, -827.25f), 5f, 4f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.PetalodusProgeny1));
        Arena.Actor(PrimaryActor);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 20f);
}
