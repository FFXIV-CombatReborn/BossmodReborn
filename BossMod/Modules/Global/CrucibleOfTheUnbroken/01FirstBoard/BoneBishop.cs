namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop;

public enum OID : uint
{
    BoneKnight = 0x4B86, // R0.900, x?
    BoneBishop = 0x4B87, // R0.900, x?
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50784, // BoneKnight->player, no cast, single-target

    Blizzard = 50788, // BoneBishop->player, no cast, single-target
    DeathSpiral = 46867, // BoneBishop->self, 5.0s cast, single-target
    DeathSpiral1 = 46868, // Helper->self, 6.0s cast, range 4-40 donut
    Ossify = 46871, // BoneKnight->self, 8.0s cast, single-target, interruptible, applies Physical Damage Up 
    ForwardGuard = 46864, // BoneKnight->self, 5.0s cast, single-target
    ForwardGuardEnd = 46865, // BoneKnight->self, no cast, single-target
    BlackEruption = 46873, // BoneBishop->self, 5.0+1.0s cast, single-target
    BlackEruption1 = 46874, // Helper->location, 6.0s cast, range 5 circle
    BlackEruption2 = 46900, // Helper->location, 1.5s cast, range 5 circle
    AncientAero = 46869, // BoneBishop->self, 5.0+0.7s cast, single-target
    AncientAero1 = 46870, // Helper->self, 5.7s cast, range 40 width 8 rect
    Skullsplinter = 46872, // BoneKnight->player, 5.0s cast, single-target
    Tumulus = 46866 // BoneKnight->self, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    PhysicalDamageUp = 2074, // BoneKnight->BoneKnight, extra=0x0
    DirectionalParry = 680, // BoneKnight->BoneKnight, extra=0x1
    DirectionalParryHidden = 2552, // BoneKnight->BoneKnight, extra=0x425 : hidden status during directional parry
    Rehabilitation = 1263 // BoneKnight->BoneKnight, extra=0x0
}

sealed class Ossify(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Ossify, showNameInHint: true);
sealed class DeathSpiral(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathSpiral1, new AOEShapeDonut(4f, 40f));
sealed class Skullsplinter(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Skullsplinter);

// Puts a shield in front of himself. Player should have pet snarl why they get behind and beat him up.
sealed class ForwardGuard(BoneBishop module) : Components.DirectionalParry(module, [(uint)OID.BoneKnight])
{
    private bool active;
    private readonly BoneBishop bossModule = module;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DirectionalParry)
        {
            active = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ForwardGuard)
        {
            PredictParrySide(caster.InstanceID, Side.Front);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DirectionalParry)
        {
            active = false;
            UpdateState(actor.InstanceID, 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!active || bossModule.BoneKnight is not Actor boneKnight)
        {
            return;
        }

        if (boneKnight.TargetID == actor.InstanceID)
        {
            hints.FindEnemy(boneKnight)?.PreferShirking = true;
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero1, new AOEShapeRect(40f, 4f));

sealed class Tumulus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tumulus, 6f);

sealed class BlackEruption(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(24)];
    private readonly AOEShapeCircle _circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = aoes.Length;
        var max = count > 4 ? 4 : count;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.BlackEruption2) // ensure pixel perfectness, .Quantized() still introduces rounding errors
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var pos = spell.LocXZ;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (pos.AlmostEqual(aoe.Origin, 1f))
                {
                    aoe.Origin = pos;
                    return;
                }
            }
            ReportError($"Failed to update AOE for location: {pos}"); // not sure if possible, maybe if radius = 22.5 assumption is wrong
        }
        else if (id == (uint)AID.BlackEruption1)
        {
            var loc = spell.LocXZ;
            _aoes.Add(new(_circle, loc, default, Module.CastFinishAt(spell), shapeDistance: _circle.Distance(loc, default)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.BlackEruption2)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var pos = spell.LocXZ;
            for (var i = 0; i < len; ++i)
            {
                if (pos == aoes[i].Origin)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to remove AOE for location: {pos}"); // not sure if possible, maybe if radius = 22.5 assumption is wrong
        }
        else if (_aoes.Count == 1 && id == (uint)AID.BlackEruption1) // actor can die during cast
        {
            _aoes.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BlackEruption1) // only add aoes if cast actually happened
        {
            _aoes.Clear();
            var position = caster.Position;
            var rotation = caster.Rotation;
            var distance = 3f;
            var remaining = 24;
            var castFinish = WorldState.CurrentTime;
            var rays = new (Angle, WDir)[4];
            for (var i = 0; i < 4; ++i)
            {
                var rot = rotation + i * 90f.Degrees();
                rays[i] = new(rot, rot.ToDirection() * distance);
            }

            var center = Arena.Center;
            // 24 aoes in total, if more than 2.5y outside of arena radius line ends and other lines become longer
            for (var i = 1; remaining > 0; ++i)
            {
                var addedThisStep = false;
                var activation = castFinish.AddSeconds(i * 2.5d);

                for (var j = 0; j < 4; ++j)
                {
                    ref var ray = ref rays[j];
                    var loc = (position + ray.Item2 * i).Quantized();

                    if (!loc.InCircle(center, 22.5f))
                    {
                        continue;
                    }
                    _aoes.Add(new(_circle, loc, ray.Item1, activation, shapeDistance: _circle.Distance(loc, default)));

                    addedThisStep = true;

                    if (--remaining == 0)
                    {
                        return;
                    }
                }

                if (!addedThisStep)
                {
                    ReportError($"Failed to place 24 AOEs: only {24 - remaining} positions fit with the current directions and spacing.");
                    return;
                }
            }
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.BoneBishop)
        {
            _aoes.Clear();
        }
    }
}

sealed class BoneBishopStates : StateMachineBuilder
{
    private readonly BoneBishop _module;

    public BoneBishopStates(BoneBishop module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .ActivateOnEnter<Ossify>()
            .ActivateOnEnter<Skullsplinter>()
            .ActivateOnEnter<DeathSpiral>()
            .ActivateOnEnter<ForwardGuard>()
            .ActivateOnEnter<BlackEruption>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<Tumulus>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BoneKnight?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.BoneBishop, Contributors = "wen", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088u, NameID = 14532u, SortOrder = 1)]
[PreferredKinship(BST.Kinship.Soul, "Interrupt Bone Knight's Ossify")]
[PreferredKinship(BST.Kinship.Wave, "Remove Bone Knight's Physical Damage Up if Ossify goes off")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Windburn from Ancient Aero")]
public sealed class BoneBishop(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    public Actor? BoneKnight;

    protected override void UpdatePreModuleActivation()
    {
        BoneKnight ??= GetActor((uint)OID.BoneKnight);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BoneKnight);
    }
}
