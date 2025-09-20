namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class UnsealedAura(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnsealedAura);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Magitaur, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018u, NameID = 13947u, PlanLevel = 100, SortOrder = 5, Category = BossModuleInfo.Category.Foray, Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class FTB4Magitaur(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena, true)
{
    private static readonly WPos arenaCenter = new(700f, -674f);
    private static readonly Square[] baseArena = [new Square(arenaCenter, 31.5f)];
    private static readonly ArenaBoundsCustom arena = new([new Polygon(arenaCenter, 31.5f, 90)], [new Rectangle(new(700f, -705.916f), 7.5f, 1.25f), new Rectangle(new(700f, -641.5f), 7.5f, 1.25f)]);
    public static readonly WPos[] SquarePositions = [new(700f, -659.504f), new(712.554f, -681.248f), new(687.443f, -681.25f)]; // starting in south, ccw order
    public static readonly Angle[] SquareAngles = [-45f.Degrees(), -15f.Degrees(), 105f.Degrees()];
    public static readonly WDir[] SquareDirs = [SquareAngles[0].ToDirection(), SquareAngles[1].ToDirection(), SquareAngles[2].ToDirection()];
    private static readonly Square[] squares = [new Square(SquarePositions[0], 10f, SquareAngles[0]), new Square(SquarePositions[1], 10f, SquareAngles[1]),
    new Square(SquarePositions[2], 10f, SquareAngles[2])];
    public static readonly AOEShapeCustom CircleMinusSquares = new(baseArena, squares);
    public static readonly AOEShapeCustom BigSpreadHint = GenerateBigSpreadHint();
    public static readonly AOEShapeCustom CircleMinusSquaresSpread = new(baseArena, [new Square(SquarePositions[0], 5f, SquareAngles[0]), new Square(SquarePositions[1], 5f, SquareAngles[1]),
    new Square(SquarePositions[2], 5f, SquareAngles[2])]);
    public static readonly AOEShapeRect Square = new(10f, 10f, 10f);
    public static readonly AOEShapeCustom[] StackInsideSquare = [new(baseArena, [new Square(SquarePositions[0], 4f, SquareAngles[0])]), new(baseArena, [new Square(SquarePositions[1], 4f, SquareAngles[1])]),
    new(baseArena, [new Square(SquarePositions[2], 4f, SquareAngles[2])])];
    public static readonly AOEShapeCustom[] StackOutsideSquare = [GenerateStackHints(0), GenerateStackHints(1), GenerateStackHints(2)];

    private static AOEShapeCustom GenerateBigSpreadHint()
    {
        var shape = new AOEShapeCustom(squares);
        shape.Polygon = shape.GetCombinedPolygon(arenaCenter).Offset(11f, Clipper2Lib.JoinType.Round);
        return shape;
    }

    private static AOEShapeCustom GenerateStackHints(int squareIndex)
    {
        var untouchableSquares = new Square[2];
        var index = 0;
        for (var i = 0; i < 3; ++i)
        {
            if (i == squareIndex)
            {
                continue;
            }
            else
            {
                untouchableSquares[index++] = squares[i];
            }
        }
        var shape = new AOEShapeCustom(untouchableSquares);
        shape.Polygon = shape.GetCombinedPolygon(arenaCenter).Offset(6f, Clipper2Lib.JoinType.Round);
        return shape;
    }
}
