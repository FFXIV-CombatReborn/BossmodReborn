﻿namespace BossMod.Endwalker.Alliance.A34Eulogia;

class AsAboveSoBelow(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EverfireFirst or AID.OnceBurnedFirst)
        {
            var advance = 6 * spell.Rotation.ToDirection();
            var pos = spell.LocXZ;
            var activation = Module.CastFinishAt(spell);
            // outer lines have 4 explosion only, rest 5
            var numExplosions = (pos - Arena.Center).LengthSq() > 500 ? 4 : 6;
            Lines.Add(new() { Next = pos, Advance = advance, NextExplosion = activation, TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
            Lines.Add(new() { Next = pos, Advance = -advance, NextExplosion = activation, TimeToMove = 1.5f, ExplosionsLeft = numExplosions, MaxShownExplosions = 5 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EverfireFirst:
            case AID.OnceBurnedFirst:
                var dir = caster.Rotation.ToDirection();
                Advance(caster.Position, dir);
                Advance(caster.Position, -dir);
                ++NumCasts;
                break;
            case AID.EverfireRest:
            case AID.OnceBurnedRest:
                Advance(caster.Position, caster.Rotation.ToDirection());
                ++NumCasts;
                break;
        }
    }

    private void Advance(WPos position, WDir dir)
    {
        var index = Lines.FindIndex(item => item.Next.AlmostEqual(position, 1) && item.Advance.Dot(dir) > 5);
        if (index == -1)
        {
            ReportError($"Failed to find entry for {position} / {dir}");
            return;
        }

        AdvanceLine(Lines[index], position);
        if (Lines[index].ExplosionsLeft == 0)
            Lines.RemoveAt(index);
    }
}
