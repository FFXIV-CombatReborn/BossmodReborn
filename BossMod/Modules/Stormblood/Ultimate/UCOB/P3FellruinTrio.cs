namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3AethericProfusion(UCOB module) : Components.CastCounter(module, (uint)AID.AethericProfusion)
{
    public bool Active;
    private DateTime _deadline = module.WorldState.FutureTime(14d);
    private WPos RelativeNorth;
    private readonly Actor _bahamut = module.BahamutPrime()!;
    private readonly Actor _nael = module.Nael()!;
    private readonly List<Actor> _neurolinks = module.Enemies((uint)OID.Neurolink);

    // OT -> party -> MT (using clockwise order)
    private Actor[] _neurolinksSorted = [];
    private PartyRolesConfig.Assignment[] _assignments = [];
    private readonly PartyRolesConfig _configParty = Service.Config.Get<PartyRolesConfig>();
    private readonly P3BahamutPositioning _positioning = module.FindComponent<P3BahamutPositioning>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AethericProfusion)
        {
            _deadline = Module.CastFinishAt(spell);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var myLink = _assignments.Length == 0 ? -1 : _assignments[pcSlot] switch
        {
            PartyRolesConfig.Assignment.MT => 2,
            PartyRolesConfig.Assignment.OT => 0,
            _ => 1
        };

        var len = _neurolinksSorted.Length;
        for (var i = 0; i < len; ++i)
        {
            Arena.ZoneCircleOutline(_neurolinksSorted[i].Position, 2, i == myLink ? Colors.Safe : default);
        }

        Arena.Actor(_bahamut, Colors.Enemy, true);
        Arena.Actor(_nael, Colors.Object, true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active || _neurolinksSorted.Length < 3 || _assignments.Length == 0)
        {
            return;
        }

        var myLink = _assignments[slot] switch
        {
            PartyRolesConfig.Assignment.MT => 2,
            PartyRolesConfig.Assignment.OT => 0,
            _ => 1
        };

        hints.AddForbiddenZone(new SDInvertedCircle(_neurolinksSorted[myLink].Position, 2f), _deadline);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.BahamutPrime && id == 0x1E43 && RelativeNorth == default)
        {
            RelativeNorth = actor.Position;
            _neurolinksSorted = _neurolinks.ClockOrder(actor, Arena.Center);
            _assignments = _configParty.AssignmentsPerSlot(Raid);

            _positioning.DesiredPosition = actor.Position;
            _positioning.DesiredRotation = (actor.Position - Arena.Center).ToAngle();
        }
    }
}

sealed class P3DynamoTetherHelper(UCOB module) : BossComponent(module)
{
    private readonly Quote _quote = module.FindComponent<Quote>()!;
    private readonly Actor _bahamut = module.BahamutPrime()!;
    private readonly Actor _nael = module.Nael()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment is not (PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT) && _quote is { PendingMechanics: [(uint)AID.LunarDynamo, ..], NextActivation: var nextActivation })
        {
            var bahaPos = _bahamut.Position;
            var naelPos = _nael.Position;
            hints.AddForbiddenZone(new SDCircle(bahaPos, (bahaPos - naelPos).Length()), nextActivation);
            // if squishies dodge outside the donut with tether that's game over
            hints.AddForbiddenZone(new SDInvertedCircle(naelPos, 10f), nextActivation);
        }
    }
}
