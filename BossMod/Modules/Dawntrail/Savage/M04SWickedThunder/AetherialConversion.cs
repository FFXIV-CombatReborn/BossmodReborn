namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class AetherialConversion(BossModule module) : Components.CastCounter(module, default)
{
    public enum Mechanic { None, AOE, Knockback }

    public Mechanic CurMechanic;
    public float FirstOffsetX;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (CurMechanic != default)
            hints.Add($"{CurMechanic} {(FirstOffsetX < 0 ? "L->R" : "R->L")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (mechanic, firstOffset) = spell.Action.ID switch
        {
            (uint)AID.AetherialConversionHitLR => (Mechanic.AOE, -10f),
            (uint)AID.AetherialConversionKnockbackLR => (Mechanic.Knockback, -10f),
            (uint)AID.AetherialConversionHitRL => (Mechanic.AOE, +10f),
            (uint)AID.AetherialConversionKnockbackRL => (Mechanic.Knockback, +10f),
            _ => default
        };
        if (mechanic != default)
        {
            CurMechanic = mechanic;
            FirstOffsetX = firstOffset;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TailThrust or (uint)AID.SwitchOfTides)
            ++NumCasts;
    }
}

sealed class AetherialConversionTailThrust(BossModule module) : Components.GenericAOEs(module, (uint)AID.TailThrust)
{
    private readonly AetherialConversion? _comp = module.FindComponent<AetherialConversion>();

    private static readonly AOEShapeCircle _shape = new(18f);
    private readonly AOEInstance[] _aoe1 = new AOEInstance[1];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_comp?.CurMechanic == AetherialConversion.Mechanic.AOE && _comp.NumCasts < 2)
        {
            _aoe1[0] = new(_shape, (Arena.Center + new WDir(_comp.NumCasts == 0 ? _comp.FirstOffsetX : -_comp.FirstOffsetX, default)).Quantized(), default);
            return _aoe1;
        }
        return [];
    }
}

sealed class AetherialConversionSwitchOfTides(BossModule module) : Components.GenericKnockback(module, (uint)AID.SwitchOfTides)
{
    private readonly Knockback[] _kb = new Knockback[1];
    private readonly AetherialConversion? _comp = module.FindComponent<AetherialConversion>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_comp?.CurMechanic == AetherialConversion.Mechanic.Knockback && _comp.NumCasts < 2)
        {
            _kb[0] = new(Arena.Center + new WDir(_comp.NumCasts == 0 ? _comp.FirstOffsetX : -_comp.FirstOffsetX, 0), 25f, ignoreImmunes: true);
            return _kb;
        }
        return [];
    }
}
