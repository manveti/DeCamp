using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace DeCamp {
    public class Ruleset {
        protected class EventImpl {
            private ConstructorInfo evt, dlg;
            public Func<Campaign, Event, Timestamp, String, String, Window, EventDialog> dialog;

            public EventImpl(Type eventType, Type dialogType) {
                this.evt = eventType.GetConstructor(new Type[] { typeof(String), typeof(String), typeof(EventResult) });
                this.dlg = dialogType.GetConstructor(new Type[] { typeof(Campaign), typeof(Event), typeof(Timestamp), typeof(String), typeof(String), typeof(Window) });
            }

            public Event newEvent(String type, String creator, EventResult parent) {
                return (Event)(this.evt.Invoke(new object[] { type, creator, parent }));
            }

            public EventDialog newDialog(Campaign campaign, Event evt, Timestamp timestamp, String title, String player, Window owner = null) {
                object[] args = new object[] { campaign, evt, timestamp, title, player, owner };
                return (EventDialog)(this.dlg.Invoke(args));
            }
        }

        protected SortedDictionary<String, EventImpl> events = new SortedDictionary<String, EventImpl>() {
            { "Generic", new EventImpl(typeof(Event), typeof(EventDialog)) }
        };

        public virtual Character newCharacter() {
            return new Character();
        }

        public virtual ICollection<String> getEventTypes() {
            return this.events.Keys;
        }

        public virtual Event newEvent(String type, String creator, EventResult parent = null) {
            if (!this.events.ContainsKey(type)) { throw new ArgumentException("Unknown event type: " + type); }
            return this.events[type].newEvent(type, creator, parent);
        }

        public virtual EventDialog viewEvent(Campaign campaign, Event evt, Timestamp timestamp, String title, String player, Window owner = null) {
            EventDialog dlg = this.events[evt.type].newDialog(campaign, evt, timestamp, title, player, owner);
            dlg.ShowDialog();
            if (!dlg.valid) { return null; }
            return dlg;
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
