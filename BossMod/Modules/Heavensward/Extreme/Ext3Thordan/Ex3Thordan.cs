namespace BossMod.Heavensward.Extreme.Ex3Thordan;

sealed class AscalonsMight(BossModule module) : Components.Cleave(module, (uint)AID.AscalonsMight, new AOEShapeCone(11.8f, 45f.Degrees()));

abstract class HeavenlySlash(BossModule module, OID oid) : Components.Cleave(module, (uint)AID.HeavenlySlash, new AOEShapeCone(10.2f, 45f.Degrees()), [(uint)oid]);
sealed class HeavenlySlashAdelphel(BossModule module) : HeavenlySlash(module, OID.SerAdelphel);
sealed class HeavenlySlashJanlenoux(BossModule module) : HeavenlySlash(module, OID.SerJanlenoux);

sealed class Meteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorainAOE, 6f);
sealed class AscalonsMercy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercy, new AOEShapeCone(34.8f, 10f.Degrees()));
sealed class AscalonsMercyHelper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercyAOE, new AOEShapeCone(34.5f, 10f.Degrees()));
sealed class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DragonsRage, 6f, 8, 8);
sealed class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LightningStormAOE, 5f);
sealed class Heavensflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensflameAOE, 6f);
sealed class Conviction(BossModule module) : Components.CastTowers(module, (uint)AID.ConvictionAOE, 3f);
sealed class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.HolyChain);
sealed class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.SerZephirin);

sealed class BossReappear(BossModule module) : Components.CastCounter(module, (uint)AID.BossReappear);

sealed class LightOfAscalon(BossModule module) : Components.GenericKnockback(module, (uint)AID.LightOfAscalon)
{
    private readonly List<Knockback> _sources = new(7);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (_sources.Count > 0 && spell.Action.ID == (uint)AID.LightOfAscalon)
        {
            _sources.RemoveAt(0);
        }
        else if (spell.Action.ID == (uint)AID.BossReappear)
        {
            var origin = new WPos(-0.822f, -16.314f); // knockback originates from a helper
            for (var i = 0d; i < 7d; ++i)
            {
                _sources.Add(new(origin, 3f, WorldState.FutureTime(10.7d + i * 1.3d), ignoreImmunes: true));
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sources.Count != 0)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(new WPos(default, -15f), 2f, 2f));
        }
    }
}

sealed class UltimateEnd(BossModule module) : Components.CastCounter(module, (uint)AID.UltimateEndAOE);
sealed class HeavenswardLeap(BossModule module) : Components.CastCounter(module, (uint)AID.HeavenswardLeap);
sealed class PureOfSoul(BossModule module) : Components.CastCounter(module, (uint)AID.PureOfSoul);
sealed class AbsoluteConviction(BossModule module) : Components.CastCounter(module, (uint)AID.AbsoluteConviction);

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(21f, 24f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(donut, default) };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 91, NameID = 3632, PlanLevel = 60)]
public sealed class Ex3Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, default, startingArena)
{
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(default, 23.5f, 48)]);
    public static readonly ArenaBoundsCustom DefaultBounds = new([new Polygon(default, 21.052f, 40)]);
    private Actor? _bossAdelphel;
    public Actor? BossAdelphel() => _bossAdelphel;
    private Actor? _bossJanlenoux;
    public Actor? BossJanlenoux() => _bossJanlenoux;

    protected override void UpdateModule()
    {
        if (StateMachine.ActivePhaseIndex >= 1)
        {
            _bossJanlenoux ??= GetActor((uint)OID.SerJanlenoux);
            _bossAdelphel ??= GetActor((uint)OID.SerAdelphel);
        }
    }
}
