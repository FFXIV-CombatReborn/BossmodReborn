namespace BossMod.Dawntrail.Trial.T07Doomtrain;

sealed class ElectrayShort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrayShort, new AOEShapeRect(10f, 2.5f));
sealed class ElectrayMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrayMedium, new AOEShapeRect(15f, 2.5f));
sealed class ElectrayLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrayLong, new AOEShapeRect(25f, 2.5f));
sealed class LightningBurstTankBuster(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.LightningBurstIcon, (uint)AID.LightningBurst, 5f, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

// For first pass the knockback distance is estimated.
sealed class LightningExpress(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.LightningExpress, 16f, kind: Kind.DirForward);

/**
 * Plasma beams have a cast of 0.7 seconds. The Levin signal actors spawn about 7 seconds earlier.
 * We can extrapolate the plasma beam casts based off levin signal positions. Then we create the aoe
 * instances ahead of time so we have time to move out of the way.
 */
class LevinSignal(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect10 = new(10f, 2.5f), rect20 = new(20f, 2.5f), rect30 = new(30f, 2.5f);
    private readonly List<(Actor Caster, DateTime Activation, AOEShapeRect Shape, float Offset)> _casters = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LevinSignal)
        {
            // if actor.PosRot.Y then they are at ground level
            var ground = actor.PosRot.Y < 2;

            var car = Module.FindComponent<CarGeometry>();
            // set default value for ground to 30 and upperdeck to 0 cull levin signal that are elevated and not over platforms.
            var maxLen = ground ? 30 : 0;
            var offset = 0;
            var posX = actor.PosRot.X;
            if (car?.Car == 2)
            {
                if (posX is > 95f and < 100)
                {
                    maxLen = 20;
                }
                else if (posX is > 100f and < 105)
                {
                    maxLen = 10;
                }
            }
            // car 3 has two platforms of length 20
            if (car?.Car == 3)
            {
                // left side platforms
                if (posX < 95f)
                {
                    if (ground)
                    {
                        maxLen = 10;
                    }
                    else
                    {
                        maxLen = 20;
                        offset = 10;
                    }
                }
                // right side platforms
                else if (posX > 105f)
                {
                    if (ground)
                    {
                        maxLen = 10;
                    }
                    else
                    {
                        maxLen = 20;
                        offset = 10;
                    }
                }
            }
            if (car?.Car == 5)
            {
                // left side platform, length 10
                if (posX < 95f)
                {
                    if (ground)
                    {
                        maxLen = 20;
                    }
                    else
                    {
                        maxLen = 10;
                        offset = 20;
                    }
                }
                // right side platform
                else if (posX > 105f)
                {
                    if (ground)
                    {
                        maxLen = 10;
                    }
                    else
                    {
                        maxLen = 10;
                        offset = 10;
                    }
                }
            }
            if (maxLen != 0) // elevated signals that are not over a platform can't hit anyone
            {
                _casters.Add((actor, WorldState.FutureTime(7d), maxLen == 10 ? rect10 : maxLen == 20 ? rect20 : rect30, offset));
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];

            aoes[i] = new(c.Shape, c.Caster.Position + new WDir(default, c.Offset), default, c.Activation);
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PlasmaBeamLower or (uint)AID.PlasmaBeamMedium or (uint)AID.PlasmaBeamShort or (uint)AID.PlasmaBeamUpper)
        {
            for (var i = _casters.Count - 1; i >= 0; --i)
            {
                if (_casters[i].Caster == caster)
                {
                    _casters.RemoveAt(i);
                }
            }
            ++NumCasts;
        }
    }
}

// TODO: Show the indicator arrow.
sealed class WindpipeDrawIn(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WindpipeDrawIn, 20f)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;
    private List<Knockback> _kb = [];

    // need safe walls for car 2 and car 5
    private static readonly SafeWall[] safeWalls2 = [new(new(100.1f, 150.1f), new(104.9f, 150.1f)), new(new(95.1f, 160.1f), new(99.9f, 160.1f))];
    private static readonly SafeWall[] safeWalls5 = [new(new(105.1f, 305.1f), new(109.9f, 305.1f))];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindpipeDrawIn)
        {
            switch (_car.Car)
            {
                case 2:
                    _kb = [new(spell.LocXZ, 20f, Module.CastFinishAt(spell), safeWalls: safeWalls2, kind: Kind.DirBackward, ignoreImmunes: true)];
                    break;
                case 5:
                    _kb = [new(spell.LocXZ, 20f, Module.CastFinishAt(spell), safeWalls: safeWalls5, kind: Kind.DirBackward, ignoreImmunes: true)];
                    break;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindpipeDrawIn)
        {
            _kb.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Count != 0)
        {
            var kb = _kb[0]; // single knockback source, not indexed by player slot
            if (!IsImmune(slot, WorldState.CurrentTime))
            {
                // It is a draw in, we use mirrorZ to make safe zones to keep us from moving towards boss.
                var dir = kb.Direction.ToDirection().MirrorZ();
                var walls = _car.Car == 2 ? safeWalls2 : safeWalls5;
                hints.AddForbiddenZone(new SDKnockbackFixedDirectionAgainstSafewalls(dir, walls, 20f, walls.Length), WorldState.CurrentTime);
            }
        }
    }
}

sealed class Blastpipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Blastpipe, new AOEShapeRect(10f, 10f), riskyWithSecondsLeft: 3);

sealed class UnlimitedExpress(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnlimitedExpress);

/**
 * Show the respective aoe shapes for Thunderous breath and Headlight.
 */
sealed class HeadOnEmission(BossModule module) : Components.GenericAOEs(module)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;
    private readonly List<AOEInstance> _aoes = [];
    //private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.ThunderousBreathLowerDeck)
        {
            _aoes.Add(new(_car.GroundShape, Module.Arena.Center));
        }
        else if (id == (uint)AID.HeadlightUpperDeck)
        {
            _aoes.Add(new(_car.AirShape, Module.Arena.Center));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ThunderousBreathLowerDeck or (uint)AID.HeadlightUpperDeck)
        {
            _aoes.Clear();
        }
    }
}

// Aether surge : multiple cone aoe from center outwards. 6.0s cast, range 30 45.000-degree cone
sealed class AetherSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherSurge, new AOEShapeCone(15f, 22.5f.Degrees()), riskyWithSecondsLeft: 6d);

/**
 * Ghost train targets a party member inside the arena with Aetherial Ray:
 * a range 50 cone from Ghost train to target party member.
 * Ghost train gets an additional status 'Distance' to determine how much they will rotate around the arena between
 * cast start and end.
 * SID.Distance = 4541
 * SID.Distance.Extra 0x578 : 170 degree,
 * SID.Distance.Extra 0x960 (106 degree rotation)
 */
class AetherialRay(BossModule module) : Components.GenericBaitAway(module, (uint)AID.AetherialRay, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Angle _nextRotation;
    private Actor? _nextTarget;
    private DateTime _nextActivation;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.Distance)
        {
            _nextRotation = status.Extra switch
            {
                0x578 => 170.Degrees(),
                0x960 => 106.Degrees(),
                _ => default
            };
            if (_nextRotation == default)
                ReportError($"Unrecognized status {status.Extra:X} on Ghost Train, don't know where to predict");
        }

        if ((SID)status.ID == SID.Stop)
        {
            foreach (ref var bait in CollectionsMarshal.AsSpan(CurrentBaits))
                bait.Source = actor;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Horn)
        {
            _nextTarget = WorldState.Actors.Find(targetID);
            _nextActivation = WorldState.FutureTime(9.7f);
        }
        // source is GhostTrain, _nextTarget is the party member that is dragging the cone marker
        if ((IconID)iconID == IconID.Horn && _nextTarget != null)
        {
            CurrentBaits.Add(new(actor, _nextTarget, new AOEShapeCone(50, 17.5f.Degrees()), _nextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

//RunawayTrainRaidwide = 45645, // 233C->self, no cast, range 20 circle
sealed class RunawayTrainRaidwide(BossModule module) : Components.RaidwideInstant(module, (uint)AID.RunawayTrainRaidwide, 15.2d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.RunawayTrainVisual)
            Activation = WorldState.FutureTime(Delay);
    }
}

// arcane revelation. This is the spell with the diamond pattern on the floor and the lightning
// traverses n points to cast Hail of thunder.  Use the actor OID.ArcaneRevelation to mark the indicator for hail of thunder.
// The circle is so big that pc gets trapped in a corner pocket on the wrong end
// Would be good to have some logic that can avoid the center radius of aoe and be +/- range of
// middle of car to more easily avoid pathing into crush zone.
// like if bait.center.z is > arena.center.z: path to safezones north, else path safezone south
// if bait.center.x > arena.center.x: path to safezones.west, else path safezone east
sealed class ArcaneRevelation(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    //when arcane revelation mob created, add bait.
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneRevelation)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCircle(16f)));
    }
    //when arcane revelation mob is gone, remove it.
    public override void Update()
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        var adds = Module.Enemies((uint)OID.ArcaneRevelation);
        var count = adds.Count;
        for (var i = 0; i < count; ++i)
        {
            if (!adds[i].IsDead)
            {
                return;
            }
        }
        CurrentBaits.Clear(); // if adds die baits get cancelled
    }
}

// HailOfThunder, kept this separate because it highlights the difference between the arcane revelation
// bait indicator and the HailOfThunder cast.
sealed class HailOfThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HailOfThunder, new AOEShapeCircle(16f), riskyWithSecondsLeft: 3.2d);

sealed class Electray3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray3, new AOEShapeRect(20f, 2.5f), riskyWithSecondsLeft: 5d);

sealed class ElectrayUpper(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(10f, 2.5f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrayUpper)
        {
            // make the rectangle lengthFront shorter because it only needs cover the platform from edge of arena.
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrayUpper)
        {
            _aoes.Clear();
        }
    }
}

sealed class RunawayTrain(BossModule module) : Components.RaidwideCast(module, (uint)AID.RunawayTrain);

sealed class Shockwave(BossModule module) : Components.RaidwideInstant(module, (uint)AID.Shockwave, 5.8d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
            Activation = WorldState.FutureTime(Delay);
    }
}

// Rectangle that covers all of car 4, portal opens in the back to escape to car 5
sealed class Derail(BossModule module) : Components.GenericAOEs(module)
{
    // Cheat by making portal visible as an inverted danger zone at WPos
    private readonly WPos escapePortal = new(100f, 262.5f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Derail)
        {
            _aoes.Add(new(new AOEShapeDonut(2, 32), escapePortal, default, Module.CastFinishAt(spell, 0.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Derail)
        {
            _aoes.Clear();
        }
    }
}

// Derailment Siege stack
sealed class DerailmentSiegeCircle(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DerailmentSiegeCircle, 5, 8);

// Would be cleaner probably to have all arena changes tied to a ENVC state switch.
// TODO should probably move this over to geometry with the rest of the arena changes.
sealed class ArenaChangeCar4(BossModule module) : BossComponent(module)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Derail)
        {
            _car.Car = 5;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.Doomtrain, Contributors = "wen, Xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1076u, NameID = 14284u)]
public class T07Doomtrain(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(10f, 15f))
{
    public Actor? AetherIntermission;
    private Actor? ghostTrain;

    protected override void UpdateModule()
    {
        AetherIntermission ??= GetActor((uint)OID.AetherIntermission);
        ghostTrain ??= GetActor((uint)OID.GhostTrain);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(AetherIntermission);
        Arena.Actor(ghostTrain, Colors.Object, true);
        // use this to see where the arcane revelation aoe center is floating.
        Arena.Actors(Enemies((uint)OID.ArcaneRevelation), Colors.Object, true);
    }
}
