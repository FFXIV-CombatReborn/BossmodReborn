namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD70Kenko;

public enum OID : uint
{
    Boss = 0x23EB, // R6.0
    InnerspaceVoidzone = 0x1E9829 // R0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    PredatorClaws = 12205, // Boss->self, 3.0s cast, range 9+R 120-degree cone
    Slabber = 12203, // Boss->location, 3.0s cast, range 8 circle
    Innerspace = 12207, // Boss->player, 3.0s cast, single-target
    Ululation = 12208, // Boss->self, 3.0s cast, range 80+R circle
    HoundOutOfHell = 12206, // Boss->player, 5.0s cast, width 14 rect charge
    Devour = 12204 // Boss->location, no cast, range 4+R 90-degree cone
}

public enum IconID : uint
{
    HoundOutOfHell = 1 // player->self
}

sealed class PredatorClaws(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PredatorClaws, new AOEShapeCone(15f, 60f.Degrees()));
sealed class Slabber(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Slabber, 8f);

sealed class InnerspaceSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Innerspace, 3f);
sealed class InnerspaceVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private AOEInstance[] _aoeTarget = [];
    private static readonly AOEShapeCircle circleInv = new(3f, true), circle = new(3f);
    private int target = -1;
    private bool ululation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (slot != target || ululation)
            return _aoe;
        else
            return _aoeTarget;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Innerspace && WorldState.Actors.Find(spell.MainTargetID) is Actor t)
        {
            _aoe = [new(circle, t.Position.Quantized(), default, WorldState.FutureTime(7.2d))];
        }
        else if (id == (uint)AID.Devour)
        {
            target = -1;
            _aoeTarget = [];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.InnerspaceVoidzone)
        {
            _aoe = [new(circle, actor.Position)];
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HoundOutOfHell)
        {
            target = Raid.FindSlot(targetID);
            if (_aoe.Length != 0)
            {
                ref var aoe = ref _aoe[0];
                _aoeTarget = [new(circleInv, aoe.Origin, color: Colors.SafeFromAOE)];
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Ululation)
        {
            ululation = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Ululation)
        {
            ululation = false;
        }
    }

    public override void Update()
    {
        if (_aoe.Length != 0)
        {
            var p = Module.Enemies((uint)OID.InnerspaceVoidzone);
            if (p.Count != 0 && p[0].EventState == 7)
            {
                _aoe = [];
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        var isInside = aoe.Check(actor.Position);
        if (slot == target && !ululation)
        {
            hints.Add("Go inside puddle!", !isInside);
        }
        else if (isInside)
        {
            hints.Add("GTFO from AOE!");
        }
    }
}

sealed class Devour(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone cone = new(10f, 45f.Degrees()); // TODO: verify angle

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HoundOutOfHell)
        {
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.TargetID)!, cone, Module.CastFinishAt(spell, 3.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Devour)
            CurrentBaits.Clear();
    }
}

sealed class Ululation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Ululation);
sealed class HoundOutOfHell(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.HoundOutOfHell, 7f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).Count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class DD70KenkoStates : StateMachineBuilder
{
    public DD70KenkoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PredatorClaws>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<InnerspaceSpread>()
            .ActivateOnEnter<InnerspaceVoidzone>()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<Ululation>()
            .ActivateOnEnter<HoundOutOfHell>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 546, NameID = 7489)]
public sealed class DD70Kenko(WorldState ws, Actor primary) : HoHArena2(ws, primary);
