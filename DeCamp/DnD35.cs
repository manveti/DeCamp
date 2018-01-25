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
        public DnD35CombatEvent(String creator, EventResult parent = null) : base(creator, parent) { }
        //...
    }

    class DnD35 : Ruleset {
        public DnD35() {
            this.events["Combat"] = (creator, parent) => new DnD35CombatEvent(creator, parent);
        }

        public override Character newCharacter() {
            return new DnD35Character();
        }
    }
}
