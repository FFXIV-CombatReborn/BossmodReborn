namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class RuthlessRegalia(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(100f, 6f), rectWide = new(100f, 12f);
    private (Actor, DateTime)? _source;
    private readonly List<Actor> _tethered = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
        {
            var s = _source.Value;
            return new AOEInstance[1] { new(rect, s.Item1.Position, s.Item1.Rotation, s.Item2) };
        }
        return [];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.QueenEternal2 && id == 0x11D2)
            _source = new(actor, WorldState.FutureTime(11.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RuthlessRegalia)
            _source = null;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        _tethered.Add(source);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _tethered.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_source == null)
            return;

        if (_tethered.Count > 1 && _tethered.Contains(actor))
        {
            var s = _source.Value.Item1;
            var t = _tethered.FirstOrDefault(x => x != actor);
            ref var sPosRot = ref s.PosRot;
            hints.AddForbiddenZone(rectWide, new(sPosRot.X, t!.PosRot.Z), sPosRot.W.Degrees());
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
