namespace BossMod.Dawntrail.Unreal.Un4Tsukuyomi;

sealed class Reprimand(BossModule module) : Components.RaidwideCast(module, (uint)AID.Reprimand);
sealed class Nightbloom(BossModule module) : Components.RaidwideCast(module, (uint)AID.Nightbloom);
sealed class NightbloomHelper(BossModule module) : Components.RaidwideCast(module, (uint)AID.NightbloomHelper);
sealed class Antitwilight(BossModule module) : Components.RaidwideCast(module, (uint)AID.Antitwilight, "Raidwide, lethal if your debuff matches the floor");
sealed class MidnightRain(BossModule module) : Components.CastHint(module, (uint)AID.MidnightRain, "Floor turns one colour");

sealed class DanceOfTheDead(BossModule module) : Components.RaidwideInstant(module, (uint)AID.DanceOfTheDead, 5d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.DanceOfTheDeadVisual2)
            Activation = WorldState.FutureTime(Delay);
    }
}

sealed class NightfallHint(BossModule module) : Components.CastHints(module, [(uint)AID.NightfallGun, (uint)AID.NightfallSpear], "")
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Casters.Count != 0)
            hints.Add(Casters[0].CastInfo!.IsSpell(AID.NightfallGun) ? "Gun: stack for the line" : "Spear: split tanks / healers / DPS");
    }
}

sealed class LeadOfTheUnderworld(BossModule module) : BossComponent(module)
{
    private DateTime _activation;

    private WPos StackSpot()
    {
        var party = Raid.WithoutSlot();
        var len = party.Length;
        if (len == 0)
            return Arena.Center;
        var sum = new WDir();
        for (var i = 0; i < len; ++i)
            sum += party[i].Position - Arena.Center;
        return Arena.Center + sum / len;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NightfallGun)
            _activation = WorldState.FutureTime(6.4d);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LeadOfTheUnderworld)
            _activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation != default && !actor.Position.InCircle(StackSpot(), 3f))
            hints.Add("Stack for the line!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddForbiddenZone(new SDInvertedCircle(StackSpot(), 2.5f), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            Arena.AddCircleUnfilled(StackSpot(), 3f, Colors.Safe);
    }
}

sealed class SteelOfTheUnderworld(BossModule module) : BossComponent(module)
{
    private DateTime _activation;
    private Angle _facing;
    private int _numCasts;

    private WPos Spot(Actor actor)
    {
        var offset = actor.Role switch
        {
            Role.Tank => default,
            Role.Healer => 120f.Degrees(),
            _ => -120f.Degrees()
        };
        return Module.PrimaryActor.Position + 6f * (_facing + offset).ToDirection();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NightfallSpear)
        {
            _activation = WorldState.FutureTime(6.8d);
            _facing = caster.Rotation;
            _numCasts = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SteelOfTheUnderworld && ++_numCasts >= 3)
            _activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation != default && !actor.Position.InCircle(Spot(actor), 3f))
            hints.Add("Go to your role group for the spear cleaves!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
            hints.AddForbiddenZone(new SDInvertedCircle(Spot(actor), 2.5f), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            Arena.AddCircleUnfilled(Spot(pc), 3f, Colors.Safe);
    }
}

sealed class TormentUntoDeath(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TormentUntoDeath1, new AOEShapeCone(18.25f, 30f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class TormentUntoDeathPhysical(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TormentUntoDeath2, new AOEShapeCone(18.25f, 30f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class GosetsuCharge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpecterOfZenosVisual)
        {
            var gosetsu = Module.Enemies((uint)OID.SpecterOfGosetsu);
            if (gosetsu.Count != 0)
            {
                var g = gosetsu[0];
                var dir = caster.Position - g.Position;
                _aoe = [new(new AOEShapeRect(dir.Length(), 4f), g.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell))];
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GosetsuCharge)
            _aoe = [];
    }
}

sealed class TetheredSpecters(BossModule module) : Components.AddsMulti(module, [(uint)OID.SpecterOfThePatriarch, (uint)OID.SpecterOfTheMatriarch, (uint)OID.SpecterOfAsahi, (uint)OID.SpecterOfZenos], 2);
sealed class Specters(BossModule module) : Components.AddsMulti(module, [(uint)OID.SpecterOfTheHomeland1, (uint)OID.SpecterOfTheHomeland2, (uint)OID.SpecterOfTheEmpire1, (uint)OID.SpecterOfTheEmpire2], 1);

sealed class LunarHalo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LunarHalo, new AOEShapeDonut(5f, 15f));
sealed class Moonfall(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Moonfall, (uint)AID.Moonbeam, (uint)AID.Crater], 20f);

sealed class MoonfallBait(BossModule module) : BossComponent(module)
{
    private BitMask _targets;
    private DateTime _activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Moonfall)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            _activation = WorldState.FutureTime(5.4d);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Moondust)
        {
            _targets = default;
            _activation = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets[slot])
            hints.Add("Drop the meteor at the edge!", actor.Position.InCircle(Arena.Center, 14f));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_targets[slot])
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 15f), _activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
}

sealed class TsukiNoMaiogi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TsukiNoMaiogi, 10f, 6);

sealed class BrightDarkBlade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BrightBlade, (uint)AID.DarkBlade], new AOEShapeCone(43.25f, 105f.Degrees()));

sealed class WaxingWaningGrudge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BrightBlade or (uint)AID.DarkBlade)
        {
            AOEShape? shape = caster.FindStatus((uint)SID.FullMoon) != null ? circle : caster.FindStatus((uint)SID.NewMoon) != null ? donut : null;
            if (shape != null)
                _aoe = [new(shape, caster.Position, default, Module.CastFinishAt(spell, 1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WaxingGrudge or (uint)AID.WaningGrudge)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

sealed class Lunacy(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    private DateTime _lastHit;

    public override void Update()
    {
        if (_lastHit != default && WorldState.CurrentTime > _lastHit.AddSeconds(2.5d))
        {
            _lastHit = default;
            Stacks.Clear();
        }
        base.Update();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Lunacy)
        {
            _lastHit = default;
            Stacks.Clear();
            AddStack(actor, WorldState.FutureTime(5d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LunacyFirst or (uint)AID.LunacyRest && Stacks.Count != 0)
        {
            _lastHit = WorldState.CurrentTime;
            ref var stack = ref Stacks.Ref(0);
            stack.Activation = WorldState.FutureTime(1.1d);
            if (WorldState.Actors.Find(spell.MainTargetID) is Actor target)
                stack.Target = target;
        }
    }
}

sealed class Hagetsu(BossModule module) : Components.UniformStackSpread(module, default, 6f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Hagetsu)
            AddSpread(actor, WorldState.FutureTime(5.2d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HagetsuFirst or (uint)AID.HagetsuRest)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

sealed class MoonStatus(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var lit = actor.FindStatus((uint)SID.Moonlit);
        var shadow = actor.FindStatus((uint)SID.Moonshadowed);
        if (lit is ActorStatus l && (l.Extra & 0xFF) >= 3)
            hints.Add($"Moonlit x{l.Extra & 0xFF}: move to the dark side!", (l.Extra & 0xFF) >= 4);
        else if (shadow is ActorStatus s && (s.Extra & 0xFF) >= 3)
            hints.Add($"Moonshadowed x{s.Extra & 0xFF}: move to the light side!", (s.Extra & 0xFF) >= 4);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 1067u, NameID = 7225u, PlanLevel = 100)]
public sealed class Un4Tsukuyomi(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
