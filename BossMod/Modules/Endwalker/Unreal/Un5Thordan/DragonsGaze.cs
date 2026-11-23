namespace BossMod.Endwalker.Unreal.Un5Thordan;

sealed class DragonsGaze(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Actor> _casters = [];
    private WPos _posHint;

    private readonly List<Eye> _eyes = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        _eyes.Clear();
        foreach (var c in _casters)
        {
            if (c.CastInfo is ActorCastInfo ci)
                _eyes.Add(new(c.Position, Module.CastFinishAt(ci)));
        }
        return CollectionsMarshal.AsSpan(_eyes);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_posHint != default)
            Arena.ZoneCircleOutline(_posHint, 1, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DragonsGaze or (uint)AID.DragonsGlory)
        {
            _casters.Add(caster);
            _posHint = default;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DragonsGaze or (uint)AID.DragonsGlory)
            _casters.Remove(caster);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && actor.OID is >= (uint)OID.DragonEyeN and <= (uint)OID.DragonEyeNW)
        {
            var index = actor.OID - (uint)OID.DragonEyeN; // 0 = N, then CW
            _posHint = Module.Center + 19 * (180 - (int)index * 45).Degrees().ToDirection();
        }
    }
}
