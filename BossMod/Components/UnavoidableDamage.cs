namespace BossMod.Components;

// generic unavoidable raidwide, started and finished by a single cast
[SkipLocalsInit]
public class RaidwideCast(BossModule module, uint aid, string hint = "Raidwide") : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(Casters[i].CastInfo));
    }
}

public class RaidwideCasts(BossModule module, uint[] aids, string hint = "Raidwide") : RaidwideCast(module, default, hint)
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
[SkipLocalsInit]
public class RaidwideInstant(BossModule module, uint aid, double delay = default, string hint = "Raidwide") : CastCounter(module, aid)
{
    public readonly double Delay = delay;
    public readonly string Hint = hint;
    public DateTime Activation; // default if inactive, otherwise expected cast time

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Activation != default && Hint.Length > 0)
        {
            hints.Add(Hint);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default)
        {
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Activation);
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
[SkipLocalsInit]
public class RaidwideCastDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Raidwide") : RaidwideInstant(module, actionAOE, delay, hint)
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

[SkipLocalsInit]
public class RaidwideCastsDelay(BossModule module, uint[] aidsVisual, uint[] aidsAOE, double delay, string hint = "Raidwide") : RaidwideCastDelay(module, default, default, delay, hint)
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
[SkipLocalsInit]
public class RaidwideAfterNPCYell(BossModule module, uint aid, uint npcYellID, double delay, string hint = "Raidwide") : RaidwideInstant(module, aid, delay, hint)
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
[SkipLocalsInit]
public class SingleTargetCast(BossModule module, uint aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            if (c.CastInfo != null)
            {
                var target = c.CastInfo.TargetID != c.InstanceID ? c.CastInfo.TargetID : c.TargetID; // assume self-targeted casts actually hit main target
                hints.AddPredictedDamage(new BitMask().WithBit(Raid.FindSlot(target)), Module.CastFinishAt(c.CastInfo), damageType);
            }
        }
    }
}

[SkipLocalsInit]
public class SingleTargetCasts(BossModule module, uint[] aids, string hint = "Tankbuster") : SingleTargetCast(module, default, hint)
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
[SkipLocalsInit]
public class SingleTargetInstant(BossModule module, uint aid, double delay = default, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : CastCounter(module, aid)
{
    public readonly double Delay = delay; // delay from visual cast end to cast event
    public readonly string Hint = hint;
    public readonly List<(int slot, DateTime activation, ulong instanceID, Actor caster, Actor target)> Targets = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Targets.Count != 0 && Hint.Length != 0)
        {
            hints.Add(Hint);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Targets.Count;
        if (count == 0)
        {
            return;
        }
        var targets = CollectionsMarshal.AsSpan(Targets);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref targets[i];
            hints.AddPredictedDamage(new BitMask().WithBit(t.slot), t.activation, damageType);
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
[SkipLocalsInit]
public class SingleTargetCastDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetInstant(module, actionAOE, delay, hint, damageType)
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
[SkipLocalsInit]
public class SingleTargetEventDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Tankbuster") : SingleTargetInstant(module, actionAOE, delay, hint)
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
[SkipLocalsInit]
public class SingleTargetDelayableCast(BossModule module, uint aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetCastDelay(module, aid, aid, default, hint, damageType);

[SkipLocalsInit]
public class SingleTargetDelayableCasts(BossModule module, uint[] aids, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetCastDelay(module, default, default, default, hint, damageType)
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
