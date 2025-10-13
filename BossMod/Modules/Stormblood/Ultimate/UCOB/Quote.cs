namespace BossMod.Stormblood.Ultimate.UCOB;

class Quote(BossModule module) : BossComponent(module)
{
    public Actor? Source;
    public List<uint> PendingMechanics = [];
    public DateTime NextActivation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = PendingMechanics.Count;
        if (count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; ++i)
            {
                var hint = PendingMechanics[i] switch
                {
                    (uint)AID.IronChariot => "Out",
                    (uint)AID.LunarDynamo => "In",
                    (uint)AID.ThermionicBeam => "Stack",
                    (uint)AID.RavenDive or (uint)AID.MeteorStream => "Spread",
                    (uint)AID.DalamudDive => "Tankbuster",
                    _ => "???"
                };

                if (sb.Length > 0)
                    sb.Append(" > ");
                sb.Append(hint);
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        List<uint> aids = id switch
        {
            6492 => [(uint)AID.LunarDynamo, (uint)AID.IronChariot],
            6493 => [(uint)AID.LunarDynamo, (uint)AID.ThermionicBeam],
            6494 => [(uint)AID.ThermionicBeam, (uint)AID.IronChariot],
            6495 => [(uint)AID.ThermionicBeam, (uint)AID.LunarDynamo],
            6496 => [(uint)AID.RavenDive, (uint)AID.IronChariot],
            6497 => [(uint)AID.RavenDive, (uint)AID.LunarDynamo],
            6500 => [(uint)AID.MeteorStream, (uint)AID.DalamudDive],
            6501 => [(uint)AID.DalamudDive, (uint)AID.ThermionicBeam],
            6502 => [(uint)AID.RavenDive, (uint)AID.LunarDynamo, (uint)AID.MeteorStream],
            6503 => [(uint)AID.LunarDynamo, (uint)AID.RavenDive, (uint)AID.MeteorStream],
            6504 => [(uint)AID.IronChariot, (uint)AID.ThermionicBeam, (uint)AID.RavenDive],
            6505 => [(uint)AID.IronChariot, (uint)AID.RavenDive, (uint)AID.ThermionicBeam],
            6506 => [(uint)AID.LunarDynamo, (uint)AID.RavenDive, (uint)AID.ThermionicBeam],
            6507 => [(uint)AID.LunarDynamo, (uint)AID.IronChariot, (uint)AID.RavenDive],
            _ => []
        };
        if (aids.Count > 0)
        {
            Source = actor;
            PendingMechanics = aids;
            NextActivation = WorldState.FutureTime(5.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (PendingMechanics.Count != 0 && spell.Action.ID == PendingMechanics[0])
        {
            PendingMechanics.RemoveAt(0);
            NextActivation = WorldState.FutureTime(3.1d);
        }
    }
}

class QuoteIronChariotLunarDynamo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    private static readonly AOEShapeCircle _shapeChariot = new(8.55f);
    private static readonly AOEShapeDonut _shapeDynamo = new(6, 22); // TODO: verify inner radius

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        AOEShape? shape = _quote != null && _quote.PendingMechanics.Count != 0 ? _quote.PendingMechanics[0] switch
        {
            (uint)AID.IronChariot => _shapeChariot,
            (uint)AID.LunarDynamo => _shapeDynamo,
            _ => null
        } : null;
        if (shape != null && _quote?.Source != null)
            return new AOEInstance[1] { new(shape, _quote.Source.Position, default, _quote.NextActivation) };
        return [];
    }
}

class QuoteThermionicBeam(BossModule module) : Components.UniformStackSpread(module, 4f, default, 8, 8)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    public override void Update()
    {
        var stackImminent = _quote != null && _quote.PendingMechanics.Count != 0 && _quote.PendingMechanics[0] == (uint)AID.ThermionicBeam;
        if (stackImminent && Stacks.Count == 0 && Raid.Player() is var target && target != null) // note: target is random
            AddStack(target, _quote!.NextActivation);
        else if (!stackImminent && Stacks.Count > 0)
            Stacks.Clear();
        base.Update();
    }
}

class QuoteRavenDive(BossModule module) : Components.UniformStackSpread(module, default, 3f)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    public override void Update()
    {
        var spreadImminent = _quote != null && _quote.PendingMechanics.Count != 0 && _quote.PendingMechanics[0] == (uint)AID.RavenDive;
        if (spreadImminent && Spreads.Count == 0)
            AddSpreads(Raid.WithoutSlot(true, true, true), _quote!.NextActivation);
        else if (!spreadImminent && Spreads.Count > 0)
            Spreads.Clear();
        base.Update();
    }
}

class QuoteMeteorStream(BossModule module) : Components.UniformStackSpread(module, default, 4f)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    public override void Update()
    {
        var spreadImminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == (uint)AID.MeteorStream;
        if (spreadImminent && Spreads.Count == 0)
            AddSpreads(Raid.WithoutSlot(true, true, true), _quote!.NextActivation);
        else if (!spreadImminent && Spreads.Count > 0)
            Spreads.Clear();
        base.Update();
    }
}

class QuoteDalamudDive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DalamudDive, true, true)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    private static readonly AOEShapeCircle _shape = new(5f);

    public override void Update()
    {
        var imminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == (uint)AID.DalamudDive;
        if (imminent && CurrentBaits.Count == 0 && Module.Enemies((uint)OID.NaelDeusDarnus)[0] is var source && WorldState.Actors.Find(source?.TargetID ?? 0) is var target && target != null)
            CurrentBaits.Add(new(target, target, _shape));
        else if (!imminent && CurrentBaits.Count > 0)
            CurrentBaits.Clear();
    }
}
