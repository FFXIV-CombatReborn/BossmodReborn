namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class Turrets(BossModule module) : Components.GenericKnockback(module, (uint)AID.PealOfCondemnation, 1, stopAfterWall: true)
{
    private readonly Actor?[] _turrets = new Actor?[8]; // pairs in order of activation
    private DateTime _activation;
    private BitMask _forbidden;

    private const float _distance = 17f;
    private readonly AOEShapeRect _shape = new(50f, 2.5f);

    private readonly List<Knockback> _sourcesBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var turrets = ImminentTurretsWithTargets();
        var count = turrets.Count;
        if (count == 0)
            return [];
        var sources = _sourcesBuffer;
        sources.Clear();
        for (var i = 0; i < count; ++i)
        {
            var t = turrets[i];
            if (t.source != null && t.target != null)
                sources.Add(new(t.source.Position, _distance, _activation, _shape, Angle.FromDirection(t.target.Position - t.source.Position), ignoreImmunes: true));
        }
        return CollectionsMarshal.AsSpan(sources);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var turrets = ImminentTurretsWithTargets();
        var count = turrets.Count;
        if (count == 0)
            return;

        base.AddHints(slot, actor, hints);
        var inCount = 0;

        for (var i = 0; i < count; ++i)
        {
            var t = turrets[i];
            if (t.source == null || t.target == null || !_shape.Check(actor.Position, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position)))
                continue; // not in aoe
            ++inCount;
        }

        if (inCount > 1)
            hints.Add("GTFO from one of the knockbacks!");
        else if (inCount > 0 && _forbidden[slot])
            hints.Add("GTFO from knockback!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var turrets = ImminentTurretsWithTargets();
        var count = turrets.Count;
        if (count == 0)
            return PlayerPriority.Irrelevant;
        for (var i = 0; i < count; ++i)
        {
            if (turrets[i].target == player)
            {
                return PlayerPriority.Interesting;
            }
        }
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var turretsI = ImminentTurretsWithTargets();
        var countI = turretsI.Count;
        for (var i = 0; i < countI; ++i)
        {
            var t = turretsI[i];
            Arena.Actor(t.source, Colors.Enemy, true);
            if (t.source != null && t.target != null)
                _shape.Outline(Arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position));
        }
        var turretsF = FutureTurrets();
        var countF = turretsI.Count;
        for (var i = 0; i < countF; ++i)
            Arena.Actor(turretsF[i], Colors.Object, true);

        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DarkResistanceDown)
            _forbidden[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = iconID switch
        {
            (uint)IconID.Order1 => 0,
            (uint)IconID.Order2 => 1,
            (uint)IconID.Order3 => 2,
            (uint)IconID.Order4 => 3,
            _ => -1
        };
        if (order < 0)
            return;

        _activation = WorldState.FutureTime(8.1d);
        if (_turrets[order * 2] == null)
            _turrets[order * 2] = actor;
        else if (_turrets[order * 2 + 1] == null)
            _turrets[order * 2 + 1] = actor;
        else
            ReportError($"More than 2 turrets of order {order}");
    }

    private readonly List<(Actor? source, Actor? target)> _turretsBuffer = [];
    private List<(Actor? source, Actor? target)> ImminentTurretsWithTargets()
    {
        var turrets = _turretsBuffer;
        turrets.Clear();
        var count = 0;
        var len = _turrets.Length;
        for (var i = NumCasts; i < len && count < 2; ++i)
        {
            var turret = _turrets[i];
            var target = WorldState.Actors.Find(turret?.TargetID ?? 0);
            turrets.Add((turret, target));
            ++count;
        }
        return turrets;
    }

    private readonly List<Actor?> _futureTurretsBuffer = [];
    private List<Actor?> FutureTurrets()
    {
        var turrets = _futureTurretsBuffer;
        turrets.Clear();
        var count = 0;
        var len = _turrets.Length;
        for (var i = NumCasts + 2; i < len && count < 2; ++i)
        {
            turrets.Add(_turrets[i]);
            ++count;
        }
        return turrets;
    }
}
