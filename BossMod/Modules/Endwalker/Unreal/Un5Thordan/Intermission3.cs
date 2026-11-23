namespace BossMod.Endwalker.Unreal.Un5Thordan;

sealed class HiemalStormSpread(BossModule module) : Components.UniformStackSpread(module, 0, 6)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HiemalStorm)
            AddSpread(actor, WorldState.FutureTime(3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HiemalStormAOE)
            Spreads.Clear();
    }
}

sealed class HiemalStormVoidzone(BossModule module) : Components.Voidzone(module, 6, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.HiemalStorm);
        var count = enemies.Count;
        if (count == 0)
            return [];
        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class SpiralPierce(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 6), (uint)TetherID.SpiralPierce, (uint)AID.SpiralPierce);
sealed class DimensionalCollapse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalCollapseAOE, 9);

sealed class FaithUnmoving(BossModule module) : Components.GenericKnockback(module, (uint)AID.FaithUnmoving)
{
    private readonly List<Knockback> _kbBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbBuffer.Clear();
        _kbBuffer.Add(new(Module.Center, 16f, ignoreImmunes: true));
        return CollectionsMarshal.AsSpan(_kbBuffer);
    }
}

sealed class CometCircle(BossModule module) : Components.Adds(module, (uint)OID.CometCircle);
sealed class MeteorCircle(BossModule module) : Components.Adds(module, (uint)OID.MeteorCircle);

sealed class HeavyImpact(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, 135.Degrees()), new AOEShapeDonutSector(6.5f, 12.5f, 135.Degrees()), new AOEShapeDonutSector(12.5f, 18.5f, 135.Degrees()), new AOEShapeDonutSector(18.5f, 27.5f, 135.Degrees())];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyImpactAOE1)
            AddSequence(caster.Position, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.HeavyImpactAOE1 => 0,
            (uint)AID.HeavyImpactAOE2 => 1,
            (uint)AID.HeavyImpactAOE3 => 2,
            (uint)AID.HeavyImpactAOE4 => 3,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2), caster.Rotation))
            ReportError($"Unexpected ring {order}");
    }
}
