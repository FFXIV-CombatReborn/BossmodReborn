namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class BladeOfEntropy(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a90 = 90f.Degrees();
    private static readonly AOEShapeCone cone = new(40f, a90), coneInv = new(40f, a90, invertForbiddenZone: true);
    private readonly AOEInstance[][] _aoes = new AOEInstance[PartyState.MaxAllianceSize][];
    private readonly PlayerTemperatures _temps = module.FindComponent<PlayerTemperatures>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => slot is < 0 or > 23 ? [] : _aoes[slot];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var temp = spell.Action.ID switch
        {
            (uint)AID.BladeOfEntropyCold11 or (uint)AID.BladeOfEntropyCold12 => 1u,
            (uint)AID.BladeOfEntropyHot11 or (uint)AID.BladeOfEntropyHot12 => 3u,
            (uint)AID.BladeOfEntropyCold21 or (uint)AID.BladeOfEntropyCold22 => 2u,
            (uint)AID.BladeOfEntropyHot21 or (uint)AID.BladeOfEntropyHot22 => 4u,
            _ => default
        };
        if (temp != default)
        {
            var temps = _temps.Temperatures;
            for (var i = 0; i < 24; ++i)
            {
                var playertemp = temps[i];
                uint color = default;
                var shape = cone;
                if (playertemp != default && playertemp == temp)
                {
                    color = Colors.SafeFromAOE;
                    shape = coneInv;
                }
                _aoes[i] = [new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), color)];
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BladeOfEntropyCold12:
            case (uint)AID.BladeOfEntropyCold22:
            case (uint)AID.BladeOfEntropyHot22:
            case (uint)AID.BladeOfEntropyHot12:
            case (uint)AID.BladeOfEntropyCold11:
            case (uint)AID.BladeOfEntropyCold21:
            case (uint)AID.BladeOfEntropyHot11:
            case (uint)AID.BladeOfEntropyHot21:
                Array.Clear(_aoes);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        ref readonly var aoe = ref aoes[0];
        var isInside = aoe.Check(actor.Position);
        if (aoe.Color != Colors.SafeFromAOE)
        {
            if (isInside)
                hints.Add(WarningText);
        }
        else
        {
            hints.Add("Get hit by AOE!", !isInside);
        }
    }
}
