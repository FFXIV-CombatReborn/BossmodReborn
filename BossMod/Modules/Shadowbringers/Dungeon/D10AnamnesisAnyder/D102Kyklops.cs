namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D102Kyklops;

public enum OID : uint
{
    Boss = 0x2CFE, // R4.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 19283, // Boss->player, no cast, single-target

    TheFinalVerse = 19288, // Boss->self, 4.0s cast, range 40 circle
    EyeOfTheCyclone = 19287, // Boss->self, 4.0s cast, range 8-25 donut

    TerribleHammerVisual = 19289, // Boss->self, 3.0s cast, single-target, spawns blade icons
    TerribleBladeVisual = 19290, // Boss->self, 3.0s cast, single-target, spawns hammer icons
    TerribleHammer = 19293, // Helper->self, no cast, range 10 width 10 rect
    TerribleBlade = 19294, // Helper->self, no cast, range 10 width 10 rect

    RagingGlower = 19286, // Boss->self, 3.0s cast, range 45 width 6 rect
    MinaSwipe2000 = 19284, // Boss->self, 4.0s cast, range 12 120-degree cone
    MinaSwing2000 = 19285, // Boss->self, 4.0s cast, range 12 circle

    OpenHearthVisual = 19292, // Boss->self, no cast, single-target, stack
    OpenHearth = 19296, // Helper->player, 5.0s cast, range 6 circle

    WanderersPyreVisual = 19291, // Boss->self, no cast, single-target, spread
    WanderersPyre = 19295 // Helper->player, 5.0s cast, range 5 circle
}

class TheFinalVerse(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheFinalVerse);
class WanderersPyre(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WanderersPyre, 5f);
class OpenHearth(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.OpenHearth, 6f);
class EyeOfTheCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyeOfTheCyclone, new AOEShapeDonut(8f, 25f));
class RagingGlower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RagingGlower, new AOEShapeRect(45f, 3f));
class MinaSwipe2000(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MinaSwipe2000, new AOEShapeCone(12f, 60f.Degrees()));
class MinaSwing2000(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MinaSwing2000, 12f);

class TerribleBladeHammer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly WPos[] xPattern = [new(10f, -70f), new(30f, -70f), new(20f, -80f), new(10f, -90f), new(30f, -90f)];
    private static readonly WPos[] crossPattern = [new(20f, -70f), new(10f, -80f), new(30f, -80f), new(20f, -90f)];
    private static readonly AOEShapeRect rect = new(5f, 5f, 5f);
    private const double XPatternActivationTime = 16d, CrossPatternActivationTime = 18.2d;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref readonly var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // 0D - 08000400 - hammer X pattern, 20001000 - hammer cross pattern
        // 0E - 00800040 - blade cross pattern, 02000100 - blade X pattern
        // any - 00080004 - pattern disappears
        if (_aoes.Count > 0 || index is not 0xD and not 0xE)
        {
            return;
        }

        if (state is 0x08000400u or 0x02000100u)
        {
            SetupPattern(xPattern, crossPattern, XPatternActivationTime, CrossPatternActivationTime);
        }
        else if (state is 0x20001000u or 0x00800040u)
        {
            SetupPattern(crossPattern, xPattern, XPatternActivationTime, CrossPatternActivationTime);
        }

        void SetupPattern(WPos[] primaryPattern, WPos[] secondaryPattern, double primaryTime, double secondaryTime)
        {
            AddAOEs(primaryPattern, primaryTime);
            AddAOEs(secondaryPattern, secondaryTime);

            void AddAOEs(WPos[] pattern, double activationTime)
            {
                var len = pattern.Length;
                var act = WorldState.FutureTime(activationTime);
                for (var i = 0; i < len; ++i)
                {
                    _aoes.Add(new(rect, pattern[i], Angle.AnglesCardinals[1], act));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && spell.Action.ID is (uint)AID.TerribleHammer or (uint)AID.TerribleBlade)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class D102KyklopsStates : StateMachineBuilder
{
    public D102KyklopsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerribleBladeHammer>()
            .ActivateOnEnter<TheFinalVerse>()
            .ActivateOnEnter<WanderersPyre>()
            .ActivateOnEnter<OpenHearth>()
            .ActivateOnEnter<EyeOfTheCyclone>()
            .ActivateOnEnter<RagingGlower>()
            .ActivateOnEnter<MinaSwing2000>()
            .ActivateOnEnter<MinaSwipe2000>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9263)]
public class D102Kyklops(WorldState ws, Actor primary) : BossModule(ws, primary, new(20, -80.1f), new ArenaBoundsRect(14.5f, 14.4f));
