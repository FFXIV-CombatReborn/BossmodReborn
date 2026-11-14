namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8f, 20f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CeaselessCurrent1)
        {
            var dir = spell.Rotation.ToDirection();
            Lines.Add(new(spell.LocXZ, 8f * dir, Module.CastFinishAt(spell), 2.1d, 5, 2, dir.Rounded().ToAngle()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CeaselessCurrent1 or (uint)AID.CeaselessCurrent2)
        {
            ++NumCasts;
            var count = Lines.Count;
            var rot = caster.Rotation;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Rotation.AlmostEqual(rot, 0.1f))
                {
                    AdvanceLine(line, line.Next);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
}
