namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x3E: // byakko
                switch (state)
                {
                    case 0x00020001u:
                        Arena.Bounds = A21FaithboundKirin.ByakkoArena;
                        Arena.Center = A21FaithboundKirin.ByakkoArena.Center;
                        break;
                    case 0x00080004u:
                        Arena.Bounds = A21FaithboundKirin.DefaultArena;
                        Arena.Center = A21FaithboundKirin.DefaultCenter;
                        break;
                }
                break;
            case 0x46: // mighty grip
                switch (state)
                {
                    case 0x00200010u:
                        _aoe = [new(A21FaithboundKirin.RectArenaAOE, Arena.Center, default, WorldState.FutureTime(11.1d))];
                        break;
                    case 0x00020001u:
                        Arena.Bounds = new ArenaBoundsRect(12.5f, 15f);
                        Arena.Center = A21FaithboundKirin.RectCenter;
                        _aoe = [];
                        break;
                    case 0x00080004u:
                        Arena.Bounds = A21FaithboundKirin.DefaultArena;
                        Arena.Center = A21FaithboundKirin.DefaultCenter;
                        break;
                }
                break;
            case 0x4B: // suzaku
                switch (state)
                {
                    case 0x00020001u:
                        Arena.Bounds = new ArenaBoundsSquare(20f);
                        break;
                    case 0x00080004u:
                        Arena.Bounds = A21FaithboundKirin.DefaultArena;
                        break;
                }
                break;
        }
    }
}
