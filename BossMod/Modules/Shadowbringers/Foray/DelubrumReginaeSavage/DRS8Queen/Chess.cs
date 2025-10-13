namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

abstract class Chess(BossModule module) : Components.GenericAOEs(module)
{
    public struct GuardState
    {
        public Actor? Actor;
        public WPos FinalPosition;
    }

    protected GuardState[] GuardStates = new GuardState[4];
    protected static readonly AOEShapeCross Shape = new(60f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts >= 4)
            return [];

        var start = NumCasts < 2 ? 0 : 2;
        var count = Math.Min(2, GuardStates.Length - start);

        if (count <= 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = start; i < start + count; ++i)
        {
            ref readonly var gs = ref GuardStates[i];
            if (gs.Actor != null)
                aoes[index++] = new(Shape, gs.FinalPosition, gs.Actor.Rotation);
        }

        return aoes.AsSpan()[..index];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MovementIndicator)
        {
            var distance = status.Extra switch
            {
                0xE2 => 1,
                0xE3 => 2,
                0xE4 => 3,
                _ => 0
            };
            var index = GuardIndex(actor);
            if (distance != 0 && index >= 0 && GuardStates[index].Actor == null)
            {
                GuardStates[index] = new() { Actor = actor, FinalPosition = actor.Position + distance * 10 * actor.Rotation.ToDirection() };
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EndsKnight or (uint)AID.MeansWarrior or (uint)AID.EndsSoldier or (uint)AID.MeansGunner)
            ++NumCasts;
    }

    protected static int GuardIndex(Actor actor) => actor.OID switch
    {
        (uint)OID.QueensKnight => 0,
        (uint)OID.QueensWarrior => 1,
        (uint)OID.QueensSoldier => 2,
        (uint)OID.QueensGunner => 3,
        _ => -1
    };
}

sealed class QueensWill(BossModule module) : Chess(module) { }

// TODO: enumerate all possible safespots instead? after first pair of casts, select still suitable second safespots
sealed class QueensEdict(BossModule module) : Chess(module)
{
    public class PlayerState
    {
        public int FirstEdict;
        public int SecondEdict;
        public List<WPos> Safespots = [];
    }

    public int NumStuns;
    private readonly Dictionary<ulong, PlayerState> _playerStates = [];
    private int _safespotZOffset;

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var m in GetSafeSpotMoves(actor))
            movementHints.Add(m);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in GetSafeSpotMoves(pc))
            Arena.AddLine(m.from, m.to, m.color);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        base.OnStatusGain(actor, status);
        switch (status.ID)
        {
            case (uint)SID.Stun:
                ++NumStuns;
                break;
            case (uint)SID.MovementEdictShort2:
                _playerStates.GetOrAdd(actor.InstanceID).FirstEdict = 2;
                break;
            case (uint)SID.MovementEdictShort3:
                _playerStates.GetOrAdd(actor.InstanceID).FirstEdict = 3;
                break;
            case (uint)SID.MovementEdictShort4:
                _playerStates.GetOrAdd(actor.InstanceID).FirstEdict = 4;
                break;
            case (uint)SID.MovementEdictLong2:
                _playerStates.GetOrAdd(actor.InstanceID).SecondEdict = 2;
                break;
            case (uint)SID.MovementEdictLong3:
                _playerStates.GetOrAdd(actor.InstanceID).SecondEdict = 3;
                break;
            case (uint)SID.MovementEdictLong4:
                _playerStates.GetOrAdd(actor.InstanceID).SecondEdict = 4;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Stun)
            --NumStuns;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is 0x1C or 0x1D && state == 0x00020001u)
            _safespotZOffset = index == 0x1D ? 2 : -2;
    }

    private List<(WPos from, WPos to, uint color)> GetSafeSpotMoves(Actor actor)
    {
        var state = _playerStates.GetValueOrDefault(actor.InstanceID);
        if (state == null)
            return [];

        if (state.Safespots.Count == 0)
        {
            // try initializing safespots on demand
            if (_safespotZOffset == 0 || state.FirstEdict == 0 || state.SecondEdict == 0 ||
                GuardStates[0].Actor == null || GuardStates[1].Actor == null || GuardStates[2].Actor == null || GuardStates[3].Actor == null)
                return [];
            // initialize second safespot: select cells that are SecondEdict distance from safespot and not in columns clipped by second set of guards
            var centerX = Arena.Center.X;
            var centerZ = Arena.Center.Z;
            var forbiddenCol1 = OffsetToCell(GuardStates[2].FinalPosition.X - centerX);
            var forbiddenCol2 = OffsetToCell(GuardStates[3].FinalPosition.X - centerX);
            var forbiddenRow1 = OffsetToCell(GuardStates[0].FinalPosition.Z - centerZ);
            var forbiddenRow2 = OffsetToCell(GuardStates[1].FinalPosition.Z - centerZ);

            var secondSafeSpots = CellsAtManhattanDistance((0, _safespotZOffset), state.SecondEdict);
            var countSS = secondSafeSpots.Count;
            for (var i = 0; i < countSS; ++i)
            {
                var s2 = secondSafeSpots[i];
                if (s2.x == forbiddenCol1 || s2.x == forbiddenCol2)
                    continue;

                var firstSafeSpots = CellsAtManhattanDistance(s2, state.FirstEdict);
                var countFS = firstSafeSpots.Count;
                for (var j = 0; j < countFS; ++j)
                {
                    var s1 = firstSafeSpots[j];
                    if (s1.z == forbiddenRow1 || s1.z == forbiddenRow2)
                        continue;

                    state.Safespots.Add(CellCenter(s1));
                    state.Safespots.Add(CellCenter(s2));
                    state.Safespots.Add(CellCenter((0, _safespotZOffset)));
                    break;
                }
                if (state.Safespots.Count > 0)
                    break;
            }
        }

        var color = Colors.Safe;
        var from = actor.Position;
        var moves = new List<(WPos, WPos, uint)>();
        var count = state.Safespots.Count;
        for (var i = NumCasts / 2; i < count; ++i)
        {
            var p = state.Safespots[i];
            moves.Add((from, p, color));
            from = p;
            color = Colors.Danger;
        }

        return moves;
    }

    private static int OffsetToCell(float offset) => offset switch
    {
        < -25f => -3,
        < -15f => -2,
        < -5f => -1,
        < 5f => 0,
        < 15f => 1,
        < 25f => 2,
        _ => 3
    };

    private WPos CellCenter((int x, int z) cell) => Arena.Center + 10 * new WDir(cell.x, cell.z);

    private List<(int x, int z)> CellsAtManhattanDistance((int x, int z) origin, int distance)
    {
        var cells = new List<(int x, int z)>();
        for (var x = -2; x <= 2; ++x)
        {
            var dz = distance - Math.Abs(x - origin.x);
            if (dz == 0)
            {
                cells.Add((x, origin.z));
            }
            else if (dz > 0)
            {
                var z1 = origin.z - dz;
                var z2 = origin.z + dz;
                if (z1 >= -2)
                    cells.Add((x, z1));
                if (z2 <= +2)
                    cells.Add((x, z2));
            }
        }
        return cells;
    }
}
