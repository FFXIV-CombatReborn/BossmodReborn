namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class FulminousField(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _dir;
    private DateTime _activation;
    private WPos _pos;
    public bool Active => _activation != default;

    private static readonly AOEShapeCone _shape = new(30f, 15f.Degrees());
    private readonly AOEInstance[] _aoes = new AOEInstance[8];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active)
        {
            for (var i = 0; i < 8; ++i)
                _aoes[i] = new(_shape, _pos, _dir + i * 45f.Degrees(), _activation);
            return _aoes;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FulminousField)
        {
            _dir = spell.Rotation;
            _pos = spell.LocXZ;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Active && spell.Action.ID is (uint)AID.FulminousField or (uint)AID.FulminousFieldRest)
        {
            ++NumCasts;
            _dir = caster.Rotation + 22.5f.Degrees();
            _activation = WorldState.FutureTime(3d);
        }
    }
}

sealed class ConductionPoint : Components.UniformStackSpread
{
    public ConductionPoint(BossModule module) : base(module, default, 6f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(12d));
    }
}

sealed class ForkedFissures : Components.GenericWildCharge
{
    public ForkedFissures(BossModule module) : base(module, 5f, (uint)AID.ForkedFissures, 40f)
    {
        Array.Fill(PlayerRoles, PlayerRole.Share);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ConductionPoint:
                Source = caster;
                var slot = Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                    PlayerRoles[slot] = PlayerRole.TargetNotFirst;
                break;
            case (uint)AID.ForkedFissures:
                ++NumCasts;
                break;
        }
    }
}
