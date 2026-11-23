namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

sealed class ManifoldFlames : Components.UniformStackSpread
{
    public ManifoldFlames(BossModule module) : base(module, 0f, 6f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }
}

abstract class NestOfFlamevipersCommon(BossModule module) : Components.CastCounter(module, (uint)AID.NestOfFlamevipersAOE)
{
    protected BitMask BaitingPlayers;

    private readonly AOEShapeRect _shape = new(60f, 2.5f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (s, p) in Raid.WithSlot(false, true, true))
        {
            if (BaitingPlayers[s] && p != actor && _shape.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(p.Position - Module.PrimaryActor.Position)))
            {
                hints.Add("GTFO from baited aoe!");
                break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (s_, player) in Raid.WithSlot(false, true, true))
            if (BaitingPlayers[s_])
                _shape.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(player.Position - Module.PrimaryActor.Position));
    }
}

// variant that happens right after manifold flames and baits to 4 closest players
sealed class NestOfFlamevipersBaited(BossModule module) : NestOfFlamevipersCommon(module)
{
    private BitMask _forbiddenPlayers;
    public bool Active => NumCasts == 0 && _forbiddenPlayers.Any();

    private readonly List<(int, Actor)> _sorted = new(PartyState.MaxPartySize);

    public override void Update()
    {
        BaitingPlayers = default;
        if (Active)
        {
            Raid.WithSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position, _sorted);
            for (var i = 0; i < _sorted.Count && i < 4; ++i)
                BaitingPlayers.Set(_sorted[i].Item1);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Active)
        {
            var shouldBait = !_forbiddenPlayers[slot];
            hints.Add(shouldBait ? "Move closer to bait" : "GTFO to avoid baits", shouldBait != BaitingPlayers[slot]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.HemitheosFlare)
            _forbiddenPlayers.Set(Raid.FindSlot(spell.MainTargetID));
    }
}

// variant that happens when cast is started and baits to everyone
sealed class NestOfFlamevipersEveryone(BossModule module) : NestOfFlamevipersCommon(module)
{
    public bool Active => NumCasts == 0 && BaitingPlayers.Any();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NestOfFlamevipers)
            BaitingPlayers = Raid.WithSlot(false, true, true).Mask();
    }
}
