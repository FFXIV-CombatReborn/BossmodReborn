namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.DaenOseTheAvariciousTyphon;

public enum OID : uint
{
    DaenOseTheAvariciousTyphon = 0x301A, // R0.75-4.5
    WrigglingMenace = 0x3024, // R1.8
    LingeringSnort = 0x301B, // R0.8
    DaenOseTheAvaricious1 = 0x3082, // R1.0, TODO: rotation ccw?
    DaenOseTheAvaricious2 = 0x3051, // R1.0, TODO: rotation helper?
    DaenOseTheAvaricious3 = 0x3050, // R1.0, TODO: rotation cw?
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // DaenOseTheAvariciousTyphon/WrigglingMenace->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras->player, no cast, single-target

    AChoo = 21689, // DaenOseTheAvariciousTyphon->self, 3.0s cast, range 12 90-degree cone

    UnpleasantBreezeVisual = 21696, // DaenOseTheAvariciousTyphon->self, 3.0s cast, single-target
    UnpleasantBreeze = 21697, // Helper->location, 3.0s cast, range 6 circle
    StoutSnort = 21687, // DaenOseTheAvariciousTyphon->self, 4.0s cast, range 40 circle, raidwide

    VisualLingeringSnort = 21694, // DaenOseTheAvariciousTyphon->self, 3.0s cast, single-target
    LingeringSnort = 21695, // Helper->self, 6.5s cast, range 50 circle, damage fall off aoe
    SnortsaultKB = 21690, // DaenOseTheAvariciousTyphon->self, 6.5s cast, range 40 circle, knockback 20, away from source
    SnortsaultCircle = 21782, // DaenOseTheAvariciousTyphon->self, no cast, range 5 circle
    SnortsaultCone = 21781, // Helper->self, no cast, range 20 45-degree cone
    SnortAssaultEnd = 21691, // DaenOseTheAvariciousTyphon->self, no cast, range 40 circle

    FellSwipe = 21771, // WrigglingMenace->self, 3.0s cast, range 8 120-degree cone
    WindShot = 21772, // LingeringSnort->self, 3.0s cast, range 40 width 6 rect
    Fireball = 21688, // Boss->players, 5.0s cast, range 6 circle, stack

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

sealed class AChoo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AChoo, new AOEShapeCone(12f, 45f.Degrees()));
sealed class FellSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FellSwipe, new AOEShapeCone(8f, 60f.Degrees()));
sealed class WindShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindShot, new AOEShapeRect(40f, 3f));
sealed class LingeringSnort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LingeringSnort, 20f);
sealed class UnpleasantBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnpleasantBreeze, 6f);
sealed class Fireball(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Fireball, 6f, 8, 8);

sealed class SnortsaultKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SnortsaultKB, 20f, stopAtWall: true);
sealed class SnortsaultCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly LingeringSnort _aoes = module.FindComponent<LingeringSnort>()!;
    private readonly AOEShapeCircle circle = new(5f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _aoes.ActiveCasters.Length == 0 ? _aoe : [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DaenOseTheAvaricious2)
        {
            var pos = Arena.Center.Quantized();
            _aoe = [new(circle, pos, default, WorldState.FutureTime(14.3d), shapeDistance: circle.Distance(pos, default))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnortAssaultEnd)
        {
            _aoe = [];
        }
    }
}

sealed class Snortsault(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly LingeringSnort _aoe = module.FindComponent<LingeringSnort>()!;
    private readonly AOEShapeCone cone = new(20f, 22.5f.Degrees());

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.DaenOseTheAvaricious3:
                AddSequences(false);
                break;
            case (uint)OID.DaenOseTheAvaricious1:
                AddSequences(true);
                break;
        }
        void AddSequences(bool isClockwise)
        {
            var inc = 6f.Degrees();
            var rotationIncrement = isClockwise ? inc : -inc;
            AddSequence(default);
            AddSequence(180f.Degrees());
            void AddSequence(Angle offset) => Sequences.Add(new(cone, Arena.Center.Quantized(), actor.Rotation + offset, rotationIncrement, WorldState.FutureTime(14.5d), 1.1d, 31, 9));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnortsaultCone)
        {
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.ActiveCasters.Length == 0)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.ActiveCasters.Length == 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_aoe.ActiveCasters.Length == 0)
        {
            base.DrawArenaBackground(pcSlot, pc);
        }
    }
}

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class DaenOseTheAvariciousTyphonStates : StateMachineBuilder
{
    public DaenOseTheAvariciousTyphonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AChoo>()
            .ActivateOnEnter<FellSwipe>()
            .ActivateOnEnter<WindShot>()
            .ActivateOnEnter<UnpleasantBreeze>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<LingeringSnort>()
            .ActivateOnEnter<SnortsaultCircle>()
            .ActivateOnEnter<Snortsault>()
            .ActivateOnEnter<SnortsaultKB>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(DaenOseTheAvariciousTyphon.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DaenOseTheAvariciousTyphonStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.DaenOseTheAvariciousTyphon,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Shadowbringers,
Category = BossModuleInfo.Category.TreasureHunt,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 745u,
NameID = 9808u,
SortOrder = 14,
PlanLevel = 0)]
public sealed class DaenOseTheAvariciousTyphon : THTemplate
{
    public DaenOseTheAvariciousTyphon(WorldState ws, Actor primary) : base(ws, primary)
    {
        menaces = Enemies((uint)OID.WrigglingMenace);
    }
    private readonly List<Actor> menaces;
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen];
    public static readonly uint[] All = [(uint)OID.DaenOseTheAvariciousTyphon, (uint)OID.WrigglingMenace, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(menaces);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato => 3,
                (uint)OID.SecretQueen => 2,
                (uint)OID.WrigglingMenace => 1,
                _ => 0
            };
        }
    }
}
