namespace BossMod.Dawntrail.Quest.TheIcingOnTheCake;

// Valentione's Day 2026
public enum OID : uint
{
    MammetPtissier = 0x48DF, // R0.150, x?
    KupkaKupp = 0x48E0, // R0.300, x?
    Boss = KupkaKupp,

    CakeStand = 0x1EBE61,

    WhippedCreamFrosting = 0x1EBE4D, // 0x56
    ChocolateFrosting = 0x1EBE4E,
    PistachioFrosting = 0x1EBE4F,
    BerryFrosting = 0x1EBE50,
    LemonFrosting = 0x1EBE51,

    WhiteRolanberries = 0x1EBE52,
    Almonds = 0x1EBE53,
    Branchberries = 0x1EBE54,
    Rolanberries = 0x1EBE55,
    SunLemons = 0x1EBE56,

    SugarPearls = 0x1EBE57,
    ChocolateCubes = 0x1EBE58,
    ChocolateFlowers = 0x1EBE59,
    ChocolateHearts = 0x1EBE5A,
    LemonMacarons = 0x1EBE5B,

    MoogleCakeTopper = 0x1EBE5C,
    ChocolateEggCakeTopper = 0x1EBE5D,
    CactuarCakeTopper = 0x1EBE5E,
    HeartCakeTopper = 0x1EBE5F,
    ChocoboCakeTopper = 0x1EBE60, // 0x6A

    Exit = 0x1EB2CB,
}

public enum AID : uint
{
    SugarBlizzard = 44036, // 48DF->self, 3.0s cast, range 15 90-degree cone
}

public enum SID : uint
{
    Transporting = 404 // none->player, extra=0x15
}

sealed class SugarBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SugarBlizzard, new AOEShapeCone(15f, 45f.Degrees()));

// carry ingredients that light up to the cake stand; the mammets must not be attacked
sealed class DecorateCake(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _ingredients = [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 0x56 or > 0x6A)
            return;
        var oid = (uint)OID.WhippedCreamFrosting + (uint)(index - 0x57);
        switch (state)
        {
            case 0x00020001u: // ingredient deactivated
                for (var i = _ingredients.Count - 1; i >= 0; --i)
                {
                    if (_ingredients[i].OID == oid)
                        _ingredients.RemoveAt(i);
                }
                break;
            case 0x00080004u: // ingredient activated
                foreach (var a in WorldState.Actors)
                {
                    if (a.OID == oid)
                    {
                        _ingredients.Add(a);
                        break;
                    }
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = AIHints.Enemy.PriorityForbidden;

        if (actor.FindStatus((uint)SID.Transporting) == null)
        {
            Actor? closest = null;
            var closestDist = float.MaxValue;
            for (var i = 0; i < _ingredients.Count; ++i)
            {
                var ing = _ingredients[i];
                var dist = actor.DistanceToHitbox(ing);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = ing;
                }
            }
            hints.InteractWithTarget = closest;
        }
        else
        {
            foreach (var a in WorldState.Actors)
            {
                if (a.OID == (uint)OID.CakeStand && a.IsTargetable)
                {
                    hints.InteractWithTarget = a;
                    break;
                }
            }
        }
    }
}

sealed class TheIcingOnTheCakeStates : StateMachineBuilder
{
    public TheIcingOnTheCakeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DecorateCake>()
            .ActivateOnEnter<SugarBlizzard>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "croizat, ported from upstream", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1045u, NameID = 14234u)]
public sealed class TheIcingOnTheCake(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(45f))
{
    protected override bool CheckPull() => !PrimaryActor.IsDead;
}
