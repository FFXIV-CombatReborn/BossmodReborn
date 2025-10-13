namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

class Gaiaochos(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GaiaochosTransition, new AOEShapeDonut(7f, 30f));

// TODO: we could show it earlier, casters do PATE 11D2 ~4s before starting cast
class UltimaRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UltimaRay, new AOEShapeRect(20f, 3f));

class MissingLink(BossModule module) : Components.Chains(module, (uint)TetherID.MissingLink, (uint)AID.MissingLink);

class DemiParhelion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DemiParhelionAOE, new AOEShapeCircle(2f));

class Geocentrism(BossModule module) : Components.GenericAOEs(module)
{
    public int NumConcurrentAOEs;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeLine = new(20f, 2f);
    private static readonly AOEShapeCircle _shapeCircle = new(2f);
    private static readonly AOEShapeDonut _shapeDonut = new(3f, 7f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GeocentrismV:
                var angle = Angle.AnglesCardinals[1];
                AddAOE(_shapeLine, new(95f, 83f), angle);
                AddAOE(_shapeLine, new(100f, 83f), angle);
                AddAOE(_shapeLine, new(105f, 83f), angle);
                NumConcurrentAOEs = 3;
                break;
            case (uint)AID.GeocentrismC:
                WPos pos = new(100f, 90f);
                AddAOE(_shapeCircle, pos, default);
                AddAOE(_shapeDonut, pos, default);
                NumConcurrentAOEs = 2;
                break;
            case (uint)AID.GeocentrismH:
                var angle2 = Angle.AnglesCardinals[3];
                AddAOE(_shapeLine, new(93f, 85f), angle2);
                AddAOE(_shapeLine, new(93f, 90f), angle2);
                AddAOE(_shapeLine, new(93f, 95f), angle2);
                NumConcurrentAOEs = 3;
                break;
        }
        void AddAOE(AOEShape shape, WPos origin, Angle angle) => _aoes.Add(new(shape, origin.Quantized(), angle, Module.CastFinishAt(spell, 0.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DemiParhelionGeoLine or (uint)AID.DemiParhelionGeoDonut or (uint)AID.DemiParhelionGeoCircle)
            ++NumCasts;
    }
}

class DivineExcoriation(BossModule module) : Components.UniformStackSpread(module, default, 1f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DivineExcoriation)
            AddSpread(actor, WorldState.FutureTime(3.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DivineExcoriation)
            Spreads.Clear();
    }
}

class GaiaochosEnd(BossModule module) : BossComponent(module)
{
    public bool Finished;

    public override void OnMapEffect(byte index, uint state)
    {
        // note: there are 3 env controls happening at the same time, not sure which is the actual trigger: .9=02000001, .11=00800001, .12=00080004
        if (index == 9 && state == 0x02000001)
            Finished = true;
    }
}

// TODO: assign pairs, draw wrong pairs as aoes
class UltimaBlow(BossModule module) : Components.CastCounter(module, (uint)AID.UltimaBlow)
{
    private readonly List<(Actor source, Actor target)> _tethers = [];
    private BitMask _vulnerable;

    private static readonly AOEShapeRect _shape = new(20f, 3f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_vulnerable[slot])
        {
            var source = _tethers.Find(t => t.target == actor).source;
            var numHit = source != null ? Raid.WithoutSlot(false, true, true).Exclude(actor).InShape(_shape, source.Position, Angle.FromDirection(actor.Position - source.Position)).Count : 0;
            if (numHit == 0)
                hints.Add("Hide behind partner!");
            else if (numHit > 1)
                hints.Add("Bait away from raid!");
        }
        else if (_tethers.Count > 0)
        {
            var numHit = _tethers.Count(t => _shape.Check(actor.Position, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position)));
            if (numHit == 0)
                hints.Add("Intercept the charge!");
            else if (numHit > 1)
                hints.Add("GTFO from other charges!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _tethers.Any(t => t.target == player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_vulnerable[pcSlot]) // TODO: reconsider
            foreach (var t in _tethers.Where(t => t.target != pc))
                _shape.Draw(Arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
        {
            Arena.Actor(t.source, Colors.Object, true);
            Arena.AddLine(t.source.Position, t.target.Position, 0);
            if (t.target == pc || !_vulnerable[pcSlot])
                _shape.Outline(Arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position), t.target == pc ? Colors.Safe : 0); // TODO: reconsider...
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ClassicalConceptsShapes && WorldState.Actors.Find(tether.Target) is var target && target != null)
        {
            _tethers.Add((source, target));
            _vulnerable.Set(Raid.FindSlot(tether.Target));
        }
    }
}
