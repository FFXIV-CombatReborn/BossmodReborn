namespace BossMod.Shadowbringers.Dungeon.D01Holmintser.D013Philia;

public enum OID : uint
{
    Boss = 0x278C, // R9.8
    IronChain = 0x2895, // R1.0
    SludgeVoidzone = 0x1EABFA,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ScavengersDaughter = 15832, // Boss->self, 4.0s cast, range 40 circle
    HeadCrusher = 15831, // Boss->player, 4.0s cast, single-target
    Pendulum = 16777, // Boss->self, 5.0s cast, single-target, cast to jump
    PendulumAOE1 = 16790, // Boss->location, no cast, range 40 circle, jump to target
    PendulumAOE2 = 15833, // Boss->location, no cast, range 40 circle, jump back to center
    PendulumAOE3 = 16778, // Helper->location, 4.5s cast, range 40 circle, damage fall off AOE visual
    ChainDown = 17052, // Boss->self, 5.0s cast, single-target 
    AethersupFirst = 15848, // Boss->self, 15.0s cast, range 21 120-degree cone
    AethersupRest = 15849, // Helper->self, no cast, range 24+R 120-degree cone
    RightKnout = 15846, // Boss->self, 5.0s cast, range 24 210-degree cone
    LeftKnout = 15847, // Boss->self, 5.0s cast, range 24 210-degree cone
    TaphephobiaVisual = 15842, // Boss->self, 4.5s cast, single-target
    Taphephobia = 16769, // Helper->player, 5.0s cast, range 6 circle
    IntoTheLightMarker = 15844, // Helper->player, no cast, single-target, line stack
    IntoTheLightVisual = 17232, // Boss->self, 5.0s cast, single-target
    IntoTheLight = 15845, // Boss->self, no cast, range 50 width 8 rect
    FierceBeatingRotationVisual = 15834, // Boss->self, 5.0s cast, single-target
    FierceBeatingVisual1 = 15836, // Boss->self, no cast, single-target
    FierceBeatingVisual2 = 15835, // Boss->self, no cast, single-target
    FierceBeatingExaFirst = 15837, // Helper->self, 5.0s cast, range 4 circle
    FierceBeatingExaRestFirst = 15838, // Helper->self, no cast, range 4 circle
    FierceBeatingExaRestRest = 15839, // Helper->location, no cast, range 4 circle
    CatONineTailsVisual = 15840, // Boss->self, no cast, single-target
    CatONineTails = 15841 // Helper->self, 2.0s cast, range 25 120-degree cone
}

public enum IconID : uint
{
    SpreadFlare = 87, // player
    ChainTarget = 92 // player
}

public enum SID : uint
{
    Fetters = 1849 // none->player, extra=0xEC4
}

sealed class SludgeVoidzone(BossModule module) : Components.Voidzone(module, 9f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SludgeVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class ScavengersDaughter(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScavengersDaughter);
sealed class HeadCrusher(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HeadCrusher);

sealed class Fetters(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus((uint)SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && !chainsactive)
            hints.Add($"{chaintarget.Name} is about to be fettered!");
        else if (chaintarget != null && chainsactive)
            hints.Add($"Destroy fetters on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chained && actor != chaintarget)
            for (var i = 0; i < hints.PotentialTargets.Count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.IronChain => 1,
                    (uint)OID.Boss => AIHints.Enemy.PriorityInvincible,
                    _ => 0
                };
            }
        var chain = Module.Enemies((uint)OID.IronChain);
        var ironchain = chain.Count != 0 ? chain[0] : null;
        if (ironchain != null && !ironchain.IsDead)
            hints.AddForbiddenZone(new SDInvertedCircle(ironchain.Position, 3.6f));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChainTarget)
        {
            chaintarget = actor;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainDown)
        {
            casting = false;
        }
    }
}

sealed class Aethersup(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone cone = new(24f, 60f.Degrees());
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length != 0)
        {
            var chain = Module.Enemies((uint)OID.IronChain);
            var count = chain.Count;
            ref var aoe = ref _aoe[0];
            aoe.Risky = count == 0 || count != 0 && chain[0].IsDead;
        }
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AethersupFirst)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            _aoe = [new(cone, origin, rotation, Module.CastFinishAt(spell), shapeDistance: cone.Distance(origin, rotation))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AethersupFirst:
            case (uint)AID.AethersupRest:
                if (++NumCasts == 4)
                {
                    _aoe = [];
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class PendulumFlare(BossModule module) : Components.BaitAwayIcon(module, 20f, (uint)IconID.SpreadFlare, (uint)AID.PendulumAOE1, 5.1d)
{
    private readonly SDCircle shapedistance = new(D013Philia.ArenaCenter, 18.5f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.AddForbiddenZone(shapedistance);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.Add("Bait away!");
        }
    }
}

sealed class PendulumAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PendulumAOE3, 15f);

sealed class Knout(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftKnout, (uint)AID.RightKnout], new AOEShapeCone(24f, 105f.Degrees()));

sealed class Taphephobia(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Taphephobia, 6f);

sealed class IntoTheLight(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.IntoTheLightMarker, (uint)AID.IntoTheLight, 5.3d);

sealed class CatONineTails(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly AOEShapeCone _shape = new(25f, 60f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingRotationVisual)
        {
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation + 180f.Degrees(), -45f.Degrees(), Module.CastFinishAt(spell), 2d, 8));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CatONineTails)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}

sealed class FierceBeating(BossModule module) : Components.Exaflare(module, 4f)
{
    private readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _predictions = new(2);

    public override void Update()
    {
        var linesCount = Lines.Count;
        if (lastCount != linesCount || currentVersion != lastVersion)
        {
            var futureAOEs = CollectionsMarshal.AsSpan(FutureAOEs(linesCount));
            var imminentAOEs = ImminentAOEs(linesCount);
            var futureLen = futureAOEs.Length;
            var imminentLen = imminentAOEs.Length;
            var predictionsCount = _predictions.Count;
            var total = futureLen + imminentLen + predictionsCount;

            _aoes = new AOEInstance[total];
            for (var i = 0; i < futureLen; ++i)
            {
                ref var aoe = ref futureAOEs[i];
                var origin = aoe.Item1;
                var rotation = aoe.Item3;
                _aoes[i] = new(Shape, origin, rotation, aoe.Item2, FutureColor, shapeDistance: Shape.Distance(origin, rotation));
            }

            for (var i = 0; i < imminentLen; ++i)
            {
                ref var aoe = ref imminentAOEs[i];
                var origin = aoe.Item1;
                var rotation = aoe.Item3;
                _aoes[futureLen + i] = new(Shape, origin, rotation, aoe.Item2, ImminentColor, shapeDistance: Shape.Distance(origin, rotation));
            }
            var predictions = CollectionsMarshal.AsSpan(_predictions);
            for (var i = 0; i < predictionsCount; ++i)
            {
                ref var aoe = ref predictions[i];
                _aoes[imminentLen + futureLen + i] = aoe;
            }
            lastCount = linesCount;
            lastVersion = currentVersion;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingExaFirst)
        {
            AddLine(ref caster, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.FierceBeatingExaRestFirst)
        {
            AddLine(ref caster, WorldState.FutureTime(1d));
        }
        if (Lines.Count != 0)
        {
            if (id is (uint)AID.FierceBeatingExaFirst or (uint)AID.FierceBeatingExaRestFirst)
            {
                Advance(caster.Position);
            }
            else if (id == (uint)AID.FierceBeatingExaRestRest)
            {
                Advance(spell.TargetXZ);
            }
        }

        void Advance(WPos pos)
        {
            var count = Lines.Count;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public void AddLine(ref Actor caster, DateTime activation)
    {
        var adv = 2.5f * caster.Rotation.ToDirection();
        Lines.Add(new(caster.Position, adv, activation, 1d, 7, 3));
        ++NumCasts;
        if (_predictions.Count != 0 && NumCasts > 2)
        {
            _predictions.RemoveAt(0);
            ++currentVersion;
        }
        if (NumCasts <= 14)
        {
            var origin = WPos.RotateAroundOrigin(45f, D013Philia.ArenaCenter, caster.Position.Quantized());
            _predictions.Add(new(circle, origin, default, WorldState.FutureTime(3.7d), shapeDistance: circle.Distance(origin, default)));
            ++currentVersion;
        }
        if (NumCasts == 16)
        {
            NumCasts = 0;
        }
    }
}

sealed class D013PhiliaStates : StateMachineBuilder
{
    public D013PhiliaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScavengersDaughter>()
            .ActivateOnEnter<HeadCrusher>()
            .ActivateOnEnter<PendulumFlare>()
            .ActivateOnEnter<PendulumAOE>()
            .ActivateOnEnter<Aethersup>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<SludgeVoidzone>()
            .ActivateOnEnter<Knout>()
            .ActivateOnEnter<Taphephobia>()
            .ActivateOnEnter<IntoTheLight>()
            .ActivateOnEnter<CatONineTails>()
            .ActivateOnEnter<FierceBeating>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8301)]
public sealed class D013Philia(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(134f, -465f); // slightly different from calculated center due to difference operation
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi64th, 64)], [new Rectangle(new(134f, -445.277f), 20f, 1.25f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IronChain), Colors.Vulnerable);
    }
}