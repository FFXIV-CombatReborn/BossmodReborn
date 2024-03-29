// CONTRIB: made by legendoficeman
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Heavensword.DeepDungeons.PalaceoftheDead.Floors61to70.D70Taquaru
{
    public enum OID : uint
    {
        Boss = 0x1815, // R5.750, x1
        Actor1e86e0 = 0x1E86E0, // R2.000, x1, EventObj type
        Actor1e9998 = 0x1E9998, // R0.500, EventObj type, spawn during fight
        Helper = 0x233C, // R0.500, x12, 523 type
    };

    public enum AID : uint
    {
        AutoAttack = 6497, // Boss->player, no cast, single-target
        Douse = 7091, // Boss->self, 3.0s cast, range 8 circle
        Drench = 7093, // Boss->self, no cast, range 10+R ?-degree cone 
        Electrogenesis = 7094, // Boss->location, 3.0s cast, range 8 circle
        FangsEnd = 7092, // Boss->player, no cast, single-target
    };

    class Douse : Components.PersistentVoidzoneAtCastTarget // need to add a hint if the boss has haste to drag it out of the puddle here
    {
        public Douse() : base(8, ActionID.MakeSpell(AID.Douse), m => m.Enemies(OID.Actor1e9998).Where(z => z.EventState != 7), 0.8f) { }
    }

    class Drench : Components.BaitAwayCast // Can just leave this as it is, need to look into a cast counter as well potentionally. 
    {
        public Drench() : base(ActionID.MakeSpell(AID.Drench), new AOEShapeCone(11, 45.Degrees())) { }
    }

    class Electrogenesis : Components.LocationTargetedAOEs
    {
        public Electrogenesis() : base(ActionID.MakeSpell(AID.Electrogenesis), 8, "Get out of the AOE") { }
    }

    

    class D70TaquaruStates : StateMachineBuilder
    {
        public D70TaquaruStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Douse>()
                .ActivateOnEnter<Drench>()
                .ActivateOnEnter<Electrogenesis>();
        }
    }

    [ModuleInfo(CFCID = 204, NameID = 5309)]
    public class D70Taquaru : BossModule
    {
        public D70Taquaru(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-300, -220), 25)) { }

    }
}