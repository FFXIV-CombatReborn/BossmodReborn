namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN4Phantom;

sealed class UndyingHatred(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.UndyingHatred, 30f, true, kind: Kind.DirForward)
{
    private readonly SwirlingMiasma _aoe = module.FindComponent<SwirlingMiasma>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var count = _aoe.Lines.Count;
        var center = Arena.Center;
        ref readonly var c = ref Casters.Ref(0);
        var act = c.Activation;
        if (count == 0)
        {
            var dir = c.Direction.ToDirection();
            hints.AddForbiddenZone(new SDRect(center + 24f * dir, center - 16f * dir, 24f), act);
        }
        else
        {
            var forbidden = new ShapeDistance[count];
            var direction = new WDir(default, 1f);
            for (var i = 0; i < count; ++i)
            {
                var line = _aoe.Lines[i];
                var dir = line.Advance * 6.66f;
                forbidden[i] = new SDInvertedRect(line.Next + dir, direction, 1f, 1f, 1f);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), act);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Color == Colors.Danger && aoe.Check(pos))
                return true;
        }
        return !Arena.InBounds(pos);
    }
}
