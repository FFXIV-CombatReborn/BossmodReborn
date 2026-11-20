namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class HangryHiss(BossModule module) : Components.RaidwideCast(module, (uint)AID.HangryHiss, "Cat enrage!");

sealed class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Mu, (uint)OID.Yan, (uint)OID.FeatherRay, (uint)OID.GimmeCat, (uint)OID.Jabberwock])
{
    public int CountMu;
    public int CountYan;
    public int CountFeatherRay;
    public int CountJabberwock;
    public int CountGimmeCat;
    private int phase = 1;

    private static readonly M06SSugarRiotConfig _config = Service.Config.Get<M06SSugarRiotConfig>();
    private static readonly WPos featherRayNW = new(90f, 95f), featherRayNE = new(110f, 95f), featherRaySW = new(90f, 105f), featherRaySE = new(110f, 105f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.EnablePriorityList)
            return;
        var count = hints.PotentialTargets.Count;
        var config = _config.AddsPriorityOrder;
        Span<int> priorities = stackalloc int[9];

        for (var i = 0; i < 16; ++i)
        {
            var e = config[i];
            var prio = 15 - i;

            switch (e)
            {
                case 0: priorities[0] = prio; break; // Boss
                case 1 when phase == 1: priorities[1] = prio; break; // Mu P1
                case 2 when phase <= 2: priorities[2] = prio; break; // Yan P1
                case 3 when phase <= 2: priorities[3] = prio; break; // Gimme Cat P1
                case 4 when phase is >= 2 and not 4: priorities[1] = prio; break; // Mu P2
                case 5: priorities[4] = prio; break; // FeatherRay NW
                case 6: priorities[5] = prio; break; // FeatherRay NE
                case 7 when phase == 3: priorities[2] = prio; break; // Yan P3
                case 8 when phase == 3: priorities[3] = prio; break; // Gimme Cat P3
                case 9 when phase == 3: priorities[6] = prio; break; // Jabberwock P3
                case 10: priorities[7] = prio; break; // FeatherRay SW
                case 11: priorities[8] = prio; break; // FeatherRay SE
                case 12 when phase == 4: priorities[1] = prio; break; // Mu P4
                case 13 when phase == 4: priorities[3] = prio; break; // Gimme Cat P4
                case 14 when phase == 4: priorities[6] = prio; break; // Jabberwock P4
                case 15 when phase == 4: priorities[2] = prio; break; // Yan P4
            }
        }

        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Jabberwock => priorities[6],
                (uint)OID.GimmeCat => priorities[3],
                (uint)OID.FeatherRay when e.Actor.Position.AlmostEqual(featherRayNW, 1f) => priorities[4],
                (uint)OID.FeatherRay when e.Actor.Position.AlmostEqual(featherRayNE, 1f) => priorities[5],
                (uint)OID.FeatherRay when e.Actor.Position.AlmostEqual(featherRaySW, 1f) => priorities[7],
                (uint)OID.FeatherRay when e.Actor.Position.AlmostEqual(featherRaySE, 1f) => priorities[8],
                (uint)OID.Mu => priorities[1],
                (uint)OID.Yan => priorities[2],
                (uint)OID.Boss => priorities[0],
                _ => 0
            };
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies((uint)OID.Jabberwock), Colors.Danger);
        Arena.Actors(Module.Enemies((uint)OID.FeatherRay), Colors.Other1);
        Arena.Actors(Module.Enemies((uint)OID.Mu), Colors.Other2);
        Arena.Actors(Module.Enemies((uint)OID.GimmeCat), Colors.Object);
        Arena.Actors(Module.Enemies((uint)OID.Yan), Colors.Other4);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1)
        {
            switch (actor.OID)
            {
                case (uint)OID.Mu:
                    ++CountMu;
                    break;
                case (uint)OID.Yan:
                    ++CountYan;
                    break;
                case (uint)OID.Jabberwock:
                    ++CountJabberwock;
                    break;
                case (uint)OID.FeatherRay:
                    ++CountFeatherRay;
                    break;
                case (uint)OID.GimmeCat:
                    ++CountGimmeCat;
                    break;
            }
            var total = CountMu + CountYan + CountJabberwock + CountFeatherRay + CountGimmeCat;
            phase = total switch
            {
                8 => 2,
                11 => 3,
                18 => 4,
                _ => phase
            };
        }
    }
}

class ICraveViolence(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ICraveViolence, 6f);
class OreRigato(BossModule module) : Components.RaidwideCast(module, (uint)AID.OreRigato);
sealed class ReadyOreNot(BossModule module) : Components.RaidwideCast(module, (uint)AID.ReadyOreNot); // ported from upstream

// ported from upstream: show how many Huffy Cat stacks the cats have (3 stacks = cat enrage)
// reads the stack count straight from the cats' status each frame, so it never drifts
sealed class HuffyCatHint(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _cats = module.Enemies((uint)OID.GimmeCat);

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var max = 0;
        var count = _cats.Count;
        for (var i = 0; i < count; ++i)
        {
            var cat = _cats[i];
            if (cat.IsDead)
                continue;
            if (cat.FindStatus((uint)SID.HuffyCat) is ActorStatus status && status.Extra > max)
                max = status.Extra;
        }
        if (max > 0)
            hints.Add($"Cat stacks: {max}");
    }
}
class WaterIIIBait(BossModule module) : Components.BaitAwayTethers(module, 8f, (uint)TetherID.WaterIII, (uint)AID.WaterIII, (uint)OID.FeatherRay, 7.3f);
class WaterIIIVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 9f, (uint)AID.WaterIII, GetVoidzones, 1.6f) // apparently the voidzone is bigger than the AOE?
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.WaterVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class ManxomeWindersnatch(BossModule module) : Components.SingleTargetInstant(module, (uint)AID.ManxomeWindersnatch, 5f)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Targets.Count != 0)
        {
            ref readonly var t = ref Targets.Ref(0);
            hints.Add($"Big hit on {t.target.Name}!");
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ManxomeWindersnatch)
        {
            var jabberwocks = Module.Enemies((uint)OID.Jabberwock);
            var count = jabberwocks.Count;
            for (var i = 0; i < count; ++i)
            {
                var enemy = jabberwocks[i];
                if (!enemy.IsDeadOrDestroyed)
                {
                    Targets.Add((Raid.FindSlot(targetID), WorldState.FutureTime(5d), targetID, enemy, actor));
                    break;
                }
            }
        }
    }

    public override void Update()
    {
        if (Targets.Count != 0)
        {
            ref readonly var t = ref Targets.Ref(0);
            if (t.target.IsDead || t.caster.IsDead)
            {
                Targets.Clear();
            }
        }
    }
}
