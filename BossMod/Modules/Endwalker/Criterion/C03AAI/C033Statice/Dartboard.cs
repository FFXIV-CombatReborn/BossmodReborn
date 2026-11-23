namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

// dartboard layout:
// - inner/outer rings split is at radius 12
// - there are 12 segments total, meaning 30 degrees per segment
// - starting from S (0deg) and CCW (increasing angle), colors are red->blue->yellow on outer segments and yellow->red->blue on inner segments
sealed class Dartboard(BossModule module) : BossComponent(module)
{
    public enum Color { None, Red, Blue, Yellow }

    public int NumCasts;
    public Color ForbiddenColor;
    public BitMask Bullseye;

    private bool SharesColor(Actor actor, Color color)
    {
        foreach (var (i, p) in Raid.WithSlot(true, true, true))
        {
            if (p != actor && Bullseye[i] && PosToColor(p.Position) == color)
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Bullseye[slot] || ForbiddenColor != Color.None)
        {
            var color = PosToColor(actor.Position);
            if (color == ForbiddenColor)
            {
                hints.Add("GTFO from forbidden color!");
            }
            else if (Bullseye[slot] && SharesColor(actor, color))
            {
                hints.Add("Stay on different segments!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Bullseye[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Interesting;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (ForbiddenColor != Color.None)
        {
            DrawSegmentsOfColor(ForbiddenColor, Colors.AOE);
        }
        if (Bullseye[pcSlot])
        {
            var color = PosToColor(pc.Position);
            if (color != ForbiddenColor)
            {
                DrawSegmentsOfColor(color, Colors.SafeFromAOE);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NHomingPattern or (uint)OID.SHomingPattern)
        {
            ForbiddenColor = PosToColor(actor.Position);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BullsEye)
        {
            Bullseye.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NUncommonGroundSuccess or (uint)AID.NUncommonGroundFail or (uint)AID.SUncommonGroundSuccess or (uint)AID.SUncommonGroundFail)
        {
            Bullseye.Reset();
            ++NumCasts;
        }
    }

    public Color DirToColor(Angle dir, bool inner)
    {
        const float inv30 = 1f / 30f;
        var segIndex = (int)Math.Floor(dir.Deg * inv30);
        if (inner)
        {
            --segIndex;
        }
        if (segIndex < 0)
        {
            segIndex += 9;
        }
        return Color.Red + segIndex % 3;
    }

    private Color PosToColor(WPos pos)
    {
        var off = pos - Arena.Center;
        return DirToColor(Angle.FromDirection(off), off.LengthSq() < 144f);
    }

    private void DrawSegmentsOfColor(Color color, uint zoneColor)
    {
        var index = (int)color - 1;
        var dirOut = (15 + index * 30).Degrees();
        for (var i = 0; i < 4; ++i)
        {
            Arena.ZoneCone(Arena.Center, 0f, 12f, dirOut + 30f.Degrees(), 15f.Degrees(), zoneColor);
            Arena.ZoneCone(Arena.Center, 12f, Arena.Bounds.Radius, dirOut, 15f.Degrees(), zoneColor);
            dirOut += 90f.Degrees();
        }
    }
}
