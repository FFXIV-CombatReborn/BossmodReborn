namespace BossMod.Shadowbringers.Ultimate.TEA;

abstract class P1ProteanWaveTornado : Components.GenericBaitAway
{
    private readonly List<Actor> _liquidRage;
    private readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public P1ProteanWaveTornado(BossModule module, bool enableHints) : base(module, (uint)AID.ProteanWaveTornadoInvis)
    {
        _liquidRage = module.Enemies((uint)OID.LiquidRage);
        EnableHints = enableHints;
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var tornado in _liquidRage)
        {
            var target = Raid.WithoutSlot(false, true, true).ClosestTo(tornado.Position);
            if (target != null)
                CurrentBaits.Add(new(tornado, target, cone));
        }
    }
}

sealed class P1ProteanWaveTornadoVisCast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProteanWaveTornadoVis, new AOEShapeCone(40f, 15f.Degrees()));
sealed class P1ProteanWaveTornadoVisBait(BossModule module) : P1ProteanWaveTornado(module, false);
sealed class P1ProteanWaveTornadoInvis(BossModule module) : P1ProteanWaveTornado(module, true);
