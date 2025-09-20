namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class AlexandrianThunderII(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private readonly List<Angle> _rotation = new(3);
    private bool? clockwise;
    private static readonly AOEShapeDonutSector sector = new(2f, 8f, 15f.Degrees(), invertForbiddenZone: true);
    private static readonly Angle a75 = 7.5f.Degrees();
    private static readonly AOEShapeCone _shape = new(24f, 22.5f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -10f.Degrees(),
            (uint)IconID.RotateCCW => 10f.Degrees(),
            _ => default
        };
        _activation = WorldState.FutureTime(5.7d);
        clockwise = iconID == (uint)IconID.RotateCW;
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (_rotation.Count == 3 && _increment != default)
        {
            for (var i = 0; i < 3; ++i)
            {
                Sequences.Add(new(_shape, Arena.Center.Quantized(), _rotation[i], _increment, _activation, 1d, 15));
            }
            _rotation.Clear();
            _increment = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AlexandrianThunderIIFirst)
        {
            _rotation.Add(spell.Rotation);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlexandrianThunderIIFirst or (uint)AID.AlexandrianThunderIIRepeat)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts != 0)
            return;
        if (clockwise != null)
        {
            sector.Outline(Arena, Arena.Center, clockwise == true ? FloorTiles.TileAngles[2] + a75 : FloorTiles.TileAngles[7] - a75, Colors.Safe, 2f);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (NumCasts != 0)
            return;
        if (clockwise != null)
            hints.Add("Go to marked area for rotation start!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (NumCasts != 0)
            return;
        if (clockwise != null)
        {
            var act = Sequences.Count != 0 ? Sequences[0].NextActivation : DateTime.MaxValue;
            hints.AddForbiddenZone(sector, Arena.Center, clockwise == true ? FloorTiles.TileAngles[2] + a75 : FloorTiles.TileAngles[7] - a75, act);
        }
    }
}
