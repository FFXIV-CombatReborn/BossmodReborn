namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

sealed class Spiritflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spiritflame, 6f);

sealed class Spiritflames(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 2.4f;
    private const int Length = 6;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _flames = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _flames.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var f = _flames[i];
            aoes[i] = new(capsule, f.Position, f.Rotation);
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Spiritflame)
        {
            if (id == 0x1E46)
            {
                _flames.Add(actor);
            }
            else if (id == 0x1E3C)
            {
                _flames.Remove(actor);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _flames.Count;
        if (count == 0)
            return;
        var forbiddenImminent = new ShapeDistance[count];
        var forbiddenFuture = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            var f = _flames[i];
            forbiddenFuture[i] = new SDCapsule(f.Position, f.Rotation, Length, Radius);
            forbiddenImminent[i] = new SDCircle(f.Position, Radius);
        }
        hints.AddForbiddenZone(new SDUnion(forbiddenFuture), WorldState.FutureTime(1.5d));
        hints.AddForbiddenZone(new SDUnion(forbiddenImminent));
    }
}
