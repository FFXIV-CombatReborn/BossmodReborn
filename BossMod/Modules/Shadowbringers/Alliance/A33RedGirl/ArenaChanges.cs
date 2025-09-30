namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private WipeBlackWhite? _wipe;
    private AOEInstance[] _aoe = [];
    private DateTime activation;
    private static readonly Angle am90 = -89.9802f.Degrees();
    public readonly (WPos position, Angle rotation, bool? isWhite)[] Walls =
    [
        (new(845f, -873f), am90, null), // 0x1D
        (new(845f, -867f), am90, null), // 0x1E
        (new(845f, -861f), am90, null), // 0x1F
        (new(845f, -855f), am90, null), // 0x20
        (new(845f, -847f), am90, null), // 0x21
        (new(845f, -841f), am90, null), // 0x22
        (new(845f, -835f), am90, null), // 0x23
        (new(845f, -829f), am90, null), // 0x24
        (new(823f, -851f), default, null), // 0x25
        (new(829f, -851f), default, null), // 0x26
        (new(835f, -851f), default, null), // 0x27
        (new(841f, -851f), default, null), // 0x28
        (new(849f, -851f), default, null), // 0x29
        (new(855f, -851f), default, null), // 0x2A
        (new(861f, -851f), default, null), // 0x2B
        (new(867f, -851f), default, null), // 0x2C
        (new(838f, -860f), default, null), // 0x2D
        (new(844f, -860f), default, null), // 0x2E
        (new(850f, -860f), default, null), // 0x2F
        (new(854f, -858f), am90, null), // 0x30
        (new(854f, -852f), am90, null), // 0x31
        (new(854f, -846f), am90, null), // 0x32
        (new(852f, -842f), default, null), // 0x33
        (new(846f, -842f), default, null), // 0x34
        (new(840f, -842f), default, null), // 0x35
        (new(836f, -844f), am90, null), // 0x36
        (new(836f, -850f), am90, null), // 0x37
        (new(836f, -856f), am90, null), // 0x38
    ];
    private bool isDefaultArena;
    public int NumWalls;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1C and <= 0x38)
        {
            switch (index)
            {
                case 0x01C:
                    if (state == 0x00020001u)
                    {
                        activation = WorldState.FutureTime(3d);
                        _aoe = [new(A33RedGirl.ArenaTransition, Arena.Center, default, activation)];
                    }
                    else if (state == 0x00080004u)
                    {
                        Arena.Bounds = A33RedGirl.StartingArena;
                        isDefaultArena = false;
                    }
                    break;
                default:
                    ref var w = ref Walls[index - 0x1D];
                    w.isWhite = state == 0x00020001u ? false : state == 0x00800040u ? true : null;
                    var differenceShapes = new List<Shape>(17);
                    if (isDefaultArena)
                    {
                        differenceShapes.Add(A33RedGirl.InnerSquare);
                    }
                    NumWalls = 0;
                    for (var i = 0; i < 28; ++i)
                    {
                        ref readonly var wall = ref Walls[i];
                        if (wall.isWhite != null)
                        {
                            differenceShapes.Add(new Rectangle(wall.position, 3.5f, 1.5f, wall.rotation));
                            ++NumWalls;
                        }
                    }

                    _wipe ??= Module.FindComponent<WipeBlackWhite>();
                    _wipe?.UpdateAOE();
                    Arena.Bounds = new ArenaBoundsCustom(isDefaultArena ? A33RedGirl.DefaultSquare : A33RedGirl.BigSquare, [.. differenceShapes]);
                    break;
            }
        }
    }

    public override void Update()
    {
        if (_aoe.Length != 0 && activation < WorldState.CurrentTime)
        {
            _aoe = [];
            Arena.Bounds = A33RedGirl.DefaultArena;
            isDefaultArena = true;
        }
    }
}
