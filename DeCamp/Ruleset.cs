using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Ruleset {
        protected SortedDictionary<String, Func<String, EventResult, Event>> events = new SortedDictionary<string, Func<String, EventResult, Event>>() {
            { "Generic", (creator, parent) => new Event(creator, parent) }
        };

        public virtual Character newCharacter() {
            return new Character();
        }

        public virtual ICollection<String> getEventTypes() {
            return this.events.Keys;
        }

        public virtual Event newEvent(String type, String creator, EventResult parent = null) {
            if (!this.events.ContainsKey(type)) { throw new ArgumentException("Unknown event type: " + type); }
            return this.events[type].Invoke(creator, parent);
        }

        //...

        //gui stuff
    }

    static class Rulesets {
        public const String defaultRuleset = "Generic";

        private static SortedDictionary<String, Func<Ruleset>> rulesets = new SortedDictionary<string, Func<Ruleset>>() {
            { "D&D 3.5", () => new DnD35() },
            { defaultRuleset, () => new Ruleset() }
        };

        public static ICollection<String> getRulesets() {
            return rulesets.Keys;
        }

        public static Ruleset newRuleset(String ruleset) {
            return (rulesets.ContainsKey(ruleset) ? rulesets[ruleset] : rulesets[defaultRuleset]).Invoke();
        }
    }
}
