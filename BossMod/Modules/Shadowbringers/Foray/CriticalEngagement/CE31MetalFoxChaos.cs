namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE31MetalFoxChaos;

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
    LaserShowerVisual = 20136, // Boss->self, 3.0s cast, single-target
    LaserShower = 20140, // Helper->location, 5.0s cast, range 10 circle
    Rush = 20139, // Boss->player, 3.0s cast, width 14 rect charge
    SatelliteLaser = 20137 // Boss->self, 10.0s cast, range 100 circle
}

sealed class MagitekBitLasers(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime[] _times = [];
    private Angle startrotation;
    public enum Types { None, SatelliteLaser, DiffractiveLaser, LaserShower }
    public Types Type;
    private static readonly AOEShapeRect rect = new(100f, 3f);
    private static readonly Angle a90 = 90f.Degrees(), a180 = 180f.Degrees();

    private AOEInstance[] _bitAOEs = [];
    private AOEInstance[] BitAOEsFor(int n) => _bitAOEs.Length >= n ? _bitAOEs : (_bitAOEs = new AOEInstance[n]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Type == Types.None)
            return [];

        var bits = Module.Enemies((uint)OID.MagitekBit);
        var count = bits.Count;
        Span<AOEInstance> aoes = BitAOEsFor(count);
        var index = 0;
        var time = WorldState.CurrentTime > _times[0];
        if (Type == Types.SatelliteLaser && time)
        {
            for (var i = 0; i < count; ++i)
            {
                var p = bits[i];
                aoes[index++] = new(rect, p.Position.Quantized(), p.Rotation, _times[1]);
            }
        }
        else if (Type == Types.DiffractiveLaser && time || Type == Types.LaserShower)
        {
            var isMax4 = NumCasts < 5;
            var color = Colors.Danger;
            for (var i = 0; i < count; ++i)
            {
                var p = bits[i];
                var pos = p.Position.Quantized();
                var rot = p.Rotation;
                if (rot.AlmostEqual(startrotation + (isMax4 ? default : a180), Angle.DegToRad))
                    aoes[index++] = new(rect, pos, rot, isMax4 ? _times[1] : _times[3], isMax4 ? color : default);
                else if (NumCasts < 9 && (rot.AlmostEqual(startrotation + a90, Angle.DegToRad) || rot.AlmostEqual(startrotation - a90, Angle.DegToRad)))
                    aoes[index++] = new(rect, pos, rot, _times[2], isMax4 ? default : color);
            }
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Type == Types.None)
        {
            var _time = WorldState.CurrentTime;
            switch (spell.Action.ID)
            {
                case (uint)AID.SatelliteLaser:
                    Type = Types.SatelliteLaser;
                    _times = [_time.AddSeconds(2.5d), _time.AddSeconds(12.3d)];
                    break;
                case (uint)AID.DiffractiveLaser:
                    startrotation = spell.Rotation + a180;
                    Type = Types.DiffractiveLaser;
                    _times = [_time.AddSeconds(2d), _time.AddSeconds(8.8d), _time.AddSeconds(10.6d), _time.AddSeconds(12.4d)];
                    break;
                case (uint)AID.LaserShower:
                    startrotation = caster.Rotation;
                    Type = Types.LaserShower;
                    _times = [_time, _time.AddSeconds(6.5d), _time.AddSeconds(8.3d), _time.AddSeconds(10.1d)];
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RefractedLaser)
        {
            if (++NumCasts == 14)
            {
                NumCasts = 0;
                Type = Types.None;
            }
        }
    }
}

sealed class Rush(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.Rush, 7f);
sealed class LaserShower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserShower, 10f);
sealed class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffractiveLaser, new AOEShapeCone(60f, 75f.Degrees()));
sealed class SatelliteLaser(BossModule module) : Components.RaidwideCast(module, (uint)AID.SatelliteLaser, "Raidwide + all lasers fire at the same time");

sealed class CE31MetalFoxChaosStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 13)] // bnpcname=9424
public sealed class CE31MetalFoxChaos(WorldState ws, Actor primary) : BossModule(ws, primary, new(-234f, 262f), new ArenaBoundsSquare(29.5f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InSquare(Arena.Center, 30f);
}
