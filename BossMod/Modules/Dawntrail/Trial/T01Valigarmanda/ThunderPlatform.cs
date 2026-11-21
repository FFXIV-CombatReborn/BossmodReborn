namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class ThunderPlatform(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask requireLevitating;
    private BitMask requireHint;
    private BitMask levitating;
    private DateTime activation;

    private static readonly AOEShapeRect rect = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!requireHint[slot])
        {
            return [];
        }

        var highlightLevitate = requireLevitating[slot];
        var aoes = new AOEInstance[12];
        var index = 0;
        var center = Arena.Center;
        for (var x = 0; x < 2; ++x)
        {
            for (var z = 0; z < 3; ++z)
            {
                var cellLevitating = ((x ^ z) & 1) != 0;
                if (cellLevitating != highlightLevitate)
                {
                    aoes[index++] = new(rect, center + new WDir(-5f - 10f * x, -10f + 10f * z), default, activation);
                    aoes[index++] = new(rect, center + new WDir(+5f + 10f * x, -10f + 10f * z), default, activation);
                }
            }
        }

        return aoes.AsSpan()[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.ThunderousBreath)
        {
            var party = Module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var slot = p.Item1;
                requireHint.Set(slot);
                requireLevitating.Set(slot);
            }
            activation = Module.CastFinishAt(spell);
        }
        else if (id == (uint)AID.BlightedBoltVisual)
        {
            var party = Module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var slot = p.Item1;
                requireHint.Set(slot);
                requireLevitating.Clear(slot);
            }
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ThunderousBreath or (uint)AID.BlightedBolt2)
        {
            requireHint = default;
            requireLevitating = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (requireHint[slot])
        {
            hints.Add(requireLevitating[slot] ? "Levitate" : "Stay on ground", requireLevitating[slot] != levitating[slot]);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Levitate && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            levitating.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Levitate && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            levitating.Clear(slot);
        }
    }
}

sealed class BlightedBolt1(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ThunderPlatform _levitate = module.FindComponent<ThunderPlatform>()!;
    private static readonly AOEShapeCircle circle = new(3);
    private bool active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!active)
        {
            return [];
        }

        var party = Raid.WithSlot(false, true, true);
        var partyLen = party.Length;
        var levitateSpan = _levitate.ActiveAOEs(slot, actor);
        var levitateLen = levitateSpan.Length;
        if (partyLen == 0 || levitateLen == 0)
        {
            return [];
        }
        var index = 0;
        var aoes = new AOEInstance[partyLen];

        for (var i = 0; i < partyLen; ++i)
        {
            ref readonly var player = ref party[i];

            if (player.Item1 == slot)
            {
                continue;
            }

            var pos = player.Item2.Position;

            for (var j = 0; j < levitateLen; ++j)
            {
                if (levitateSpan[j].Check(pos))
                {
                    aoes[index++] = new AOEInstance(circle, pos);
                    break;
                }
            }
        }

        return aoes.AsSpan()[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlightedBoltVisual)
        {
            active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlightedBoltVisual)
        {
            active = false;
        }
    }
}

sealed class BlightedBolt2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlightedBolt2, 7f);
