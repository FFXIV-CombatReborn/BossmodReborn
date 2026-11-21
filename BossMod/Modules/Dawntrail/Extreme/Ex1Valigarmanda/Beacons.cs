namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class Beacons(BossModule module) : BossComponent(module)
{
    public readonly List<Actor> Actors = [];
    public bool AnyActive()
    {
        var count = Actors.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = Actors[i];
            if (a.IsTargetable && !a.IsDead)
            {
                return true;
            }
        }
        return false;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.ThunderousBeacon or (uint)OID.FlameKissedBeacon or (uint)OID.GlacialBeacon)
            Actors.Add(actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Actors);
    }
}

sealed class CalamitousCry : Components.GenericWildCharge
{
    public CalamitousCry(BossModule module) : base(module, 3f, (uint)AID.CalamitousCryAOE, 80f)
    {
        Reset();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CalamitousCryTargetFirst:
            case (uint)AID.CalamitousCryTargetRest:
                Source = Module.PrimaryActor;
                var slot = Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                    PlayerRoles[slot] = PlayerRole.Target;
                break;
            case (uint)AID.CalamitousCryAOE:
                ++NumCasts;
                Reset();
                break;
        }
    }

    private void Reset()
    {
        Source = null;
        foreach (var (i, p) in Module.Raid.WithSlot(true, true, true))
            PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
    }
}

sealed class CalamitousEcho(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CalamitousEcho, new AOEShapeCone(40f, 10f.Degrees()));
