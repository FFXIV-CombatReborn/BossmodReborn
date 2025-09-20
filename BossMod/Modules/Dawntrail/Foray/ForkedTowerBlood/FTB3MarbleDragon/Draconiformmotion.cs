namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class DraconiformMotion(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DraconiformMotion1, (uint)AID.DraconiformMotion2], new AOEShapeCone(60f, 45f.Degrees()));

sealed class DraconiformMotionBait(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(99f, 2f);
    public DateTime Activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation != default)
        {
            var pos = Module.PrimaryActor.Position;
            var dir = new WDir(default, 7.6f);
            var color = Colors.SafeFromAOE;
            return new AOEInstance[2] { new(rect, pos + dir, default, Activation, color), new(rect, pos - dir, 180f.Degrees(), Activation, color) };
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DraconiformMotionVisual)
        {
            Activation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default)
        {
            var aoes = ActiveAOEs(slot, actor);
            var forbidden = new ShapeDistance[2];
            for (var i = 0; i < 2; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                forbidden[i] = aoe.Shape.InvertedDistance(aoe.Origin, aoe.Rotation);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), aoes[0].Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Activation != default)
        {
            var aoes = ActiveAOEs(slot, actor);
            var isInside = false;
            for (var i = 0; i < 2; ++i)
            {
                if (aoes[i].Check(actor.Position))
                {
                    isInside = true;
                    break;
                }
            }
            hints.Add("Bait the cone AOEs!", !isInside);
        }
    }
}
