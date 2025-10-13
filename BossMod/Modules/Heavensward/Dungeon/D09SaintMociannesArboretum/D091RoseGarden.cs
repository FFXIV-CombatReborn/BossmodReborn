namespace BossMod.Heavensward.Dungeon.D09SaintMociannesArboretum.D091RoseGarden;

public enum OID : uint
{
    Boss = 0x142F, // R5.775
    RoseBud = 0x1430, // R0.9
    RoseHip = 0x1431, // R1.5
    ResinVoidzone = 0x1E9E38, // R0.5
    Helper2 = 0x14C6,
    Helper1 = 0x14C5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    VineProbe = 5228, // Boss->self, no cast, range 6+R width 7 rect
    EarthyBreathVisual = 5231, // RoseBud->self, 5.0s cast, single-target
    EarthyBreathFirst = 5316, // Helper2->self, 5.0s cast, range 7+R 30-degree cone
    EarthyBreathRest = 5232, // Helper2->self, no cast, range 7+R width 3 rect
    ExtremelyBadBreathVisual = 5224, // Boss->self, 5.0s cast, single-target
    ExtremelyBadBreath = 5333, // Boss->self, 5.0s cast, range 19+R 90-degree cone
    ExtremelyBadBreathFirst = 5230, // Helper1->self, 5.0s cast, range 24+R 90-degree cone
    ExtremelyBadBreathRepeat = 5225, // Helper1->self, no cast, range 20+R 90-degree cone
    Schizocarps = 5226, // Boss->self, no cast, single-target, spwn Rose Hips
    BurrFester = 5229, // Boss->self, no cast, range 40 circle, raidwide
    ExplosiveDehiscence = 5234 // RoseHip->self, no cast, range 40 circle, rose hips if not killed fast enough
}

sealed class VineProbe(BossModule module) : Components.Cleave(module, (uint)AID.VineProbe, new AOEShapeRect(11.775f, 3.5f), activeWhileCasting: false)
{
    private readonly ExtremelyBadBreathRotation _rot = module.FindComponent<ExtremelyBadBreathRotation>()!;
    private bool RotationInactive => _rot.Sequences.Count == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (RotationInactive)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (RotationInactive)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (RotationInactive)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class ExtremelyBadBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExtremelyBadBreath, ExtremelyBadBreathRotation.Cone);

sealed class ExtremelyBadBreathRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _rot1;
    public static readonly AOEShapeCone Cone = new(24.775f, 45f.Degrees());
    private bool first = true;
    private int correctSteps;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // direction seems to be server side until after first rotation
        if (Sequences.Count == 0)
        {
            var count = _aoes.Count;
            if (count == 0)
            {
                return [];
            }

            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var act0 = aoes[0].Activation;
            var color = Colors.Danger;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if ((aoe.Activation - act0).TotalSeconds < 1d)
                {
                    aoes[i].Color = color;
                }
            }
            return aoes;
        }
        else
        {
            return base.ActiveAOEs(slot, actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // rotation direction seems to be unknown until 2nd repeat, so we predict start into both directions
        void AddAOE(Angle offset, double delay = 1d)
        {
            var rot = spell.Rotation + offset;
            var pos = spell.LocXZ;
            _aoes.Add(new(Cone, pos, rot, Module.CastFinishAt(spell, delay), shapeDistance: Cone.Distance(pos, rot)));
        }

        if (spell.Action.ID == (uint)AID.ExtremelyBadBreathFirst)
        {
            _rot1 = spell.Rotation;
            AddAOE(default, default);
            AddAOE(45f.Degrees());
            AddAOE(-45f.Degrees());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExtremelyBadBreathFirst:
                if (_aoes.Count != 0)
                {
                    _aoes.RemoveAt(0);
                }
                break;
            case (uint)AID.ExtremelyBadBreathRepeat:
                var count = Sequences.Count;
                if (count == 0)
                {
                    var rot2 = spell.Rotation;
                    var rotDelta = (_rot1 - rot2).Normalized().Rad;
                    if (Math.Abs(rotDelta) < 1e-3f) // rotation usually starts after 1 or 2 repeats, also checking for small delta since there are miniscule errors that causes _rot1 != rot2
                    {
                        ++correctSteps;
                        return;
                    }
                    var inc = (rotDelta > 0f ? -1 : 1) * 11.6f.Degrees(); // last hit is only about 7.4°, but shouldnt matter for us, let's consider it extra safety margin
                    Sequences.Add(new(Cone, Module.PrimaryActor.Position.Quantized(), rot2, inc, WorldState.FutureTime(1d), 1d, (first ? 13 : 25) - correctSteps, 8));
                    first = false;
                }
                else
                {
                    AdvanceSequence(0, WorldState.CurrentTime);
                    if (Sequences.Count == 0)
                    {
                        correctSteps = 0;
                    }
                }
                break;
        }
    }
}

sealed class EarthyBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone cone = new(7.5f, 15f.Degrees());
    private readonly AOEShapeRect rect = new(7.5f, 1.5f);
    private readonly List<AOEInstance> _aoes = new(10);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape)
        {
            var rot = spell.Rotation;
            var pos = spell.LocXZ;
            _aoes.Add(new(shape, pos, rot, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(pos, rot)));
        }
        if (spell.Action.ID == (uint)AID.EarthyBreathFirst)
        {
            AddAOE(cone);
            AddAOE(rect);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.EarthyBreathFirst)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (_aoes.Count != 0 && modelState == 0)
        {
            _aoes.Clear();
        }
    }
}

sealed class ResinVoidzone(BossModule module) : Components.Voidzone(module, 5f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ResinVoidzone);
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

sealed class BurrFester(BossModule module) : Components.RaidwideCast(module, (uint)AID.BurrFester);

sealed class D091RoseGardenStates : StateMachineBuilder
{
    public D091RoseGardenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExtremelyBadBreathRotation>()
            .ActivateOnEnter<EarthyBreath>()
            .ActivateOnEnter<ResinVoidzone>()
            .ActivateOnEnter<BurrFester>()
            .ActivateOnEnter<ExtremelyBadBreath>()
            .ActivateOnEnter<VineProbe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 41, NameID = 4653, SortOrder = 2)]
public sealed class D091RoseGarden(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(default, -82.146f), 19.5f, 48)], [new Rectangle(new(18.221f, -90.993f), 20f, 1.25f, -65f.Degrees()),
    new Rectangle(new(-20.214f, -82.492f), 20f, 1.25f, -88.9f.Degrees())]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.RoseHip));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.RoseHip => 1,
                _ => 0
            };
        }
    }
}
