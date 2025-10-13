namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class RollingBoulder(BossModule module) : Components.Exaflare(module, new AOEShapeRect(10f, 5f))
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0A)
        {
            var (originZ1, originZ2) = state switch
            {
                0x00020001u => (-15f, 5f),
                0x00200010u => (-15f, 15f),
                0x02000100u => (-5f, 5f),
                0x00800040u => (-5f, 15f),
                _ => default
            };
            if (originZ1 != default)
            {
                var act = WorldState.FutureTime(16.2d);
                var angle = Angle.AnglesCardinals[1];
                var dir = new WDir(default, 5f);
                AddLine(new(15f, originZ1), -2f);
                AddLine(new(-15f, originZ2), 2f);
                void AddLine(WPos origin, float dirAdj) => Lines.Add(new(origin - dir, dir.OrthoL() * dirAdj, act, 0.3d, 4, 4, angle));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RollingBoulder)
        {
            var count = Lines.Count;
            var pos = caster.Position - new WDir(default, 5f);
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}