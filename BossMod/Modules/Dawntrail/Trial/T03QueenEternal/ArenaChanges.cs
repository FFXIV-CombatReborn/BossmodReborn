namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00040004u)
        {
            SetArena(T03QueenEternal.DefaultBounds, T03QueenEternal.ArenaCenter);
        }
        else if (state == 0x00020001u)
        {
            switch (index)
            {
                case 0x00: // x arena
                    SetArena(T03QueenEternal.XArena, T03QueenEternal.XArena.Center);
                    break;
                case 0x02: // disjointed rect arena
                    SetArena(T03QueenEternal.SplitArena, T03QueenEternal.SplitArena.Center);
                    break;
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x2BFE) // final phase arena
        {
            SetArena(T03QueenEternal.FinalBounds, T03QueenEternal.FinalCenter);
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }

    public override void Update()
    {
        if (Arena.Center != T03QueenEternal.SplitArena.Center)
            return;
        if (Raid.WithoutSlot(false, true, true).Any(p => p.FindStatus((uint)SID.GravitationalAnomaly) != null))
            SetArena(T03QueenEternal.SplitGravityBounds, T03QueenEternal.SplitArena.Center);
        else if (Raid.WithoutSlot(false, true, true).All(p => p.FindStatus((uint)SID.GravitationalAnomaly) == null))
            SetArena(T03QueenEternal.SplitArena, T03QueenEternal.SplitArena.Center);
    }
}
