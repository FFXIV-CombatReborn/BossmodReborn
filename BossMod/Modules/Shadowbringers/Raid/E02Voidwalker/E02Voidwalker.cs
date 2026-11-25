namespace BossMod.Shadowbringers.Raid.E02Voidwalker;

public enum OID : uint
{
    Boss = 0x290C,
    Helper = 0x233C,
    VoidwalkerPart = 0x2912, // R0.520, x1, Part type
    Nyx = 0x290D, // R1.503, x0 untargettable adds, don't touch
    TheHandOfErebos = 0x290E, // R24.000, x0 (spawn during fight)
    BleedTower = 0x1EAC9A, // single player towers. causes bleeding while soaking
}

public enum AID : uint
{
    DoomvoidGuillotine = 15931, // Boss->self, 4.0s cast, range 50 width 10 rect
    DarkFireIIINormal = 15939, // Helper->players, no cast, range 8 circle
    UnholyDarknessNormal = 15936, // Helper->players, no cast, range 6 circle
    EntropySummonBleedTowers = 15981, // Boss->self, 6.0s cast, single-target, casts shortly before towers - does it do damage?
    EntropyRaidwide = 15982, // Helper->self, 6.5s cast, range 50 circle
    EmptyHateKnockback = 15941, // 290E->self, 5.5s cast, single-target
    EmptyHateDamage = 15942, // Helper->self, 6.0s cast, range 50 width 40 rect
    PunishingRay = 15943, // Helper->self, no cast, range 80 circle, tower check, small damage if success.
    UnholyDarknessDelayed = 15937, // Helper->player, no cast, single-target
    DarkFireIIIDelayed = 15940, // Helper->player, no cast, single-target
    ShadoweyeGaze = 16567, // Helper->self, no cast, range 100 circle gaze
    ShadoweyeNormal = 16566, //Boss->self
    ShadoweyeDelayed = 16568, // Helper->player, no cast, single-target
    DoomvoidSlicer = 15932, // Boss->self, 4.0s cast, range ?-30 donut
    Shadowflame = 15950, // 2912->player, 4.0s cast, single-target
    Shadowflame1 = 15949, // Boss->player, 4.0s cast, single-target
}

public enum SID : uint
{
    SpellInWaitingUnholyDarkness = 1809, // none->player, extra=0x0
    SpellInWaitingDarkFireIII = 1810, // none->player, extra=0x0
    SpellInWaitingShadoweye = 1812, // none->player, extra=0x0
}

public enum IconID : uint
{
    DarkFireIII = 76, // player->self Fire 3 spread marker, not stored/delayed.
    UnholyDarkness = 62, // player->self  stack on player, not stored/delayed
    ShadowEye = 179, // player->self, gaze
}

sealed class EmptyHateKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.EmptyHateKnockback, 25, kind: Kind.DirForward);

sealed class DarkFireIIINormal(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.DarkFireIII, default, (uint)AID.DarkFireIIINormal, 0, 8, 0);

sealed class UnholyDarknessNormal(BossModule module) : Components.IconStackSpread(module, (uint)IconID.UnholyDarkness, 0, (uint)AID.UnholyDarknessNormal, default, 6, 0, 0);

sealed class ShadowEye(BossModule module) : Components.GenericGaze(module)
{
    public Actor? ShadowEyeTarget;
    public DateTime Activation;
    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
        => ShadowEyeTarget is Actor t && t != actor ? new Eye[1] { new(t.Position, Activation) } : [];
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ShadowEye)
        {
            ShadowEyeTarget = actor;
            Activation = WorldState.FutureTime(6d);
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShadoweyeNormal && ShadowEyeTarget != null)
        {
            Activation = Module.CastFinishAt(spell, 0.9d);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShadoweyeGaze)
        {
            ShadowEyeTarget = null;
        }
    }
}

sealed class SpellInWaitingShadowEye(BossModule module) : Components.GenericGaze(module)
{
    public Actor? ShadowEyeTarget;
    public DateTime Activation;
    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
        => ShadowEyeTarget is Actor t && t != actor ? new Eye[1] { new(t.Position, Activation) } : [];
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingShadoweye)
        {
            ShadowEyeTarget = actor;
            Activation = status.ExpireAt.AddSeconds(0.2d);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShadoweyeGaze && ShadowEyeTarget != null && WorldState.CurrentTime >= Activation.AddSeconds(-2d))
        {
            ShadowEyeTarget = null;
        }
    }
    public override void OnActorDeath(Actor actor)
    {
        if (actor == ShadowEyeTarget)
        {
            ShadowEyeTarget = null;
        }
    }
}

sealed class SpellInWaitingStackSpread(BossModule module) : Components.GenericStackSpread(module)
{
    public float StackRadius = 6;
    public float SpreadRadius = 8;
    public int MinStackSize = 4;
    public int MaxStackSize = 8;

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SpellInWaitingDarkFireIII:
                AddSpread(actor, status.ExpireAt);
                break;
            case (uint)SID.SpellInWaitingUnholyDarkness:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SpellInWaitingDarkFireIII:
                Spreads.RemoveAll(s => s.Target == actor);
                break;
            case (uint)SID.SpellInWaitingUnholyDarkness:
                Stacks.RemoveAll(s => s.Target == actor);
                break;

        }
    }
}

//Source of confusion: There are actually two waves of bleed towers that happen at once, so 5 bleed towers (almost the same as invertible void zones) can amount to 10 actors with different spawn times.
//6 seconds after the first set spawns, another set spawns in the same place, presumably doubling the damage players take? although maybe not.
//approximately 6 seconds after that, the towers must be resolved by the time Punishing Ray is instantly cast.
//we were tracking the tower objects as the target to stand in, but instead, must track the distinct positions of the towers.
//Additional complication: OnActorCreated building a towers list ends up with different orders on different clients.  M1 might get assigned to top left on one client, and middle left on another, despite being M1 on both.
//To be consistent, we need to sort them either by position or instanceID.
//Since we are tracking positions instead of actors, it is better to sort by X,Z, maybe?
sealed class BleedTower(BossModule module) : Components.GenericTowers(module)
{
    public List<Tower> futureTowers = [];
    public DateTime activationTime;

    public float waitForTimeRemaining = 9;

    public float towerLifespanSeconds = 12;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            var towerAlreadyInPosition = false;
            foreach (var tower in futureTowers)
            {
                if (tower.Position.X == actor.Position.X && tower.Position.Z == actor.Position.Z)
                {
                    towerAlreadyInPosition = true;
                }
            }
            if (!towerAlreadyInPosition)
            {
                futureTowers.Add(new(actor.Position, 2f, 1, 1, default, WorldState.FutureTime(towerLifespanSeconds)));
                // keep a deterministic order (by X, then Z) so every client assigns the same tower to the same role
                futureTowers.Sort(static (a, b) => a.Position.X != b.Position.X ? a.Position.X.CompareTo(b.Position.X) : a.Position.Z.CompareTo(b.Position.Z));
            }
            activationTime = DateTime.MaxValue;
            for (var i = 0; i < futureTowers.Count; ++i)
            {
                if (futureTowers[i].Activation < activationTime)
                    activationTime = futureTowers[i].Activation;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            for (var i = Towers.Count - 1; i >= 0; --i)
            {
                if (Towers[i].Position.AlmostEqual(actor.Position, 1f))
                    Towers.RemoveAt(i);
            }
            if (Towers.Count == 0)
            {
                futureTowers.Clear();
            }
        }
    }

    public override void Update()
    {
        base.Update();
        for (var i = 0; i < futureTowers.Count; i++)
        {
            if (Towers.Count <= i)
            {
                if (activationTime < WorldState.FutureTime(waitForTimeRemaining))
                {
                    if (Towers.Count <= i)
                    {
                        Towers.Add(futureTowers[i]);
                    }
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count == 0 || !EnableHints)
            return;

        var assignmentTower = 5; //towers 0-4 are assigned for role responsibility, the other 3 players can try to fill in any gaps.
        switch (assignment)
        {
            case PartyRolesConfig.Assignment.MT: assignmentTower = 0; break;
            case PartyRolesConfig.Assignment.OT: assignmentTower = 1; break;
            case PartyRolesConfig.Assignment.M1: assignmentTower = 2; break;
            case PartyRolesConfig.Assignment.M2: assignmentTower = 3; break;
            case PartyRolesConfig.Assignment.R1: assignmentTower = 4; break;
        }

        // we consider some list of towers part of the same "group" if their activations are within 500ms, as there can be varying delays between helper actors in an encounter casting the "same" spell
        // generally, successive towers that are meant to be soaked by one player (i.e. in quest battles) activate no more frequently than about 2 seconds apart, so this is pretty conservative
        var count = Towers.Count;
        var firstActivation = DateTime.MaxValue;
        for (var i = 0; i < count; ++i)
        {
            if (Towers[i].Activation < firstActivation)
                firstActivation = Towers[i].Activation;
        }
        var deadline = firstActivation.AddSeconds(0.5d);

        var soakingPlayers = new BitMask();
        var party = Raid.WithSlot();

        // first see if we have one or more towers we need to soak - if so, add hints to take one of them
        // if there are no towers to soak, add hints to avoid forbidden ones
        // note that if we're currently inside a tower that has min number of soakers, we can't leave it
        List<ShapeDistance> zones = [];
        List<ShapeDistance> forbiddenZones = [];
        var haveTowersToSoak = false;
        for (var ti = 0; ti < count; ++ti)
        {
            var t = Towers[ti];
            if (t.Activation > deadline)
                continue;
            if (ti == assignmentTower || assignmentTower == 5) // should be 5 towers, 3 players should remain to flex soak
            {
                for (var p = 0; p < party.Length; ++p)
                {
                    if (t.IsInside(party[p].Item2))
                        soakingPlayers.Set(party[p].Item1);
                }

                var effNumSoakers = t.ForbiddenSoakers[slot] ? int.MaxValue : t.NumInside(Module);
                if (effNumSoakers < t.MinSoakers || effNumSoakers == t.MinSoakers && t.IsInside(actor))
                {
                    // this tower needs to be soaked; if this is the first one, clear out any previously found towers to avoid
                    if (!haveTowersToSoak)
                    {
                        zones.Clear();
                        haveTowersToSoak = true;
                    }
                    zones.Add(new SDCircle(t.Position, 2f));
                }
                else if (effNumSoakers > t.MaxSoakers && !haveTowersToSoak)
                {
                    // this tower needs to be avoided; if we already have towers to soak, do nothing - presumably soaking other tower will automatically avoid this one
                    zones.Add(new SDCircle(t.Position, 2f));
                }
                else if (t.ForbiddenSoakers[slot])
                    forbiddenZones.Add(new SDCircle(t.Position, 2f));
            }
        }
        if (zones.Count > 0)
        {
            hints.AddForbiddenZone(haveTowersToSoak ? new SDInvertedUnion([.. zones]) : new SDUnion([.. zones]), firstActivation);
        }
        if (forbiddenZones.Count > 0)
        {
            hints.AddForbiddenZone(new SDUnion([.. forbiddenZones]), firstActivation);
        }
        if (soakingPlayers.Any() && DamageType != AIHints.PredictedDamageType.None)
            hints.AddPredictedDamage(soakingPlayers, firstActivation, DamageType);
    }
}
sealed class BleedTowerDanger(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(2f);
    private readonly List<AOEInstance> _aoes = [];
    public struct TowerPosition(WPos position, DateTime activation = default)
    {
        public WPos Position = position;
        public DateTime Activation = activation;
    }

    public DateTime Activation;

    public List<TowerPosition> TowerPositions = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            var alreadyKnownTowerPosition = false;
            foreach (var towerPosition in TowerPositions)
            {
                if (actor.Position.X == towerPosition.Position.X && actor.Position.Z == towerPosition.Position.Z)
                {
                    alreadyKnownTowerPosition = true;
                }
            }
            if (!alreadyKnownTowerPosition)
            {
                TowerPositions.Add(new TowerPosition(actor.Position, WorldState.FutureTime(9d)));
                Activation = DateTime.MaxValue;
                for (var i = 0; i < TowerPositions.Count; ++i)
                {
                    if (TowerPositions[i].Activation < Activation)
                        Activation = TowerPositions[i].Activation;
                }
            }
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.BleedTower)
        {
            TowerPositions.Clear();
        }
    }
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var now = WorldState.CurrentTime;
        for (var i = 0; i < TowerPositions.Count; ++i)
        {
            if (now < TowerPositions[i].Activation)
                _aoes.Add(new(Shape, TowerPositions[i].Position));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    //need to clear any forbidden zones associated with no-longer ActiveAOES.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation > WorldState.CurrentTime)
        {
            var preActivation = Activation.AddSeconds(-3f);
            if (WorldState.CurrentTime < preActivation)
            {
                foreach (var t in TowerPositions)
                {
                    hints.AddForbiddenZone(new SDCircle(t.Position, 2f), WorldState.CurrentTime); // void zone, active until it's not, don't try to pass through.
                    hints.AddForbiddenZone(new SDCircle(t.Position, 2f), preActivation); // perhaps this will signal to the AI that it WILL be "safe" to enter? We need the AI to make their way to their tower, even though it's a void zone.
                }
            }
        }
    }
}

sealed class DoomvoidGuillotine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoomvoidGuillotine, new AOEShapeRect(50f, 5f));
sealed class EntropyRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.EntropyRaidwide);
sealed class EmptyHateDamage(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyHateDamage);
sealed class DoomvoidSlicer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoomvoidSlicer, new AOEShapeDonut(5f, 30));
sealed class Shadowflame(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Shadowflame);
sealed class Shadowflame1(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Shadowflame1);

//Nyx radius seems more like 2, but janky
sealed class Nyx(BossModule module) : Components.Voidzone(module, 4f, m => m.Enemies((uint)OID.Nyx))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var nyxes = Module.Enemies((uint)OID.Nyx);
        var count = nyxes.Count;
        for (var i = 0; i < count; ++i)
        {
            var n = nyxes[i];
            hints.AddForbiddenZone(new SDRect(n.Position, n.Rotation, 7f, 4f, 2f));
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class E02VoidwalkerStates : StateMachineBuilder
{
    public E02VoidwalkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmptyHateKnockback>()
            .ActivateOnEnter<DarkFireIIINormal>()
            .ActivateOnEnter<UnholyDarknessNormal>()
            .ActivateOnEnter<DoomvoidGuillotine>()
            .ActivateOnEnter<EntropyRaidwide>()
            .ActivateOnEnter<EmptyHateDamage>()
            .ActivateOnEnter<DoomvoidSlicer>()
            .ActivateOnEnter<Shadowflame>()
            .ActivateOnEnter<Shadowflame1>()
            .ActivateOnEnter<Nyx>()
            .ActivateOnEnter<SpellInWaitingStackSpread>()
            .ActivateOnEnter<BleedTower>()
            .ActivateOnEnter<BleedTowerDanger>()
            .ActivateOnEnter<ShadowEye>()
            .ActivateOnEnter<SpellInWaitingShadowEye>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "AndMyAxe (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 684, NameID = 8382)]
public sealed class E02Voidwalker(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(15f, 20f));
