namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.CatoblepasPiece;

public enum OID : uint
{
    CatoblepasPiece = 0x4C9B,
    DemonicEyeCircle = 0x4C9C, // R1.500, x0 (spawn during fight)
    DemonicEyeDonut = 0x4C9D, // R1.500, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49682, // CatoblepasPiece->player, no cast, single-target
    BestialRoar = 48504, // CatoblepasPiece->self, 3.0s cast, range 60 circle

    Farburst = 48507, // 4C9D->self, no cast, single-target
    Farburst1 = 48508, // Helper->self, 0.5s cast, range 5-50 donut
    Nearburst = 48505, // 4C9C->self, no cast, single-target
    Nearburst1 = 48506, // Helper->self, 0.5s cast, range 25 circle
    ShiftingGaze = 48509, // CatoblepasPiece->self, 3.0s cast, single-target
    ShiftingGaze1 = 48954, // CatoblepasPiece->4C9D/4C9C, no cast, single-target
    FalseDemonEye = 48510, // Helper->self, no cast, range 100 circle, cast by tethered eye
    _Weaponskill_SinisterGleam = 48511, // CatoblepasPiece->self, 6.0s cast, single-target
    SinisterGleam = 48512, // Helper->self, 6.5s cast, range 60 180.000-degree cone

}

public enum SID : uint
{
    Unknown = 2056, // CatoblepasPiece->4C9D/4C9C, extra=0xAE, possibly eye with gaze
    Petrification = 4891, // Helper->player, extra=0x0

}

public enum TetherID : uint
{
    ShiftingGaze = 195, // 4C9D/4C9C->CatoblepasPiece
}

sealed class BestialRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.BestialRoar);

sealed class SinisterGleam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SinisterGleam, new AOEShapeCone(60f, 90f.Degrees()));

sealed class DemonicEyeCircle(BossModule module) : Components.Voidzone(module, 2f, module => module.Enemies((uint)OID.DemonicEyeCircle).Where(z => z.Renderflags == 0), 2f);

sealed class DemonicEyeDonut(BossModule module) : Components.Voidzone(module, 2f, module => Service.Config.Get<CatoblepasPieceConfig>().PopDonut ? [] : module.Enemies((uint)OID.DemonicEyeDonut).Where(z => z.Renderflags == 0), 2f);

sealed class DemonicEyes(BossModule module) : BossComponent(module)
{
    private readonly List<DemonicEye> _eyes = [];
    private readonly List<DemonicEye> _near = [];
    private readonly List<DemonicEye> _far = [];
    public ReadOnlySpan<DemonicEye> Near => CollectionsMarshal.AsSpan(_near);
    public ReadOnlySpan<DemonicEye> Far => CollectionsMarshal.AsSpan(_far);
    public ReadOnlySpan<DemonicEye> Gazes
    {
        get
        {
            List<DemonicEye> eyes = [];
            var nearcount = _near.Count;
            for (var i = 0; i < nearcount; ++i)
            {
                var eye = _near[i];
                if (eye.HasGaze)
                {
                    eyes.Add(eye);
                }
            }

            var farcount = _far.Count;
            for (var i = 0; i < farcount; ++i)
            {
                var eye = _far[i];
                if (eye.HasGaze)
                {
                    eyes.Add(eye);
                }
            }

            var eyespan = CollectionsMarshal.AsSpan(eyes);
            RefSort.Sort(eyespan, new DemonicEyeActivationComparer());
            return eyespan;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.DemonicEyeCircle or (uint)OID.DemonicEyeDonut)
        {
            _eyes.Add(new(actor, false, actor.Position));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        // all 3 tethers go out after eyes spawn and before any move
        if (tether.ID == (uint)TetherID.ShiftingGaze)
        {
            var count = _eyes.Count;
            for (var i = 0; i < count; ++i)
            {
                var eye = _eyes[i];
                if (eye.Actor == source)
                {
                    eye.HasGaze = true;
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var list = spell.Action.ID switch
        {
            (uint)AID.Nearburst1 => _near,
            (uint)AID.Farburst1 => _far,
            _ => []
        };
        if (list.Count > 0)
        {
            var count = list.Count;
            for (var i = 0; i < count; ++i)
            {
                if (list[i].Actor.Position.AlmostEqual(spell.LocXZ, 1f))
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void Update()
    {
        // all eyes are spawned before 1st one starts moving
        // any better way to determine orb moving CW CCW?
        var count = _eyes.Count;
        if (count == 0)
        {
            return;
        }
        var center = Arena.Center;
        var activation = WorldState.CurrentTime.AddSeconds(19d);
        for (var i = 0; i < count; ++i)
        {
            var eye = _eyes[i];
            var actor = eye.Actor;
            var start = eye.StartPosition;
            var cur = actor.Position;

            if (start.AlmostEqual(cur, 0.5f))
            {
                continue;
            }
            var startrot = (start - center).ToAngle();
            var currot = (cur - center).ToAngle();
            var ccw = startrot.DistanceToAngle(currot).Deg > 0f;

            for (var j = 0; j < 8; ++j)
            {
                var angle = Angle.AnglesFullCompass[j];
                if (startrot.AlmostEqual(angle, 20f.Degrees().Rad))
                {
                    // ring slightly larger than arena
                    var finalPos = center + (angle + 135f.Degrees() * (ccw ? 1f : -1f)).ToDirection() * 21f;
                    if (actor.OID == (uint)OID.DemonicEyeCircle)
                    {
                        _near.Add(new(actor, false, start, finalPos, activation, eye.HasGaze));
                    }
                    else
                    {
                        _far.Add(new(actor, true, start, finalPos, activation, eye.HasGaze));
                    }
                    _eyes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public class DemonicEye(Actor actor, bool isDonut, WPos startPos, WPos endPos = default, DateTime activation = default, bool hasGaze = false)
    {
        public Actor Actor = actor;
        public bool IsDonut = isDonut;
        public WPos StartPosition = startPos;
        public WPos EndPosition = endPos;
        public DateTime Activation = activation;
        public bool HasGaze = hasGaze;
    }

    private readonly struct DemonicEyeActivationComparer : IRefComparer<DemonicEye>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref DemonicEye a, ref DemonicEye b) => a.Activation.CompareTo(b.Activation);
    }
}

sealed class NearBurst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DemonicEyes _eyes = module.FindComponent<DemonicEyes>()!;
    private readonly CatoblepasPieceConfig _config = Service.Config.Get<CatoblepasPieceConfig>();
    private readonly AOEShapeCircle _circle = new(25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var near = _eyes.Near;
        if (near.Length == 0)
        {
            return [];
        }

        // if popping early leave some time until risky so AI can run to donut eye first
        // better way than creating new AOEInstance each time?
        var eye = near[0];
        var risky = !_config.PopDonut || WorldState.CurrentTime > eye.Activation.AddSeconds(-10d);
        return eye.EndPosition == default ? [] : CollectionsMarshal.AsSpan([new AOEInstance(_circle, eye.EndPosition, activation: eye.Activation, risky: risky, actorID: eye.Actor.InstanceID, shapeDistance: _circle.Distance(eye.EndPosition, default))]);
    }
}

sealed class FarBurst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly DemonicEyes _eyes = module.FindComponent<DemonicEyes>()!;
    private readonly CatoblepasPieceConfig _config = Service.Config.Get<CatoblepasPieceConfig>();
    private readonly AOEShapeDonut _donut = new(5f, 50f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_config.PopDonut)
        {
            var far = _eyes.Far;
            if (far.Length == 0)
            {
                return [];
            }

            var eye = far[0];
            return eye.EndPosition == default ? [] : CollectionsMarshal.AsSpan([new AOEInstance(_donut, eye.EndPosition, activation: eye.Activation, actorID: eye.Actor.InstanceID, shapeDistance: _donut.Distance(eye.EndPosition, default))]);
        }

        return [];
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_config.PopDonut)
        {
            base.DrawArenaBackground(pcSlot, pc);
        }
        else
        {
            var far = _eyes.Far;
            if (far.Length != 0)
            {
                var eye = far[0];
                Arena.ZoneCircle(eye.Actor.Position, 1.5f, Colors.SafeFromAOE);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_config.PopDonut)
        {
            base.AddHints(slot, actor, hints);
        }
        else
        {
            if (_eyes.Far.Length != 0)
            {
                hints.Add("Run to donut orb to pop early!", false);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.PopDonut)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
        else
        {
            var far = _eyes.Far;
            if (far.Length != 0)
            {
                // have to be in front to trigger explosion
                var eye = far[0].Actor;
                var position = eye.Position;
                var rotation = eye.Rotation;
                var direction = rotation.ToDirection() * 1.5f;
                var goalpos = position + direction;
                hints.GoalZones.Add(AIHints.GoalSingleTarget(goalpos, 1f, 5f));
            }
        }
    }
}

sealed class FalseDemonEye(BossModule module) : Components.GenericGaze(module, (uint)AID.FalseDemonEye)
{
    // what is best to handle gaze if popping early? 0.5s cast time, actual gaze is castevent
    // want to keep gaze for circles
    private readonly DemonicEyes _eyes = module.FindComponent<DemonicEyes>()!;
    private readonly CatoblepasPieceConfig _config = Service.Config.Get<CatoblepasPieceConfig>();
    private Eye? _gaze = null;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var gazes = _eyes.Gazes;
        var gcount = gazes.Length;
        if (gcount == 0)
        {
            return [];
        }

        var eye = gazes[0];
        if (eye.EndPosition == default)
        {
            return [];
        }

        if (_config.PopDonut)
        {
            if (eye.IsDonut)
            {
                if (_gaze != null)
                {
                    return CollectionsMarshal.AsSpan([_gaze.Value]);
                }
            }
            else
            {
                return CollectionsMarshal.AsSpan([new Eye(eye.EndPosition, eye.Activation, actorID: eye.Actor.InstanceID, eyeCenter: eye.EndPosition)]);
            }
        }
        else
        {
            return CollectionsMarshal.AsSpan([new Eye(eye.EndPosition, eye.Activation, actorID: eye.Actor.InstanceID, eyeCenter: eye.EndPosition)]);
        }

        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_config.PopDonut && spell.Action.ID == (uint)AID.Farburst1 && _eyes.Gazes is var gaze && gaze.Length != 0)
        {
            var eye = gaze[0];
            if (spell.LocXZ.AlmostEqual(eye.Actor.Position, 1f))
            {
                _gaze = new(spell.LocXZ, eyeCenter: IndicatorWorldPos(spell.LocXZ));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_config.PopDonut && spell.Action.ID == (uint)AID.Farburst1 && _gaze != null)
        {
            _gaze = null;
        }
    }
}

sealed class CatoblepasPieceStates : StateMachineBuilder
{
    public CatoblepasPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BestialRoar>()
            .ActivateOnEnter<DemonicEyes>()
            .ActivateOnEnter<DemonicEyeCircle>()
            .ActivateOnEnter<DemonicEyeDonut>()
            .ActivateOnEnter<NearBurst>()
            .ActivateOnEnter<FarBurst>()
            .ActivateOnEnter<FalseDemonEye>()
            .ActivateOnEnter<SinisterGleam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.CatoblepasPiece, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14577u, SortOrder = 3)]
public sealed class CatoblepasPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    private readonly string[] _prePullHints = [
        "After the raidwide, spawns 3x circle orbs and 3x donut orbs. Getting hit will petrify the player",
        "Orbs explode either on reaching the edge of the arena or the player running into it. Pop donuts early if you want",
        "After the 1st time, boss will tether 3 orbs that will have an additional gaze mechanic that petrifies"
    ];

    public override string[] PrePullHints => _prePullHints;
}

[ConfigDisplay(Order = 0x10, Parent = typeof(GlobalConfig))]
public sealed class CatoblepasPieceConfig : ConfigNode
{
    [PropertyDisplay("Try to pop donut AOEs early by running into it")]
    public bool PopDonut = false;
}
