namespace BossMod.Dawntrail.Savage.M1SBlackCat;

public class QuadrupleCrossing(BossModule module) : Components.GenericBaitAway(module)
{
    public static readonly AOEShapeCone Shape = new(60, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();

        var sourceActor = SourceActor();
        if (sourceActor == null)
            return;

        var players = Raid.WithoutSlot().SortedByRange(sourceActor.Position).ToList();
        foreach (var p in players.Take(4))
            CurrentBaits.Add(new(sourceActor, p, Shape));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SlashingResistanceDown && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            ForbiddenPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SlashingResistanceDown && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            ForbiddenPlayers.Clear(slot);
        }
    }

    public virtual Actor? SourceActor()
    {
        return Module.PrimaryActor;
    }
}

public class LeapingQuadrupleCrossing(BossModule module) : QuadrupleCrossing(module)
{
    public Actor? Source;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapingTarget)
            Source = actor;
    }

    public override Actor? SourceActor()
    {
        return Source;
    }
}

public class QuadrupleCrossingRepeat(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts < 4 && _aoes.Count == 4)
        {
            for (int i = 0; i < 4; i++)
                yield return _aoes[i] with { Color = Colors.AOE, Risky = false };
        }
        else if (NumCasts < 4 && _aoes.Count == 8)
        {
            for (int i = 0; i < 4; i++)
                yield return _aoes[i] with { Color = Colors.Danger };
            for (int i = 4; i < 8; i++)
                yield return _aoes[i] with { Color = Colors.AOE, Risky = false };
        }
        else if (NumCasts < 8 && _aoes.Count == 8)
        {
            for (int i = 4; i < 8; i++)
                yield return _aoes[i] with { Color = Colors.Danger };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.QuadrupleCrossingAOERepeat or AID.LeapingQuadrupleCrossingAOERepeat)
            ++NumCasts;

        if ((AID)spell.Action.ID == AID.QuadrupleCrossingAOE)
            _aoes.Add(new(QuadrupleCrossing.Shape, caster.Position, caster.Rotation, Module.WorldState.FutureTime(6)));
        if ((AID)spell.Action.ID == AID.LeapingQuadrupleCrossingAOE)
            _aoes.Add(new(QuadrupleCrossing.Shape, caster.Position, caster.Rotation, Module.WorldState.FutureTime(6)));
    }
}
