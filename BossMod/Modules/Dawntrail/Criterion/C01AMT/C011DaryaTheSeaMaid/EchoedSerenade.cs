namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

static class Familiars
{
    public const uint VfxHorse = 2741u;
    public const uint VfxTurtle = 2743u;
    public const uint VfxCrab = 2744u;
    public const uint VfxBomb = 2745u;
    public const uint VfxBird = 2746u;

    public static readonly AOEShapeRect Rect = new(40f, 4f);
    public static readonly AOEShapeCone Bomb = new(20f, 90f.Degrees());
    public static readonly AOEShapeCone Bird = new(45f, 30f.Degrees());

    public static bool TryGet(uint vfxID, out uint oid, out AOEShape shape, out string name)
    {
        switch (vfxID)
        {
            case VfxHorse:
                (oid, shape, name) = ((uint)OID.SeabornSteed, Rect, "horse");
                return true;
            case VfxTurtle:
                (oid, shape, name) = ((uint)OID.SeabornSteward, Rect, "turtle");
                return true;
            case VfxCrab:
                (oid, shape, name) = ((uint)OID.SeabornSoldier, Rect, "crab");
                return true;
            case VfxBomb:
                (oid, shape, name) = ((uint)OID.SeabornServant, Bomb, "bomb");
                return true;
            case VfxBird:
                (oid, shape, name) = ((uint)OID.SeabornShrike, Bird, "bird");
                return true;
            default:
                (oid, shape, name) = (default, Rect, "");
                return false;
        }
    }

    public static bool IsWatersong(uint aid) => aid is (uint)AID.Watersong or (uint)AID.Watersong1 or (uint)AID.Watersong2 or (uint)AID.Watersong3 or (uint)AID.Watersong4;
}

class FamiliarCall(BossModule module, int numShown = 2, bool highlightImminent = true) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<int> _groupSizes = [];
    private readonly List<string> _names = [];
    private readonly int _numShown = numShown;
    private readonly bool _highlightImminent = highlightImminent;
    private DateTime _lastActivation;
    private string _order = "";

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var groups = Math.Min(_groupSizes.Count, _numShown);
        var count = 0;
        for (var i = 0; i < groups; ++i)
        {
            count += _groupSizes[i];
        }
        return count == 0 ? [] : CollectionsMarshal.AsSpan(_aoes)[..count];
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_order.Length != 0)
        {
            hints.Add(_order);
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if (actor.OID != (uint)OID.DaryaTheSeaMaid || !Familiars.TryGet(vfxID, out var oid, out var shape, out var name))
        {
            return;
        }
        var enemies = Module.Enemies(oid);
        var count = enemies.Count;
        if (count == 0)
        {
            return;
        }
        _lastActivation = _groupSizes.Count != 0 ? _lastActivation.AddSeconds(3d) : WorldState.FutureTime(11.6d);
        for (var i = 0; i < count; ++i)
        {
            var e = enemies[i];
            _aoes.Add(new(shape, e.Position, e.Rotation, _lastActivation, risky: false, actorID: e.InstanceID));
        }
        _groupSizes.Add(count);
        _names.Add(name);
        Refresh();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_groupSizes.Count == 0 || !Familiars.IsWatersong(spell.Action.ID))
        {
            return;
        }
        var first = _groupSizes[0];
        var id = caster.InstanceID;
        for (var i = 0; i < first; ++i)
        {
            if (_aoes[i].ActorID == id)
            {
                _aoes.RemoveAt(i);
                --first;
                break;
            }
        }
        if (first == 0)
        {
            ++NumCasts;
            _groupSizes.RemoveAt(0);
            _names.RemoveAt(0);
            Refresh();
        }
        else
        {
            _groupSizes[0] = first;
        }
    }

    private void Refresh()
    {
        _order = _names.Count != 0 ? "Order: " + string.Join(", ", _names) : "";
        if (_groupSizes.Count == 0)
        {
            return;
        }
        var first = _groupSizes[0];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = _highlightImminent ? Colors.Danger : default;
        for (var i = 0; i < first; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Risky = true;
            aoe.Color = color;
        }
    }
}

sealed class FamiliarCall2(BossModule module) : FamiliarCall(module, 1, false);

sealed class EchoedReprise(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(uint OID, DateTime Activation)> _order = [];
    private readonly List<AOEInstance> _aoes = [];
    private bool _active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active || _order.Count == 0)
        {
            return [];
        }
        _aoes.Clear();
        var (oid, activation) = _order[0];
        if (oid != default)
        {
            var enemies = Module.Enemies(oid);
            var count = enemies.Count;
            for (var i = 0; i < count; ++i)
            {
                var e = enemies[i];
                _aoes.Add(new(Familiars.Rect, e.Position, e.Rotation, activation));
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public void Activate()
    {
        _active = true;
        var count = _order.Count;
        for (var i = 0; i < count; ++i)
        {
            _order.Ref(i).Activation = WorldState.FutureTime(7.6d + 3.2d * i);
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if (!_active && actor.OID == (uint)OID.DaryaTheSeaMaid && Familiars.TryGet(vfxID, out var oid, out var shape, out _))
        {
            _order.Add((shape == Familiars.Rect ? oid : default, default));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_active && _order.Count != 0 && spell.Action.ID is (uint)AID.Watersong1 or (uint)AID.Watersong2 or (uint)AID.Watersong4
            && Math.Abs((WorldState.CurrentTime - _order[0].Activation).TotalSeconds) < 1d)
        {
            ++NumCasts;
            _order.RemoveAt(0);
        }
    }
}
