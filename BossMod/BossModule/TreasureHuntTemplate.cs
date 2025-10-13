namespace BossMod;

// for treasure hunt roulettes
public abstract class THTemplate : BossModule
{
    public THTemplate(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private THTemplate(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 19f, 48)]);
        return (arena.Center, arena);
    }
}
