namespace BossMod.Dawntrail.Raid.M11NTheTyrant;

sealed class CrownOfArcadia(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect rect = new(20, 6, 20);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            _aoes.Add(new(rect, new WPos(74, 100)));
            _aoes.Add(new(rect, new WPos(126, 100)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}

sealed class Smashdown1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_SmashdownScytheAOE, new AOEShapeDonut(5, 60));
sealed class Smashdown2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_SmashdownAxeAOE, 8f);
sealed class Smashdown3(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_SmashdownSwordAOE, new AOEShapeCross(40f, 5f));

/**
 * Void Stardust: Marks all players with AoEs that continue hitting them as well as spawning a ground AoE underneath.
 */
sealed class VoidStardust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidStardustSpread, (uint)AID._Spell_VoidStardust, 4f, 4.7f)
{
    public int _numCasts;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_VoidStardust)
            _numCasts = 0;
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID._Spell_Cometite or (uint)AID._Spell_Comet1))
        {
            return;
        }
        if (++_numCasts >= 16)
        {
            var id = spell.MainTargetID;
            var count = Spreads.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                if (Spreads[i].Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    ++NumFinishedSpreads;
                }
            }
        }
    }
}
sealed class Cometite(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_Cometite, 4f);

sealed class AssaultEvolved1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_AssaultEvolvedSword, new AOEShapeCross(40, 5f));
sealed class AssaultEvolved2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_AssaultEvolvedScythe, new AOEShapeDonut(5, 60));

sealed class AssaultEvolved3(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_AssaultEvolvedAxe, 8f);
sealed class DanceOfDomination(BossModule module) : Components.RaidwideCast(module, (uint)AID._Weaponskill_DanceOfDomination);
sealed class Explosion1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_Explosion, new AOEShapeRect(60f, 5f));
sealed class Explosion2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_Explosion1, new AOEShapeRect(60f, 5f));
sealed class Explosion3(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_Explosion2, new AOEShapeRect(60f, 3f));

sealed class RawSteelTankBuster(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.RawSteelSharedTankbuster, (uint)AID._Weaponskill_RawSteel1, 6f);
sealed class RawSteelSpreads(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.RawSteelSpread, (uint)AID._Weaponskill_Impact, 6, 0);
sealed class Charybdistopia(BossModule module) : Components.RaidwideCast(module, (uint)AID._Spell_Charybdistopia);

sealed class Maelstrom(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circ = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);
        if ((OID)actor.OID is OID.Maelstrom)
        {
            _aoes.Add(new(circ, actor.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Maelstrom)
        {
            _aoes.Clear();
        }
    }
}

//TODO : There is a fast cast of powerful gust that happens when boss teleports back to center that covers all radar and makes ai have a seizure.
// would be great to get rid of that.
sealed class PowerfulGust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        return CollectionsMarshal.AsSpan(_aoes)[..(count > 8 ? 8 : count)];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_PowerfulGust)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_PowerfulGust)
        {
            Remove(caster.InstanceID);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID._Spell_PowerfulGust)
        {
            Remove(caster.InstanceID);
        }
    }

    private void Remove(ulong id)
    {
        for (var i = _aoes.Count - 1; i >= 0; --i)
        {
            if (_aoes[i].ActorID == id)
            {
                _aoes.RemoveAt(i);
            }
        }
    }
}

sealed class OneAndOnly(BossModule module) : Components.RaidwideCast(module, (uint)AID._Weaponskill_OneAndOnly);
sealed class CosmicKiss(BossModule module) : Components.CastTowers(module, (uint)AID._Ability_CosmicKiss, 4f);
sealed class MassiveMeteor(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MassiveMeteorStack, (uint)AID._Ability_MassiveMeteor, 6, 0);
sealed class ForegoneFatality(BossModule module) : Components.TankbusterTether(module, (uint)AID._Spell_ForegoneFatality, (uint)TetherID._Gen_TankInterceptTether, 6f)
{
    private readonly List<(Actor Source, Actor Target)> _cometTethers = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (!Active && _cometTethers.Count != 0 && actor.Role == Role.Tank)
        {
            hints.Add("Grab the tether!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var count = _cometTethers.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = _cometTethers[i];
            Arena.AddLine(t.Source.Position, t.Target.Position, Colors.Danger);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == TID && source.OID == (uint)OID.TheTyrant && WorldState.Actors.Find(tether.Target) is Actor target && target.OID == (uint)OID.Comet)
        {
            _cometTethers.Add((source, target));
        }
        else
        {
            base.OnTethered(source, tether);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == TID && RemoveCometTethers(source, tether.Target) != 0)
        {
            return;
        }
        base.OnUntethered(source, tether);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        for (var i = _cometTethers.Count - 1; i >= 0; --i)
        {
            var t = _cometTethers[i];
            if (t.Source == actor || t.Target == actor)
            {
                _cometTethers.RemoveAt(i);
            }
        }
    }

    private int RemoveCometTethers(Actor source, ulong targetID)
    {
        var removed = 0;
        for (var i = _cometTethers.Count - 1; i >= 0; --i)
        {
            var t = _cometTethers[i];
            if (t.Source == source && t.Target.InstanceID == targetID)
            {
                _cometTethers.RemoveAt(i);
                ++removed;
            }
        }
        return removed;
    }
}

abstract class Tyrannhilation(BossModule module, uint aid, bool blockersImpassable) : Components.CastLineOfSightAOE(module, aid, 60f, blockersImpassable)
{
    private readonly List<Actor> _blockers = [];

    public override ReadOnlySpan<Actor> BlockerActors()
    {
        _blockers.Clear();
        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;
        var boss = Module.PrimaryActor;
        var bossPos = boss.Position;
        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (!c.IsDead && !c.Position.InCircle(bossPos, boss.HitboxRadius + c.HitboxRadius + 9f))
            {
                _blockers.Add(c);
            }
        }
        return CollectionsMarshal.AsSpan(_blockers);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Comet)
        {
            Refresh();
        }
    }
}

sealed class DoubleTyrannhilation(BossModule module) : Tyrannhilation(module, (uint)AID._Weaponskill_DoubleTyrannhilation1, false);
sealed class HiddenTyrannhilation(BossModule module) : Tyrannhilation(module, (uint)AID._Weaponskill_HiddenTyrannhilation, false);

sealed class Flatliner(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 6, 20);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Flatliner)
        {
            _aoes.Add(new(rect, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // We cheat here instead of drawing two new shapes, we make a danger zone between the rectangles.
        if ((AID)spell.Action.ID is AID._Weaponskill_Flatliner)
        {
            Arena.Bounds = new ArenaBoundsRect(26, 20);
        }
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            Arena.Bounds = new ArenaBoundsRect(20, 20);
            _aoes.Clear();
        }
    }
}

sealed class FlatlinerKnockUp(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID._Weaponskill_FlatlinerKnockup, 15, true);

sealed class MajesticMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_MajesticMeteor1, 6f);
sealed class MajesticMeteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_MajesticMeteorain, new AOEShapeRect(60f, 5f));
sealed class MammothMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_MammothMeteor, new AOEShapeCircle(22));
sealed class FireAndFury1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_FireAndFuryCone1, new AOEShapeCone(60f, 45.Degrees()));
sealed class FireAndFury2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_FireAndFuryCone2, new AOEShapeCone(60f, 45.Degrees()));

/**
 * We create the explosion towers with GenericTowers so we can access the constructor fields for ExplosionKnockUp
 */
sealed class ExplosionTower(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosionKnockUp)
        {
            Towers.Add(new(spell.LocXZ, 4f, 1, 8));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ExplosionKnockUp)
        {
            Towers.Clear();
        }
    }
}

sealed class ExplosionKnockUp(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly ExplosionTower _tower = module.FindComponent<ExplosionTower>()!;
    private Knockback[] _kbs = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kbs;

    public override void Update()
    {
        var towers = CollectionsMarshal.AsSpan(_tower.Towers);
        var len = towers.Length;
        if (len == 0)
        {
            _kbs = [];
            return;
        }
        if (_kbs.Length != len)
        {
            _kbs = new Knockback[len];
        }
        for (var i = 0; i < len; ++i)
        {
            ref var t = ref towers[i];
            _kbs[i] = new(t.Position, 25f, t.Activation, circle);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var towers = CollectionsMarshal.AsSpan(_tower.Towers);
        var len = towers.Length;
        if (len == 0)
        {
            return;
        }
        ref var t0 = ref towers[0];
        if ((t0.Activation - WorldState.CurrentTime).TotalSeconds < 6d)
        {
            for (var i = 0; i < len; ++i)
            {
                ref var t = ref towers[i];
                if (t.IsInside(actor))
                {
                    hints.ActionsToExecute.Push(ActionDefinitions.Armslength, actor, ActionQueue.Priority.High);
                    hints.ActionsToExecute.Push(ActionDefinitions.Surecast, actor, ActionQueue.Priority.High);
                    return;
                }
            }
        }
    }
}

// TODO: doesn't identify which platform will be destroyed in order to jump away.
sealed class ArcadionAvalanche(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40f, 20f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalanche)
        {
            _aoes.Add(new(rect, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}

// TODO: the platform that gets smashed should be marked dangerous. Then need to show where the aoe will hit on surviving platform.
sealed class ArcadionAvalancheToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcadionAvalancheToss, new AOEShapeRect(40f, 20f));
sealed class HeartbreakKick(BossModule module) : Components.CastTowers(module, (uint)AID._Weaponskill_HeartbreakKick, 4, 1, 8, damageType: AIHints.PredictedDamageType.Shared)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
        if ((uint)(AID)spell.Action.ID is (uint)AID._Spell_MajesticMeteor)
        {
            var pos = spell.LocXZ;
            for (var i = Towers.Count - 1; i >= 0; --i)
            {
                if (Towers[i].Position.AlmostEqual(pos, 1f))
                {
                    Towers.RemoveAt(i);
                }
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    { }
}

sealed class GreatWallOfFire(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.WallOfFireTankbuster, (uint)AID._Weaponskill_GreatWallOfFire, new AOEShapeRect(60, 3f));

// =========================
// Module
// =========================

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.Boss, Contributors = "VeraNala, wen, Topas", GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1072u, NameID = 14305u)]
public sealed class M11NTheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
