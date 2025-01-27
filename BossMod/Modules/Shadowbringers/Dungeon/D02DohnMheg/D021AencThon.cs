﻿namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D021AencThon;

public enum OID : uint
{
    Boss = 0x3F2, // R=2.0
    GeyserHelper1 = 0x1EAAA1, // controls animations for 2 geysers
    GeyserHelper2 = 0x1EAAA2, // controls animations for 3 geysers
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    CandyCane = 8857, // Boss->player, 4.0s cast, single-target
    Hydrofall1 = 8871, // Boss->self, 3.0s cast, single-target
    Hydrofall2 = 8893, // Helper->location, 3.0s cast, range 6 circle
    LaughingLeap1 = 8852, // Boss->location, 4.0s cast, range 4 circle
    LaughingLeap2 = 8840, // Boss->players, no cast, range 4 circle
    Landsblood1 = 7822, // Boss->self, 3.0s cast, range 40 circle
    Landsblood2 = 7899, // Boss->self, no cast, range 40 circle
    Geyser = 8800 // Helper->self, no cast, range 6 circle
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Stackmarker = 62 // player
}

class Landsblood(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Landsblood1), "Raidwides + Geysers");
class CandyCane(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CandyCane));
class Hydrofall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Hydrofall2), 6);
class LaughingLeap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaughingLeap1), 4);
class LaughingLeapStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.LaughingLeap2), 4, 5.15f, 4, 4);

class Geyser(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);

    private static readonly Dictionary<OID, Dictionary<Angle, WPos[]>> GeyserPositions = new()
    {
        {
            OID.GeyserHelper1, new Dictionary<Angle, WPos[]>
            {
                { 0.Degrees(), [new(0, 14.16f), new(-9, 45.16f)] },
                { 180.Degrees(), [new(9, 15.16f), new(0, 46.16f)] },
                { -90.Degrees(), [new(-15, 21.16f), new(16, 30.16f)] },
                { 90.Degrees(), [new(-16, 30.16f), new(15, 39.16f)] }
            }
        },
        {
            OID.GeyserHelper2, new Dictionary<Angle, WPos[]>
            {
                { 0.Degrees(), [new(0, 35.16f), new(-9, 15.16f), new(7, 23.16f)] },
                { 90.Degrees(),  [new(-15, 39.16f), new(-7, 23.16f), new(5, 30.16f)] },
                { 180.Degrees(), [new(9, 45.16f), new(-7, 37.16f), new(0, 25.16f)] },
                { -90.Degrees(), [new(7, 37.16f), new(15, 21.16f), new(-5, 30.16f)] }
            }
        }
    };

    private readonly List<AOEInstance> _geysers = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _geysers.Count;
        if (count == 0)
            yield break;
        for (var i = 0; i < count; ++i)
        {
            var g = _geysers[i];
            yield return new(g.Shape, g.Origin, default, g.Activation, g.Activation == _geysers[0].Activation ? Colors.Danger : Colors.AOE, g.Activation == _geysers[0].Activation);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            if (GeyserPositions.TryGetValue((OID)actor.OID, out var positionsByRotation))
            {
                var activation = WorldState.FutureTime(5.1f);
                foreach (var (rotation, positions) in positionsByRotation)
                    if (actor.Rotation.AlmostEqual(rotation, Angle.DegToRad))
                    {
                        for (var i = 0; i < positions.Length; ++i)
                            _geysers.Add(new(circle, positions[i], default, activation));
                        break;
                    }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_geysers.Count != 0 && (AID)spell.Action.ID == AID.Geyser)
            _geysers.RemoveAt(0);
    }
}

class D021AencThonStates : StateMachineBuilder
{
    public D021AencThonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandyCane>()
            .ActivateOnEnter<Landsblood>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<LaughingLeapStack>()
            .ActivateOnEnter<Geyser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8141)]
public class D021AencThon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(0, 30), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(0, 50), 20, 1), new Rectangle(new(0, 10), 20, 1.4f)]);
}
