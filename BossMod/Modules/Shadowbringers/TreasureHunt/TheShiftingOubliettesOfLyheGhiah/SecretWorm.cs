namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretWorm;

public enum OID : uint
{
    Boss = 0x3029, //R=6.0
    Bubble = 0x302A, //R=1.5
    SecretQueen = 0x3021, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras->player, no cast, single-target

    Hydroburst = 21714, // Bubble->self, 1.0s cast, range 8 circle
    Hydrocannon = 21713, // Boss->location, 3.0s cast, range 8 circle
    AquaBurst = 21715, // Bubble->self, 5.0s cast, range 50 circle, damage fall off AOE, optimal range seems to be 10
    FreshwaterCannon = 21711, // Boss->self, 2.5s cast, range 46 width 4 rect
    BrineBreath = 21710, // Boss->player, 4.0s cast, single-target

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class Hydrocannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrocannon, 8f);
class FreshwaterCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FreshwaterCannon, new AOEShapeRect(46f, 2f));
class AquaBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaBurst, 10f);
class BrineBreath(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BrineBreath);
class Hydroburst(BossModule module) : Components.Voidzone(module, 10f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Bubble);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead && !(z.CastInfo != null && z.CastInfo.IsSpell(AID.AquaBurst)))
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class Bubble(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(10f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, WorldState.FutureTime(5d)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrocannon)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count != 0)
        {
            ref var b = ref CurrentBaits.Ref(0);
            if (b.Target == actor)
            {
                hints.AddForbiddenZone(new SDCircle(Arena.Center, 17.5f), b.Activation);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        if (CurrentBaits.Ref(0).Target != actor)
        {
            base.AddHints(slot, actor, hints);
        }
        else
        {
            hints.Add("Bait bubble away!");
        }
    }
}

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class SecretWormStates : StateMachineBuilder
{
    public SecretWormStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FreshwaterCannon>()
            .ActivateOnEnter<AquaBurst>()
            .ActivateOnEnter<BrineBreath>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Bubble>()
            .ActivateOnEnter<Hydroburst>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretWorm.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9780)]
public class SecretWorm(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
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
                (uint)OID.SecretOnion => 5,
                (uint)OID.SecretEgg => 4,
                (uint)OID.SecretGarlic => 3,
                (uint)OID.SecretTomato => 2,
                (uint)OID.SecretQueen => 1,
                _ => 0
            };
        }
    }
}
