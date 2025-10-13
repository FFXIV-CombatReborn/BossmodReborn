namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class AbsoluteAuthorityPuddles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbsoluteAuthorityPuddlesAOE, 8f);

sealed class AbsoluteAuthorityExpansionBoot(BossModule module) : Components.UniformStackSpread(module, 6f, 15f, 4) // TODO: verify falloff
{
    public int NumCasts;
    private readonly Ex3QueenEternalConfig _config = Service.Config.Get<Ex3QueenEternalConfig>();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.AuthoritysExpansion:
                if (!_config.AbsoluteAuthorityIgnoreFlares)
                    AddSpread(actor, status.ExpireAt);
                break;
            case (uint)SID.AuthoritysBoot:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AbsoluteAuthorityExpansion or (uint)AID.AbsoluteAuthorityBoot)
        {
            ++NumCasts;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

sealed class AbsoluteAuthorityHeel(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.AuthoritysHeel && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 8, 8, activation: WorldState.FutureTime(5.1d)));
    }

    public override void Update()
    {
        if (Stacks.Count != 0)
        {
            var player = Raid.Player()!;
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            Actor? closestActor = null;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p == player)
                {
                    continue;
                }

                var dx = player.Position - p.Position;
                var dist = dx.LengthSq();

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestActor = p;
                }
            }
            Stacks.Ref(0).Target = closestActor ?? player;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AbsoluteAuthorityHeel or (uint)AID.AbsoluteAuthorityHeelFail)
        {
            Stacks.Clear();
            ++NumCasts;
        }
    }
}

sealed class AbsoluteAuthorityKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AbsoluteAuthorityKnockback, 30f, kind: Kind.DirForward);
