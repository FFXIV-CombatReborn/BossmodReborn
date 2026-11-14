namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

sealed class ImpassionedSparks(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparksPuddle, 8f);
sealed class ChainsOfPassion(BossModule module) : Components.Chains(module, (uint)TetherID.Chain, (uint)AID.HotFoot, 35f);
sealed class HotFoot(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread10, (uint)AID.HotFoot, 10f, 5.2d);

sealed class BurningPillar(BossModule module) : Components.VoidzoneAtCastTarget(module, 10f, (uint)AID.BurningPillar, GetPuddles, 0.8d)
{
    public int Groups;
    private DateTime _prevCast;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            var now = WorldState.CurrentTime;
            if (now > _prevCast.AddSeconds(1d))
            {
                ++Groups;
            }
            _prevCast = now;
        }
    }

    private static Actor[] GetPuddles(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.BurnPuddle);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }
        var result = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                result[index++] = z;
            }
        }
        return index == count ? result : result[..index];
    }
}
