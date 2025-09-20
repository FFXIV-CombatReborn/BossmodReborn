namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

sealed class IncendiaryBombing(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.IncendiaryBombing, GetVoidzones, 0.6d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzoneSmall);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class IncendiaryBarrage(BossModule module) : Components.VoidzoneAtCastTarget(module, 27f, (uint)AID.IncendiaryBarrage, GetVoidzones, 0.9d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzoneBig);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class IncendiaryBombingBait : Components.GenericBaitAway
{
    private static readonly AOEShapeCircle circle = new(8f);
    private RelSimplifiedComplexPolygon polygon;
    private bool polygonInit;

    public IncendiaryBombingBait(BossModule module) : base(module, centerAtTarget: true)
    {
        IgnoreOtherBaits = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.IncendiaryBombing)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, WorldState.FutureTime(6.1d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IncendiaryBombing)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
        {
            if (!polygonInit)
            {
                polygon = A22SuperiorFlightUnits.ArenaShape.Polygon.Offset(-1.5f);
                polygonInit = true;
            }

            hints.AddForbiddenZone(new SDComplexPolygonInvertedContains(polygon, Arena.Center), CurrentBaits.Ref(0).Activation);
        }
    }
}
