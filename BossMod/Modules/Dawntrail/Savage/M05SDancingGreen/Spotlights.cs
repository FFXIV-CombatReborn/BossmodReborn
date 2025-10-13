namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

class Spotlights1(BossModule module) : Components.GenericTowers(module)
{
    protected readonly int[] _orders = new int[PartyState.MaxPartySize];
    private bool? patternENVC200010;
    public int FinishedCount;
    protected bool? spotlightSet; // true set 1, false set 2
    private static readonly WPos[] spotlightSet1 =
    [
        new(87.48f, 112.474f), new(107.5f, 102.495f), new(92.485f, 97.49f), new(112.474f, 87.48f),
        new(102.495f, 92.485f), new(87.48f, 87.48f), new(97.49f, 107.5f), new(112.474f, 112.474f)
    ];

    private static readonly WPos[] spotlightSet2 =
    [
        new(87.48f, 112.474f), new(97.49f, 92.49f), new(112.474f, 87.48f), new(102.495f, 107.5f),
        new(107.5f, 97.49f), new(112.474f, 112.474f), new(87.48f, 87.48f), new(92.485f, 102.495f)
    ];

    private readonly M05SDancingGreenConfig _config = Service.Config.Get<M05SDancingGreenConfig>();

    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var showDiff = _config.ShowFromDifferentOrder;
        var showSame = _config.ShowFromSameOrder;
        var now = WorldState.CurrentTime;
        var spotlightTime = _config.SpotlightTimer;

        var filtered = new Tower[towers.Length];
        var count = 0;
        var len = towers.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var tower = ref towers[i];
            var isFromDifferentOrder = tower.ForbiddenSoakers[slot];
            if (Math.Max(0d, (tower.Activation - now).TotalSeconds) < spotlightTime && (isFromDifferentOrder && showDiff || !isFromDifferentOrder && showSame))
            {
                filtered[count++] = tower;
            }
        }

        return filtered.AsSpan()[..count];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_orders[slot] > 0)
        {
            hints.Add($"Spotlight order: {_orders[slot]}", false);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DC && spotlightSet == null && actor.OID == (uint)OID.Spotlight)
        {
            var position = actor.Position;
            if (position == new WPos(102.5f, 107.5f))
            {
                spotlightSet = true;
            }
            else if (position == new WPos(102.5f, 92.5f))
            {
                spotlightSet = false;
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03 || patternENVC200010 != null)
        {
            return;
        }

        patternENVC200010 ??= state switch
        {
            0x00020001u => false,
            0x00200010u => true,
            _ => null
        };

        if (patternENVC200010 is not bool pattern || spotlightSet is not bool set)
        {
            return;
        }

        var (spotlights, spotlights2) = GetSpotlightSets(pattern, set);

        var (forbiddenFirst, forbiddenSecond) = ForbiddenBitMasks;

        var activation2 = WorldState.FutureTime(33.6d);
        for (var i = 0; i < 4; ++i)  // 2nd order first to prevent allowed towers overlapping forbidden
        {
            AddTower(spotlights2[i], forbiddenSecond, activation2);
        }
        var activation1 = WorldState.FutureTime(22d);
        for (var i = 0; i < 4; ++i)
        {
            AddTower(spotlights[i], forbiddenFirst, activation1);
        }
    }

    private static (WPos[], WPos[]) GetSpotlightSets(bool pattern, bool set) => (GetSpotlightSet(pattern, set, true), GetSpotlightSet(pattern, set, false));

    private static WPos[] GetSpotlightSet(bool pattern, bool set, bool firstSet)
    {
        var source = firstSet
            ? (set ? spotlightSet1 : spotlightSet2)
            : (set ? spotlightSet2 : spotlightSet1);

        var spotlights = new WPos[4];
        Array.Copy(source, pattern ? 0 : 4, spotlights, 0, 4);
        return spotlights;
    }

    protected (BitMask, BitMask) ForbiddenBitMasks
    {
        get
        {
            BitMask forbiddenFirst = default;
            BitMask forbiddenSecond = default;

            for (var i = 0; i < 8; ++i)
            {
                if (_orders[i] == 2)
                {
                    forbiddenFirst[i] = true;
                }
                else
                {
                    forbiddenSecond[i] = true;
                }
            }

            return (forbiddenFirst, forbiddenSecond);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BurnBabyBurn && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _orders[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30d ? 1 : 2;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BurnBabyBurn && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _orders[slot] = 0;
            if (++FinishedCount >= 4)
            {
                if (Towers.Count == 8)
                {
                    Towers.RemoveRange(4, 4);
                }
                else
                {
                    Towers.Clear();
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var towers = ActiveTowers(pcSlot, pc);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            var isInside = t.IsInside(pc);
            var numInside = t.NumInside(Module);
            var safe = !t.ForbiddenSoakers[pcSlot] && (numInside < t.MaxSoakers || isInside && numInside <= t.MaxSoakers);
            t.Shape.Outline(Arena, t.Position, t.Rotation, safe ? Colors.Safe : default, 2f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) { }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;
        if (len == 0)
        {
            return;
        }
        var now = WorldState.CurrentTime;
        var forbiddenInverted = new List<ShapeDistance>(len);
        var forbidden = new List<ShapeDistance>(len);
        var inTower = false;
        for (var i = 0; i < len; ++i)
        {
            ref var t = ref towers[i];
            if (t.IsInside(actor) && t.NumInside(Module) == 1)
            {
                inTower = true;
                break;
            }
        }
        for (var i = 0; i < len; ++i)
        {
            ref var t = ref towers[i];
            if (t.ForbiddenSoakers[slot]) // nothing to do for forbidden players, players of different orders can share a spotlight
            {
                continue;
            }
            if (Math.Max(0d, (t.Activation - now).TotalSeconds) < 10d)
            {
                var isInside = t.IsInside(actor);
                var numInside = t.NumInside(Module);
                var max = t.MaxSoakers;
                var correctAmount = numInside >= t.MinSoakers && numInside <= max;
                var forbiddenSlot = t.ForbiddenSoakers[slot];
                if (!forbiddenSlot && (numInside < max || isInside && correctAmount))
                {
                    forbiddenInverted.Add(t.Shape.InvertedDistance(t.Position, t.Rotation));
                }
                else if (forbiddenSlot || numInside > max || !isInside && correctAmount)
                {
                    forbidden.Add(t.Shape.Distance(t.Position, t.Rotation));
                }
            }
        }
        var missingSoakers = !inTower;
        if (missingSoakers)
        {
            for (var i = 0; i < len; ++i)
            {
                if (towers[i].NumInside(Module) == 0)
                {
                    missingSoakers = true;
                    break;
                }
            }
        }
        var fcount = forbidden.Count;
        if ((fcount == 0 || inTower || missingSoakers) && forbiddenInverted.Count != 0)
        {
            hints.AddForbiddenZone(new SDIntersection([.. forbiddenInverted]), towers[0].Activation);
        }
        else if (fcount != 0 && !inTower)
        {
            var act = Towers.Ref(0).Activation;
            for (var i = 0; i < fcount; ++i)
            {
                hints.AddForbiddenZone(forbidden[i], act);
            }
        }
    }

    protected void AddTower(WPos position, BitMask forbidden, DateTime activation)
    {
        Towers.Add(new(position, 2.5f, forbiddenSoakers: forbidden, activation: activation));
    }
}

sealed class Spotlights2(BossModule module) : Spotlights1(module)
{
    private static readonly WPos[] spotlightSet1 = [new(84.977f, 84.977f), new(114.977f, 84.977f), new(84.977f, 114.977f), new(114.977f, 114.977f)];
    private static readonly WPos[] spotlightSet2 = [new(84.977f, 99.992f), new(99.992f, 84.977f), new(99.992f, 114.977f), new(114.977f, 99.992f)];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (spotlightSet == null && actor.OID == (uint)OID.Spotlight && id == 0x11DC)
        {
            if (actor.Position == new WPos(115f, 115f))
            {
                spotlightSet = true;
            }
            else if (actor.Position == new WPos(115f, 100f))
            {
                spotlightSet = false;
            }
            if (spotlightSet is bool set)
            {
                var spotlights1 = set ? spotlightSet2 : spotlightSet1;
                var spotlights2 = set ? spotlightSet1 : spotlightSet2;

                var (forbiddenFirst, forbiddenSecond) = ForbiddenBitMasks;

                var activation2 = WorldState.FutureTime(19.5d);
                for (var i = 0; i < 4; ++i)
                {
                    AddTower(spotlights2[i], forbiddenSecond, activation2);
                }
                var activation1 = WorldState.FutureTime(9.5d);
                for (var i = 0; i < 4; ++i)
                {
                    AddTower(spotlights1[i], forbiddenFirst, activation1);
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BurnBabyBurn && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _orders[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 15d ? 1 : 2;
        }
    }
}
