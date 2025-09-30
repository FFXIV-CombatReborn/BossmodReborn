namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Border(BossModule module) : BossComponent(module)
{
    public bool LBridgeActive;
    public bool RBridgeActive;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state is 0x00020001 or 0x00080004)
        {
            switch (index)
            {
                case 2: RBridgeActive = state == 0x00020001; break;
                case 3: LBridgeActive = state == 0x00020001; break;
            }
        }
        if (!LBridgeActive && !RBridgeActive)
            Arena.Bounds = P10SPandaemonium.DefaultArena;
        else if (!LBridgeActive && RBridgeActive)
            Arena.Bounds = P10SPandaemonium.ArenaR;
        else if (LBridgeActive && !RBridgeActive)
            Arena.Bounds = P10SPandaemonium.ArenaL;
        else if (LBridgeActive && RBridgeActive)
            Arena.Bounds = P10SPandaemonium.ArenaLR;
    }
}
