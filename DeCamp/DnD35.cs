using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class DnD35Character : Character {
        //...
    }

    class DnD35CombatEvent : Event {
        public DnD35CombatEvent(String type, String creator, EventResult parent = null) : base(type, creator, parent) { }
        //...
    }

    class DnD35 : Ruleset {
        public DnD35() {
            this.events["Combat"] = new EventImpl(typeof(DnD35CombatEvent), typeof(EventDialog));
        }

        public override Character newCharacter() {
            return new DnD35Character();
        }
    }
}
