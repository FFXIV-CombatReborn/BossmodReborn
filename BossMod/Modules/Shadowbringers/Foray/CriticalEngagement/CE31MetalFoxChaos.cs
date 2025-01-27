﻿namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE31MetalFoxChaos;

public enum OID : uint
{
    Boss = 0x2DB5, // R=8.0
    MagitekBit = 0x2DB6, // R=1.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 20192, // MagitekBit->location, no cast, ???

    DiffractiveLaser = 20138, // Boss->self, 7.0s cast, range 60 150-degree cone
    RefractedLaser = 20141, // MagitekBit->self, no cast, range 100 width 6 rect
    LaserShower = 20136, // Boss->self, 3.0s cast, single-target
    LaserShower2 = 20140, // Helper->location, 5.0s cast, range 10 circle
    Rush = 20139, // Boss->player, 3.0s cast, width 14 rect charge
    SatelliteLaser = 20137 // Boss->self, 10.0s cast, range 100 circle
}

class MagitekBitLasers(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<DateTime> _times = [];
    private Angle startrotation;
    public enum Types { None, SatelliteLaser, DiffractiveLaser, LaserShower }
    public Types Type;

    private static readonly AOEShapeRect rect = new(100, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_times.Count > 0)
            foreach (var p in Module.Enemies(OID.MagitekBit))
            {
                if (Type == Types.SatelliteLaser && WorldState.CurrentTime > _times[0])
                    yield return new(rect, p.Position, p.Rotation, _times[1]);
                if (Type == Types.DiffractiveLaser && WorldState.CurrentTime > _times[0] || Type == Types.LaserShower)
                {
                    if (NumCasts < 5 && p.Rotation.AlmostEqual(startrotation, Angle.DegToRad))
                        yield return new(rect, p.Position, p.Rotation, _times[1], Colors.Danger);
                    if (NumCasts < 5 && (p.Rotation.AlmostEqual(startrotation + 90.Degrees(), Angle.DegToRad) || p.Rotation.AlmostEqual(startrotation - 90.Degrees(), Angle.DegToRad)))
                        yield return new(rect, p.Position, p.Rotation, _times[2]);
                    if (NumCasts >= 5 && NumCasts < 9 && (p.Rotation.AlmostEqual(startrotation + 90.Degrees(), Angle.DegToRad) || p.Rotation.AlmostEqual(startrotation - 90.Degrees(), Angle.DegToRad)))
                        yield return new(rect, p.Position, p.Rotation, _times[2], Colors.Danger);
                    if (NumCasts >= 5 && NumCasts < 9 && p.Rotation.AlmostEqual(startrotation + 180.Degrees(), Angle.DegToRad))
                        yield return new(rect, p.Position, p.Rotation, _times[3]);
                    if (NumCasts >= 9 && p.Rotation.AlmostEqual(startrotation + 180.Degrees(), Angle.DegToRad))
                        yield return new(rect, p.Position, p.Rotation, _times[3], Colors.Danger);
                }
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var _time = WorldState.CurrentTime;
        if ((AID)spell.Action.ID == AID.SatelliteLaser)
        {
            Type = Types.SatelliteLaser;
            _times.Add(_time.AddSeconds(2.5f));
            _times.Add(_time.AddSeconds(12.3f));
        }
        else if ((AID)spell.Action.ID == AID.DiffractiveLaser)
        {
            DateTime[] times = [_time.AddSeconds(2), _time.AddSeconds(8.8f), _time.AddSeconds(10.6f), _time.AddSeconds(12.4f)];
            startrotation = Angle.AnglesCardinals.FirstOrDefault(r => spell.Rotation.AlmostEqual(r, Angle.DegToRad)) + 180.Degrees();
            Type = Types.DiffractiveLaser;
            _times.AddRange(times);
        }
        else if ((AID)spell.Action.ID == AID.LaserShower2)
        {
            DateTime[] times = [_time, _time.AddSeconds(6.5f), _time.AddSeconds(8.3f), _time.AddSeconds(10.1f)];
            startrotation = Angle.AnglesCardinals.FirstOrDefault(r => caster.Rotation.AlmostEqual(r, Angle.DegToRad));
            Type = Types.LaserShower;
            _times.AddRange(times);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RefractedLaser)
        {
            ++NumCasts;
            if (NumCasts == 14)
            {
                NumCasts = 0;
                _times.Clear();
                Type = Types.None;
            }
        }
    }
}

class Rush(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.Rush), 7);
class LaserShower(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaserShower2), 10);
class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeCone(60, 75.Degrees()));
class SatelliteLaser(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SatelliteLaser), "Raidwide + all lasers fire at the same time");

class CE31MetalFoxChaosStates : StateMachineBuilder
{
    public CE31MetalFoxChaosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SatelliteLaser>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<LaserShower>()
            .ActivateOnEnter<MagitekBitLasers>()
            .ActivateOnEnter<Rush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 13)] // bnpcname=9424
public class CE31MetalFoxChaos(WorldState ws, Actor primary) : BossModule(ws, primary, new(-234, 262), new ArenaBoundsSquare(30));
