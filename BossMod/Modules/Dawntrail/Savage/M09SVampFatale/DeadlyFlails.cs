namespace BossMod.Dawntrail.Savage.M09SVampFatale;

// all adds spawn before arena change, including doornail and flail
// prioritize doornail 1st for all, then lower priority for melees once outside of melee range
sealed class DeadlyDoornail(BossModule module) : Components.Adds(module, (uint)OID.DeadlyDoornail, 3);
// prioritize for melees once doornail outside melee range
sealed class FatalFlail(BossModule module) : Components.Adds(module, (uint)OID.FatalFlail, 2);
sealed class Plummet(BossModule module) : Components.CastTowers(module, (uint)AID.Plummet, 3f)
{
    private BitMask _nonTanks;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _nonTanks = default;
            var party = Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Item2.Role != Role.Tank)
                    _nonTanks.Set(i);
            }

            Towers.Add(new(spell.LocXZ, 3f, forbiddenSoakers: _nonTanks, actorID: caster.InstanceID));
        }
    }
}
sealed class Electrocution(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrocution, 3f);
sealed class ElectrocutionVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    // no spells to mark start of growing AOE
    // is there gap between Electrocution finishing, doornail spawning, and aoe appearing? small window, not worth considering
    // map effect 13 0x00200010 for start, same time it becomes targetable
    // map effect 13 0x00080004 for end, same time it dies
    // starts 1s after Electrocution ends
    // starts at 3f, grows at a rate of .5/s
    // is there a maximum size? max around 10f

    private bool _show = false;
    private bool _active = false;
    private WPos _doornailPos;
    private DateTime _start;
    private readonly AOEInstance[] _aoe = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_show)
            return [];

        var aoe = _aoe;
        var radius = 3f;

        if (!_active || _start == default)
        {
            aoe[0] = new(new AOEShapeCircle(radius), _doornailPos);
        }
        else
        {
            var elapsed = (WorldState.CurrentTime - _start).TotalMilliseconds;
            radius += (float)(elapsed / 2000d);
            radius = radius >= 10f ? 10f : radius;
            aoe[0] = new(new AOEShapeCircle(radius), _doornailPos);
        }

        return aoe;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrocution)
        {
            _doornailPos = caster.Position;
            _show = true;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x12 and <= 0x19)
        {
            // spawn
            if (state == 0x00200010)
            {
                // EObjAnim 1EBF1F -> 0x00100020, 3s after map effect
                // map effect is spawn, EObjAnim is actual growing start
                _active = true;
            }
            // destroyed
            else if (state == 0x00080004)
            {
                _show = false;
                _active = false;
                _start = default;
            }
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            _start = WorldState.CurrentTime;
        }
    }
}
// saws spawn on sadistic screech cast start, but don't show AOE until arena changes
sealed class GravegrazerSmall(BossModule module) : Components.GenericAOEs(module)
{
    // neckbiters get status 2056 with extra 0x443 before moving
    // make it bigger than actual cast to avoid
    // casts full AOE periodically, is voidzone ahead of actor enough?
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _neckbiters = [];
    private readonly AOEShapeRect _rect = new(5f + 5f, 2.5f);
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.Active)
            return [];

        var count = _neckbiters.Count;
        if (count == 0)
            return [];

        _aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            _aoes.Add(new(_rect, _neckbiters[i].Position, _neckbiters[i].Rotation));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Neckbiter)
        {
            _neckbiters.Add(actor);
        }
    }
}
sealed class GravegrazerBig(BossModule module) : Components.GenericAOEs(module)
{
    // coffinmaker spawn + movement by mapeffects 0x0D and 0x0E -> 0x00800040 and 0x01000020
    private readonly List<Actor> _coffinmakers = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(10f + 10f, 2.5f);
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.Active)
            return [];

        var count = _coffinmakers.Count;
        if (count == 0)
            return [];

        _aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            _aoes.Add(new(_rect, _coffinmakers[i].Position, _coffinmakers[i].Rotation));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.CoffinmakerSmall)
        {
            _coffinmakers.Add(actor);
        }
    }
}
