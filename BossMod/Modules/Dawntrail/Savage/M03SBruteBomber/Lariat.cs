﻿namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

abstract class LariatOut(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 10);
class OctupleLariatOut(BossModule module) : LariatOut(module, AID.OctupleLariatOutAOE);
class QuadrupleLariatOut(BossModule module) : LariatOut(module, AID.QuadrupleLariatOutAOE);

abstract class LariatIn(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(10, 60));
class OctupleLariatIn(BossModule module) : LariatIn(module, AID.OctupleLariatInAOE);
class QuadrupleLariatIn(BossModule module) : LariatIn(module, AID.QuadrupleLariatInAOE);

// TODO: generalize to a conal stack/spread with role-based targets
class BlazingLariat(BossModule module) : Components.CastCounter(module, default)
{
    private Actor? _source;
    private bool _stack;

    private AOEShape ActiveShape => _stack ? _shapePairs : _shapeSpread;

    private static readonly AOEShapeCone _shapeSpread = new(40, 22.5f.Degrees());
    private static readonly AOEShapeCone _shapePairs = new(40, 10.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            var clipped = Raid.WithoutSlot(false, true, true).Exclude(actor).InShape(ActiveShape, _source.Position, pcDir);
            if (_stack)
            {
                var cond = clipped.CountByCondition(a => a.Class.IsSupport() == actor.Class.IsSupport());
                hints.Add("Stack in pairs!", cond.match != 0 || cond.mismatch != 1);
            }
            else
            {
                hints.Add("Spread!", clipped.Count != 0);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _source == null ? PlayerPriority.Irrelevant : player.Class.IsSupport() == pc.Class.IsSupport() ? PlayerPriority.Normal : PlayerPriority.Interesting;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(pc.Position - _source.Position);
            ActiveShape.Outline(Arena, _source.Position, pcDir, _stack ? Colors.Safe : Colors.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OctupleLariatIn or AID.OctupleLariatOut or AID.QuadrupleLariatIn or AID.QuadrupleLariatOut)
        {
            _source = caster;
            _stack = (AID)spell.Action.ID is AID.QuadrupleLariatIn or AID.QuadrupleLariatOut;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BlazingLariatSpread or AID.BlazingLariatPair)
        {
            ++NumCasts;
            _source = null;
        }
    }
}
