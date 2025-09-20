namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

// these two abilities are very similar, only differ by activation delay and action id
// TODO: not all the wall is safe...
abstract class FeralHowlHystericAssault(BossModule module, uint aidCast, uint aidAOE, double delay) : Components.GenericKnockback(module, aidAOE, stopAtWall: true)
{
    private Knockback[] _kb = [];
    private HuntersClaw? _aoe = module.FindComponent<HuntersClaw>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == aidCast)
        {
            _kb = [new(caster.Position, 30f, Module.CastFinishAt(spell, delay), ignoreImmunes: true)];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _aoe ??= Module.FindComponent<HuntersClaw>();
        if (_aoe == null)
            return;
        var count = _aoe.Casters.Count;
        if (count == 0)
            return;
        if (_kb.Length != 0)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
            var forbidden = new ShapeDistance[count];
            var pos = Module.PrimaryActor.Position;
            ref var kb = ref _kb[0];
            for (var i = 0; i < count; ++i)
            {
                ref var a = ref aoes[i];
                var origin = a.Origin;
                forbidden[i] = new SDCone(pos, 100f, Module.PrimaryActor.AngleTo(origin), Angle.Asin(8f / (origin - pos).Length()));
            }
            hints.AddForbiddenZone(new SDUnion(forbidden), kb.Activation);
        }
    }
}

sealed class FeralHowl(BossModule module) : FeralHowlHystericAssault(module, (uint)AID.FeralHowl, (uint)AID.FeralHowlAOE, 2.1d);
sealed class HystericAssault(BossModule module) : FeralHowlHystericAssault(module, (uint)AID.HystericAssault, (uint)AID.HystericAssaultAOE, 0.9d);
