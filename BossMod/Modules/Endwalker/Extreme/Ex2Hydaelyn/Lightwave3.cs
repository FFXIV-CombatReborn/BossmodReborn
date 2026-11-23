namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for third lightwave (safe zone)
sealed class Lightwave3(BossModule module) : LightwaveCommon(module)
{
    public override void Update()
    {
        // try to find two helpers with Z=70 before first cast
        if (Waves.Count == 0)
        {
            var helpers = Module.Enemies((uint)OID.Helper);
            for (var i = 0; i < helpers.Count; ++i)
            {
                if (IsInitialLightwave(helpers[i]))
                    Waves.Add(helpers[i]);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Waves.Count == 0)
            return;

        for (var i = 0; i < Waves.Count; ++i)
        {
            if (WaveAOE.Check(actor.Position, Waves[i]))
            {
                hints.Add("GTFO from wave!");
                break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Waves.Count == 0)
            return;

        foreach (var wave in Waves)
            WaveAOE.Draw(Arena, wave);
    }

    private bool IsInitialLightwave(Actor a)
    {
        var pos = a.Position;
        return Math.Abs(pos.X - 70f) < 1f || Math.Abs(pos.X - 130f) < 1f || Math.Abs(pos.Z - 70f) < 1f || Math.Abs(pos.Z - 130f) < 1f;
    }
}
