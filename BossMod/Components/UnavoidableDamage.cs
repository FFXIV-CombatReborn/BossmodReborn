namespace BossMod.Components;

// generic unavoidable raidwide, started and finished by a single cast
public abstract class RaidwideCast(BossModule module, uint aid, string hint = "Raidwide", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : CastHint(module, aid, hint)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            base.AddGlobalHints(actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            return;

        var affected = AffectedPlayers();
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            if (affected != default)
            {
                hints.AddPredictedDamage(affected, Module.CastFinishAt(Casters[i].CastInfo));
            }
        }
    }

    protected BitMask AffectedPlayers()
    {
        BitMask affected = default;
        var raid = Raid.WithSlot();
        var count = raid.Length;
        for (var i = 0; i < count; ++i)
        {
            var p = raid[i];
            if (ArenaProjectionLayerParticipantApplies(p.Item2, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                affected.Set(p.Item1);
            }
        }
        return affected;
    }
}

public abstract class RaidwideCasts(BossModule module, uint[] aids, string hint = "Raidwide", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    : RaidwideCast(module, default, hint, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Add(caster);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Remove(caster);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                ++NumCasts;
            }
        }
    }
}

// generic unavoidable raidwide, initiated by a custom condition and applied by an instant cast after a delay
public abstract class RaidwideInstant(BossModule module, uint aid, double delay = default, string hint = "Raidwide", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : CastCounter(module, aid)
{
    public readonly double Delay = delay;
    public readonly string Hint = hint;
    public DateTime Activation; // default if inactive, otherwise expected cast time
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Activation != default && Hint.Length > 0 && ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            hints.Add(Hint);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default && ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            BitMask affected = default;
            var raid = Raid.WithSlot();
            var count = raid.Length;
            for (var i = 0; i < count; ++i)
            {
                var p = raid[i];
                if (ArenaProjectionLayerParticipantApplies(p.Item2, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    affected.Set(p.Item1);
                }
            }
            if (affected != default)
            {
                hints.AddPredictedDamage(affected, Activation);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Activation = default;
        }
    }
}

// generic unavoidable instant raidwide initiated by a cast (usually visual-only)
public abstract class RaidwideCastDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Raidwide", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    : RaidwideInstant(module, actionAOE, delay, hint, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public uint ActionVisual = actionVisual;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionVisual)
        {
            Activation = Module.CastFinishAt(spell, Delay);
        }
    }
}

public abstract class RaidwideCastsDelay(BossModule module, uint[] aidsVisual, uint[] aidsAOE, double delay, string hint = "Raidwide", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    : RaidwideCastDelay(module, default, default, delay, hint, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    private readonly uint[] AIDsVisual = aidsVisual;
    private readonly uint[] AIDsAOE = aidsAOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDsVisual.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDsVisual[i])
            {
                Activation = Module.CastFinishAt(spell, Delay);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDsAOE.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDsAOE[i])
            {
                ++NumCasts;
                Activation = default;
            }
        }
    }
}

// generic unavoidable instant raidwide cast initiated by NPC yell
public abstract class RaidwideAfterNPCYell(BossModule module, uint aid, uint npcYellID, double delay, string hint = "Raidwide", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    : RaidwideInstant(module, aid, delay, hint, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public uint NPCYellID = npcYellID;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id == NPCYellID)
        {
            Activation = WorldState.FutureTime(Delay);
        }
    }
}

// generic unavoidable single-target damage, started and finished by a single cast (typically tankbuster, but not necessary)
public abstract class SingleTargetCast(BossModule module, uint aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : CastHint(module, aid, hint)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            base.AddGlobalHints(actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            var castInfo = c.CastInfo;
            if (castInfo != null)
            {
                var target = castInfo.TargetID != c.InstanceID ? castInfo.TargetID : c.TargetID; // assume self-targeted casts actually hit main target
                if (WorldState.Actors.Find(target) is Actor t && ArenaProjectionLayerParticipantApplies(t, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    hints.AddPredictedDamage(new BitMask().WithBit(Raid.FindSlot(target)), Module.CastFinishAt(c.CastInfo), damageType);
                }
            }
        }
    }
}

public abstract class SingleTargetCasts(BossModule module, uint[] aids, string hint = "Tankbuster", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : SingleTargetCast(module, default, hint, AIHints.PredictedDamageType.Tankbuster, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Add(caster);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Remove(caster);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                ++NumCasts;
            }
        }
    }
}

// generic unavoidable single-target damage, initiated by a custom condition and applied by an instant cast after a delay
public abstract class SingleTargetInstant(BossModule module, uint aid, double delay = default, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : CastCounter(module, aid)
{
    public readonly double Delay = delay; // delay from visual cast end to cast event
    public readonly string Hint = hint;
    public readonly List<(int slot, DateTime activation, ulong instanceID, Actor caster, Actor target)> Targets = [];
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Targets.Count != 0 && Hint.Length != 0 && ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            hints.Add(Hint);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var count = Targets.Count;
        if (count == 0)
        {
            return;
        }
        var targets = CollectionsMarshal.AsSpan(Targets);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref targets[i];
            if (ArenaProjectionLayerParticipantApplies(t.target, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                hints.AddPredictedDamage(new BitMask().WithBit(t.slot), t.activation, damageType);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var targets = CollectionsMarshal.AsSpan(Targets);
            var len = targets.Length;
            var id = spell.MainTargetID;
            for (var i = 0; i < len; ++i)
            {
                if (targets[i].instanceID == id)
                {
                    Targets.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// generic unavoidable instant single-target damage initiated by a cast (usually visual-only)
public abstract class SingleTargetCastDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : SingleTargetInstant(module, actionAOE, delay, hint, damageType, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public uint ActionVisual = actionVisual;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionVisual)
        {
            var target = spell.TargetID != caster.InstanceID ? spell.TargetID : caster.TargetID; // assume self-targeted casts actually hit main target
            if (WorldState.Actors.Find(target) is Actor t)
            {
                Targets.Add((Raid.FindSlot(target), Module.CastFinishAt(spell, Delay), target, caster, t));
            }
        }
    }
}

// generic unavoidable instant single-target damage initiated by a cast (usually visual-only)
public abstract class SingleTargetEventDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Tankbuster", int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : SingleTargetInstant(module, actionAOE, delay, hint, AIHints.PredictedDamageType.Tankbuster, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public uint ActionVisual = actionVisual;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == ActionVisual)
        {
            var target = spell.MainTargetID != caster.InstanceID ? spell.MainTargetID : caster.TargetID; // assume self-targeted casts actually hit main target
            if (WorldState.Actors.Find(target) is Actor t)
            {
                Targets.Add((Raid.FindSlot(target), WorldState.FutureTime(Delay), target, caster, t));
            }
        }
    }
}

// generic unavoidable single-target damage, started and finished by a single cast, that can be delayed by moving out of range (typically tankbuster, but not necessary)
public abstract class SingleTargetDelayableCast(BossModule module, uint aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : SingleTargetCastDelay(module, aid, aid, default, hint, damageType, arenaProjectionLayer, restrictToArenaProjectionLayer);

public abstract class SingleTargetDelayableCasts(BossModule module, uint[] aids, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : SingleTargetCastDelay(module, default, default, default, hint, damageType, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                var target = spell.TargetID != caster.InstanceID ? spell.TargetID : caster.TargetID; // assume self-targeted casts actually hit main target
                if (WorldState.Actors.Find(target) is Actor t)
                {
                    Targets.Add((Raid.FindSlot(target), Module.CastFinishAt(spell, Delay), target, caster, t));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                ++NumCasts;
                var targets = CollectionsMarshal.AsSpan(Targets);
                var lenT = targets.Length;
                var tid = spell.MainTargetID;
                for (var j = 0; j < lenT; ++j)
                {
                    if (targets[j].instanceID == tid)
                    {
                        Targets.RemoveAt(j);
                        return;
                    }
                }
            }
        }
    }
}
