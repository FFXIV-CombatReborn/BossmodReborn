namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);
    private static readonly AOEShapeCircle circle = new(15f);
    public static readonly AOEShapeDonut Donut = new(3f, 17f);
    private bool draw = true;
    private bool intake;
    private bool boulders;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => draw ? CollectionsMarshal.AsSpan(AOEs) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (draw || AOEs.Count == 0)
        {
            return;
        }
        var id = spell.Action.ID;
        var delay = id switch
        {
            (uint)AID.ColossalSlam or (uint)AID.ColossalCharge1 or (uint)AID.ColossalCharge2 => 5.4d,
            (uint)AID.ColossalSwing => 5.6d,
            (uint)AID.GigantomillFirstCW or (uint)AID.GigantomillFirstCCW => 5.6d,
            (uint)AID.ColossalLaunch => 6.7d,
            (uint)AID.RunawayRunoff => 4.5d,
            _ => default
        };
        if (delay != default)
        {
            draw = true;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var len = aoes.Length;
            var act = Module.CastFinishAt(spell, delay);
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = act;
            }
            var distance = id switch
            {
                (uint)AID.ColossalCharge1 => -9f,
                (uint)AID.ColossalCharge2 => 9f,
                _ => default
            };
            if (distance != default)
            {
                var pos = Module.PrimaryActor.Position;
                var dir = caster.Rotation.Round(90f).ToDirection();
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    var origin = aoe.Origin;
                    if (origin.InRect(pos, dir, 40f, default, 7f))
                    {
                        // undo quantisation
                        var x = MathF.Round(origin.X);
                        var z = MathF.Round(origin.Z);
                        aoe.Origin = (new WPos(x, z) + distance * dir.OrthoL()).Quantized();
                        return; // never seen more than one keg getting moved
                    }
                }
            }
            if (delay == 6.7d) // colossal launch, switch shapes
            {
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    aoe.Shape = aoe.Shape == circle ? Donut : circle;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.PowderKegRed => circle,
            (uint)OID.PowderKegBlue => Donut,
            _ => null
        };
        if (shape != null)
        {
            AOEs.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(intake ? 11.3d : boulders ? 12.3d : 14.3d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ExplosionCircle or (uint)AID.ExplosionDonut)
        {
            AOEs.Clear();
            draw = false;
            intake = false;
            boulders = false;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x09 && state is 0x00200010u or 0x00020001u)
        {
            draw = true;
            intake = true;
        }
        else if (index == 0x0A && state is 0x00020001u or 0x00200010u or 0x02000100u or 0x00800040u)
        {
            draw = true;
            boulders = true;
        }
    }
}
