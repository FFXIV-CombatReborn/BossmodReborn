namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class UltraviolentRay(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UltraviolentRay, onlyShowOutlines: true, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private readonly AOEShapeRect rect = new(40f, 8.5f);
    private readonly M08SHowlingBlade bossmod = (M08SHowlingBlade)module;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.UltraviolentRay)
        {
            CurrentBaits.Add(new(bossmod.BossP2()!, actor, rect, WorldState.FutureTime(6.1d)));
        }
    }

    public override void Update()
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            for (var j = 0; j < 5; ++j)
            {
                var center = ArenaChanges.EndArenaPlatforms[j].Center;
                if (b.Target.Position.InCircle(center, 8f))
                {
                    b.CustomRotation = ArenaChanges.PlatformAngles[j];
                    break;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;

        Angle playerPlatform = default;
        var pos = actor.Position;
        for (var i = 0; i < 5; ++i)
        {
            if (pos.InCircle(ArenaChanges.EndArenaPlatforms[i].Center, 8f))
            {
                playerPlatform = ArenaChanges.PlatformAngles[i];
                break;
            }
        }
        var occupiedPlatforms = new List<Angle>(5);
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            for (var j = 0; j < 5; ++j)
            {
                if (b.Target.Position.InCircle(ArenaChanges.EndArenaPlatforms[j].Center, 8f))
                {
                    var count = occupiedPlatforms.Count;
                    var angle = ArenaChanges.PlatformAngles[j];
                    for (var k = 0; k < count; ++k)
                    {
                        if (occupiedPlatforms[k] == angle && playerPlatform == angle)
                        {
                            hints.Add("More than 1 defamation on your platform!");
                            return;
                        }
                    }
                    occupiedPlatforms.Add(angle);
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var playerBait = ActiveBaitsOn(actor);
        if (playerBait.Count != 0)
        {
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            var len = baits.Length;
            Span<int> playersPerPlatform = stackalloc int[5];
            var pos = actor.Position;
            var playerPlatform = -1;
            for (var i = 0; i < len; ++i)
            {
                ref var b = ref baits[i];
                for (var j = 0; j < 5; ++j)
                {
                    var t = b.Target;
                    if (t.Position.InCircle(ArenaChanges.EndArenaPlatforms[j].Center, 8f))
                    {
                        playersPerPlatform[j] += 1;
                        if (t == actor)
                        {
                            playerPlatform = j;
                        }
                        break;
                    }
                }
            }

            var loc = M08SHowlingBlade.ArenaCenter.Quantized();
            var act = baits[0].Activation;
            for (var i = 0; i < 5; ++i)
            {
                var platform = playersPerPlatform[i];
                if (platform > 1 || platform == 1 && playerPlatform != i)
                {
                    hints.AddForbiddenZone(rect.Distance(loc, ArenaChanges.PlatformAngles[i]), act);
                }
            }
        }
    }
}

sealed class GleamingBeam(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeRect Rect = new(31f, 4f);
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D3 && actor.OID == (uint)OID.GleamingFangP21)
        {
            _aoes.Add(new(Rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(6.1d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GleamingBeam)
        {
            ++NumCasts;
        }
    }
}
