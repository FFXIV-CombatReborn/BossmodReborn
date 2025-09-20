namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class GigaSlash(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(3);
    private static readonly AOEShapeCone[] _shapes = [new(60f, 112.5f.Degrees()), new(60f, 135f.Degrees()), new(60f, 105f.Degrees())];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return AOEs.Count != 0 ? new AOEInstance[1] { AOEs[0] with { Risky = Module.FindComponent<DarkNebula>()?.Casters.Count == 0 } } : [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = AOEs.Count;
        if (count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        var aoe = AOEs[0];
        // stay close to the middle if there is next imminent aoe from same origin
        if (Module.FindComponent<DarkNebula>()?.Casters.Count == 0 && count > 1 && aoe.Origin == AOEs[1].Origin)
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 3f), aoe.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShapeCone shape, float rotationOffset, float finishOffset)
        => AOEs.Add(new(shape, spell.LocXZ, spell.Rotation + rotationOffset.Degrees(), Module.CastFinishAt(spell, finishOffset)));

        switch (spell.Action.ID)
        {
            case (uint)AID.GigaSlashL:
                AddAOE(_shapes[0], 67.5f, 1f);
                AddAOE(_shapes[1], -90f, 3.1f);
                break;
            case (uint)AID.GigaSlashR:
                AddAOE(_shapes[0], -67.5f, 1f);
                AddAOE(_shapes[1], 90f, 3.1f);
                break;
            case (uint)AID.GigaSlashNightfallLRF:
                AddAOE(_shapes[0], 67.5f, 1f);
                AddAOE(_shapes[1], -90, 3.1f);
                AddAOE(_shapes[2], 0f, 5.2f);
                break;
            case (uint)AID.GigaSlashNightfallLRB:
                AddAOE(_shapes[0], 67.5f, 1f);
                AddAOE(_shapes[1], -90f, 3.1f);
                AddAOE(_shapes[2], 180f, 5.2f);
                break;
            case (uint)AID.GigaSlashNightfallRLF:
                AddAOE(_shapes[0], -67.5f, 1f);
                AddAOE(_shapes[1], 90f, 3.1f);
                AddAOE(_shapes[2], 0f, 5.2f);
                break;
            case (uint)AID.GigaSlashNightfallRLB:
                AddAOE(_shapes[0], -67.5f, 1f);
                AddAOE(_shapes[1], 90f, 3.1f);
                AddAOE(_shapes[2], 180f, 5.2f);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GigaSlashLAOE1:
            case (uint)AID.GigaSlashRAOE2:
            case (uint)AID.GigaSlashRAOE1:
            case (uint)AID.GigaSlashLAOE2:
            case (uint)AID.GigaSlashNightfallFAOE3:
            case (uint)AID.GigaSlashNightfallBAOE3:
            case (uint)AID.GigaSlashNightfallLAOE1:
            case (uint)AID.GigaSlashNightfallRAOE2:
            case (uint)AID.GigaSlashNightfallRAOE1:
            case (uint)AID.GigaSlashNightfallLAOE2:
                ++NumCasts;
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
