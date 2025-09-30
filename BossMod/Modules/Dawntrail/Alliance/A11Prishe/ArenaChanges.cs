namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module, (uint)AID.Thornbite)
{
    public bool Active => _aoe.Length != 0 || Arena.Bounds != A11Prishe.DefaultBounds;
    private AOEInstance[] _aoe = [];
    private static readonly Square[] defaultSquare = [new(A11Prishe.ArenaCenter, 35f)];
    public static readonly Square[] MiddleENVC00020001 = [new(new(795f, 405f), 10f), new(new(805f, 395f), 10f)];
    private static readonly Shape[] differenceENVC00020001 = [.. MiddleENVC00020001, new Rectangle(new(810f, 430f), 15f, 5f),
    new Rectangle(new(830f, 420f), 5f, 15f), new Rectangle(new(790f, 370f), 15f, 5f), new Rectangle(new(770f, 380f), 5f, 15f)];
    private static readonly AOEShapeCustom arenaChangeENVC00020001 = new(defaultSquare, differenceENVC00020001);
    public static readonly ArenaBoundsCustom ArenaENVC00020001 = new(differenceENVC00020001);
    public static readonly Square[] MiddleENVC02000100 = [new(new(795f, 395f), 10f), new(new(805f, 405f), 10f)];
    private static readonly Shape[] differenceENVC02000100 = [.. MiddleENVC02000100, new Rectangle(new(820f, 370f), 15f, 5f),
    new Rectangle(new(830f, 390f), 5f, 15f), new Rectangle(new(780f, 430f), 15f, 5f), new Rectangle(new(770f, 410f), 5f, 15f)];
    private static readonly AOEShapeCustom arenaChangeENVC02000100 = new(defaultSquare, differenceENVC02000100);
    public static readonly ArenaBoundsCustom ArenaENVC02000100 = new(differenceENVC02000100);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                SetAOE(arenaChangeENVC00020001);
                break;
            case 0x02000100u:
                SetAOE(arenaChangeENVC02000100);
                break;
            case 0x00200010u:
                SetArena(ArenaENVC00020001);
                break;
            case 0x08000400u:
                SetArena(ArenaENVC02000100);
                break;
            case 0x00080004u or 0x00800004u:
                Arena.Bounds = A11Prishe.DefaultBounds;
                Arena.Center = A11Prishe.ArenaCenter;
                break;
        }

        void SetArena(ArenaBoundsCustom bounds)
        {
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
            _aoe = [];
        }

        void SetAOE(AOEShapeCustom shape) => _aoe = [new(shape, Arena.Center, default, WorldState.FutureTime(5d))];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // no need to generate a hint here, we generate a special hint in CrystallineThornsHint
    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

sealed class CrystallineThornsHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCustom hintENVC00020001 = new(ArenaChanges.MiddleENVC00020001, invertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100 = new(ArenaChanges.MiddleENVC02000100, invertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                SetAOE(hintENVC00020001);
                break;
            case 0x02000100u:
                SetAOE(hintENVC02000100);
                break;
            case 0x00200010u:
            case 0x08000400u:
                _aoe = [];
                break;
        }
        void SetAOE(AOEShapeCustom shape) => _aoe = [new(shape, Arena.Center, default, WorldState.FutureTime(5d), Colors.SafeFromAOE)];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        if (!aoe.Check(actor.Position))
        {
            hints.Add("Go into middle to prepare for knockback!");
        }
    }
}
