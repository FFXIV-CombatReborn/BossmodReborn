namespace BossMod;

// debug-only: repeatedly drops a fake circle aoe under the player, forcing whichever movement system is active (automovement or the automatic movement module) to dodge
// with a short activation delay the dodge has no leeway, so it interrupts any cast in progress - this reproduces cast/move interactions with external rotations (e.g. RSR)
// without needing a real encounter; toggled from the debug window, never persisted
public static class FakeAoeTest
{
    public static bool Enabled;
    public static float Radius = 3f;
    public static float ActivationDelay = 1.5f; // pathfinding treats zones as active 1s early, so this leaves ~0.5s - shorter than a typical cast
    public static float Interval = 3f; // how often the aoe is re-dropped under the player

    public static WPos Center { get; private set; }
    public static DateTime Activation { get; private set; }
    public static DateTime NextDrop { get; private set; }
    public static int Drops { get; private set; }

    public static void Reset()
    {
        NextDrop = default;
        Drops = 0;
    }

    // called by the hints builder every frame, after module/auto hints are filled
    public static void Apply(WorldState ws, Actor player, AIHints hints)
    {
        if (!Enabled)
        {
            return;
        }

        var now = ws.CurrentTime;
        if (now >= NextDrop)
        {
            Center = player.Position;
            Activation = now.AddSeconds(ActivationDelay);
            NextDrop = now.AddSeconds(Interval);
            ++Drops;
        }

        hints.AddForbiddenZone(new SDCircle(Center, Radius), Activation);
        Camera.Instance?.DrawWorldCircle(Center.ToVec3(player.PosRot.Y), Radius, Colors.Danger, 2f);
    }
}
