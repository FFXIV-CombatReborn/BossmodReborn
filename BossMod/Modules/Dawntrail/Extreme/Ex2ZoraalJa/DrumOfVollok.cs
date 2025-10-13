namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class DrumOfVollokPlatforms(BossModule module) : BossComponent(module)
{
    public bool Active;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x0B)
        {
            return;
        }

        switch (state)
        {
            case 0x00800040u:
                Arena.Bounds = Ex2ZoraalJa.NWPlatformBounds;
                Arena.Center += 15f * 135f.Degrees().ToDirection();
                Active = true;
                break;
            case 0x02000100u:
                Arena.Bounds = Ex2ZoraalJa.NEPlatformBounds;
                Arena.Center += 15f * (-135f).Degrees().ToDirection();
                Active = true;
                break;
        }
    }
}

sealed class DrumOfVollok(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DrumOfVollokAOE, 4f, 2, 2);

sealed class DrumOfVollokKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly DrumOfVollok? _main = module.FindComponent<DrumOfVollok>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_main == null)
            return [];
        var count = _main.Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_main.Stacks[i].Target == actor)
                return [];
        }
        var sources = new List<Knockback>();
        for (var i = 0; i < count; ++i)
        {
            var s = _main.Stacks[i];
            if (actor.Position.InCircle(s.Target.Position, s.Radius))
                sources.Add(new(s.Target.Position, 25f, s.Activation, ignoreImmunes: true));
        }
        return CollectionsMarshal.AsSpan(sources);
    }
}
