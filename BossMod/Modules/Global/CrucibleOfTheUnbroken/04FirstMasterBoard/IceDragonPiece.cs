namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.IceDragonPiece;

public enum OID : uint
{
    IceDragonPiece = 0x4CC0,
    IceSprite = 0x4CC1, // R1.200, x0 (spawn during fight)
    IcePuddle = 0x1E972A, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50784, // IceDragonPiece->none, no cast, single-target
    AutoAttackBlizzard = 48621, // 4CC1->player, no cast, single-target
    Teleport = 48695, // IceDragonPiece->location, no cast, single-target
    IcyTormentBoss = 48696, // IceDragonPiece->self, 5.0+1.5s cast, single-target
    IcyTorment = 48697, // Helper->self, 6.5s cast, range 100 width 15 rect
    RimeWreath = 48709, // IceDragonPiece->self, 5.0s cast, range 100 circle
    IcyTormentCrystalsBoss = 48698, // IceDragonPiece->self, 4.7+1.8s cast, single-target
    IcyTormentCrystals = 48699, // Helper->self, 6.5s cast, range 100 width 15 rect
    WitheringEternityBoss = 48707, // IceDragonPiece->self, 3.0s cast, single-target
    WitheringEternity = 48708, // Helper->location, 5.0s cast, range 9 circle
    SheetOfIceBoss = 48710, // IceDragonPiece->location, 4.5s cast, single-target
    SheetOfIceBossTeleport = 48711, // IceDragonPiece->location, no cast, single-target
    SheetOfIce = 48712, // Helper->location, 5.5s cast, range 5 circle

    Cauterize = 48700, // IceDragonPiece->self, 10.0s cast, single-target
    CauterizeBoss = 48703, // IceDragonPiece->self, no cast, single-target - Fly away
    CauterizeRectVisual = 48701, // Helper->self, 2.0s cast, range 46 width 23 rect
    CauterizeRect = 48704, // Helper->self, 1.0s cast, range 46 width 23 rect
    CauterizeCircleVisual = 48702, // Helper->self, 2.0s cast, range 60 circle
    TouchdownFlyDown = 48705, // IceDragonPiece->self, no cast, single-target
    Touchdown = 48706, // Helper->self, 0.5s cast, range 60 circle
}

public enum SID : uint
{
    Gen = 2552, // none->IceDragonPiece, extra=0x459/0x463
    Freezing = 5177, // none->player, extra=0x1/0x2/0x3/0x4
}

sealed class IcyTorment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcyTorment, new AOEShapeRect(100.0f, 7.5f));
sealed class RimeWreath(BossModule module) : Components.RaidwideCast(module, (uint)AID.RimeWreath, "Raidwide + Applies Frostbite");
sealed class IcyTormentCrystals(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcyTormentCrystals, new AOEShapeRect(100.0f, 7.5f));
sealed class WitheringEternity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WitheringEternity, 9.0f);
sealed class SheetOfIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SheetOfIce, 5.0f);

sealed class IcePuddles(BossModule module) : Components.Voidzone(module, 9.0f, GetVoidzones)
{
    private BitMask affectedPlayers;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Freezing && status.Extra == 0x5 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Freezing && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = false;
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        // If we're at 5 stacks of the debuffs we should avoid going into the puddles anymore
        if (affectedPlayers[slot])
        {
            foreach (var s in Sources(Module))
            {
                if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    hints.TemporaryObstacles.Add(hints.ClipToArenaProjectionLayer(new SDCircle(s.Position, 9.0f), ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer)));
                }
            }

            return;
        }

        // If we're below 5 stacks of the debuff, then it is fine to dodge into puddles for a couple of seconds while baiting the other aoes
        foreach (var s in Sources(Module))
        {
            if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                hints.AddForbiddenZone(Shape, s.Position, s.Rotation, WorldState.FutureTime(4.0f));
            }
        }
    }

    // So we can change the colour from danger to aoe at specific points
    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = _aoesBuffer;
        aoes.Clear();
        foreach (var source in Sources(Module))
        {
            if (ArenaProjectionLayerParticipantApplies(source, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                aoes.Add(new(Shape, source.Position, source.Rotation, color: affectedPlayers[slot] ? Colors.Danger : Colors.AOE, arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.IcePuddle);
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

// TODO consider making a component which accepts knockbacks & aoes these are common
sealed class Cauterize(BossModule module) : BossComponent(module)
{
    private readonly record struct AOE(WPos Origin, Angle Rotation, bool IsKnockback, DateTime Activation);
    private readonly List<AOE> aoes = [];
    private const float knockbackDistance = 30f;
    private readonly AOEShapeRect rect = new(46f, 11.5f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var oid && oid == (uint)AID.CauterizeRectVisual)
        {
            aoes.Add(new(spell.LocXZ, spell.Rotation, false, WorldState.FutureTime(15.1f + 2.2f * aoes.Count)));
        }
        else if (oid == (uint)AID.CauterizeCircleVisual)
        {
            aoes.Add(new(spell.LocXZ, spell.Rotation, true, WorldState.FutureTime(15.1f + 2.2f * aoes.Count)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CauterizeRect or (uint)AID.Touchdown)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return;
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var max = count > 2 ? 2 : count;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            if (aoe.IsKnockback)
            {
                Arena.ZoneCircle(aoe.Origin, 2.0f, Colors.Other7);
            }
            else
            {
                rect.Draw(Arena, aoe.Origin, aoe.Rotation, i == 0 ? Colors.Danger : Colors.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return;
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var max = count > 2 ? 2 : count;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            if (aoe.IsKnockback)
            {
                var endPoint = Components.GenericKnockback.AwayFromSource(pc.Position, aoe.Origin, knockbackDistance);
                Components.GenericKnockback.DrawKnockback(pc, endPoint, Arena);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return;
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var max = count > 2 ? 2 : count;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            if (!aoe.IsKnockback && i == 0 && rect.Check(actor.Position, aoe.Origin, aoe.Rotation))
            {
                hints.Add("GTFO from aoe!");
            }

            if (aoe.IsKnockback)
            {
                var endPoint = Components.GenericKnockback.AwayFromSource(actor.Position, aoe.Origin, knockbackDistance);
                if (!Arena.InBounds(endPoint))
                {
                    hints.Add("About to be knocked into wall!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return;
        }

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var max = count > 2 ? 2 : count;

        for (var i = 0; i < max; i++)
        {
            ref var aoe = ref incomingAOEs[i];
            if (aoe.IsKnockback)
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOrigin(Arena.Center, aoe.Origin, knockbackDistance, 19.0f, 19.0f), aoe.Activation);
            }

            if (!aoe.IsKnockback)
            {
                hints.AddForbiddenZone(rect, aoe.Origin, aoe.Rotation, aoe.Activation);
            }
        }
    }
}

sealed class IceDragonPieceStates : StateMachineBuilder
{
    public IceDragonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcyTorment>()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<IcyTormentCrystals>()
            .ActivateOnEnter<WitheringEternity>()
            .ActivateOnEnter<SheetOfIce>()
            .ActivateOnEnter<IcePuddles>()
            .ActivateOnEnter<Cauterize>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.IceDragonPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14606u, SortOrder = 4)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Cauterize's knockback")]
public sealed class IceDragonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.IceSprite => 2,
                (uint)OID.IceDragonPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IceSprite));
    }

    private readonly string[] _prePullHints = [
        "The ice puddles will give stacks of Freezing when standing inside them, once you gain 8 stacks you will be frozen in place",
    ];

    public override string[] PrePullHints => _prePullHints;
}
