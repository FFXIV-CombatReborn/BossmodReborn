namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P2EarthMissileBaited(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.EarthMissileBaited, m => m.Enemies((uint)OID.VoidzoneEarthMissileBaited).Where(z => z.EventState != 7), 0.9d);

sealed class P2EarthMissileIce(BossModule module) : Components.VoidzoneAtCastTarget(module, 10f, (uint)AID.EarthMissileIce, Voidzones, 0.8d) // TODO: verify larger radius...
{
    private static IEnumerable<Actor> Voidzones(BossModule m)
    {
        foreach (var z in m.Enemies((uint)OID.VoidzoneEarthMissileIceSmall).Where(z => z.EventState != 7))
        {
            yield return z;
            yield break;
        }
        foreach (var z in m.Enemies((uint)OID.VoidzoneEarthMissileIceLarge).Where(z => z.EventState != 7))
        {
            yield return z;
            yield break;
        }
    }
}

// note: we use a single spread/stack component for both enumerations and ice missile spreads, since they happen at the same time
// TODO: add hint for spread target to stay close to tornado...
sealed class P2Enumeration(BossModule module) : Components.UniformStackSpread(module, 5f, 6f, 3, 3, true, false) // TODO: verify enumeration radius
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.Enumeration:
                // note: we assume tanks never share enumeration
                AddStack(actor, WorldState.FutureTime(5.1d), Raid.WithSlot(true, true, true).WhereActor(p => p.Role == Role.Tank).Mask());
                break;
            case (uint)IconID.EarthMissileIce:
                AddSpread(actor, WorldState.FutureTime(5.1d));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Enumeration:
                Stacks.Clear();
                break;
            case (uint)AID.EarthMissileIce:
                Spreads.Clear();
                break;
        }
    }
}

sealed class P2HiddenMinefield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HiddenMinefield, 5f)
{
    private readonly List<WPos> _mines = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var m in _mines)
            Arena.Actor(m, default, Colors.Object);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
            _mines.Add(caster.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID is (uint)AID.HiddenMine or (uint)AID.HiddenMineShrapnel)
            _mines.RemoveAll(m => m.AlmostEqual(caster.Position, 1f));
    }
}
