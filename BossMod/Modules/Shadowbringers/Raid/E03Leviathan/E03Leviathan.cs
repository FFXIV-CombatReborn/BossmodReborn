namespace BossMod.Shadowbringers.Raid.E03Leviathan;

public enum OID : uint
{
    Boss = 0x296B,
    Helper = 0x233C,
    MaelstromVoidZone = 0x1EAC2B,
}
public enum AID : uint
{
    TidalRoar = 16324, // Boss->self, 4.0s cast, single-target
    RipCurrentCast = 16326, // Boss->self, 5.0s cast, single-target
    RipCurrentHit = 16327, // Helper->self, no cast, range 50 width 6 rect
    TemporaryCurrentRight = 16334, // Boss->self, 6.0s cast, range 58+R width 30 rect
    DrenchingPulse = 16328, // Boss->self, 3.0s cast, single-target
    MonsterWave = 16330, // Helper->location, 4.0s cast, range 6 circle
    FreakWave = 16331, // Helper->players, 6.0s cast, range 5 circle
    TemporaryCurrentLeft = 16333, // Boss->self, 6.0s cast, range 58+R width 30 rect
    TidalWave = 16339, // Helper->self, 10.0s cast, range 60+R width 60 rect
    UnderseaQuakeSides = 16337, // Helper->self, 6.0s cast, range 40+R width 10 rect
    CrashingPulse = 16329, // Boss->self, 3.0s cast, single-target
    KillerWave = 16332, // Helper->players, 6.0s cast, range 6 circle
    Tsunami = 17435, // Helper->self, 11.0s cast, range 80 circle
    SurgingTsunami = 16401, // Helper->self, no cast, range 80 circle
    SmotheringTsunami = 16343, // Helper->players, no cast, range 6 circle
    SplashingTsunami = 16342, // Helper->self, no cast, range 6 circle
    SwirlingTsunami = 16341, // Helper->players, no cast, range ?-10 donut
    Maelstrom = 16344, // Boss->self, 3.0s cast, single-target
    SpinningDive = 16347, // Boss->self, 5.0s cast, range 46+R width 20 rect
    UnderseaQuakeMid = 16335, // Boss->self, 6.0s cast, range 40+R width 20 rect
}

public enum SID : uint
{
    SplashingWaters = 1851, // Spread mechanic when debuff expires, dorito-esque icon.
    SmotheringWaters = 1853, // Stack mechanic when debuff expires.
    SurgingWaters = 1850, // Knockback mechanic when debuff expires
    SwirlingWaters = 1852, // Donut? mechanic when debuff expires

}

public enum IconID : uint
{
    RipCurrentTarget = 23, // player->self
    DrenchingPulseSpread = 169, // player->self
    CrashingPulseStack = 62, // player->self
    TsunamiKnockbackFromPlayer = 173, // player->self
}

sealed class RipCurrent(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(50f, 3f), (uint)IconID.RipCurrentTarget, (uint)AID.RipCurrentCast, activationDelay: 5.1f, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    // if not targetted, limit how far they will run to try to dodge bait.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaits)
        {
            if (b.Target != actor)
            {
                hints.AddForbiddenZone(new AOEShapeDonut(8, 100), new WPos(100, 95), default, b.Activation);
            }
        }
    }
}

sealed class TemporaryCurrentRight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TemporaryCurrentRight, new AOEShapeRect(76f, 15f));
sealed class TemporaryCurrentLeft(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TemporaryCurrentLeft, new AOEShapeRect(76f, 15f));
sealed class TidalRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.TidalRoar);
sealed class Tsunami(BossModule module) : Components.RaidwideCast(module, (uint)AID.Tsunami);

sealed class MonsterWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MonsterWave, 6f);

sealed class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWave, 18f, kind: Kind.DirForward);

sealed class UnderseaQuakeSides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnderseaQuakeSides, new AOEShapeRect(40f, 5f));
sealed class UnderseaQuakeMid(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnderseaQuakeMid, new AOEShapeRect(40f, 10f))
{
    public DateTime Activation;
    public DateTime VoidZoneTime;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.UnderseaQuakeMid)
        {
            Activation = Module.CastFinishAt(spell);
            VoidZoneTime = Activation.AddSeconds(3f);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        //split to sides by light party to make the next spread more feasible for AI.
        if (WorldState.CurrentTime < Activation)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.VeryHigh);
            switch (assignment)
            {
                case PartyRolesConfig.Assignment.OT:
                case PartyRolesConfig.Assignment.M2:
                case PartyRolesConfig.Assignment.H2:
                case PartyRolesConfig.Assignment.R2:
                    hints.AddForbiddenZone(new SDRect(new WPos(90, 80), new WPos(90, 120), 10f), Activation);//don't go left
                    break;
                default:
                    hints.AddForbiddenZone(new SDRect(new WPos(110, 80), new WPos(110, 120), 10f), Activation);//don't go right
                    break;
            }
        }
        //There is a frame after the aoe before the floor drops out and arena changes. AI healer will try to run in to heal, and plummet to their death after successfully dodging the aoe.
        //This should treat it as a void zone for an additional 3 seconds.
        else if (WorldState.CurrentTime < VoidZoneTime)
        {
            hints.AddForbiddenZone(new SDRect(new WPos(100, 80), new WPos(100, 120), 10f));
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    // two 10y wide side platforms remain once the middle is gone (built once, not on every director update)
    private static readonly ArenaBoundsCustom splitArena = new([new Rectangle(new(85f, 100f), 5f, 20f), new Rectangle(new(115f, 100f), 5f, 20f)]);

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        base.OnEventDirectorUpdate(updateID, param1, param2, param3, param4);
        if (updateID == 0x8000000D)
        {
            if (param1 == 15) //Ryne restored arena.
            {
                Arena.Bounds = new ArenaBoundsRect(20f, 20f);
                Arena.Center = new(100f, 100f);
            }
            if (param1 == 63) //Leviathan heads removed sides of platform.
            {
                Arena.Bounds = new ArenaBoundsRect(10f, 20f); //center of platform remains.
                Arena.Center = new(100f, 100f);
            }
            if (param1 == 1984) //Leviathan has removed the middle of the arena.
            {
                Arena.Bounds = splitArena;
                Arena.Center = splitArena.Center;
            }
        }
    }
}

//Difficulty: These voidzones only have an animation state as far as I can tell. The underlying eventObj persists long after the void zones are safe. We need to make them safe once the corresponding animation has finished.
sealed class MaelstromVoidZones(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.MaelstromVoidZone)
        {
            _aoes.Add(new(circle, actor.Position, actorID: actor.InstanceID));
        }
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.MaelstromVoidZone && state == 0x00040008)
        {
            Remove(actor);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.MaelstromVoidZone)
        {
            Remove(actor);
        }
    }
    private void Remove(Actor actor)
    {
        var id = actor.InstanceID;
        for (var i = _aoes.Count - 1; i >= 0; --i)
        {
            if (_aoes[i].ActorID == id)
                _aoes.RemoveAt(i);
        }
    }
}
sealed class SpinningDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningDive, new AOEShapeRect(64f, 10f)); //I think this is the Tsunami portal charges?

sealed class DrenchingPulseSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.DrenchingPulseSpread, (uint)AID.FreakWave, 5f, 7f);
sealed class CrashingPulseStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.CrashingPulseStack, (uint)AID.KillerWave, 6f, 7f, 4)
{
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        base.OnStatusLose(actor, ref status);
        if (status.ID == (uint)SID.SmotheringWaters)
        {
            Stacks.Clear();
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var count = Stacks.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(new WPos(100f, 80f), 19f), Stacks[i].Activation); // bring stack marker to boss.
        }
    }
}

sealed class TsunamiDonut(BossModule module) : Components.GenericBaitAway(module)
{
    public static readonly AOEShape Donut = new AOEShapeDonut(5, 10);

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SwirlingWaters:
                CurrentBaits.Add(new(actor, actor, Donut, status.ExpireAt));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SwirlingTsunami:
                for (var i = CurrentBaits.Count - 1; i >= 0; --i)
                {
                    if (CurrentBaits[i].Target.InstanceID == spell.MainTargetID)
                        CurrentBaits.RemoveAt(i);
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableHints)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var count = CurrentBaits.Count;
            for (var i = 0; i < count; ++i)
            {
                var b = CurrentBaits[i];
                if (actor == b.Source)
                {
                    hints.AddForbiddenZone(new SDInvertedCircle(new WPos(100f, 84f), 1f), b.Activation); // stack right in front of boss.
                }
            }
        }
    }
}
sealed class SurgingWaters(BossModule module) : Components.GenericKnockback(module)
{
    readonly List<(Actor Source, DateTime Activation)> _sources = [];
    private readonly List<Knockback> _kbs = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbs.Clear();
        for (var i = 0; i < _sources.Count; ++i)
        {
            var s = _sources[i];
            _kbs.Add(new(s.Source.Position, 13f, s.Activation));
        }
        return CollectionsMarshal.AsSpan(_kbs);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SurgingWaters)
            _sources.Add((actor, status.ExpireAt));
    }
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SurgingWaters)
        {
            for (var i = _sources.Count - 1; i >= 0; --i)
            {
                if (_sources[i].Source == actor)
                    _sources.RemoveAt(i);
            }
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        for (var i = 0; i < _sources.Count; ++i)
        {
            var s = _sources[i];
            // knock group straight back to max melee / group stack in center
            hints.AddForbiddenZone(new SDInvertedCircle(actor == s.Source ? new WPos(100f, 81f) : new WPos(100f, 84f), 1f), s.Activation);
        }
    }
}

sealed class TsunamiStackSpread(BossModule module) : Components.GenericStackSpread(module)
{
    public float StackRadius = 6;
    public float SpreadRadius = 6;
    public int MinStackSize = 4;
    public int MaxStackSize = 8;

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SplashingWaters:
                AddSpread(actor, status.ExpireAt);
                break;
            case (uint)SID.SmotheringWaters:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SplashingWaters:
                Spreads.Clear();
                break;
            case (uint)SID.SmotheringWaters:
                Stacks.Clear();
                break;

        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var s in Stacks)
        {
            if (s.Target == actor)
            {
                hints.AddForbiddenZone(new AOEShapeDonut(1, 100), new WPos(100, 84), default, s.Activation); // stack in the donut
            }
        }
    }
}

sealed class E03LeviathanStates : StateMachineBuilder
{
    public E03LeviathanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<RipCurrent>()
            .ActivateOnEnter<TemporaryCurrentRight>()
            .ActivateOnEnter<TemporaryCurrentLeft>()
            .ActivateOnEnter<MonsterWave>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<UnderseaQuakeSides>()
            .ActivateOnEnter<UnderseaQuakeMid>()
            .ActivateOnEnter<Tsunami>()
            .ActivateOnEnter<SurgingWaters>()
            .ActivateOnEnter<TsunamiStackSpread>()
            .ActivateOnEnter<TsunamiDonut>()
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<SpinningDive>()
            .ActivateOnEnter<DrenchingPulseSpread>()
            .ActivateOnEnter<CrashingPulseStack>()
            .ActivateOnEnter<TidalRoar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "ported from upstream and modified by LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 682, NameID = 8486)]
public sealed class E03Leviathan(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 20));
