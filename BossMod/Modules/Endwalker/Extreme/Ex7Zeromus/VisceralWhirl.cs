namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

// note: apparently there's a slight overlap between aoes in the center, which looks ugly, but at least that's the truth...
class VisceralWhirl(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeNormal = new(29f, 14f);
    private static readonly AOEShapeRect _shapeOffset = new(60f, 14f);

    public bool Active => _aoes.Count != 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.VisceralWhirlRAOE1:
            case (uint)AID.VisceralWhirlLAOE1:
                _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.VisceralWhirlRAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.VisceralWhirlLAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.VisceralWhirlRAOE1 or (uint)AID.VisceralWhirlRAOE2 or (uint)AID.VisceralWhirlLAOE1 or (uint)AID.VisceralWhirlLAOE2)
            _aoes.RemoveAll(a => a.Rotation.AlmostEqual(spell.Rotation, 0.05f));
    }
}

class MiasmicBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MiasmicBlast, new AOEShapeCross(60f, 5f));

class VoidBio(BossModule module) : Components.GenericAOEs(module)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ToxicBubble);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }

    private static readonly AOEShapeCapsule _shape = new(2f, 3f); // TODO: verify explosion radius

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var voidzones = GetVoidzones(Module);
        var count = voidzones.Length;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            ref readonly var vz = ref voidzones[i];
            aoes[i] = new(_shape, vz.Position);
        }
        return aoes;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var voidzones = GetVoidzones(Module);
        var count = voidzones.Length;
        if (count == 0)
            return;
        var forbiddenImminent = new ShapeDistance[count];
        var forbiddenFuture = new ShapeDistance[count];
        var angle = Angle.AnglesCardinals[1];
        for (var i = 0; i < count; ++i)
        {
            ref var h = ref voidzones[i];
            forbiddenFuture[i] = new SDCapsule(h.Position, angle, 3f, 2f);
            forbiddenImminent[i] = new SDCircle(h.Position, 2f);
        }
        hints.AddForbiddenZone(new SDUnion(forbiddenFuture), WorldState.FutureTime(1.5d));
        hints.AddForbiddenZone(new SDUnion(forbiddenImminent));
    }
}

class BondsOfDarkness(BossModule module) : BossComponent(module)
{
    public int NumTethers;
    private readonly int[] _partners = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partners[slot] >= 0)
            hints.Add("Break tether!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _partners[pcSlot] == playerSlot ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = Raid[_partners[pcSlot]];
        if (partner != null)
            Arena.AddLine(pc.Position, partner.Position, Colors.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BondsOfDarkness)
        {
            var slot1 = Raid.FindSlot(source.InstanceID);
            var slot2 = Raid.FindSlot(tether.Target);
            if (slot1 >= 0 && slot2 >= 0)
            {
                ++NumTethers;
                _partners[slot1] = slot2;
                _partners[slot2] = slot1;
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BondsOfDarkness)
        {
            var slot1 = Raid.FindSlot(source.InstanceID);
            var slot2 = Raid.FindSlot(tether.Target);
            if (slot1 >= 0 && slot2 >= 0)
            {
                --NumTethers;
                _partners[slot1] = -1;
                _partners[slot2] = -1;
            }
        }
    }
}

class DarkDivides(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.DivisiveDark)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkDivides)
            Spreads.Clear();
    }
}
