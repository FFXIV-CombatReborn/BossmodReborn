namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class MousseDripStack(BossModule module) : Components.UniformStackSpread(module, 5f, default, 4, 4)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MousseDrip)
            AddStack(actor, WorldState.FutureTime(5.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MousseDrip)
        {
            if (++NumCasts == 8)
                Stacks.Clear();
        }
    }

    public override void Update()
    {
        var count = Stacks.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            if (Stacks[i].Target.IsDead)
            {
                Stacks.RemoveAt(i);
            }
        }
    }
}

sealed class MousseDripVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.MousseDrip, GetVoidzones, 1.4f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MousseDripVoidzone);
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

sealed class MousseDripTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001u)
            return;
        WPos? pos = index switch
        {
            0x45 => new(83f, 91f),
            0x46 => new(93f, 89f),
            0x47 => new(92f, 96f),
            0x48 => new(83f, 102f),
            0x49 => new(94f, 84f),
            0x4A => new(83f, 88f),
            0x4B => new(90f, 89f),
            0x4C => new(83f, 95f),
            0x4D => new(90f, 97.5f),
            0x4E => new(83f, 104f),
            0x4F => new(110f, 93f),
            0x50 => new(117f, 92f),
            0x51 => new(109f, 97f),
            0x52 => new(115f, 105f),
            0x53 => new(110f, 83f),
            0x54 => new(117f, 85f),
            0x55 => new(110f, 91f),
            0x56 => new(117f, 96f),
            0x57 => new(111f, 100f),
            0x58 => new(117f, 106f),
            0x59 => new(100f, 108f),
            0x5A => new(85f, 114f),
            0x5B => new(98f, 117f),
            0x5C => new(112f, 116f),
            0x5D => new(92f, 110f),
            0x5E => new(91f, 117f),
            0x5F => new(107f, 111f),
            0x60 => new(105f, 117f),
            _ => null
        };
        if (pos is WPos origin)
            Towers.Add(new(origin.Quantized(), 3f, activation: WorldState.FutureTime(19.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
            Towers.Clear();
    }
}
