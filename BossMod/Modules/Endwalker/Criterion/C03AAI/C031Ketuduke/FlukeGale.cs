namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

class FlukeGale(BossModule module) : Components.GenericKnockback(module)
{
    public enum Debuff { None, BubbleWeave, FoamyFetters }
    public enum Resolve { None, Stack, Spread }

    public List<Knockback> Gales = [];
    private readonly SpringCrystalsRect? _crystals = module.FindComponent<SpringCrystalsRect>();
    private readonly Debuff[] _debuffs = new Debuff[PartyState.MaxPartySize];
    private Resolve _resolution;

    private static readonly AOEShapeRect _shape = new(20f, 10f);
    private static readonly AOEShapeRect _safeZone = new(5f, 5f, 5f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _debuffs[slot] != Debuff.FoamyFetters ? CollectionsMarshal.AsSpan(Gales) : [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_debuffs[slot] != Debuff.None)
            hints.Add($"Debuff: {(_debuffs[slot] == Debuff.BubbleWeave ? "bubble" : "bind")}", false);
        if (_resolution != Resolve.None && _debuffs[slot] != Debuff.None && Gales.Count == 4 && _crystals != null)
        {
            var finalPos = CalculateMovements(slot, actor).LastOrDefault((actor.Position, actor.Position)).Item2;
            if (!SafeZones(slot).Any(c => _safeZone.Check(finalPos, c, default)))
                hints.Add("Aim towards safe zone!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in SafeZones(pcSlot))
            _safeZone.Draw(Arena, c, default, Colors.SafeFromAOE);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var debuff = status.ID switch
        {
            (uint)SID.BubbleWeave => Debuff.BubbleWeave,
            (uint)SID.FoamyFetters => Debuff.FoamyFetters,
            _ => Debuff.None
        };
        if (debuff != Debuff.None && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _debuffs[slot] = debuff;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlukeGaleAOE1:
            case (uint)AID.FlukeGaleAOE2:
                Gales.Add(new(caster.Position, 20f, Module.CastFinishAt(spell), _shape, spell.Rotation, Kind.DirForward));
                Gales.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                break;
            case (uint)AID.Hydrofall:
                _resolution = Resolve.Stack;
                break;
            case (uint)AID.Hydrobullet:
                _resolution = Resolve.Spread;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FlukeGaleAOE1 or (uint)AID.FlukeGaleAOE2)
        {
            ++NumCasts;
            Gales.RemoveAll(s => s.Origin.AlmostEqual(caster.Position, 1f));
        }
    }

    private IEnumerable<WPos> SafeZones(int slot)
    {
        if (_resolution == Resolve.None || _debuffs[slot] == Debuff.None || Gales.Count < 4 || _crystals == null)
            yield break;
        // bind will stay, bubble will always end in '1', so bind has to end in '1' or '2' depending on stack/spread
        var wantedOrder = _debuffs[slot] == Debuff.FoamyFetters && _resolution == Resolve.Spread ? 2 : 0;
        foreach (var c in _crystals.SafeZoneCenters)
        {
            var order = Gales.FindIndex(s => s.Shape!.Check(c, s.Origin, s.Direction));
            if (order == wantedOrder || order == wantedOrder + 1)
                yield return c;
        }
    }
}
