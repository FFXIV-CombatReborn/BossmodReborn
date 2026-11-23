namespace BossMod.Endwalker.Quest.TheGiftOfMercy;

public enum OID : uint
{
    Boss = 0x3596, // R7.500, x1
    Helper = 0x233C, // R0.500, x5, Helper type
    Levinsphere = 0x34D5, // R1.000, x0 (spawn during fight)
    Thunderhead = 0x359D, // R1.000, x0 (spawn during fight)
    Raubahn = 0x3597, // R0.590, x1, ally
    FordolaLupis = 0x3598, // R0.500, x1, ally
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->Raubahn, no cast, single-target
    ThroesOfRemorse = 26682, // Boss->self, 4.0s cast, range 40 width 6 rect
    ThroesOfRemorseLong = 26683, // Boss->self, 8.0s cast, range 40 width 6 rect
    LevinsphereEvocation = 28319, // Boss->self, 3.0s cast, single-target
    LevinsphereBlast = 28320, // Levinsphere->self, 5.0s cast, range 8 circle
    GroundPound = 26684, // Boss->self, 5.0s cast, range 40 circle, damage falloff
    CrushingBolt = 28367, // Boss->self, 5.0s cast, single-target
    CrushingBoltTB = 28368, // Helper->Raubahn, 5.0s cast, single-target
    FulminousTorment = 26685, // Boss->FordolaLupis, 5.0s cast, single-target
    FulminousTormentHit = 26686, // Helper->FordolaLupis, no cast, single-target, applies esunable Electrocution
    LethalLevin = 26689, // Boss->self, 5.0s cast, single-target
    LethalLevinStack = 26690, // Helper->FordolaLupis, 5.0s cast, range 6 circle stack
    Shock = 26691, // Boss->self, 6.0s cast, single-target
    ShockTower = 26692, // Helper->self, 7.0s cast, range 4 circle tower
    Electrify = 26693, // Helper->self, no cast, range 40 circle, unsoaked tower
    CloudCall = 26681, // Boss->self, 3.0s cast, single-target
    LightningBolt = 26698, // Thunderhead->self, 6.0s cast, range 8 circle
    TizonasBastion = 26701, // Raubahn->self, no cast, protective ground zone
    RuefulOutburstVisual = 26696, // Boss->self, 15.0s cast, single-target
    RuefulOutburst = 26697, // Helper->self, 16.0s cast, range 40 circle
    RuefulOutburstRepeatVisual = 26728, // Boss->self, 2.0s cast, single-target
    RuefulOutburstRepeat = 28318, // Helper->self, 3.0s cast, range 40 circle
    PenitentBlade = 26702, // FordolaLupis->Boss, 36.0s cast, single-target
}

public enum SID : uint
{
    Electrocution = 271, // Boss->FordolaLupis, extra=0x0, esunable
    TizonasBastion = 2908, // none->player/FordolaLupis, extra=0x0
}

sealed class ThroesOfRemorse(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThroesOfRemorse, (uint)AID.ThroesOfRemorseLong], new AOEShapeRect(40, 3));
sealed class LevinsphereBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LevinsphereBlast, 8);
sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 8);
sealed class GroundPound(BossModule module) : Components.RaidwideCast(module, (uint)AID.GroundPound);
sealed class CrushingBolt(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CrushingBoltTB);
sealed class FulminousTorment(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FulminousTorment, "Esuna Fordola after the hit", AIHints.PredictedDamageType.None);
sealed class LethalLevin(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.LethalLevinStack, 6);
sealed class Shock(BossModule module) : Components.CastTowers(module, (uint)AID.ShockTower, 4);
sealed class RuefulOutburst(BossModule module) : Components.RaidwideCast(module, (uint)AID.RuefulOutburst);
sealed class RuefulOutburstRepeat(BossModule module) : Components.RaidwideCast(module, (uint)AID.RuefulOutburstRepeat);

sealed class Electrocution(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            if (party[i].FindStatus((uint)SID.Electrocution) != null)
            {
                hints.Add("Esuna Fordola!");
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (i, p) in Raid.WithSlot())
            if (p.FindStatus((uint)SID.Electrocution) != null)
                hints.ShouldCleanse.Set(i);
    }
}

sealed class TizonasBastion(BossModule module) : Components.GenericAOEs(module, default, "Get inside Raubahn's bastion!")
{
    private AOEInstance? _aoe;

    private readonly AOEInstance[] _oneAOE = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe is AOEInstance aoe ? _oneAOE.Single(aoe) : [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe is AOEInstance aoe && !aoe.Check(actor.Position))
            hints.Add(WarningText);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TizonasBastion)
            _aoe = new(new AOEShapeCircle(6.5f, true), caster.Position, color: Colors.SafeFromAOE);
    }
}

sealed class CharletStates : StateMachineBuilder
{
    public CharletStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThroesOfRemorse>()
            .ActivateOnEnter<LevinsphereBlast>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<GroundPound>()
            .ActivateOnEnter<CrushingBolt>()
            .ActivateOnEnter<FulminousTorment>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<LethalLevin>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<RuefulOutburst>()
            .ActivateOnEnter<RuefulOutburstRepeat>()
            .ActivateOnEnter<TizonasBastion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Kagekazu (ported from upstream by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 820, NameID = 10341)]
public sealed class Charlet(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, 130), new ArenaBoundsCircle(20));
