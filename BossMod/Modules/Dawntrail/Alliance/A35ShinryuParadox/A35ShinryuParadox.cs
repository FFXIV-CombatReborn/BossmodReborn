namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

// Puts AOE or Safezone over launchpad to avoid room wide aoe
sealed class FloorAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoes = [];
    private readonly AOEShapeDonut bottomTeleporter = new(2f, 50f);
    private readonly AOEShapeDonut topHole = new(6f, 50f);
    private readonly WPos holePos = new(820f, -826f);
    private bool twilight;
    private int wantLayer = -1;
    private BitMask _dark;
    private BitMask _light;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (twilight && _aoes.Length == 2)
        {
            var shouldBeTop = _light[slot];
            if (!shouldBeTop && !_dark[slot])
            {
                return []; // player must have died during debuff application and doesn't care
            }
            _topHoleSD ??= topHole.Distance(holePos, default);
            _topHoleInvSD ??= topHole.InvertedDistance(holePos, default);
            _bottomSD ??= bottomTeleporter.Distance(holePos, default);
            _bottomInvSD ??= bottomTeleporter.InvertedDistance(holePos, default);
            ref var aoe0 = ref _aoes[0];
            var safeColor = Colors.SafeFromAOE;
            aoe0.ShapeDistance = shouldBeTop ? _topHoleSD : _topHoleInvSD;
            aoe0.Color = shouldBeTop ? safeColor : default;
            ref var aoe1 = ref _aoes[1];
            aoe1.ShapeDistance = !shouldBeTop ? _bottomSD : _bottomInvSD;
            aoe1.Color = !shouldBeTop ? safeColor : default;
        }
        return _aoes;
    }

    private ShapeDistance? _topHoleSD, _topHoleInvSD, _bottomSD, _bottomInvSD;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.CosmicBreath:
            case (uint)AID.TwilightNebula1:
                if (id == (uint)AID.TwilightNebula1)
                {
                    twilight = true;
                }
                var act = Module.CastFinishAt(spell);
                _aoes =
                [
                    new(topHole, holePos, default, act, shapeDistance: topHole.Distance(holePos, default), arenaProjectionLayer: 1, restrictToArenaProjectionLayer: true),
                    new(bottomTeleporter, holePos, default, act, color: Colors.SafeFromAOE, shapeDistance: bottomTeleporter.InvertedDistance(holePos, default), arenaProjectionLayer: 0, restrictToArenaProjectionLayer: true),
                ];
                wantLayer = 0;
                break;
            case (uint)AID.CosmicTail:
            case (uint)AID.AtomicTail:
                var act2 = Module.CastFinishAt(spell);
                _aoes =
                [
                    new(bottomTeleporter, holePos, default, act2, shapeDistance: bottomTeleporter.Distance(holePos, default), arenaProjectionLayer: 0, restrictToArenaProjectionLayer: true),
                    new(topHole, holePos, default, act2, color: Colors.SafeFromAOE, shapeDistance: topHole.InvertedDistance(holePos, default), arenaProjectionLayer: 1, restrictToArenaProjectionLayer: true),
                ];
                wantLayer = 1;
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id == (uint)SID.CloakOfWaningLight)
        {
            SetBitMask(ref _light, actor.InstanceID);
        }
        else if (id == (uint)SID.CloakOfWaxingDark)
        {
            SetBitMask(ref _dark, actor.InstanceID);
        }
        void SetBitMask(ref BitMask mask, ulong targetID) => mask.Set(Raid.FindSlot(targetID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.CosmicBreath or (uint)AID.CosmicTail or (uint)AID.AtomicTail or (uint)AID.TwilightNebula1)
        {
            _aoes = [];
            if (id == (uint)AID.TwilightNebula1)
            {
                twilight = false;
                _dark.Reset();
                _light.Reset();
                ++NumCasts;
            }
            wantLayer = -1;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (wantLayer == -1)
        {
            return;
        }

        var onBottomFloor = actor.PosRot.Y < -890f;

        bool? shouldBeOnBottom = twilight ? _light[slot] ? false
            : _dark[slot] ? true
            : null
            : wantLayer == 0;

        if (shouldBeOnBottom is bool bottom)
        {
            hints.Add(bottom ? "Be on bottom floor!" : "Be on top floor!", onBottomFloor != bottom);
        }
    }
}

sealed class P2ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x02000100u)
        {
            Arena.Bounds = new ArenaBoundsRect(30f, 20f) { Y = -878.9f, WorldProjectionHeight = 0f, BorderY = -879f };
        }
    }
}

// Starflare: Two sets of crisscrossing line AoE telegraphs, hitting both levels at once.
sealed class StarflareP1(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.StarflareP1Fast, (uint)AID.StarflareP1Slow], new AOEShapeRect(60f, 5f), 10, 20, arenaProjectionLayers: [-900f, -879f]);

sealed class StarflareP2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.StarflareP2Fast, (uint)AID.StarflareP2Slow], new AOEShapeRect(60f, 5f), 5, 10);

// Icon to look away from the boss
sealed class VortexGaze(BossModule module) : Components.GenericGaze(module)
{
    private BitMask _affectedLook;
    private BitMask _affectedLookAway;
    private Eye[] _eyeLook = [];
    private Eye[] _eyeLookAway = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_affectedLook[slot])
        {
            return _eyeLook;
        }
        else if (_affectedLookAway[slot])
        {
            return _eyeLookAway;
        }
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.NoLook)
        {
            SetBitMaskAndAddGaze(ref _affectedLookAway, actor.InstanceID, ref _eyeLookAway);
        }
        else if (iconID == (uint)IconID.Look)
        {
            SetBitMaskAndAddGaze(ref _affectedLook, actor.InstanceID, ref _eyeLook, true);
        }
        void SetBitMaskAndAddGaze(ref BitMask mask, ulong targetID, ref Eye[] eye, bool inverted = false)
        {
            mask.Set(Raid.FindSlot(targetID));
            if (eye.Length == 0)
            {
                var loc = Module.PrimaryActor.Position.Quantized();
                eye = [new(loc, WorldState.FutureTime(7.1d), eyeCenter: IndicatorWorldPos(loc), inverted: inverted)];
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CataclysmicVortexVisual1 or (uint)AID.CataclysmicBladeVisual)
        {
            _affectedLook.Reset();
            _affectedLookAway.Reset();
            _eyeLook = [];
            _eyeLookAway = [];
        }
    }
}

sealed class VortexStayMove(BossModule module) : Components.StayMove(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.NoMove)
        {
            SetPlayerState(Requirement.Stay, actor.InstanceID);
        }
        else if (iconID == (uint)IconID.Move)
        {
            SetPlayerState(Requirement.Move, actor.InstanceID);
        }
        void SetPlayerState(Requirement req, ulong targetID)
        {
            var state = new PlayerState(req, WorldState.FutureTime(7.1d));
            SetState(Raid.FindSlot(targetID), in state);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CataclysmicVortexVisual1 or (uint)AID.CataclysmicBladeVisual)
        {
            Array.Clear(PlayerStates);
        }
    }
}

sealed class UpDownCounter(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.CosmicBreath, (uint)AID.CosmicTail]);

sealed class DarkNova(BossModule module) : Components.BaitAwayIconMulti(module, 6f, (uint)IconID.Tankbuster, [(uint)AID.DarkNova, (uint)AID.DarkNovaP2],
    centerAtTarget: true, restrictToArenaProjectionLayer: true, damageType: AIHints.PredictedDamageType.Tankbuster);

/*
 * Spawns eight towers at the north. Tower will appear red (if empty) or blue (if taken).
 * One random player in each tower will be temporarily incapacitated with Clashing then take
 * heavy damage, receive a 20-second HP Recovery Down debuff, and be knocked back slightly.
 * Another set of eight towers will then spawn and should be taken by players without the debuff.
 */
sealed class CelestialTrail(BossModule module) : Components.CastTowers(module, (uint)AID.CelestialTrailTower, 2f, 1, 1)
{
    private BitMask _forbidden;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HPRecoveryDown)
        {
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HPRecoveryDown)
        {
            _forbidden.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                towers[i].ForbiddenSoakers = _forbidden;
            }
        }
    }
}

sealed class EmptyProclamation(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyProclamation);
sealed class Swordscross1(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwordscross1, (uint)AID.LeftSwordscross1], new AOEShapeRect(60f, 15f));
sealed class Swordscross2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwordscross2, (uint)AID.LeftSwordscross2], new AOEShapeRect(70f, 18f));
sealed class TwinBlaze1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBlazeIn, new AOEShapeDonutSector(20f, 60f, 45f.Degrees()));
sealed class TwinBlaze2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBlazeOut, new AOEShapeCone(35f, 45f.Degrees()));
sealed class CataclysmicBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CataclysmicBladeCone, new AOEShapeCone(60f, 22.5f.Degrees()));

sealed class Burst(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Burst1)
        {
            AddSequence(caster.Position, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.Burst1 => 0,
            (uint)AID.Burst2 => 1,
            (uint)AID.Burst3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2d));
    }
}

sealed class CosmicFlame(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CosmicFlameFirst)
        {
            Lines.Add(new(caster.Position, caster.Rotation.ToDirection() * 8f, Module.CastFinishAt(spell), 2.1d, 8, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CosmicFlameFirst or (uint)AID.CosmicFlameRest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class AtomicRay(BossModule module) : Components.GenericAOEs(module, (uint)AID.AtomicRay)
{
    private readonly AOEShapeRect rect = new(60f, 7.5f);
    // readonly List<(Actor Caster, WPos StartPos, WDir StartMove)> _recorded = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe.Activation = Module.CastFinishAt(spell);
                    var loc = spell.LocXZ;
                    var rot = spell.Rotation;
                    aoe.Origin = loc;
                    aoe.Rotation = rot;
                    aoe.ShapeDistance = rect.Distance(loc, rot);
                    break;
                }
            }
            // var ix = _recorded.FindIndex(p => p.Caster == caster);
            // if (ix >= 0)
            // {
            //     var (p, s, m) = _recorded[ix];
            //     _recorded.RemoveAt(ix);
            //     ReportError($"{p} casting at {caster.Position}, starting was {s} going {m}");
            // }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _aoes.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.Countdown)
        {
            return;
        }

        var position = actor.Position;
        var movement = actor.LastFrameMovement;
        // _recorded.Add((actor, position, movement));

        var destination = PredictDestination(position, movement);

        if (destination is WPos predicted)
        {
            var pos = predicted.Quantized();
            _aoes.Add(new(rect, pos, actor.Rotation, WorldState.FutureTime(6.7d), actorID: actor.InstanceID));
        }
        // else
        // {
        //     ReportError($"not sure what to predict for orb at {position} with movement {movement}");
        // }
    }

    private static WPos? PredictDestination(WPos position, WDir movement)
    {
        var mX = movement.X;
        var mZ = movement.Z;
        if (position.AlmostEqual(new(800f, -840f), 2f) && mX > 0f)
        {
            return new(842.5f, -840f);
        }
        if (position.AlmostEqual(new(826f, -840f), 2f) && mX < 0f)
        {
            return new(812.5f, -840f);
        }
        if (position.AlmostEqual(new(850f, -820f), 2f))
        {
            return new(850f, movement.Z < 0f ? -806f : -834f);
        }
        if (position.AlmostEqual(new(850f, -832f), 2f))
        {
            return new(850f, -820f);
        }
        if (position.AlmostEqual(new(814f, -840f), 2f) && mX > 0f)
        {
            return new(827.5f, -840f);
        }
        if (position.AlmostEqual(new(840f, -840f), 2f) && mX < 0f)
        {
            return new(797.5f, -840f);
        }
        if (position.AlmostEqual(new(850f, -808f), 2f) && mZ < 0f)
        {
            return new(850f, -820f);
        }
        return null;
    }
}

sealed class GyreCharge(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AtomicTailVisual1, (uint)AID.GyreCharge, 6.3d);

sealed class SuperNova(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.SuperNova, 6f, 6.1d, minStackSize: 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction)
        {
            ++NumCasts;
            if (NumCasts >= 3)
            {
                Stacks.Clear();
                NumFinishedStacks++;
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.ShinryuParadox, Contributors = "Xan, ported by wen", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14729u)]
public sealed class A35ShinryuParadox : BossModule
{
    // Set up base logic for what level of arena and which phase boss pc is fighting.
    public A35ShinryuParadox(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A35ShinryuParadox(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var center = new WPos(820f, -820f);
        var shape = new Rectangle(center, 30f, 20f);
        var poly = new RelSimplifiedComplexPolygon(shape.Contour(center));
        var arena = new ArenaBoundsCustom([shape], WorldProjectionLayers: [new(poly, -900f), new(poly, -878.9f, 0f,
        borderY: -879f, arenaStencilExclusions: [new Polygon(new(820f, -826f), 6f, 64)])]); // -900f bottom, -879f top floor
        return (arena.Center, arena);
    }

    public Actor? Groin;
    public Actor? BossP2;
    public Actor? BossP2M() => BossP2;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actor(BossP2);
    }

    protected override void UpdateModule()
    {
        Groin ??= GetActor((uint)OID.ShinryuGroin);
        BossP2 ??= GetActor((uint)OID.HollowKing);
    }

    // If we are on the 0 level we fight the tail.
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var pBoss = 0;
        var pTail = AIHints.Enemy.PriorityInvincible;
        if (actor.PosRot.Y < -890f)
        {
            (pTail, pBoss) = (pBoss, pTail);
        }

        hints.SetPriority(PrimaryActor, pBoss);
        hints.SetPriority(Groin, pTail);
    }
}
