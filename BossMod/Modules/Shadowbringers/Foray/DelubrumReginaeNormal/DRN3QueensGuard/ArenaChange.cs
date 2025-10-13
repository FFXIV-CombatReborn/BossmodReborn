namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN3QueensGuard;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];
    private bool startingArena = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        if (startingArena && _aoe.Length == 0)
        {
            var features = Module.Enemies((uint)OID.ArenaFeatures);
            var count = features.Count;
            for (var i = 0; i < count; ++i)
            {
                var f = features[i];
                if (f.EventState == default && f.Position.AlmostEqual(new(244f, -129f), 1f))
                {
                    _aoe = [new(donut, Arena.Center, default, WorldState.FutureTime(5d))];
                    return;
                }
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18 && state == 0x00020001u)
        {
            Arena.Bounds = QueensGuard.DefaultArena;
            Arena.Center = QueensGuard.DefaultArena.Center;
            _aoe = [];
            startingArena = false;
        }
    }
}
