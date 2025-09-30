namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCircle circleDistortion = new(6f), circleShockWave = new(7f);
    private static readonly AOEShapeRect rect = new(3f, 25f);
    public readonly Polygon[] Towers = new Polygon[2];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.PlaceOfPower => circleDistortion,
            (uint)AID.ShockwaveAOE => circleShockWave,
            _ => null
        };
        if (shape != null)
        {
            AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID == (uint)AID.ShockwaveAOE)
        {
            if (Towers[0] == default)
            {
                Towers[0] = new Polygon(caster.Position, 7.5f, 20);
                Arena.Bounds = new ArenaBoundsCustom(A35FalseIdol.BaseSquare, [Towers[0]]);
            }
            else
            {
                Towers[1] = Towers[0] with { Center = caster.Position };
                Arena.Bounds = new ArenaBoundsCustom(A35FalseIdol.BaseSquare, Towers);
                Array.Clear(Towers);
            }
            if (AOEs.Count != 0)
            {
                AOEs.RemoveAt(0);
            }
        }
        else if (id == (uint)AID.Towerfall)
        {
            Arena.Bounds = A35FalseIdol.ArenaP2;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01)
        {
            switch (state)
            {
                case 0x02000100u:
                    AOEs.Clear();
                    if (Arena.Bounds == A35FalseIdol.ArenaP2)
                    {
                        Arena.Bounds = A35FalseIdol.DistortionArena;
                    }
                    else
                    {
                        Arena.Bounds = A35FalseIdol.RedGirlsDistortionArena;
                    }
                    break;
                case 0x00800001u:
                    if (Arena.Bounds == A35FalseIdol.DistortionArena)
                    {
                        Arena.Bounds = A35FalseIdol.ArenaP2;
                    }
                    else
                    {
                        Arena.Bounds = A35FalseIdol.RedGirlsArena;
                    }
                    break;
                case 0x08000001u:
                    Arena.Bounds = A35FalseIdol.ArenaP2;
                    Arena.Center = A35FalseIdol.ArenaCenter;
                    break;
                case 0x80004000u:
                    AOEs.Add(new(rect, new(-700f, -725f), default, WorldState.FutureTime(10d)));
                    break;
                case 0x20001000u:
                    AOEs.Clear();
                    Arena.Bounds = A35FalseIdol.RedGirlsArena;
                    Arena.Center = A35FalseIdol.RedGirlsArena.Center;
                    break;
            }
        }
    }
}
