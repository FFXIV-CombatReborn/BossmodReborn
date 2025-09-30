namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D103RukshsDheem;

public enum OID : uint
{
    Boss = 0x2CFF, // R4.0
    QueensHarpooner = 0x2D01, // R1.56
    DepthGrip = 0x2D00, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/QueensHarpooner->player, no cast, single-target
    SwiftShift = 19331, // Boss->location, no cast, single-target, teleport

    Bonebreaker = 19340, // Boss->player, 4.0s cast, single-target, tankbuster

    SeabedCeremonyVisual = 19323, // Boss->self, 4.0s cast, single-target
    SeabedCeremony = 19324, // Helper->self, 4.0s cast, range 60 circle

    DepthGrip = 19332, // Boss->self, 4.0s cast, single-target
    Arise = 19333, // DepthGrip->self, no cast, single-target
    WavebreakerVisual1 = 19334, // DepthGrip->self, no cast, single-target
    WavebreakerVisual2 = 19335, // DepthGrip->self, no cast, single-target
    Wavebreaker1 = 13268, // Helper->self, no cast, range 36 width 8 rect
    Wavebreaker2 = 13269, // Helper->self, no cast, range 21 width 10 rect

    FallingWaterVisual = 19325, // Boss->self, 5.0s cast, single-target, spread
    FallingWater = 19326, // Helper->player, 5.0s cast, range 8 circle

    RisingTide = 19339, // Boss->self, 3.0s cast, range 50 width 6 cross

    Meatshield = 19338, // QueensHarpooner->Boss, no cast, single-target
    CoralTrident = 19337, // QueensHarpooner->self, 5.0s cast, range 6 90-degree cone
    Seafoam = 19336, // QueensHarpooner->self, 7.0s cast, range 60 circle

    FlyingFountVisual = 19327, // Boss->self, 5.0s cast, single-target, stack
    FlyingFount = 19328, // Helper->player, 5.0s cast, range 6 circle

    CommandCurrentVisual = 19329, // Boss->self, 4.9s cast, single-target
    CommandCurrent = 19330 // Helper->self, 5.0s cast, range 40 30-degree cone
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            switch (index)
            {
                case 0x17:
                    Arena.Bounds = D103RukshsDheem.NarrowBounds;
                    break;
                case 0x18:
                    Arena.Bounds = D103RukshsDheem.SplitBounds;
                    break;
            }
        }
        else if (state == 0x00080004 && index is 0x017 or 0x18)
            Arena.Bounds = D103RukshsDheem.DefaultBounds;
    }
}

class Wavebreaker(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rectNarrow = new(36f, 4f);
    private static readonly AOEShapeRect rectWide = new(21f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        if (Arena.Bounds == D103RukshsDheem.SplitBounds)
        {
            var aoes1 = new AOEInstance[count];
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                if (i == 0)
                    aoes1[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
                else
                    aoes1[i] = aoe;
            }
            return aoes1;
        }
        if (Arena.Bounds == D103RukshsDheem.DefaultBounds)
            return CollectionsMarshal.AsSpan(_aoes);

        var max = count > 4 ? 4 : count;
        List<AOEInstance> aoes = new(max);
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Rotation == _aoes[0].Rotation)
                if (i == 0)
                    aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
                else
                    aoes.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Arise:
                AddAOEs(caster);
                break;
            case (uint)AID.Wavebreaker1:
            case (uint)AID.Wavebreaker2:
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }

    private void AddAOEs(Actor caster)
    {
        double activation;
        var shape = rectNarrow;

        if (Arena.Bounds == D103RukshsDheem.NarrowBounds)
            activation = _aoes.Count > 3 ? 11.6d : 8d;
        else if (Arena.Bounds == D103RukshsDheem.SplitBounds)
            activation = 7.6d;
        else
        {
            activation = 9.8d;
            shape = rectWide;
        }
        _aoes.Add(new(shape, caster.Position.Quantized(), caster.Rotation, WorldState.FutureTime(activation)));
    }
}

class Drains(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> activeDrains = new(8), solvedDrains = new(4);
    private static readonly float[] xPositions = [-11f, 11f];
    private const float SideLength = 1.25f;
    private static readonly WPos[] drainPositions = GetDrains();
    private static readonly WDir dir = new(default, 1f);
    private DateTime activation;

    private static WPos[] GetDrains()
    {
        var index = 0;
        var drains = new WPos[8];
        for (var i = 0; i < 2; ++i)
        {
            for (var j = 0f; j < 4f; ++j)
            {
                drains[index++] = new(xPositions[i], -465f + j * 10f);
            }
        }
        return drains;
    }

    private static readonly AOEShapeRect square = new(SideLength, SideLength, SideLength);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (activation == default)
            return [];
        var countA = activeDrains.Count;
        var countS = solvedDrains.Count;
        var aoes = new AOEInstance[countA + countS];
        var index = 0;
        for (var i = 0; i < countA; ++i)
            aoes[index++] = new(square, activeDrains[i], color: Colors.SafeFromAOE, risky: false);
        for (var i = 0; i < countS; ++i)
        {
            var drain = solvedDrains[i];
            aoes[index++] = new(square, drain, color: IsBlockingDrain(actor, drain) ? Colors.SafeFromAOE : default, risky: false);
        }
        return aoes;
    }

    public static bool IsBlockingDrain(Actor actor, WPos pos) => actor.Position.InRect(pos, dir, SideLength, SideLength, SideLength);

    public override void OnMapEffect(byte index, uint state)
    {
        // 0x00020001 enabled, 0x00080004 temp disabled, 0x00200004 perm disabled disabled
        // 0x0F - -11, -465 to 0x12 - -11, -435
        // 0x13 - 11, -465 to 0x16 - 11, -435
        if (index is < 0x0F or > 0x16)
            return;

        var positionIndex = index - 0x0F;
        var drainPosition = drainPositions[positionIndex];

        switch (state)
        {
            case 0x00020001u:
                activeDrains.Add(drainPosition);
                solvedDrains.Remove(drainPosition);
                if (activation == default)
                    activation = WorldState.FutureTime(12d); // 16s if all 8 drains are active
                break;

            case 0x00080004u:
                solvedDrains.Add(drainPosition);
                activeDrains.Remove(drainPosition);
                break;

            case 0x00200004u:
                activeDrains.Clear();
                solvedDrains.Clear();
                activation = default;
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.QueensHarpooner) // there doesn't seem to be any ENVC or the like when mechanic ends
            activation = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (activation == default)
            return;
        var countS = solvedDrains.Count;
        for (var i = 0; i < countS; ++i)
        {
            var drain = solvedDrains[i];
            if (IsBlockingDrain(actor, drain))
            {
                hints.AddForbiddenZone(new SDInvertedRect(drain, dir, SideLength, SideLength, SideLength), activation);
                return; // can only block one drain at a time, no point to keep checking
            } // TODO: consider checking if more than one actor is on a drain and go somewhere else? might not help with multiboxing if every client tries to move to a different one... config might be better if needed
        }
        var countA = activeDrains.Count;
        var forbidden = new ShapeDistance[countA];
        for (var i = 0; i < countA; ++i)
            forbidden[i] = new SDInvertedRect(activeDrains[i], dir, SideLength, SideLength, SideLength);
        if (forbidden.Length != 0)
            hints.AddForbiddenZone(new SDIntersection(forbidden), activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        var isBlocking = false;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(actor.Position))
            {
                isBlocking = true;
                break;
            }
        }
        hints.Add("Block a drain!", isBlocking);
    }
}

class SeabedCeremony(BossModule module) : Components.RaidwideCast(module, (uint)AID.SeabedCeremony);
class Seafoam(BossModule module) : Components.RaidwideCast(module, (uint)AID.Seafoam);
class Bonebreaker(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Bonebreaker);
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FallingWater, 8f);
class FlyingFount(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FlyingFount, 6f);
class CommandCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CommandCurrent, new AOEShapeCone(40f, 15f.Degrees()));
class CoralTrident(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CoralTrident, new AOEShapeCone(6f, 45f.Degrees()));
class RisingTide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RisingTide, new AOEShapeCross(50f, 3f));

class D103RukshsDheemStates : StateMachineBuilder
{
    public D103RukshsDheemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<SeabedCeremony>()
            .ActivateOnEnter<Seafoam>()
            .ActivateOnEnter<Bonebreaker>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<FlyingFount>()
            .ActivateOnEnter<CommandCurrent>()
            .ActivateOnEnter<CoralTrident>()
            .ActivateOnEnter<RisingTide>()
            .ActivateOnEnter<Wavebreaker>()
            .ActivateOnEnter<Drains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9264)]
public class D103RukshsDheem(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(default, -450f);
    private const float X = 15.5f;
    private const float Z = 19.5f;
    public static readonly ArenaBoundsRect DefaultBounds = new(X, Z);
    public static readonly ArenaBoundsRect NarrowBounds = new(X - 7.5f, Z);
    public static readonly ArenaBoundsCustom SplitBounds = new([new Rectangle(ArenaCenter, X, Z)], [new Rectangle(ArenaCenter, X, 4f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.QueensHarpooner));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.QueensHarpooner => 1,
                _ => 0
            };
        }
    }
}
