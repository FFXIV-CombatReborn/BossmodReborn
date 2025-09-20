namespace BossMod.Dawntrail.Savage.M01SBlackCat;

// note: grid cell indices are same as ENVC indices: 0 for NW, then increasing along x, then increasing along z (so NE is 3, SW is 12, SE is 15)
// normally, boss does 8 sets of 3 jumps then 2 sets of 2 jumps, destroying 12 cells and damaging remaining 4
// on enrage, boss does 8 sets of 4 jumps, destroying all cells
sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    private static readonly Square[] defaultSquare = [new(ArenaCenter, 20)];
    public BitMask DamagedCells;
    public BitMask DestroyedCells;
    public static readonly Square[] Tiles = GenerateTiles();

    private static Square[] GenerateTiles()
    {
        var squares = new Square[16];
        for (var i = 0; i < 16; ++i)
            squares[i] = new Square(CellCenter(i), 5f);
        return squares;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index > 0x0F)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u: // damage tile (first jump)
                DamagedCells[index] = true;
                break;
            case 0x00200010u: // destroy tile (second jump)
                DamagedCells[index] = false;
                DestroyedCells[index] = true;
                break;
            case 0x01000004u: // repair destroyed tile (after initial jumps)
            case 0x00800004u: // repair damaged tile (mechanic end)
            case 0x00080004u: // start short repair (will finish before kb)
                DamagedCells[index] = false;
                DestroyedCells[index] = false;
                break;
            case 0x00400004u: // start long repair (won't finish before kb)
                break;
        }
        UpdateArenaBounds();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GrimalkinGaleSpreadAOE)
        {
            DamagedCells = default;
            DestroyedCells = default;
            UpdateArenaBounds();
        }
    }

    public static int CellIndex(WPos pos)
    {
        var off = pos - ArenaCenter;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private static int CoordinateIndex(float coord) => coord switch
    {
        < -10 => 0,
        < 0 => 1,
        < 10 => 2,
        _ => 3
    };

    public static WPos CellCenter(int index)
    {
        var x = -15f + 10f * (index & 3);
        var z = -15f + 10f * (index >> 2);
        return ArenaCenter + new WDir(x, z);
    }

    private void UpdateArenaBounds()
    {
        List<Square> brokenTilesList = [];

        var len = Tiles.Length;
        for (var i = 0; i < len; ++i)
        {
            if (DestroyedCells[i])
                brokenTilesList.Add(Tiles[i]);
        }

        Square[] brokenTiles = [.. brokenTilesList];
        if (brokenTiles.Length == 16) // prevents empty sequence error at end of enrage
            brokenTiles = [];
        var arena = new ArenaBoundsCustom(defaultSquare, brokenTiles, Offset: -1f);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }

    // public BitMask IntactCells => new BitMask(0xffff) ^ DamagedCells ^ DestroyedCells;
}

sealed class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(32);
    private bool enrage;
    private static readonly AOEShapeRect rect = new(10f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var countDanger = enrage ? 4 : NumCasts > 23 ? 2 : 3;
        var countsAOE = enrage ? 4 : NumCasts > 20 ? 4 : 6;
        var total = countDanger + countsAOE;
        var max = total > count ? count : total;

        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        if (count > countDanger)
        {
            var color = Colors.Danger;
            for (var i = 0; i < countDanger; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Color = color;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MouserTelegraphFirst or (uint)AID.MouserTelegraphSecond)
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, WorldState.FutureTime(9.7d)));
        else if (spell.Action.ID == (uint)AID.MouserEnrage)
            enrage = true;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 0x0F && state is 0x00020001u or 0x00200010u && _aoes.Count != 0)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Mouser)
            ++NumCasts;
    }
}

sealed class GrimalkinGaleShockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GrimalkinGaleShockwaveAOE, 21f, true, stopAfterWall: true);
sealed class GrimalkinGaleSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GrimalkinGaleSpreadAOE, 5f);

sealed class SplinteringNails(BossModule module) : Components.CastCounter(module, (uint)AID.SplinteringNailsAOE)
{
    private readonly ElevateAndEviscerate? _jumps = module.FindComponent<ElevateAndEviscerate>();
    private Actor? _source;

    private static readonly AOEShapeCone _shape = new(100f, 25f.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null && _jumps?.CurrentTarget != actor)
        {
            var pcRole = EffectiveRole(actor);
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            if (Raid.WithoutSlot(false, true, true).Exclude(_jumps?.CurrentTarget).Any(a => EffectiveRole(a) != pcRole && _shape.Check(a.Position, _source.Position, pcDir)))
                hints.Add("Spread by roles!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _source == null || _jumps?.CurrentTarget == pc || _jumps?.CurrentTarget == player ? PlayerPriority.Irrelevant : EffectiveRole(player) == EffectiveRole(pc) ? PlayerPriority.Normal : PlayerPriority.Interesting;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null && _jumps?.CurrentTarget != pc)
        {
            var pcDir = Angle.FromDirection(pc.Position - _source.Position);
            _shape.Outline(Arena, _source.Position, pcDir);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SplinteringNails)
        {
            _source = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _source = null;
        }
    }

    private Role EffectiveRole(Actor a) => a.Role == Role.Ranged ? Role.Melee : a.Role;
}
