namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE115CursedConcern;

public enum OID : uint
{
    Boss = 0x46E7, // R7.0
    GemBlue = 0x46E9, // R4.0
    GemRed = 0x46E8, // R5.0
    Shell = 0x46EA, // R1.8
    OneGlintingCoin = 0x1EBD3E, // R0.5
    TwoGlintingCoins = 0x1EBD3F, // R0.5
    FourGlintingCoins = 0x1EBD40, // R0.5
    Deathwall = 0x1EBD37, // R0.5
    DeathwallHelper = 0x4865, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Deathwall = 41535, // DeathwallHelper->self, no cast, range 25-30 donut

    IllGottenGoods = 41518, // Boss->self, 3.0s cast, single-target
    MaterialWorld = 41517, // Boss->self, 4.0s cast, single-target
    EarthquakeVisual = 41528, // Boss->self, 5.0s cast, single-target, raidwide
    Earthquake = 41529, // Helper->self, no cast, ???

    RecommendedForYou = 41993, // Boss->self, 4.0s cast, single-target
    WhatreYouBuyingVisual1 = 41515, // Boss->self, 20.0s cast, single-target
    WhatreYouBuyingVisual2 = 43452, // Boss->self, 25.0s cast, single-target
    WhatreYouBuying = 41516, // Helper->self, no cast, ???, damage if failed coin mechanic

    OnepennyInflation = 41521, // Boss->self, 7.0s cast, single-target
    TwopennyInflation = 43285, // Boss->self, 7.0s cast, single-target
    FourpennyInflation = 41520, // Boss->self, 7.0s cast, single-target
    CostOfLiving = 41522, // Helper->self, 7.0s cast, ???, knockback 30, away from source

    WaterspoutVisual = 41526, // Boss->self, 5.0s cast, single-target
    Waterspout = 41527, // Helper->self, 8.3s cast, range 12 circle
    HoardWealth = 43372 // Helper->self, 6.0s cast, range 40 60-degree cone
}

public enum TetherID : uint
{
    OneCoin = 328, // Boss->GemBlue/GemRed/Shell
    TwoCoins = 329, // Boss->GemBlue/GemRed/Shell
    ThreeCoins = 330, // Boss->GemBlue/GemRed/Shell
    FourCoins = 331, // Boss->GemBlue/GemRed/Shell
    FiveCoins = 332, // Boss->GemBlue/GemRed/Shell
    SixCoins = 333 // Boss->GemBlue/GemRed/Shell
}

public enum SID : uint
{
    BuyersRemorseExtremeCaution = 4342, // none->player, extra=0x0
    BuyersRemorseDeepFreeze = 4343, // none->player, extra=0x0
    BuyersRemorseForcedMarch = 4344, // none->player, extra=0x0
    Transporting = 4376 // none->player, extra=0x2B/0x2A/0x29/0x2E/0x2C/0x2F/0x2D/0x30/0x31
}

public enum IconID : uint
{
    Success = 503, // player->self
    Fail = 504 // player->self
}

sealed class Earthquake(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.EarthquakeVisual, (uint)AID.Earthquake, 0.9f);
sealed class Waterspout : Components.SimpleAOEs
{
    public Waterspout(BossModule module) : base(module, (uint)AID.Waterspout, 12f, 8)
    {
        MaxDangerColor = 1;
    }
}
sealed class HoardWealth(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HoardWealth, new AOEShapeCone(40f, 30f.Degrees()), 3);

sealed class CostOfLiving(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.CostOfLiving, 30f, maxCasts: 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                // circle intentionally slightly smaller to prevent sus knockback
                hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOrigin(Arena.Center, c.Origin, 30f, 23f), act);
            }
        }
    }
}

sealed class BuyersRemorseForcedMarch(BossModule module) : Components.GenericKnockback(module)
{
    private DateTime activation;
    private BitMask affectedPlayers;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (affectedPlayers[slot])
            return new Knockback[1] { new(actor.Position, 35f, activation, direction: actor.Rotation, kind: Kind.DirForward, ignoreImmunes: true) };
        return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BuyersRemorseForcedMarch && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            activation = status.ExpireAt;
            affectedPlayers[slot] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.BuyersRemorseForcedMarch && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (affectedPlayers[slot])
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 15f), activation);
            var dir = actor.Position - Arena.Center;
            var len = dir.Length();
            hints.ForbiddenDirections.Add((Angle.FromDirection(dir), Angle.Acos(Math.Clamp(-((len * len + 600f) / (len * 70f)), -1f, 1f)), activation));
        }
    }
}

sealed class BuyersRemorseECFreeze(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.BuyersRemorseExtremeCaution:
                PlayerState stateStay = new(Requirement.Stay, status.ExpireAt);
                SetState(Raid.FindSlot(actor.InstanceID), ref stateStay);
                break;
            case (uint)SID.BuyersRemorseDeepFreeze:
                PlayerState stateMove = new(Requirement.Move, status.ExpireAt);
                SetState(Raid.FindSlot(actor.InstanceID), ref stateMove);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.BuyersRemorseDeepFreeze or (uint)SID.BuyersRemorseExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }
}

sealed class WhatreYouBuying(BossModule module) : Components.GenericAOEs(module)
{
    private readonly (WPos dropofflocation, int required, int current)[] playerData = new (WPos, int, int)[PartyState.MaxPartySize];
    private readonly List<AOEInstance>[] _aoesPerPlayer = new List<AOEInstance>[PartyState.MaxPartySize];
    private static readonly AOEShapeCircle circle = new(7f), circleInv = new(7f, true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => playerData[slot] != default ? CollectionsMarshal.AsSpan(_aoesPerPlayer[slot]) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WhatreYouBuyingVisual1 or (uint)AID.WhatreYouBuyingVisual2)
        {
            var aoes = new AOEInstance[3];
            uint[] towerOIDs = [(uint)OID.GemBlue, (uint)OID.GemRed, (uint)OID.Shell];
            var act = WorldState.FutureTime(10d);
            for (var i = 0; i < 3; ++i)
            {
                var tower = Module.Enemies(towerOIDs[i]);
                if (tower.Count != 0)
                {
                    aoes[i] = new(circle, tower[0].Position, default, act);
                }
            }
            for (var i = 0; i < 8; ++i)
            {
                _aoesPerPlayer[i] = [.. aoes]; // deep copy for each player, otherwise its only copies reference causing issues when playing in a party
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (Raid.FindSlot(source.InstanceID) is var slot && slot is >= 0 and <= 7)
        {
            var price = tether.ID switch
            {
                (uint)TetherID.OneCoin => 1,
                (uint)TetherID.TwoCoins => 2,
                (uint)TetherID.ThreeCoins => 3,
                (uint)TetherID.FourCoins => 4,
                (uint)TetherID.FiveCoins => 5,
                (uint)TetherID.SixCoins => 6,
                _ => default
            };
            if (price != default)
            {
                var target = WorldState.Actors.Find(tether.Target);
                if (target is Actor t)
                {
                    playerData[slot] = new(t.Position, price, default);
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Transporting && Raid.FindSlot(actor.InstanceID) is var slot && slot is >= 0 and <= 7)
        {
            // status extra tells us which coins are being carried
            var coins = status.Extra switch
            {
                0x29 => 1, // 1x 1 coin
                0x2A => 2, // 1x 2 coins
                0x2B => 4, // 1x 4 coins
                0x2C => 2, // 2x 1 coins
                0x2D => 3, // 1x 1 + 1x 2 coins
                0x2E => 4, // 2x 2 coins
                0x2F => 5, // 1x 1 coin + 1x 4 coins
                0x30 => 6, // 1x 2 coints + 1x 4 coins
                0x31 => 8, // 2x 4 coins
                _ => default
            };
            ref var pSlot = ref playerData[slot];
            pSlot.current = coins;
            var delta = pSlot.required - pSlot.current;

            if (delta <= 0)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoesPerPlayer[slot]);
                for (var i = 0; i < 3; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Origin.AlmostEqual(pSlot.dropofflocation, 1f))
                    {
                        aoe.Color = Colors.SafeFromAOE;
                        aoe.Shape = circleInv;
                        return;
                    }
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.Fail or (uint)IconID.Success && Raid.FindSlot(actor.InstanceID) is var slot && slot is >= 0 and <= 7)
        {
            // player finished
            playerData[slot] = default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WhatreYouBuying)
        {
            // mechanic finished
            Array.Clear(playerData);
            Array.Clear(_aoesPerPlayer);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (playerData[pcSlot] is var data && data != default)
        {
            var delta = data.required - data.current;
            if (delta <= 0)
                return;
            var coinOIDs = new (int bit, uint oid)[]
            {
                (4, (uint)OID.FourGlintingCoins),
                (2, (uint)OID.TwoGlintingCoins),
                (1, (uint)OID.OneGlintingCoin)
            };
            for (var i = 0; i < 3; ++i)
            {
                var coinOID = coinOIDs[i];
                if ((delta & coinOID.bit) != 0)
                    Arena.Actors(Module.Enemies(coinOID.oid), Colors.Object);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (playerData[slot] is var data && data != default)
        {
            var delta = data.required - data.current;
            if (delta <= 0)
                return;
            var coinOIDs = new (int bit, uint oid)[]
            {
                (4, (uint)OID.FourGlintingCoins),
                (2, (uint)OID.TwoGlintingCoins),
                (1, (uint)OID.OneGlintingCoin)
            };

            uint coins = 0;
            for (var i = 0; i < 3; ++i)
            {
                var coinOID = coinOIDs[i];
                if ((delta & coinOID.bit) != 0)
                {
                    coins = coinOID.oid;
                    break;
                }
            }

            var coinsE = Module.Enemies(coins);
            Actor? closest = null;
            var minDistSq = float.MaxValue;

            var count = coinsE.Count;
            for (var i = 0; i < count; ++i)
            {
                var coin = coinsE[i];
                if (coin.IsTargetable)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(coin, 2f, 5f));
                    var distSq = (actor.Position - coin.Position).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closest = coin;
                    }
                }
            }
            hints.InteractWithTarget = closest;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (playerData[slot] is var data && data != default)
        {
            var delta = data.required - data.current;
            if (delta <= 0)
                hints.Add("Deliver the coins!", false);
            else
            {
                hints.Add($"Fetch {delta} more coin{(delta == 1 ? "" : "s")}!");
            }
        }
    }
}

sealed class CE115CursedConcernStates : StateMachineBuilder
{
    public CE115CursedConcernStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<HoardWealth>()
            .ActivateOnEnter<CostOfLiving>()
            .ActivateOnEnter<BuyersRemorseECFreeze>()
            .ActivateOnEnter<BuyersRemorseForcedMarch>()
            .ActivateOnEnter<WhatreYouBuying>()
        ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 45)]
public sealed class CE115CursedConcern(WorldState ws, Actor primary) : BossModule(ws, primary, new WPos(72f, -545f).Quantized(), new ArenaBoundsCircle(25f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
