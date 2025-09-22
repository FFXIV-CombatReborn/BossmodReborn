namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

sealed class LitFuse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly BarbarousBarrageTower _tower = module.FindComponent<BarbarousBarrageTower>()!;
    private static readonly AOEShapeCircle circle = new(8);
    private bool fusesOfFury;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 4)
                aoes[i] = count > 4 ? aoe with { Color = Colors.Danger, Risky = _tower.Towers.Count == 0 } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FusesOfFury)
            fusesOfFury = true;
    }

    public override void Update()
    {
        var count = _aoes.Count;
        if (fusesOfFury && _tower!.Towers.Count != 0 && count == 8)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var a = ref aoes[i];
                a.Activation = a.Activation.AddSeconds(3d);
            }
            fusesOfFury = false;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        void AddAOE(DateTime activation)
        {
            _aoes.Add(new(circle, actor.Position, default, activation));
            if (_aoes.Count == 8)
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
        switch (status.ID)
        {
            case (uint)SID.LitFuseLong:
                AddAOE(WorldState.FutureTime(10.4d));
                break;
            case (uint)SID.LitFuseShort:
                AddAOE(WorldState.FutureTime(7.4d));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.SelfDestruct1:
                case (uint)AID.SelfDestruct2:
                    _aoes.RemoveAt(0);
                    fusesOfFury = false;
                    break;
            }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count != 0 && _tower!.Towers.Count != 0)
            hints.Add("Don't panic! AOEs start resolving 3.8s after towers.");
    }
}
