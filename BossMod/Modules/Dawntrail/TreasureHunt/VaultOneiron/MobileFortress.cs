namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.MobileFortress;

public enum OID : uint
{
    MobileFortress = 0x48B2, // R4.0

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GoldyCat = 0x48B7, // R1.87
    Vaultkeeper = 0x48B8, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 43747, // MobileFortress->player, no cast, single-target
    Visual = 43601, // MobileFortress->self, no cast, single-target, used before Carpet Bomb and Quiet World

    QuietWorldVisual = 43731, // MobileFortress->self, 3.0s cast, single-target
    QuietWorld = 43732, // Helper->self, 13.5s cast, range 10 width 40 rect
    SurfaceMissileVisual = 43743, // MobileFortress->self, 4.0+1,0s cast, single-target
    SurfaceMissile = 43744, // Helper->self, 5.0s cast, range 50 circle
    AntiPersonnelMissileVisual = 43745, // MobileFortress->self, 3.0s cast, single-target
    AntiPersonnelMissile = 43746, // Helper->location, 3.0s cast, range 6 circle
    CarpetBombVisual = 43733, // MobileFortress->self, 3.0s cast, single-target
    CarpetBomb1 = 43734, // Helper->self, 5.0s cast, range 10 width 40 rect
    CarpetBomb2 = 43735, // Helper->self, 8.0s cast, range 10 width 40 rect
    CarpetBomb3 = 43736, // Helper->self, 11.0s cast, range 10 width 40 rect
    CarpetBomb4 = 43737, // Helper->self, 14.0s cast, range 10 width 40 rect

    GuidedMissile = 43742, // Helper->player, 7.0s cast, range 6 circle, spread
    Elimination = 43748, // MobileFortress->self/player, 5.0s cast, range 46 width 10 rect

    SystematicBombardmentVisual = 43738, // MobileFortress->self, 3.0s cast, single-target
    SystematicBombardment1 = 43739, // Helper->self, 5.0s cast, range 10 width 10 rect
    SystematicBombardment2 = 43740, // Helper->self, 8.0s cast, range 10 width 10 rect
    SystematicBombardment3 = 43741, // Helper->self, 11.0s cast, range 10 width 10 rect

    AutoAttack2 = 871, // Vaultkeeper->player, no cast, single-target
    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class AntiPersonnelMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AntiPersonnelMissile, 6f);
sealed class SurfaceMissile(BossModule module) : Components.RaidwideCast(module, (uint)AID.SurfaceMissile);
sealed class Elimination(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Elimination, new AOEShapeRect(46f, 5f), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster, endsOnCastEvent: true);
sealed class GuidedMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GuidedMissile, 6f);

sealed class CarpetBomb : Components.SimpleAOEGroups
{
    public CarpetBomb(BossModule module) : base(module, [(uint)AID.CarpetBomb1, (uint)AID.CarpetBomb2,
    (uint)AID.CarpetBomb3, (uint)AID.CarpetBomb4], new AOEShapeRect(10f, 20f), 2, 4)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }

    public override void Update()
    {
        if (Casters.Count == 8)
        {
            MaxDangerColor = 2;
            MaxRisky = 2;
            MaxCasts = 4;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.CarpetBomb4)
        {
            MaxDangerColor = 1;
            MaxRisky = 1;
            MaxCasts = 2;
        }
    }
}

sealed class QuietWorld(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuietWorld, new AOEShapeRect(10f, 20f), 4)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref readonly var aoe0 = ref aoes[0];
        var deadline = aoe0.Activation.AddSeconds(3.5d);
        var rot = aoe0.Rotation;
        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline || index > 1 && aoe.Rotation == rot)
            {
                break;
            }
            ++index;
        }
        return aoes[..index];
    }
}

sealed class SystematicBombardment(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(10f, 5f);
    private readonly List<AOEInstance> _aoes = new(16);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);

        var max = count == 16 ? 9 : count;
        ref var aoe0 = ref aoes[0];
        var deadline = aoe0.Activation.AddSeconds(1d);
        var color = Colors.Danger;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (count > 7)
                {
                    aoe.Color = color;
                }
                aoe.Risky = true;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SystematicBombardment1 or (uint)AID.SystematicBombardment2 or (uint)AID.SystematicBombardment3)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), risky: false));
            if (_aoes.Count == 16)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SystematicBombardment1 or (uint)AID.SystematicBombardment2 or (uint)AID.SystematicBombardment3)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class MobileFortressStates : StateMachineBuilder
{
    public MobileFortressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<Elimination>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<QuietWorld>()
            .ActivateOnEnter<CarpetBomb>()
            .ActivateOnEnter<SystematicBombardment>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(MobileFortress.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.MobileFortress, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14009u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 15)]
public sealed class MobileFortress(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    public static readonly uint[] All = [(uint)OID.MobileFortress, .. bonusAdds];

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
                (uint)OID.VaultOnion => 6,
                (uint)OID.VaultEggplant => 5,
                (uint)OID.VaultGarlic => 4,
                (uint)OID.VaultTomato => 3,
                (uint)OID.VaultQueen or (uint)OID.GoldyCat => 2,
                (uint)OID.Vaultkeeper => 1,
                _ => 0
            };
        }
    }
}
