namespace BossMod.Shadowbringers.Hunt.RankS.Ixtab;

public enum OID : uint
{
    Boss = 0x2838 // R=3.24
}

public enum AID : uint
{
    AutoAttack = 17850, // Boss->player, no cast, single-target

    TartareanAbyss = 17848, // Boss->players, 4.0s cast, range 6 circle
    TartareanFlare = 17846, // Boss->location, 4.5s cast, range 18 circle
    TartareanBlizzard = 17845, // Boss->self, 3.0s cast, range 40 45-degree cone
    TartareanFlame = 17999, // Boss->self, 5.0s cast, range 8-40 donut
    TartareanFlame2 = 18074, // Boss->self, no cast, range 8-40 donut
    TartareanThunder = 17843, // Boss->location, 5.0s cast, range 20 circle
    TartareanThunder2 = 18075, // Boss->location, no cast, range 20 circle
    TartareanMeteor = 17844, // Boss->players, 5.0s cast, range 10 circle
    ArchaicDualcast = 18077, // Boss->self, 3.0s cast, single-target, either out/in or in/out with Tartarean Flame and Tartarean Thunder
    Cryptcall = 17847, // Boss->self/players, 3.0s cast, range 35+R 120-degree cone, sets hp to 1, applies heal to full doom with 25s duration
    TartareanQuake = 17849, // Boss->self, 4.0s cast, range 40 circle
    TartareanTwister = 18072 // Boss->self, 5.0s cast, range 55 circle, raidwide + windburn DoT, interruptible
}

public enum SID : uint
{
    Doom = 1769 // Boss->player, extra=0x0
}

class DualCastTartareanFlameThunder(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private static readonly AOEShapeCircle circle = new(20f);
    private static readonly AOEShapeDonut donut = new(8f, 40f);
    private bool dualCast;

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoesLen = count;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, float delay = 0f) => _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
        void AddAOEs(AOEShape shape1, AOEShape shape2)
        {
            AddAOE(shape1);
            AddAOE(shape2, 5.1f);
            dualCast = false;
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.ArchaicDualcast:
                dualCast = true;
                break;
            case (uint)AID.TartareanThunder:
                if (!dualCast)
                    AddAOE(circle);
                else
                    AddAOEs(circle, donut);
                break;
            case (uint)AID.TartareanFlame:
                if (!dualCast)
                    AddAOE(donut);
                else
                    AddAOEs(donut, circle);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.TartareanFlame:
                case (uint)AID.TartareanFlame2:
                case (uint)AID.TartareanThunder:
                case (uint)AID.TartareanThunder2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class TartareanTwister(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TartareanTwister);
class TartareanBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TartareanBlizzard, new AOEShapeCone(40f, 22.5f.Degrees()));
class TartareanQuake(BossModule module) : Components.RaidwideCast(module, (uint)AID.TartareanQuake);

class TartareanAbyss(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TartareanAbyss, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class TartareanFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TartareanFlare, 18f);
class TartareanMeteor(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.TartareanMeteor, 10f, 8);
class ArchaicDualcast(BossModule module) : Components.CastHint(module, (uint)AID.ArchaicDualcast, "Preparing In/Out or Out/In AOE");

class Cryptcall(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Cryptcall, new AOEShapeCone(38.24f, 60f.Degrees()))
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }
    public override void OnEventCast(Actor caster, ActorCastEvent spell) // bait resolves on cast event instead of cast finish
    {
        if (spell.Action.ID == WatchedAction)
            CurrentBaits.Clear();
    }
}

class CryptcallHint(BossModule module) : Components.CastHint(module, (uint)AID.Cryptcall, "Cone reduces health to 1 + applies Doom");

class Doom(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _doomed = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Remove(actor);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomed.Count != 0)
            if (_doomed.Contains(actor))
                if (!(actor.Role == Role.Healer))
                    hints.Add("You were doomed! Get healed to full fast.");
                else
                    hints.Add("Heal yourself to full! (Doom).");
            else if (actor.Role == Role.Healer)
            {
                var count = _doomed.Count;
                for (var i = 0; i < count; ++i)
                {
                    hints.Add($"Heal to full {_doomed[i].Name}! (Doom)");
                }
            }
    }
}

class IxtabStates : StateMachineBuilder
{
    public IxtabStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DualCastTartareanFlameThunder>()
            .ActivateOnEnter<TartareanTwister>()
            .ActivateOnEnter<TartareanBlizzard>()
            .ActivateOnEnter<TartareanQuake>()
            .ActivateOnEnter<TartareanAbyss>()
            .ActivateOnEnter<TartareanFlare>()
            .ActivateOnEnter<TartareanMeteor>()
            .ActivateOnEnter<ArchaicDualcast>()
            .ActivateOnEnter<Cryptcall>()
            .ActivateOnEnter<CryptcallHint>()
            .ActivateOnEnter<Doom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8890)]
public class Ixtab(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
