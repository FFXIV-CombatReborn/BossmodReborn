namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

abstract class LariatOut(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 10f);
sealed class OctupleLariatOut(BossModule module) : LariatOut(module, (uint)AID.OctupleLariatOutAOE);
sealed class QuadrupleLariatOut(BossModule module) : LariatOut(module, (uint)AID.QuadrupleLariatOutAOE);

abstract class LariatIn(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(10f, 60f));
sealed class OctupleLariatIn(BossModule module) : LariatIn(module, (uint)AID.OctupleLariatInAOE);
sealed class QuadrupleLariatIn(BossModule module) : LariatIn(module, (uint)AID.QuadrupleLariatInAOE);

// TODO: generalize to a conal stack/spread with role-based targets
sealed class BlazingLariat(BossModule module) : Components.CastCounter(module, default)
{
    private Actor? _source;
    private bool _stack;

    private AOEShape ActiveShape => _stack ? _shapePairs : _shapeSpread;

    private readonly AOEShapeCone _shapeSpread = new(40f, 22.5f.Degrees());
    private readonly AOEShapeCone _shapePairs = new(40f, 10f.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null)
        {
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            int match = 0, mismatch = 0;
            var party = Raid.WithoutSlot(false, true, true);
            var isSupport = actor.Class.IsSupport();
            for (var i = 0; i < party.Length; ++i)
            {
                var p = party[i];
                if (p == actor || !ActiveShape.Check(p.Position, _source.Position, pcDir))
                    continue;
                if (p.Class.IsSupport() == isSupport)
                    ++match;
                else
                    ++mismatch;
            }
            if (_stack)
            {
                hints.Add("Stack in pairs!", match != 0 || mismatch != 1);
            }
            else
            {
                hints.Add("Spread!", match + mismatch != 0);
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
            ActiveShape.Outline(Arena, _source.Position, pcDir, _stack ? Colors.Safe : default);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OctupleLariatIn or (uint)AID.OctupleLariatOut or (uint)AID.QuadrupleLariatIn or (uint)AID.QuadrupleLariatOut)
        {
            _source = caster;
            _stack = spell.Action.ID is (uint)AID.QuadrupleLariatIn or (uint)AID.QuadrupleLariatOut;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BlazingLariatSpread or (uint)AID.BlazingLariatPair)
        {
            ++NumCasts;
            _source = null;
        }
    }
}
