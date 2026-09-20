namespace BossMod.Endwalker.Raid.P1NErichthonios;

sealed class GaolersFlail(BossModule module) : Components.SimpleAOEGroups(
    module,
    [(uint)AID.GaolersFlailLeftAOE, (uint)AID.GaolersFlailRightAOE],
    new AOEShapeCone(60f, 135f.Degrees()));

sealed class HeavyHand(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HeavyHand);

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(
    module,
    [(uint)AID.WardersWrath, (uint)AID.ShiningCells, (uint)AID.SlamShut]);

sealed class TrueHoly(BossModule module) : Components.StackWithIcon(
    module,
    (uint)IconID.TrueHoly,
    (uint)AID.TrueHoly,
    6f,
    5.1d,
    4,
    8);

sealed class PitilessFlail(BossModule module) : Components.GenericKnockback(module, (uint)AID.PitilessFlail)
{
    private readonly List<Knockback> _knockbacks = [];
    private ulong _targetID;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => actor.InstanceID == _targetID ? CollectionsMarshal.AsSpan(_knockbacks) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _targetID = spell.TargetID;
            _knockbacks.Clear();
            _knockbacks.Add(new(caster.Position, 15f, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _targetID = 0;
            _knockbacks.Clear();
        }
    }
}

sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ShiningCells:
                Arena.Bounds = new ArenaBoundsCircle(20f);
                break;
            case AID.SlamShut:
                Arena.Bounds = new ArenaBoundsSquare(20f);
                break;
        }
    }
}

sealed class AetherExplosion(BossModule module) : Components.GenericAOEs(module)
{
    private enum CellColor { None, Red, Blue }

    private static readonly AOEShapeCone InnerShape = new(P1N.InnerCircleRadius, 22.5f.Degrees());
    private static readonly AOEShapeDonutSector OuterShape = new(P1N.InnerCircleRadius, 20f, 22.5f.Degrees());
    private readonly AOEInstance[] _aoes = new AOEInstance[8];
    private CellColor _color;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_color == CellColor.None)
            return [];

        var start = _color == CellColor.Blue ? 0f : 45f;
        for (var i = 0; i < 4; ++i)
        {
            _aoes[2 * i] = new(InnerShape, Arena.Center, (start + 22.5f + 90f * i).Degrees());
            _aoes[2 * i + 1] = new(OuterShape, Arena.Center, (start + 67.5f + 90f * i).Degrees());
        }
        return _aoes;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.AetherExplosion)
        {
            _color = status.Extra switch
            {
                0x14C => CellColor.Red,
                0x14D => CellColor.Blue,
                _ => CellColor.None
            };
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (actor == Module.PrimaryActor && status.ID == (uint)SID.AetherExplosion)
            _color = CellColor.None;
    }
}

sealed class Intemperance(BossModule module) : Components.GenericAOEs(module, warningText: "Wrong-color cube!")
{
    private static readonly AOEShapeRect Shape = new(19f, 9.5f);
    private readonly List<AOEInstance> _hot = [];
    private readonly List<AOEInstance> _cold = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (actor.FindStatus((uint)SID.HotSpell) != null)
            return CollectionsMarshal.AsSpan(_hot);
        if (actor.FindStatus((uint)SID.ColdSpell) != null)
            return CollectionsMarshal.AsSpan(_cold);
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var list = spell.Action.ID switch
        {
            (uint)AID.HotSpellPreview or (uint)AID.HotSpell => _hot,
            (uint)AID.ColdSpellPreview or (uint)AID.ColdSpell => _cold,
            _ => null
        };
        list?.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var list = spell.Action.ID switch
        {
            (uint)AID.HotSpellPreview or (uint)AID.HotSpell => _hot,
            (uint)AID.ColdSpellPreview or (uint)AID.ColdSpell => _cold,
            _ => null
        };
        if (list != null)
            list.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
    }
}
