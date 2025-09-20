namespace BossMod.Shadowbringers.Foray.Duel.Duel1Gabriel;

sealed class MagitekCannonVoidzone(BossModule module) : Components.VoidzoneAtCastTargetGroup(module, 3f, [(uint)AID.MagitekCannonFirst, (uint)AID.MagitekCannonRest], GetVoidzones, 0.9f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MagitekCannonVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class MagitekCannonChase(BossModule module) : Components.StandardChasingAOEs(module, 3f, (uint)AID.MagitekCannonFirst, (uint)AID.MagitekCannonRest, 30f, 3.2f, 5, true, (uint)IconID.MagitekCannon)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID is var id && id == ActionFirst || id == ActionRest)
        {
            Advance(spell.LocXZ, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0)
            {
                ExcludedTargets = default;
                NumCasts = 0;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Targets[slot])
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 13.5f));
        }
    }
}
