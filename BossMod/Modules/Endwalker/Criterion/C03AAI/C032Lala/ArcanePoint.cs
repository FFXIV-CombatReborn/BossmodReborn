namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

// TODO: we could detect aoe positions slightly earlier, when golems spawn
abstract class ConstructiveFigure(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(50f, 4f));
sealed class NConstructiveFigure(BossModule module) : ConstructiveFigure(module, (uint)AID.NAero);
sealed class SConstructiveFigure(BossModule module) : ConstructiveFigure(module, (uint)AID.SAero);

sealed class ArcanePoint(BossModule module) : BossComponent(module)
{
    public int NumCasts;
    private readonly ArcanePlot? _plot = module.FindComponent<ArcanePlot>();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts > 0)
            return;
        var spot = CurrentSafeSpot(actor.Position);
        if (spot == null)
            return;
        var party = Raid.WithoutSlot(false, true, true);
        for (var i = 0; i < party.Length; ++i)
        {
            if (party[i] != actor && CurrentSafeSpot(party[i].Position) == spot)
            {
                hints.Add("Spread on different squares!");
                break;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return PlayerPriority.Interesting;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (NumCasts > 0)
            return;
        var spot = CurrentSafeSpot(pc.Position);
        if (spot != null)
            ArcaneArrayPlot.Shape.Draw(Arena, spot.Value, default, Colors.SafeFromAOE);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NPowerfulLight or (uint)AID.SPowerfulLight)
        {
            ++NumCasts;
            _plot?.AddAOE(caster.Position, default);
        }
    }

    public WPos? CurrentSafeSpot(WPos pos)
    {
        if (_plot == null)
            return null;
        var index = _plot.SafeZoneCenters.FindIndex(p => ArcaneArrayPlot.Shape.Check(pos, p, default));
        return index >= 0 ? _plot.SafeZoneCenters[index] : null;
    }
}

abstract class ExplosiveTheorem(BossModule module, uint aid) : Components.SpreadFromCastTargets(module, aid, 8f);
sealed class NExplosiveTheorem(BossModule module) : ExplosiveTheorem(module, (uint)AID.NExplosiveTheoremAOE);
sealed class SExplosiveTheorem(BossModule module) : ExplosiveTheorem(module, (uint)AID.SExplosiveTheoremAOE);

abstract class TelluricTheorem(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 8f);
sealed class NTelluricTheorem(BossModule module) : TelluricTheorem(module, (uint)AID.NTelluricTheorem);
sealed class STelluricTheorem(BossModule module) : TelluricTheorem(module, (uint)AID.STelluricTheorem);
