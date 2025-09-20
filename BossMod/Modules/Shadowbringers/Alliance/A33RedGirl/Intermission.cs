namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class IntermissionArena(BossModule module) : BossComponent(module)
{
    private readonly List<Rectangle> walls = new(8);
    private readonly A33RedGirl bossmod = (A33RedGirl)module;
    private PolygonCustom[] baseArena = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.WhiteWall or (uint)OID.BlackWall)
        {
            walls.Add(new Rectangle(actor.Position, 2f, 1f));
            if (walls.Count == 8)
            {
                baseArena = bossmod.RedSphere!.PosRot.Z switch
                {
                    900f => A33RedGirl.VirusArena1,
                    400f => A33RedGirl.VirusArena2,
                    _ => A33RedGirl.VirusArena3
                };
                ArenaBoundsCustom arena = new(baseArena, [.. walls]);
                Arena.Bounds = arena;
                Arena.Center = arena.Center;
            }
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID is (uint)OID.WhiteWall or (uint)OID.BlackWall)
        {
            var count = walls.Count;
            var pos = actor.Position;
            for (var i = 0; i < count; ++i)
            {
                if (walls[i].Center == pos)
                {
                    walls.RemoveAt(i);
                    break;
                }
            }
            Arena.Bounds = new ArenaBoundsCustom(baseArena, [.. walls], AdjustForHitbox: true);
        }
    }
}

sealed class WaveWhite(BossModule module) : Components.CastHint(module, (uint)AID.WaveWhite, "Be white to avoid damage!");
sealed class WaveBlack(BossModule module) : Components.CastHint(module, (uint)AID.WaveBlack, "Be black to avoid damage!");
sealed class BigExplosion(BossModule module) : Components.CastHint(module, (uint)AID.BigExplosion, "Pylons explode!", true);

sealed class IntermissionAIRotation(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 10f)
{
    private Actor? redSphere;

    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
        {
            return;
        }

        // get player color, must be either white or black
        var isWhite = Player.FindStatus((uint)SID.ProgramFFFFFFF) != null;

        bool? targetIsWhite = primaryTarget.OID switch
        {
            (uint)OID.WhiteWall or (uint)OID.WhitePylon => true,
            (uint)OID.BlackWall or (uint)OID.BlackPylon => false,
            _ => null
        };

        // change color if needed
        if (isWhite == targetIsWhite)
        {
            SwitchColors(isWhite);
        }

        if (redSphere == null)
        {
            var count = Hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var t = Hints.PotentialTargets[i];
                if (t.Actor.OID == (uint)OID.RedSphere)
                {
                    redSphere = t.Actor;
                    break;
                }
            }
        }
        else if (redSphere.CastInfo is ActorCastInfo castInfo) // be same color as boss cast to dodge raidwide
        {
            bool? waveIsWhite = castInfo.Action.ID switch
            {
                (uint)AID.WaveWhite => true,
                (uint)AID.WaveBlack => false,
                _ => null
            };
            if (waveIsWhite != isWhite)
            {
                SwitchColors(isWhite);
            }
        }

        var action = isWhite ? Roleplay.AID.LiminalFireWhite : Roleplay.AID.LiminalFireBlack;
        UseAction(action, Player, facingAngle: Player.AngleTo(primaryTarget));

        void SwitchColors(bool isWhite) => UseAction(isWhite ? Roleplay.AID.F0SwitchToBlack : Roleplay.AID.F0SwitchToWhite, Player, 10f);
    }
}

sealed class IntermissionAIModule(BossModule module) : QuestBattle.RotationModule<IntermissionAIRotation>(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID is (uint)OID.WhitePylon or (uint)OID.BlackPylon ? 2 : e.Actor.TargetID == actor.InstanceID ? 1 : 0;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}
