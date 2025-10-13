namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D055ForgivenObscenity;

public enum OID : uint
{
    Boss = 0x27CE, //R=5.0
    BossClones = 0x27CF, //R=5.0
    Orbs = 0x27D0, //R=1.0
    Rings = 0x1EAB62,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    VauthrysBlessing = 15639, // Boss->self, no cast, single-target
    OrisonFortissimo = 15636, // Boss->self, 4.0s cast, single-target
    OrisonFortissimo2 = 15637, // Helper->self, no cast, range 50 circle

    DivineDiminuendoCircle1 = 15638, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendoCircle2 = 15640, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendoCircle3 = 15649, // BossClones->self, 4.0s cast, range 8 circle
    DivineDiminuendoDonut1 = 15641, // Helper->self, 4.0s cast, range 10-16 donut
    DivineDiminuendoDonut2 = 18025, // Helper->self, 4.0s cast, range 18-32 donut

    ConvictionMarcato1 = 15642, // Boss->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato2 = 15643, // Helper->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato3 = 15648, // BossClones->self, 4.0s cast, range 40 width 5 rect
    unknown = 16846, // Helper->self, 4.0s cast, single-target
    PenancePianissimo = 15644, // Boss->self, 3.0s cast, single-target, inverted circle voidzone appears
    FeatherMarionette = 15645, // Boss->self, 3.0s cast, single-target
    SolitaireRing = 17066, // Boss->self, 3.5s cast, single-target
    Ringsmith = 15652, // Boss->self, no cast, single-target
    GoldChaser = 15653, // Boss->self, 4.0s cast, single-target
    VenaAmoris = 15655, // Orbs->self, no cast, range 40 width 5 rect
    SacramentSforzando = 15634, // Boss->self, 4.0s cast, single-target
    SacramentSforzando2 = 15635, // Helper->player, no cast, single-target
    SanctifiedStaccato = 15654 // Helper->self, no cast, range 3 circle, sort of a voidzone around the light orbs, only triggers if you get too close
}

sealed class Orbs(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly List<Actor> _orbs = new(6);
    private const float Radius = 3f;
    private static readonly AOEShapeCircle circle = new(Radius);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _orbs.Count;
        if (count == 0)
            return [];
        var rings = Module.Enemies((uint)OID.Rings);
        var countR = rings.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var o = _orbs[i];
            var found = false;
            for (var j = 0; j < countR; ++j)
            {
                var ring = rings[j];
                if (ring.Position.InRect(o.Position, 20f * o.Rotation.ToDirection(), Radius))
                {
                    aoes[i] = new(new AOEShapeCapsule(Radius, (ring.Position - o.Position).Length()), o.Position, o.Rotation);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                aoes[i] = new(circle, o.Position);
            }
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Orbs)
        {
            _orbs.Add(actor);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u)
        {
            var count = _orbs.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_orbs[i].Position.AlmostEqual(actor.Position, 4f))
                {
                    _orbs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class GoldChaser(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private readonly List<Actor> _casters = new(6);
    private readonly AOEShapeRect rect = new(40f, 2.5f);
    private readonly WPos[] positionsSet1 = [new(-227.5f, 253f), new(-232.5f, 251.5f)];
    private readonly WPos[] positionsSet2 = [new(-252.5f, 253f), new(-247.5f, 251.5f)];
    private readonly List<AOEInstance> _aoes = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Rings)
        {
            _casters.Add(actor);
            var count = _casters.Count;
            if (count < 2)
            {
                return;
            }
            var inSet1Or2 = AreCastersInPositions(positionsSet1) || AreCastersInPositions(positionsSet2);

            switch (count)
            {
                case 2:
                    AddAOE(_casters[0], 7.1d);
                    AddAOE(_casters[1], inSet1Or2 ? 7.6d : 7.1d);
                    break;
                case 4:
                    AddAOE(_casters[2], 8.1d);
                    AddAOE(_casters[3], inSet1Or2 ? 8.6d : 8.1d);
                    break;
                case 6:
                    var delay = inSet1Or2 ? 9.1d : 11.1d;
                    AddAOE(_casters[4], delay);
                    AddAOE(_casters[5], delay);
                    break;
            }
        }
        void AddAOE(Actor caster, double delay)
        {
            var pos = caster.Position.Quantized();
            var rot = Angle.AnglesCardinals[2];
            _aoes.Add(new(rect, pos, rot, _activation.AddSeconds(delay), risky: false, shapeDistance: rect.Distance(pos, rot)));
            UpdateAOEs();
        }
        bool AreCastersInPositions(WPos[] positions)
        {
            var caster0 = _casters[0].Position;
            var caster1 = _casters[1].Position;
            return caster0 == positions[0] && caster1 == positions[1] || caster0 == positions[1] && caster1 == positions[0];
        }
    }

    private void UpdateAOEs()
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            var max = count > 4 ? 4 : count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var act0 = aoes[0].Activation;
            var color = Colors.Danger;
            for (var i = 0; i < max; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Activation == act0)
                {
                    aoe.Risky = true;
                    if (count > 2)
                    {
                        aoe.Color = color;
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Ringsmith:
                _activation = WorldState.CurrentTime;
                break;
            case (uint)AID.VenaAmoris:
                if (_aoes.Count != 0)
                {
                    _aoes.RemoveAt(0);
                    UpdateAOEs();
                }
                if (++NumCasts == 6)
                {
                    _casters.Clear();
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class SacramentSforzando(BossModule module) : Components.SingleTargetCastDelay(module, (uint)AID.SacramentSforzando, (uint)AID.SacramentSforzando2, 0.8d);
sealed class OrisonFortissimo(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.OrisonFortissimo, (uint)AID.OrisonFortissimo2, 0.8d);

sealed class DivineDiminuendoCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DivineDiminuendoCircle1, (uint)AID.DivineDiminuendoCircle2,
(uint)AID.DivineDiminuendoCircle3], 8f);

sealed class DivineDiminuendoDonut1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineDiminuendoDonut1, new AOEShapeDonut(10f, 16f));
sealed class DivineDiminuendoDonut2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineDiminuendoDonut2, new AOEShapeDonut(18f, 32f));

sealed class ConvictionMarcato(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ConvictionMarcato1, (uint)AID.ConvictionMarcato2,
(uint)AID.ConvictionMarcato3], new AOEShapeRect(40f, 2.5f));

sealed class PenancePianissimo(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeDonut donut = new(14.5f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PenancePianissimo)
        {
            var center = Arena.Center;
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, 0.7d), shapeDistance: donut.Distance(center, default))];
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        switch (state)
        {
            case 0x00040008u:
                Arena.Bounds = new ArenaBoundsRect(14.5f, 19.5f);
                break;
            case 0x00010002u:
                _aoe = [];
                var center = Arena.Center;
                Arena.Bounds = new ArenaBoundsCustom([new Polygon(center, 15f, 64)], [new Rectangle(center + new WDir(15f, default), 0.5f, 20f),
                new Rectangle(center + new WDir(-15f, default), 0.5f, 20f)]);
                break;

        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Arena.Bounds.Radius != 19.5f)
        {
            hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, actor, ActionQueue.Priority.High);
        }
    }
}

sealed class D055ForgivenObscenityStates : StateMachineBuilder
{
    public D055ForgivenObscenityStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SacramentSforzando>()
            .ActivateOnEnter<DivineDiminuendoCircle>()
            .ActivateOnEnter<DivineDiminuendoDonut1>()
            .ActivateOnEnter<DivineDiminuendoDonut2>()
            .ActivateOnEnter<ConvictionMarcato>()
            .ActivateOnEnter<OrisonFortissimo>()
            .ActivateOnEnter<GoldChaser>()
            .ActivateOnEnter<Orbs>()
            .ActivateOnEnter<PenancePianissimo>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8262)]
public sealed class D055ForgivenObscenity(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240f, 237f), new ArenaBoundsRect(14.5f, 19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
    }
}
