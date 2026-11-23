namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

sealed class SnakingKick(BossModule module) : Components.GenericAOEs(module, (uint)AID.SnakingKick)
{
    private readonly AOEShapeCircle _shape = new(10f);

    private readonly AOEInstance[] _oneAOEInstance = new AOEInstance[1];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        {
            _oneAOEInstance[0] = new(_shape, Module.PrimaryActor.Position);
            return _oneAOEInstance;
        }
    }
}

// snake 'priority' depends on position, CW from N: N is 0, NE is 1, and so on
abstract class PetrifactionCommon(BossModule module) : Components.GenericGaze(module, (uint)AID.PetrifactionAOE)
{
    public int NumEyeCasts;
    public int NumBloodCasts;
    public int NumCrownCasts;
    public int NumBreathCasts;
    protected List<(Actor caster, DateTime activation, int priority)> ActiveGorgons = [];

    private Eye[] _eyesBuffer = [];
    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (ActiveGorgons.Count <= NumCasts)
        {
            return [];
        }

        var gorgons = ActiveGorgons;
        var maxActivation = gorgons[NumCasts].activation.AddSeconds(1d);
        var count = 0;
        var countG = gorgons.Count;
        for (var i = NumCasts; i < countG; ++i)
        {
            if (gorgons[i].activation >= maxActivation)
            {
                break;
            }
            ++count;
        }

        if (count == 0)
        {
            return [];
        }

        var eyesLen = count;
        if (_eyesBuffer.Length < eyesLen)
            _eyesBuffer = new Eye[eyesLen];
        var eyes = _eyesBuffer;
        var index = 0;

        for (var i = NumCasts; i < countG && gorgons[i].activation < maxActivation; ++i)
        {
            var gorgon = gorgons[i];
            var loc = gorgon.caster.Position.Quantized();
            eyes[index++] = new(loc, gorgon.activation, eyeCenter: loc);
        }
        return eyes.AsSpan(0, eyesLen);
    }

    private static List<Actor> GetGorgons(BossModule module)
    {
        var gorgons = module.Enemies((uint)OID.Gorgon);
        var count = gorgons.Count;
        var filtered = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var gorgon = gorgons[i];
            if (!gorgon.IsDead)
                filtered.Add(gorgon);
        }
        return filtered;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var gorgons = GetGorgons(Module);
        var count = gorgons.Count;
        for (var i = 0; i < count; ++i)
            Arena.Actor(gorgons[i], allowDeadAndUntargetable: true);
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Petrifaction)
        {
            var dir = Angle.FromDirection(caster.Position - Arena.Center);
            var priority = (int)MathF.Round((180f - dir.Deg) / 45f) & 7;
            ActiveGorgons.Add((caster, Module.CastFinishAt(spell, 1.1f), priority));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.EyeOfTheGorgon:
                ++NumEyeCasts;
                break;
            case (uint)AID.BloodOfTheGorgon:
                ++NumBloodCasts;
                break;
            case (uint)AID.CrownOfTheGorgon:
                ++NumCrownCasts;
                break;
            case (uint)AID.BreathOfTheGorgon:
                ++NumBreathCasts;
                break;
        }
    }

    public void DrawPetrify(Actor source, bool delayed) => Arena.ZoneConeOutline(source.Position, 0f, 25f, source.Rotation, 45f.Degrees(), delayed ? Colors.Safe : default);
    public void DrawExplode(Actor source, bool delayed) => Arena.ZoneCircleOutline(source.Position, 5f, delayed ? Colors.Safe : default);
}
