namespace BossMod.Dawntrail.Savage.M10STheXtremes;

// damage falloff based on proximity
sealed class XtremeSpectacular(BossModule module) : Components.SimpleAOEs(module, (uint)AID.XtremeSpectacular, new AOEShapeRect(50f, 18f));
sealed class XtremeSpectacularLast(BossModule module) : Components.CastCounter(module, (uint)AID.XtremeSpectacularLast);
sealed class SteamBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SteamBurst, 9f);
sealed class DebuffTracker(BossModule module) : BossComponent(module)
{
    public BitMask FirePlayers = default;
    public BitMask WaterPlayers = default;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Watersnaking or (uint)SID.XtremeWatersnaking or (uint)SID.Firesnaking or (uint)SID.XtremeFiresnaking)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot == -1)
                return;

            if (status.ID is (uint)SID.Watersnaking or (uint)SID.XtremeWatersnaking)
            {
                WaterPlayers.Set(slot);
            }
            else
            {
                FirePlayers.Set(slot);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Watersnaking or (uint)SID.XtremeWatersnaking or (uint)SID.Firesnaking or (uint)SID.XtremeFiresnaking)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot == -1)
                return;

            if (status.ID is (uint)SID.Watersnaking or (uint)SID.XtremeWatersnaking)
            {
                WaterPlayers.Clear(slot);
            }
            else
            {
                FirePlayers.Clear(slot);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (FirePlayers[slot])
        {
            hints.Add("Fire debuff", false);
        }
        else if (WaterPlayers[slot])
        {
            hints.Add("Water debuff", false);
        }
    }

    public Actor[] GetWaterActors() => GetActors(WaterPlayers);
    public Actor[] GetFireActors() => GetActors(FirePlayers);

    private Actor[] GetActors(BitMask mask)
    {
        if (mask.None())
            return [];

        var party = Raid.WithSlot(false, true, true);
        var len = 0;
        for (var i = 0; i < party.Length; ++i)
        {
            if (mask[party[i].Item1])
                ++len;
        }

        var actors = new Actor[len];
        var index = 0;
        for (var i = 0; i < party.Length && index < len; ++i)
        {
            if (!mask[party[i].Item1])
                continue;
            actors[index++] = party[i].Item2;
        }

        return actors;
    }
}
sealed class Watersnaking(BossModule module) : Components.RaidwideCast(module, (uint)AID.Watersnaking);
sealed class Firesnaking(BossModule module) : Components.RaidwideCast(module, (uint)AID.Firesnaking);
sealed class ScathingSteam(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScathingSteam);
sealed class XtremeWatersnaking(BossModule module) : Components.RaidwideCast(module, (uint)AID.XtremeWatersnaking);
sealed class XtremeFiresnaking(BossModule module) : Components.RaidwideCast(module, (uint)AID.XtremeFiresnaking);
sealed class Bailout(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Bailout1, (uint)AID.Bailout2], 15f);

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(M10STheXtremesStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.RedHot,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Savage,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1071u,
NameID = 14370u,
SortOrder = 1,
PlanLevel = 100)]
public sealed class M10STheXtremes(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsSquare ArenaBounds = new(20f);
    public static readonly uint[] Bosses = [(uint)OID.RedHot, (uint)OID.DeepBlue];

    private Actor? _deepBlue;
    private Actor? _wateryGrave;
    public Actor? DeepBlue() => _deepBlue;
    public Actor? WateryGrave() => _wateryGrave;

    protected override void UpdateModule()
    {
        _deepBlue ??= GetActor((uint)OID.DeepBlue);
        _wateryGrave ??= GetActor((uint)OID.WateryGrave);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_deepBlue);
    }
}
