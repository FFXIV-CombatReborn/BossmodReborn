namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.StrixPiece;

public enum OID : uint
{
    StrixPiece = 0x4CB6,
    Helper = 0x233C,
    TomePiece = 0x4CB7, // R1.050, x8
    Puddle = 0x1EC0E0, // R0.500, x0 (spawn during fight), EventObj type
    Duck = 0x1EC0E1, // R0.500, x0 (spawn during fight), EventObj type
    Gravity = 0x1E9582, // R0.500, x0 (spawn during fight), EventObj type
    StrixPlume = 0x4CB8, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttackAero = 50929, // StrixPiece->player, no cast, single-target
    PlummetBoss = 48654, // StrixPiece->self, 3.0s cast, single-target
    PlummetVisual = 48655, // Helper->self, 7.0+0.5s cast, single-target
    Plummet = 48656, // Helper->self, 7.5s cast, range 10 circle
    UltimateFocus = 48668, // StrixPiece->self, 3.0s cast, single-target
    OnThePropertiesOfDarkness = 48669, // StrixPiece->self, 8.0s cast, range 100 circle
    WindfeatherWhisper = 48666, // StrixPiece->self, 3.0s cast, single-target
    AeroIII = 48667, // 4CB8->self, 12.0s cast, range 50 circle
    CheckOutBoss = 48661, // StrixPiece->self, 3.0s cast, single-target
    Unknown = 48641, // 4CB7->self, no cast, single-target - most likely teleport or something
    OverdueVisual = 48664, // 4CB7->self, 6.5+0.5s cast, single-target
    Overdue = 48665, // Helper->self, 7.0s cast, range 15 circle
    OnThePropertiesOfQuakes = 48657, // StrixPiece->self, 8.0s cast, range 60 circle
    OnThePropertiesOfFloods = 48658, // StrixPiece->self, 8.0s cast, range 60 circle
    MagicalMalletTheory = 48659, // StrixPiece->self, 8.0s cast, single-target
    MagicalMalletTheoryAOE = 48660, // Helper->player, no cast, range 8 circle
}

public enum SID : uint
{
    MagicDamageUp = 5020, // StrixPiece->StrixPiece, extra=0x0
    Levitation = 12, // none->player, extra=0x0
    Imp = 1134, // none->player, extra=0x30
    Transfiguration = 1608, // none->player, extra=0x1D3
    DownForTheCount = 3908, // Helper->player, extra=0xEC7
}

sealed class UltimateFocus(BossModule module) : Components.Dispel(module, (uint)SID.MagicDamageUp, (uint)AID.UltimateFocus);
sealed class OnThePropertiesOfDarkness(BossModule module) : Components.RaidwideCast(module, (uint)AID.OnThePropertiesOfDarkness);

sealed class Plummet : Components.SimpleAOEs
{
    public Plummet(BossModule module) : base(module, (uint)AID.Plummet, 10.0f, maxCasts: 6)
    {
        MaxDangerColor = 3;
    }
}

sealed class AeroIII(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AeroIII, 25f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}
sealed class StrixPlume(BossModule module) : Components.Adds(module, (uint)OID.StrixPlume, 2)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Actors.Count == 0)
        {
            return;
        }

        if (Actors[0].IsDead)
        {
            return;
        }

        hints.Add("Kill the Plume!");
    }
}

// Leaving this voidzone will instantly remove the buff
sealed class OnThePropertiesOfQuakes(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 6f, GetVoidzones,
    (uint)AID.OnThePropertiesOfQuakes)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Gravity);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

// Entering this voidzone will give you a buff with a timer
sealed class OnThePropertiesOfFloods(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 6f, GetVoidzones,
    (uint)AID.OnThePropertiesOfFloods)
{
    private BitMask affectedPlayers;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Imp && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = true;
            InvertResolveAt = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (affectedPlayers[slot])
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Duck);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

// Entering this voidzone will give you a buff with a timer
sealed class MagicalMalletTheory(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 6f, GetVoidzones, (uint)AID.MagicalMalletTheory)
{
    private BitMask affectedPlayers;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Transfiguration && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = true;
            InvertResolveAt = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (affectedPlayers[slot])
        {
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MagicalMalletTheoryAOE)
        {
            InvertResolveAt = default;
        }
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Puddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class Overdue : Components.SimpleAOEs
{
    public Overdue(BossModule module) : base(module, (uint)AID.Overdue, 15.0f, maxCasts: 4)
    {
        MaxDangerColor = 2;
    }
}

sealed class StrixPieceStates : StateMachineBuilder
{
    public StrixPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<OnThePropertiesOfQuakes>()
            .ActivateOnEnter<OnThePropertiesOfFloods>()
            .ActivateOnEnter<MagicalMalletTheory>()
            .ActivateOnEnter<UltimateFocus>()
            .ActivateOnEnter<OnThePropertiesOfDarkness>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<StrixPlume>()
            .ActivateOnEnter<Overdue>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.StrixPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14596u, SortOrder = 1)]
[PreferredKinship(BST.Kinship.Wave, "Remove Magic Damage Up from Ultimate Focus")]
[PreferredKinship(BST.Kinship.Vile, "Ignore Aero III's knockback")]
public sealed class StrixPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.StrixPlume => 2,
                (uint)OID.StrixPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.StrixPlume));
    }
}
