namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.SphinxPiece;

public enum OID : uint
{
    SphinxPiece = 0x4CFE,
    SquareNumber1 = 0x1EC105, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber2 = 0x1EC106, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber3 = 0x1EC107, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber4 = 0x1EC108, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber5 = 0x1EC109, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber6 = 0x1EC10A, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber7 = 0x1EC10B, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber8 = 0x1EC10C, // R0.500, x0 (spawn during fight), EventObj type
    SquareNumber9 = 0x1EC10D, // R0.500, x0 (spawn during fight), EventObj type
    DodoPiece = 0x4CFF, // R0.600-1.620, x0 (spawn during fight)
    PugilPiece = 0x4D00, // R0.600-1.950, x0 (spawn during fight)
    PukPiece = 0x4D02, // R0.600-1.500, x0 (spawn during fight)
    OpoOpoPiece = 0x4D01, // R0.600-1.400, x0 (spawn during fight)
    ThisBeast = 0x1EC0E3, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49356, // SphinxPiece->player, no cast, single-target
    Teleport = 49357, // SphinxPiece->location, no cast, single-target
    BanishDonutBoss = 49334, // SphinxPiece->self, 5.0s cast, single-target
    BanishDonut = 49335, // Helper->self, 6.0s cast, range 10-60 donut
    BanishConeBoss = 49338, // SphinxPiece->self, 5.0s cast, single-target
    BanishCone = 49339, // Helper->self, 6.0s cast, range 60 180.000-degree cone
    BanishConeBoss1 = 49340, // SphinxPiece->self, 5.0s cast, single-target
    BanishCone2 = 49341, // Helper->self, 6.0s cast, range 60 180.000-degree cone
    BanishCircleBoss = 49336, // SphinxPiece->self, 5.0s cast, single-target
    BanishCircle = 49337, // Helper->self, 6.0s cast, range 18 circle
    NumericRiddleBoss = 49352, // SphinxPiece->self, 4.0s cast, single-target
    NumericRiddle = 49353, // Helper->self, 5.0s cast, range 40 circle
    Assignment = 49354, // SphinxPiece->self, 3.0s cast, single-target
    LostHope = 49342, // SphinxPiece->player, 3.0s cast, single-target
    MnemonicRiddle = 49343, // SphinxPiece->self, 4.0s cast, single-target
    MnemonicRiddle1 = 49344, // Helper->self, 5.0s cast, range 40 circle
    Transfigure = 49345, // SphinxPiece->self, 2.0s cast, single-target
    RiddleSolved = 49348, // SphinxPiece->self, 2.0s cast, single-target
    RiddleSolved1 = 49349, // Helper->self, 3.0s cast, range 50 circle

    // Adds
    AutoAttackOpoOpoPiece = 49680, // 4D01->player, no cast, single-target
    AutoAttackDodoPiece = 49681, // 4CFF->player, no cast, single-target
    AddTeleport = 49346, // 4D01/4CFF/4D00/4D02->location, no cast, single-target
    Unknown = 49347, // 4D01/4CFF/4D00/4D02->self, no cast, single-target

    // These shouldn't happen - only happens when you fail the mechanic
    RiddleFailedBoss = 49350, // SphinxPiece->self, 2.0s cast, single-target
    RiddleFailed = 49351, // Helper->self, 3.0s cast, range 40 circle
    RiddleFailed1 = 49355, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityDown = 2198, // SphinxPiece->SphinxPiece, extra=0x0
    Transfiguration = 1433, // none->4D01/4CFF/4D00/4D02, extra=0x172
    TemporaryMisdirection = 3909, // SphinxPiece->player, extra=0x2D0
    AllEvens = 5148, // none->player, extra=0x0
    AllOdds = 5149, // none->player, extra=0x0
    AllPrime = 5150, // none->player, extra=0x0
    AllThree = 5151, // none->player, extra=0x0
    DamageUp = 5147, // Helper->player, extra=0x0
    Bleeding = 3077, // none->player, extra=0x0
}

public enum IconID : uint
{
    RiddleCountDown = 718, // player->self
    Unknown = 504, // player->self - most likely misdirection
    Unknown1 = 503, // player->self - most likely misdirection as well
}

sealed class BanishDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BanishDonut, new AOEShapeDonut(10f, 60f));
sealed class BanishCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BanishCone, (uint)AID.BanishCone2],
    new AOEShapeCone(60f, 90f.Degrees()));
sealed class BanishCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BanishCircle, 18f);
sealed class NumericRiddle(BossModule module) : Components.RaidwideCast(module, (uint)AID.NumericRiddle);
sealed class MnemonicRiddleRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.MnemonicRiddle1);
sealed class LostHope(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.LostHope);

sealed class Assignment : Components.GenericAOEs
{
    private readonly AOEShapeRect shape = new(6f, 6f, 6f);
    private readonly AOEShapeRect shapeForAI = new(4f, 4f, 4f); // Used to make the AI not stand on the edge of squares
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<List<ActorStatus>> debuffs = [];

    public Assignment(BossModule module) : base(module)
    {
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            debuffs.Add([]);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is < (uint)SID.AllEvens or > (uint)SID.AllThree)
        {
            return;
        }

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
        {
            return;
        }

        debuffs[slot].Add(status);
        debuffs[slot].Sort(static (a, b) => a.ExpireAt.CompareTo(b.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is < (uint)SID.AllEvens or > (uint)SID.AllThree)
        {
            return;
        }

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
        {
            return;
        }

        var actorStatus = status;
        debuffs[slot].RemoveAll(d => d.ID == actorStatus.ID);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();

        if (debuffs[slot].Count == 0)
        {
            return [];
        }

        var sid = debuffs[slot][0].ID;
        for (var i = 1u; i <= 9u; ++i)
        {
            var safe = sid switch
            {
                (uint)SID.AllPrime => i.IsPrime(),
                (uint)SID.AllThree => i % 3 == 0,
                (uint)SID.AllEvens => (i & 1) == 0,
                (uint)SID.AllOdds => (i & 1) != 0,
                _ => false
            };

            if (safe)
            {
                var tile = TileNumber(i);
                if (tile != null)
                {
                    _aoes.Add(new(shape, tile.Position, tile.Rotation, risky: false, color: Colors.SafeFromAOE));
                }
            }
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var count = aoes.Length;
        if (count == 0)
        {
            return;
        }

        var sdfs = new ShapeDistance[count];
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            sdfs[i] = shapeForAI.Distance(aoe.Origin, aoe.Rotation);
        }

        hints.AddForbiddenZone(new SDInvertedUnion(sdfs), debuffs[slot][0].ExpireAt);
    }

    private Actor? TileNumber(uint n)
    {
        var oid = n switch
        {
            1u => (uint)OID.SquareNumber1,
            2u => (uint)OID.SquareNumber2,
            3u => (uint)OID.SquareNumber3,
            4u => (uint)OID.SquareNumber4,
            5u => (uint)OID.SquareNumber5,
            6u => (uint)OID.SquareNumber6,
            7u => (uint)OID.SquareNumber7,
            8u => (uint)OID.SquareNumber8,
            9u => (uint)OID.SquareNumber9,
            _ => 0u
        };

        if (oid == 0u)
        {
            return null;
        }

        var tiles = Module.Enemies(oid);
        return tiles.Count != 0 ? tiles[0] : null;
    }
}

sealed class MnemonicRiddle(BossModule module) : BossComponent(module)
{
    private OID? animal;
    private bool active = false;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x80000027u)
        {
            return;
        }

        switch (param1)
        {
            case 3u:
                animal = OID.DodoPiece; // CLOUD: director update: 2147483687 param1: 3 param2: 2 param3: 14665 param4: 1073884536
                break;
            case 4u:
                animal = OID.PukPiece; // SCALES: director update: 2147483687 param1: 4 param2: 2 param3: 14665 param4: 1073952921
                break;
            case 5u:
                animal = OID.OpoOpoPiece; // BEAST: director update: 2147483687 param1: 5 param2: 2 param3: 14665 param4: 1073761111
                break;
            case 6u:
                animal = OID.PugilPiece; // WATER: director update: 2147483687 param1: 6 param2: 2 param3: 14665 param4: 1073885145
                break;
            case 7u: // SUCCESS: director update: 2147483687 param1: 7 param2: 2 param3: 14665 param4: 1073952921
            case 8u: // FAIL: director update: 2147483687 param1: 8 param2: 2 param3: 14665 param4: 1073761111
                active = false;
                break;
            case 32u:
                active = true; // PICK TARGET: director update: 2147483687 param1: 32 param2: 2 param3: 14665 param4: 1073933173
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (animal == null || !active)
        {
            return;
        }

        var target = Module.Enemies((uint)animal).FirstOrDefault();
        if (target != null)
        {
            Arena.ZoneCircleOutline(target.Position, 1f, Colors.Safe);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (animal == null || !active)
        {
            return;
        }

        var targets = Module.Enemies((uint)animal);
        if (targets.Count == 0)
        {
            return;
        }
        var target = targets[0];

        Actor? closest = null;
        var bestDistance = float.MaxValue;
        foreach (var beast in Module.Enemies((uint)OID.ThisBeast))
        {
            if (!beast.IsTargetable)
            {
                continue;
            }

            var distance = (beast.Position - target.Position).LengthSq();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = beast;
            }
        }

        if (closest != null)
        {
            hints.InteractWithTarget = closest;
            hints.GoalZones.Add(AIHints.GoalSingleTarget(closest.Position, 1f, 5f));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (animal == null || !active)
        {
            return;
        }

        hints.Add("Interact with the actor on the green circle", false);
    }
}

sealed class SphinxPieceStates : StateMachineBuilder
{
    public SphinxPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BanishDonut>()
            .ActivateOnEnter<BanishCone>()
            .ActivateOnEnter<BanishCircle>()
            .ActivateOnEnter<NumericRiddle>()
            .ActivateOnEnter<MnemonicRiddleRaidwide>()
            .ActivateOnEnter<Assignment>()
            .ActivateOnEnter<LostHope>()
            .ActivateOnEnter<MnemonicRiddle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.SphinxPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14665u, SortOrder = 9)]
public sealed class SphinxPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.DodoPiece => 2,
                (uint)OID.PugilPiece => 2,
                (uint)OID.PukPiece => 2,
                (uint)OID.OpoOpoPiece => 2,
                (uint)OID.SphinxPiece => e.Actor.FindStatus((uint)SID.VulnerabilityDown) != null ? AIHints.Enemy.PriorityPointless : 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DodoPiece));
        Arena.Actors(Enemies((uint)OID.PugilPiece));
        Arena.Actors(Enemies((uint)OID.PukPiece));
        Arena.Actors(Enemies((uint)OID.OpoOpoPiece));
        Arena.Actors(Enemies((uint)OID.ThisBeast));
    }
}
