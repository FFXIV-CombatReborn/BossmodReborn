namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class BanishgaIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.BanishgaIV);
sealed class HolyII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyII, new AOEShapeCircle(6f));
sealed class BanishgaIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BanishgaIVSpread, 6f);

sealed class ImpartialRuling(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ImpartialRulingShort, (uint)AID.ImpartialRulingLong], new AOEShapeCone(60f, 90f.Degrees()), 1, 1);

sealed class RadiantSacrament(BossModule module) : Components.GenericAOEs(module, (uint)AID.RadiantSacrament1)
{
    private static readonly AOEShapeRect square = new(5f, 5f, 5f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        return aoes[..Math.Min(15, aoes.Length)];
    }

    public override void OnMapEffect(byte index, uint state) // credit for finding the way these related to the squares goes to Xan. 
    {
        if (index is >= 0x14 and <= 0x2C && state == 0x00020001)
        {
            var ix = index - 0x14;
            var row = ix % 5;
            var col = ix / 5;
            var wd = new WDir(10f * col, 10f * row) + (Arena.Center - new WDir(20f, 20f));
            _aoes.Add(new(square, wd, default, WorldState.FutureTime(8.1d)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class DivineSpear(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineSpear1, new AOEShapeTriCone(25f, 45.Degrees()));

sealed class MegaHoly(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MegaHoly, (uint)AID.MegaHolyCast, 6f, 7f, maxCasts: 3)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    { }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MegaHolyCast or (uint)AID.MegaHolySpam)
        {
            var count = Stacks.Count;
            var t = spell.MainTargetID;
            var stacks = CollectionsMarshal.AsSpan(Stacks);
            for (var i = 0; i < count; ++i)
            {
                if (stacks[i].Target.InstanceID == t)
                {
                    Stacks.RemoveAt(i);
                    return;
                }
            }
            Stacks.Clear(); // stack was not found, just clear all - this can happen if donut stacks is self targeted instead of player targeted
        }
    }
}

class GordiusSystem(BossModule module) : Components.Adds(module, (uint)OID.GordiusSystem, 1);

sealed class Activate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Activate, 3f);
class PerfectDefense(BossModule module) : Components.InvincibleStatus(module, (uint)SID.PerfectDefense2);
class HolyFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyFlame, 5f);
class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 7f);
class CircuitShock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircuitShock, new AOEShapeDonut(7, 18));
class DivineJudgment(BossModule module) : Components.RaidwideCast(module, (uint)AID.DivineJudgment);

class Electrify(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _tethers = [];
    private static readonly AOEShapeCircle _circle = new(18f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_tethers);
        return aoes[..Math.Min(2, aoes.Length)];
    }
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Electrify)
            _tethers.Add(new(_circle, source.Position, default, WorldState.FutureTime(9)));
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Electrify)
        {
            NumCasts++;
            if (_tethers.Count != 0)
            {
                _tethers.RemoveAt(0);
            }
        }
    }
}
sealed class DivineBolt(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 3f), (uint)IconID.DivineBolt, (uint)AID.DivineBolt1, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, HerStolenLight, Xan", PrimaryActorOID = (uint)OID.AlexanderResurrected, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14529u)]
public sealed class A32Alexander(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, 360f), new ArenaBoundsSquare(25f))
{
    public override bool ShouldPrioritizeAllEnemies => true;
}
