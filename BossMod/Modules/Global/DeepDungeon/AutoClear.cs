using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;

using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

enum OID : uint
{
    CairnPalace = 0x1EA094,
    BeaconHoH = 0x1EA9A3,
    PylonEO = 0x1EB867,
    SilverCoffer = 0x1EA13D,
    GoldCoffer = 0x1EA13E,
    BandedCofferIndicator = 0x1EA1F6,
    BandedCoffer = 0x1EA1F7,
}

enum SID : uint
{
    Silence = 7,
    Pacification = 620,
    ItemPenalty = 1094,

    PhysicalDamageUp = 53,
    DamageUp = 61,
    DreadBeastAura = 2056, // unnamed status, displays red fog vfx on actor
    EvasionUp = 2402, // applied by Peculiar Light from orthos diplocaulus

    StoneCurse = 437, // petrification (on enemies)
    AutoHealPenalty = 1097,
}

public abstract class AutoClear : ZoneModule
{
    public readonly int LevelCap;

    public static readonly HashSet<uint> BronzeChestIDs = [
        // PotD
        782, 783, 784, 785, 786, 787, 788, 789, 790, 802, 803, 804, 805,
        // HoH
        1036, 1037, 1038, 1039, 1040, 1041, 1042, 1043, 1044, 1045, 1046, 1047, 1048, 1049,
        // EO
        1541, 1542, 1543, 1544, 1545, 1546, 1547, 1548, 1549, 1550, 1551, 1552, 1553, 1554
    ];
    public static readonly HashSet<uint> RevealedTrapOIDs = [0x1EA08E, 0x1EA08F, 0x1EA090, 0x1EA091, 0x1EA092, 0x1EA9A0, 0x1EB864];

    protected readonly List<(Actor Source, float Inner, float Outer)> Donuts = [];
    protected readonly List<(Actor Source, float Radius)> Circles = [];
    protected readonly List<(Actor Source, float Radius)> KnockbackZones = [];
    protected readonly List<(Actor Source, AOEShape Zone)> Voidzones = [];
    private readonly List<Gaze> Gazes = [];
    protected readonly List<Actor> Interrupts = [];
    protected readonly List<Actor> Stuns = [];
    protected readonly List<(Actor Actor, DateTime Timeout)> Spikes = [];
    protected readonly List<Actor> HintDisabled = [];
    private readonly List<Actor> LOS = [];
    private readonly List<WPos> IgnoreTraps = [];

    private readonly Dictionary<ulong, (WPos, Bitmap)> _losCache = [];

    public record class Gaze(Actor Source, AOEShape Shape);

    protected static readonly AutoDDConfig Config = Service.Config.Get<AutoDDConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly WPos[] _trapsCurrentZone = [];

    private readonly Dictionary<ulong, PomanderID> _chestContentsGold = [];
    private readonly Dictionary<ulong, int> _chestContentsSilver = [];
    private readonly HashSet<ulong> _openedChests = [];
    private readonly HashSet<ulong> _fakeExits = [];
    private PomanderID? _lastChestContentsGold;
    private bool _lastChestMagicite;
    private bool _trapsHidden = true;

    private readonly List<(Wall Wall, bool Rotated)> Walls = [];
    private readonly List<WPos> RoomCenters = [];
    private readonly List<WPos> ProblematicTrapLocations = [];

    private int Kills;
    private int DesiredRoom;
    private bool BetweenFloors;

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public bool KnockbackPenalty;

        public readonly bool ImmuneAt(DateTime time) => KnockbackPenalty || RoleBuffExpire > time || JobBuffExpire > time;
    }

    private readonly PlayerImmuneState[] _playerImmunes = new PlayerImmuneState[4];

    private ObstacleMapManager _obstacles;

    protected DeepDungeonState Palace => World.DeepDungeon;

    protected AutoClear(WorldState ws, int LevelCap) : base(ws)
    {
        this.LevelCap = LevelCap;
        _obstacles = new(ws);

        _subscriptions = new(
            ws.SystemLogMessage.Subscribe(OnSystemLogMessage),
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Actors.CastEvent.Subscribe(OnEventCast),
            ws.Actors.Added.Subscribe(OnActorCreated),
            ws.Actors.InCombatChanged.Subscribe(OnActorCombatChanged),
            ws.Actors.StatusGain.Subscribe(OnActorStatusGain),
            ws.Actors.StatusLose.Subscribe(OnActorStatusLose),
            ws.Actors.IsDeadChanged.Subscribe(op =>
            {
                if (!op.IsAlly && op.IsDead)
                    ++Kills;
            }),
            ws.Actors.EventOpenTreasure.Subscribe(OnOpenTreasure),
            ws.Actors.EventObjectAnimation.Subscribe(OnEObjAnim),
            ws.DeepDungeon.MapDataChanged.Subscribe(_ =>
            {
                BetweenFloors = false;
                if (Walls.Count == 0)
                    LoadWalls();
            })
        );

        _trapsCurrentZone = GeneratedTrapData.Traps.TryGetValue(ws.CurrentZone, out var locations) ? locations : [];

        ProblematicTrapLocations.AddRange(ProblematicTrapLocations);
        IgnoreTraps.AddRange(ProblematicTrapLocations);
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        _obstacles.Dispose();
        base.Dispose(disposing);
    }

    protected virtual void OnCastStarted(Actor actor) { }

    protected virtual void OnCastFinished(Actor actor) { }
    protected virtual void OnEventCast(Actor actor, ActorCastEvent ev) { }

    private void OnActorStatusGain(Actor actor, int index)
    {
        var status = actor.Statuses[index];

        switch (status.ID)
        {
            case (uint)WHM.SID.Surecast:
            case (uint)WAR.SID.ArmsLength:
                var slot1 = World.Party.FindSlot(actor.InstanceID);
                if (slot1 >= 0)
                    _playerImmunes[slot1].RoleBuffExpire = status.ExpireAt;
                break;
            case (uint)WAR.SID.InnerStrength:
                var slot2 = World.Party.FindSlot(actor.InstanceID);
                if (slot2 >= 0)
                    _playerImmunes[slot2].JobBuffExpire = status.ExpireAt;
                break;
            // Knockback Penalty floor effect
            case 1096u:
            case 1512u:
                var slot3 = World.Party.FindSlot(actor.InstanceID);
                if (slot3 >= 0)
                    _playerImmunes[slot3].KnockbackPenalty = true;
                break;
        }

        OnStatusGain(actor, status);
    }

    protected virtual void OnStatusGain(Actor actor, ActorStatus status) { }

    private void OnActorStatusLose(Actor actor, int index)
    {
        var status = actor.Statuses[index];
        OnStatusLose(actor, status);
    }

    protected virtual void OnStatusLose(Actor actor, ActorStatus status) { }

    protected virtual void OnActorCombatChanged(Actor actor) { }

    private void OnSystemLogMessage(WorldState.OpSystemLogMessage op)
    {
        switch (op.MessageID)
        {
            case 7222u: // pomander overcap
                _lastChestContentsGold = (PomanderID)op.Args[0];
                break;
            case 7248u: // transference initiated
                ClearState();
                break;
            case 7255u: // safety used
            case 7256u: // sight used
                _trapsHidden = false;
                break;
            case 10287u: // demiclone overcap
                _lastChestMagicite = true;
                break;
        }
    }

    private void OnOpenTreasure(Actor chest) => _openedChests.Add(chest.InstanceID);

    private void OnEObjAnim(Actor actor, ushort p1, ushort p2)
    {
        // fake beacon deactivation; accompanied by system log #9217 but it does not indicate a specific actor
        if (actor.OID == (uint)OID.BeaconHoH && p1 == 0x0400 && p2 == 0x0800)
            _fakeExits.Add(actor.InstanceID);
    }

    protected virtual void OnChangeFloors() { }

    private void ClearState()
    {
        Donuts.Clear();
        Circles.Clear();
        Gazes.Clear();
        Interrupts.Clear();
        Stuns.Clear();
        Spikes.Clear();
        HintDisabled.Clear();
        LOS.Clear();
        Walls.Clear();
        RoomCenters.Clear();
        IgnoreTraps.Clear();
        IgnoreTraps.AddRange(ProblematicTrapLocations);
        DesiredRoom = 0;
        Kills = 0;
        new Span<PlayerImmuneState>(_playerImmunes).Clear();
        _lastChestContentsGold = null;
        _lastChestMagicite = false;
        _chestContentsGold.Clear();
        _chestContentsSilver.Clear();
        _trapsHidden = true;
        _openedChests.Clear();
        _fakeExits.Clear();
        OnChangeFloors();
        BetweenFloors = true;
    }

    protected void AddGaze(Actor Source, AOEShape Shape) => Gazes.Add(new(Source, Shape));
    protected void AddGaze(Actor Source, float Radius) => AddGaze(Source, new AOEShapeCircle(Radius));

    protected void AddLOS(Actor Source, float Range)
    {
        if (Config.AutoLOS)
            AddLOSFromTerrain(Source, Range);
        else
            Circles.Add((Source, Range));
    }

    private bool OpenGold => Config.GoldCoffer;
    private bool OpenSilver
    {
        get
        {
            // disabled
            if (!Config.SilverCoffer)
                return false;

            // sanity check
            if (World.Party.Player() is not { } player)
                return false;

            // explosive silver chests deal 70% max hp damage
            if (player.HPMP.CurHP <= player.HPMP.MaxHP * 0.7f)
                return false;

            // upgrade weapon if desired
            if (Palace.Progress.WeaponLevel + Palace.Progress.ArmorLevel < 198)
                return true;

            return Palace.DungeonId switch
            {
                DeepDungeonState.DungeonType.HOH or DeepDungeonState.DungeonType.EO => Palace.Floor >= 7, // magicite/demiclones start dropping on floor 7
                _ => false,
            };
        }
    }

    private bool OpenBronze => Config.BronzeCoffer;

    public override bool WantDrawExtra() => Config.EnableMinimap && !Palace.IsBossFloor;

    public sealed override string WindowName() => "BMR DD minimap###BMRDD";

    public override void DrawExtra()
    {
        var player = World.Party.Player()!;
        var targetRoom = new Minimap(Palace, player, DesiredRoom).Draw();
        if (targetRoom >= 0)
            DesiredRoom = targetRoom;

        ImGui.Text($"Kills: {Kills}");

        var maxPull = Config.MaxPull;

        ImGui.SetNextItemWidth(200);
        if (ImGui.DragInt("Max mobs to pull", ref maxPull, 0.05f, 0, 15))
        {
            Config.MaxPull = maxPull;
            Config.Modified.Fire();
        }

        if (ImGui.Button("Reload obstacles"))
        {
            _obstacles.Dispose();
            _obstacles = new(World);
        }

        if (player == null)
            return;

        var (entry, data) = _obstacles.Find(player.PosRot.XYZ());
        if (entry == null)
        {
            ImGui.SameLine();
            UIMisc.HelpMarker(() => "Obstacle map missing for floor!", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
        }

        if (data != null && data.PixelSize != 0.5f)
        {
            ImGui.SameLine();
            UIMisc.HelpMarker(() => $"Wrong resolution for map; should be 0.5, got {data.PixelSize}", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
        }

        if (ImGui.Button("Set closest trap location as ignored"))
        {
            WPos? pos = null;
            var minDistanceSq = float.MaxValue;
            var lenCurrent = _trapsCurrentZone.Length;
            var countProblematic = ProblematicTrapLocations.Count;
            for (var i = 0; i < lenCurrent; ++i)
            {
                ref readonly var trap = ref _trapsCurrentZone[i];
                var isProblematic = false;
                for (var j = 0; j < countProblematic; ++j)
                {
                    if (trap == ProblematicTrapLocations[j])
                    {
                        isProblematic = true;
                        break;
                    }
                }

                if (isProblematic)
                    continue;

                var distanceSq = (trap - player.Position).LengthSq();

                if (distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    pos = trap;
                }
            }
            if (pos is WPos position)
            {
                pos = position.Rounded(0.1f);
                ProblematicTrapLocations.Add(position);
                IgnoreTraps.Add(position);
            }
        }
    }

    private readonly List<PomanderID> AutoUsable = [
        PomanderID.Steel,
        PomanderID.Strength,
        PomanderID.Sight,
        PomanderID.Raising,
        PomanderID.Fortune,
        PomanderID.Concealment,
        PomanderID.Affluence,
        PomanderID.Frailty,
        PomanderID.ProtoSteel,
        PomanderID.ProtoStrength,
        PomanderID.ProtoSight,
        PomanderID.ProtoRaising,
        PomanderID.ProtoLethargy,
        PomanderID.ProtoFortune,
        PomanderID.ProtoAffluence
    ];

    private bool CanAutoUse(PomanderID p) => AutoUsable.Contains(p);

    private void IterAndExpire<T>(List<T> items, Func<T, bool> expire, Action<T> action, Action<T>? onRemove = null)
    {
        for (var i = items.Count - 1; i >= 0; --i)
        {
            var item = items[i];
            if (expire(item))
            {
                items.RemoveAt(i);
                onRemove?.Invoke(item);
            }
            else
                action(item);
        }
    }

    protected virtual void OnActorCreated(Actor c)
    {
        if (c.OID is (uint)OID.BeaconHoH or (uint)OID.BandedCofferIndicator)
            IgnoreTraps.Add(c.Position);
    }

    private DateTime CastFinishAt(Actor c) => World.FutureTime(c.CastInfo!.NPCRemainingTime);

    protected virtual void CalculateExtraHints(int playerSlot, Actor player, AIHints hints) { }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!Config.Enable || Palace.IsBossFloor || BetweenFloors)
            return;

        bool canNavigate;

        if (Config.MaxPull == 0)
        {
            canNavigate = !player.InCombat;
        }
        else
        {
            var count = 0;
            var countTargets = hints.PotentialTargets.Count;
            for (var i = 0; i < countTargets; ++i)
            {
                var target = hints.PotentialTargets[i];
                if (target.Actor.AggroPlayer && !target.Actor.IsDeadOrDestroyed)
                {
                    ++count;
                    if (count >= Config.MaxPull)
                        break;
                }
            }
            canNavigate = count < Config.MaxPull;
        }

        var countWalls = Walls.Count;
        for (var i = 0; i < countWalls; ++i)
        {
            var wall = Walls[i];
            var w = wall.Wall;
            hints.AddForbiddenZone(new SDRect(w.Position, (wall.Rotated ? 90f : default).Degrees(), w.Depth, w.Depth, 20f));
        }

        if (canNavigate)
            HandleFloorPathfind(player, hints);

        DrawAOEs(playerSlot, player, hints);
        CalculateExtraHints(playerSlot, player, hints);

        var isStunned = IsPlayerTransformed(player) || player.Statuses.Any(s => s.ID is (uint)SID.Silence or (uint)SID.Pacification);
        var isOccupied = player.InCombat || isStunned;

        Actor? coffer = null;
        Actor? hoardLight = null;
        Actor? passage = null;
        List<ShapeDistance> revealedTraps = [];

        PomanderID? pomanderToUseHere = null;

        foreach (var a in World.Actors)
        {
            if (_chestContentsGold.TryGetValue(a.InstanceID, out var pid) && Palace.GetPomanderState(pid).Count == 3 && a.IsTargetable)
            {
                if (CanAutoUse(pid))
                    pomanderToUseHere ??= pid;
                continue;
            }

            if (_chestContentsSilver.ContainsKey(a.InstanceID) && Palace.Magicite.All(m => m > 0))
                // TODO use magicite/demiclone to prevent overcap
                continue;

            if (_openedChests.Contains(a.InstanceID) || _fakeExits.Contains(a.InstanceID))
                continue;

            var oid = a.OID;
            if (a.IsTargetable && (
                oid == (uint)OID.GoldCoffer && OpenGold ||
                oid == (uint)OID.SilverCoffer && OpenSilver && player.HPMP.CurHP > player.HPMP.MaxHP * 0.7f ||
                BronzeChestIDs.Contains(a.OID) && OpenBronze ||
                oid == (uint)OID.BandedCoffer
            ))
            {
                if ((coffer?.DistanceToHitbox(player) ?? float.MaxValue) > a.DistanceToHitbox(player))
                    coffer = a;
            }

            if (a.OID == (uint)OID.BandedCofferIndicator)
                hoardLight = a;

            if (a.OID is (uint)OID.CairnPalace or (uint)OID.BeaconHoH or (uint)OID.PylonEO && (passage?.DistanceToHitbox(player) ?? float.MaxValue) > a.DistanceToHitbox(player))
                passage = a;

            if (RevealedTrapOIDs.Contains(a.OID))
                revealedTraps.Add(new SDCircle(a.Position, 2f));
        }

        var fullClear = false;
        if (Config.FullClear)
        {
            var unexplored = Array.FindIndex(Palace.Rooms, d => (byte)d > 0 && !d.HasFlag(RoomFlags.Revealed));
            if (unexplored > 0)
            {
                DesiredRoom = unexplored;
                fullClear = true;
            }
        }
        if (Config.TrapHints && _trapsHidden)
        {
            var countTraps = _trapsCurrentZone.Length;
            var traps = new List<ShapeDistance>(countTraps);

            for (var i = 0; i < countTraps; ++i)
            {
                var trap = _trapsCurrentZone[i];
                if (trap.InCircle(player.Position, 30f))
                {
                    var shouldIgnore = false;
                    var countIgnoreTraps = IgnoreTraps.Count;
                    for (var j = 0; j < countIgnoreTraps; ++j)
                    {
                        if (IgnoreTraps[j].AlmostEqual(trap, 1f))
                        {
                            shouldIgnore = true;
                            break;
                        }
                    }

                    if (!shouldIgnore)
                    {
                        ShapeDistance trapCircle = new SDCircle(trap, 2f);
                        traps.Add(trapCircle);
                    }
                }
            }

            if (traps.Count != 0)
                hints.AddForbiddenZone(new SDUnion([.. traps]));
        }

        if (coffer != null)
        {
            if (_lastChestContentsGold is PomanderID p)
            {
                _chestContentsGold[coffer.InstanceID] = p;
                _lastChestContentsGold = null;
                return;
            }

            if (_lastChestMagicite)
            {
                // TODO figure out why the system log args arent working
                _chestContentsSilver[coffer.InstanceID] = 1;
                _lastChestMagicite = false;
                return;
            }
        }

        if (Config.AllowPomander && !isStunned && pomanderToUseHere is PomanderID p2 && player.FindStatus((uint)SID.ItemPenalty) == null)
            hints.ActionsToExecute.Push(new ActionID(ActionType.Pomander, (uint)p2), null, ActionQueue.Priority.VeryHigh);

        Actor? wantCoffer = null;
        if (coffer is Actor t && !IsPlayerTransformed(player) && (Config.AutoMoveTreasure && canNavigate || player.DistanceToHitbox(t) < 3.5f))
            wantCoffer = t;

        if (!player.InCombat && Config.AutoPassage && Palace.PassageActive)
        {
            if (DesiredRoom == 0)
                DesiredRoom = Array.FindIndex(Palace.Rooms, d => d.HasFlag(RoomFlags.Passage));

            if (passage is Actor c && !fullClear)
            {
                hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 2f, 0.5f));
                // give pathfinder a little help lmao
                hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 25f, 0.25f));
                if (player.DistanceToHitbox(c) < player.DistanceToHitbox(coffer) && !Config.OpenChestsFirst)
                    wantCoffer = null;
            }
        }

        if (wantCoffer is Actor xxx)
        {
            wantCoffer = xxx;
            hints.GoalZones.Add(hints.GoalSingleTarget(xxx.Position, 25f));
            hints.InteractWithTarget = coffer;
        }

        if (revealedTraps.Count > 0)
            hints.AddForbiddenZone(new SDUnion([.. revealedTraps]));

        if (!IsPlayerTransformed(player) && canNavigate && Config.AutoMoveTreasure && hoardLight is Actor h && Palace.GetPomanderState(PomanderID.Intuition).Active)
            hints.GoalZones.Add(hints.GoalSingleTarget(h.Position, 2f, 10f));

        var shouldTargetMobs = Config.AutoClear switch
        {
            AutoDDConfig.ClearBehavior.Passage => !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.Leveling => player.Level < LevelCap || !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.All => true,
            _ => false
        };

        if (player.InCombat || World.Actors.Find(player.TargetID) is Actor t2 && !t2.IsAlly)
            return;

        Actor? bestTarget = null;

        void pickBetterTarget(Actor t)
        {
            if (player.DistanceToHitbox(t) < player.DistanceToHitbox(bestTarget))
                bestTarget = t;
        }

        var counttargets = hints.PotentialTargets.Count;
        for (var i = 0; i < counttargets; ++i)
        {
            var pp = hints.PotentialTargets[i];
            // enemy is petrified, any damage will kill
            if (pp.Actor.FindStatus((uint)SID.StoneCurse)?.ExpireAt > World.FutureTime(1.5d))
                pickBetterTarget(pp.Actor);

            // pomander of storms was used, enemy can't autoheal; any damage will kill
            else if (pp.Actor.FindStatus((uint)SID.AutoHealPenalty) != null && pp.Actor.HPMP.CurHP < 10u)
                pickBetterTarget(pp.Actor);

            // if player does not have a target, prioritize everything so that AI picks one - skip dangerous enemies
            else if (shouldTargetMobs)
            {
                var hasDangerousStatus = false;
                var len = pp.Actor.Statuses.Length;
                ref var statuses = ref pp.Actor.Statuses;
                for (var j = 0; j < len; ++j)
                {
                    if (IsDangerousOutOfCombatStatus(statuses[j].ID))
                    {
                        hasDangerousStatus = true;
                        break;
                    }
                }

                if (!hasDangerousStatus)
                {
                    pickBetterTarget(pp.Actor);
                }
            }
        }
        hints.ForcedTarget = bestTarget;
    }

    private void DrawAOEs(int playerSlot, Actor player, AIHints hints)
    {
        IterAndExpire(HintDisabled, g => g.CastInfo == null, g =>
        {
            var count = hints.ForbiddenZones.Count;
            for (var i = 0; i < count; ++i)
            {
                var fz = hints.ForbiddenZones[i];
                if (fz.Source == g.InstanceID)
                {
                    hints.ForbiddenZones.Remove(fz);
                    break;
                }
            }
        });

        IterAndExpire(Gazes, g => g.Source.CastInfo == null, d =>
        {
            if (d.Shape.Check(player.Position, d.Source))
                hints.ForbiddenDirections.Add((player.AngleTo(d.Source), 45f.Degrees(), CastFinishAt(d.Source)));
        });

        IterAndExpire(Donuts, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new SDDonut(d.Source.Position.Quantized(), d.Inner, d.Outer), CastFinishAt(d.Source));
        });

        IterAndExpire(Circles, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new SDCircle(d.Source.Position.Quantized(), d.Radius), CastFinishAt(d.Source));

            // some enrages are way bigger than pathfinding map size (e.g. slime explosion is 60y)
            // in these cases, if the player is inside the aoe, add a goal zone telling it to GTFO as far as possible
            if (d.Radius >= 30)
            {
                var distToSource = (player.Position - d.Source.Position).Length();
                if (distToSource <= d.Radius)
                {
                    var desiredDistance = distToSource + 10f;
                    hints.GoalZones.Add(p =>
                    {
                        var dist = (p - d.Source.Position).Length();
                        return dist >= desiredDistance ? 100f : default;
                    });
                }
            }
        });

        IterAndExpire(Interrupts, d => d.CastInfo == null, d =>
        {
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeInterrupted = true;
        });

        IterAndExpire(Stuns, d => d.CastInfo == null, d =>
        {
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeStunned = true;
        });

        IterAndExpire(LOS, d => d.CastInfo == null, caster =>
        {
            if (!_losCache.TryGetValue(caster.InstanceID, out var dangermap))
                return;

            var origin = dangermap.Item1;
            var map = dangermap.Item2;

            hints.AddForbiddenZone(new SDDeepDungeonLOS(dangermap.Item2, dangermap.Item1));
        }, d => _losCache.Remove(d.InstanceID));

        IterAndExpire(Voidzones, d => d.Source.IsDeadOrDestroyed, d =>
        {
            hints.AddForbiddenZone(d.Zone, d.Source.Position.Quantized(), d.Source.Rotation);
        });

        IterAndExpire(KnockbackZones, d => d.Source.CastInfo == null, kb =>
        {
            var castFinish = CastFinishAt(kb.Source);
            if (_playerImmunes[playerSlot].ImmuneAt(castFinish))
                return;

            hints.AddForbiddenZone(new SDCircle(kb.Source.Position, kb.Radius), castFinish);
        });

        IterAndExpire(Spikes, t => t.Timeout <= World.CurrentTime, t =>
        {
            if (hints.FindEnemy(t.Actor) is { } enemy)
                enemy.Spikes = true;
        });
    }

    private static bool IsPlayerTransformed(Actor player) => player.Statuses.Any(Autorotation.RotationModuleManager.IsTransformStatus);
    private static bool IsDangerousOutOfCombatStatus(uint statusRaw) => statusRaw is (uint)SID.DamageUp or (uint)SID.DreadBeastAura or (uint)SID.PhysicalDamageUp;

    private void HandleFloorPathfind(Actor player, AIHints hints)
    {
        var playerRoom = Palace.Party[0].Room;

        if (DesiredRoom == playerRoom || DesiredRoom == 0)
        {
            DesiredRoom = 0;
            return;
        }

        var path = new FloorPathfind(Palace.Rooms).Pathfind(playerRoom, DesiredRoom);
        if (path.Count == 0)
        {
            Service.Log($"uh-oh, no path from {playerRoom} to {DesiredRoom}");
            return;
        }
        var next = path[0];
        Direction d;
        if (next == playerRoom + 1)
            d = Direction.East;
        else if (next == playerRoom - 1)
            d = Direction.West;
        else if (next == playerRoom + 5)
            d = Direction.South;
        else if (next == playerRoom - 5)
            d = Direction.North;
        else
        {
            Service.Log($"pathfinding instructions are nonsense: {string.Join(", ", path)}");
            DesiredRoom = 0;
            return;
        }

        hints.GoalZones.Add(p =>
        {
            var pp = player.Position;
            var improvement = d switch
            {
                Direction.North => pp.Z - p.Z,
                Direction.South => p.Z - pp.Z,
                Direction.East => p.X - pp.X,
                Direction.West => pp.X - p.X,
                _ => 0,
            };
            return improvement > 10 ? 10 : 0;
        });
    }

    private void LoadWalls()
    {
        Service.Log($"loading walls for current floor...");
        Walls.Clear();
        var floorset = Palace.Floor / 10;
        var key = $"{(int)Palace.DungeonId}.{floorset + 1}";
        if (!LoadedFloors.Walls.TryGetValue(key, out var floor))
        {
            Service.Log($"unable to load floorset {key}");
            return;
        }
        Tileset<Wall> tileset;
        switch (Palace.Progress.Tileset)
        {
            case 0:
                tileset = floor.RoomsA;
                break;
            case 1:
                tileset = floor.RoomsB;
                break;
            case 2:
                Service.Log($"hall of fallacies - nothing to do");
                return;
            default:
                Service.Log($"unrecognized tileset number {Palace.Progress.Tileset}");
                return;
        }
        var len = Palace.Rooms.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var room = ref Palace.Rooms[i];
            if (room > 0)
            {
                var roomdata = tileset[i];
                RoomCenters.Add(roomdata.Center.Position);
                if (roomdata.North != default && !room.HasFlag(RoomFlags.ConnectionN))
                    Walls.Add((roomdata.North, false));
                if (roomdata.South != default && !room.HasFlag(RoomFlags.ConnectionS))
                    Walls.Add((roomdata.South, false));
                if (roomdata.East != default && !room.HasFlag(RoomFlags.ConnectionE))
                    Walls.Add((roomdata.East, true));
                if (roomdata.West != default && !room.HasFlag(RoomFlags.ConnectionW))
                    Walls.Add((roomdata.West, true));
            }
        }
    }

    protected void AddLOSFromTerrain(Actor Source, float Range)
    {
        var (entry, data) = _obstacles.Find(Source.PosRot.XYZ());
        if (entry == null || data == null)
        {
            Service.Log($"no bitmap found for {Source}, not adding LOS hints");
            return;
        }

        var pixelRange = (int)(Range / data.PixelSize);
        var casterOff = Source.Position - entry.Origin;
        var casterCell = casterOff / data.PixelSize;
        var casterX = (int)casterCell.X;
        var casterZ = (int)casterCell.Z;

        var bm = new Bitmap(data.Width, data.Height, data.Color0, data.Color1, data.Resolution);
        for (var i = Math.Max(0, casterX - pixelRange); i <= Math.Min(data.Width, casterX + pixelRange); ++i)
        {
            for (var j = Math.Max(0, casterZ - pixelRange); j <= Math.Min(data.Height, casterZ + pixelRange); ++j)
            {
                var pt = new Vector2(i, j);
                var cc = new Vector2(casterX, casterZ);
                if (!IsBlocked(data, pt, cc, pixelRange))
                    bm[i, j] = true;
            }
        }

        _losCache[Source.InstanceID] = (entry.Origin, bm);
        LOS.Add(Source);
    }

    private static bool IsBlocked(Bitmap map, Vector2 point, Vector2 origin, float maxRange)
    {
        var dir = origin - point;
        var dist = dir.Length();
        if (dist >= maxRange)
            return true;

        dir /= dist;

        var ox = point.X;
        var oy = point.Y;
        var vx = dir.X;
        var vy = dir.Y;

        for (var i = 0; i < (int)dist; ++i)
        {
            if (map[(int)ox, (int)oy])
                return true;
            ox += vx;
            oy += vy;
        }

        return false;
    }
}
