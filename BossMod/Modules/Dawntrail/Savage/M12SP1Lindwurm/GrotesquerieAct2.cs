namespace BossMod.Dawntrail.Savage.M12SLindwurm;

// players get BondsOfFlesh (Alpha or Beta) + position status (1st/2nd/3rd/4th in Line)
// blood Vessels spawn and become towers that must be soaked
// alpha players soak "boss towers" (first 4 that become visible, indices 0-3)
// beta players soak "chain towers" (next 4 that become visible, indices 4-7)
// tower order: 1st in line -> 3rd tower, 2nd -> 4th, 3rd -> 1st, 4th -> 2nd
sealed class GrotesquerieAct2(BossModule module) : Components.GenericTowers(module)
{
    private const float TowerRadius = 3f;

    private readonly ulong[] _vesselBySpawnIndex = new ulong[8];
    private int _nextSpawnIndex;
    private readonly bool[] _towerSoaked = new bool[8];
    private readonly int[] _alphaBeta = [-1, -1, -1, -1, -1, -1, -1, -1]; // debuff tracking: -1 = unknown/none, 0 = beta, 1 = alpha
    private readonly int[] _position = new int[8];
    private readonly int[] _playerAssignedSpawnIndex = [-1, -1, -1, -1, -1, -1, -1, -1]; // which player slot should soak which spawn index (-1 = no assignment)

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0 || slot >= 8)
            return;

        // got alpha/beta
        if (status.ID == (uint)SID.BondsOfFleshAlpha)
            _alphaBeta[slot] = 1;
        else if (status.ID == (uint)SID.BondsOfFleshBeta)
            _alphaBeta[slot] = 0;

        // got X in line
        if (status.ID == (uint)SID.FirstInLine)
            _position[slot] = 1;
        else if (status.ID == (uint)SID.SecondInLine)
            _position[slot] = 2;
        else if (status.ID == (uint)SID.ThirdInLine)
            _position[slot] = 3;
        else if (status.ID == (uint)SID.FourthInLine)
            _position[slot] = 4;

        if (_alphaBeta[slot] >= 0 && _position[slot] > 0)
        {
            var spawnIndex = DetermineTowerIndex(_alphaBeta[slot] == 1, _position[slot]);
            _playerAssignedSpawnIndex[slot] = spawnIndex;
        }

        // player soaked their tower, clear assignment
        if (status.ID == (uint)SID.PoisonResistanceDownII)
            _playerAssignedSpawnIndex[slot] = -1;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0 || slot >= 8)
            return;

        if (status.ID == (uint)SID.BondsOfFleshAlpha || status.ID == (uint)SID.BondsOfFleshBeta)
            _alphaBeta[slot] = -1;

        if (status.ID is (uint)SID.FirstInLine or (uint)SID.SecondInLine or (uint)SID.ThirdInLine or (uint)SID.FourthInLine)
            _position[slot] = 0;
    }

    // returns which tower (0-7) a player should soak based on their debuffs
    private static int DetermineTowerIndex(bool isAlpha, int position)
    {
        // alpha soaks boss-spawned towers (indices 0-3), beta soaks chain-spawned towers (indices 4-7)
        var baseOffset = isAlpha ? 0 : 4;
        return position switch
        {
            1 => baseOffset + 2, // 1st in line -> 3rd tower
            2 => baseOffset + 3, // 2nd in line -> 4th tower
            3 => baseOffset + 0, // 3rd in line -> 1st tower
            4 => baseOffset + 1, // 4th in line -> 2nd tower
            _ => -1
        };
    }

    // returns which position should soak a tower at given index within its set (0-3)?
    private static int GetRequiredPosition(int towerIndexInSet)
    {
        return towerIndexInSet switch
        {
            0 => 3, // 1st tower -> 3rd in line
            1 => 4, // 2nd tower -> 4th in line
            2 => 1, // 3rd tower -> 1st in line
            3 => 2, // 4th tower -> 2nd in line
            _ => 0
        };
    }

    private static bool IsVesselVisible(Actor vessel)
    {
        return (vessel.Renderflags & 0x800u) == 0 && (vessel.Renderflags & 0x4000u) == 0;
    }

    private void UpdateVesselTracking()
    {
        // check if any vessels exist at all
        var anyVessel = false;
        foreach (var actor in Module.Enemies((uint)OID.BloodVessel))
        {
            if (!actor.IsDestroyed)
            {
                anyVessel = true;
                break;
            }
        }

        if (!anyVessel)
        {
            ResetTracking();
            return;
        }

        // track newy vessels and assign them spawn indices
        foreach (var actor in Module.Enemies((uint)OID.BloodVessel))
        {
            if (actor.IsDestroyed || !IsVesselVisible(actor))
                continue;

            var alreadyTracked = false;
            for (var i = 0; i < 8; ++i)
            {
                if (_vesselBySpawnIndex[i] == actor.InstanceID)
                {
                    alreadyTracked = true;
                    break;
                }
            }

            if (!alreadyTracked && _nextSpawnIndex < 8)
            {
                _vesselBySpawnIndex[_nextSpawnIndex] = actor.InstanceID;
                _nextSpawnIndex++;
            }
        }

        RebuildTowers();
    }

    private void ResetTracking()
    {
        for (var i = 0; i < 8; ++i)
        {
            _vesselBySpawnIndex[i] = 0;
            _towerSoaked[i] = false;
            _playerAssignedSpawnIndex[i] = -1;
        }
        _nextSpawnIndex = 0;
        Towers.Clear();
    }

    private void RebuildTowers()
    {
        Towers.Clear();

        for (var spawnIndex = 0; spawnIndex < _nextSpawnIndex; ++spawnIndex)
        {
            if (_towerSoaked[spawnIndex])
                continue;

            var vesselId = _vesselBySpawnIndex[spawnIndex];
            if (vesselId == 0)
                continue;

            Actor? vessel = null;
            foreach (var actor in Module.Enemies((uint)OID.BloodVessel))
            {
                if (actor.InstanceID == vesselId && !actor.IsDestroyed && IsVesselVisible(actor))
                {
                    vessel = actor;
                    break;
                }
            }

            if (vessel == null)
                continue;

            var isAlphaTower = spawnIndex < 4;
            var towerIndexInSet = spawnIndex % 4;
            var requiredPosition = GetRequiredPosition(towerIndexInSet);

            // find the player who should soak this tower and forbid everyone else
            BitMask forbidden = new();
            for (var slot = 0; slot < 8; ++slot)
            {
                var playerIsAlpha = _alphaBeta[slot] == 1;
                var playerPosition = _position[slot];
                var shouldSoak = (isAlphaTower == playerIsAlpha) &&
                                 (playerPosition == requiredPosition) &&
                                 (_playerAssignedSpawnIndex[slot] == spawnIndex);

                if (!shouldSoak)
                    forbidden.Set(slot);
            }

            Towers.Add(new(vessel.Position, TowerRadius, 1, 1, forbidden, default, vesselId));
        }
    }

    public override void Update()
    {
        UpdateVesselTracking();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RoilingMass0 or (uint)AID.RoilingMass1)
        {
            var castPos = caster.Position;
            foreach (var actor in Module.Enemies((uint)OID.BloodVessel))
            {
                if (actor.Position.InCircle(castPos, 1f))
                {
                    for (var i = 0; i < 8; ++i)
                    {
                        if (_vesselBySpawnIndex[i] == actor.InstanceID)
                        {
                            _towerSoaked[i] = true;
                            break;
                        }
                    }
                    break;
                }
            }
            RebuildTowers();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Towers.Count == 0)
            return;

        base.AddHints(slot, actor, hints);

        var spawnIndex = _playerAssignedSpawnIndex[slot];
        if (spawnIndex < 0)
            return;

        var towerNum = (spawnIndex % 4) + 1;
        var towerType = spawnIndex < 4 ? "Boss" : "Chain";

        hints.Add($"Soak {towerType} Tower #{towerNum}", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];

            if (IsPlayerAssignedTower(pcSlot, t.ActorID))
                continue;

            t.Shape.Draw(Arena, t.Position, t.Rotation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];

            if (!IsPlayerAssignedTower(pcSlot, t.ActorID))
                continue;

            var isInside = t.IsInside(pc);
            var color = isInside ? Colors.Danger : Colors.Safe;
            t.Shape.Outline(Arena, t.Position, t.Rotation, color, 2f);
        }
    }

    private bool IsPlayerAssignedTower(int slot, ulong towerActorID)
    {
        var assignedSpawnIndex = _playerAssignedSpawnIndex[slot];
        if (assignedSpawnIndex < 0 || assignedSpawnIndex >= 8)
            return false;

        return _vesselBySpawnIndex[assignedSpawnIndex] == towerActorID;
    }
}
