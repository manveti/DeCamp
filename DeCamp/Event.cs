using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Event {
        public String creator;
        public EventResult parent;
        public TimeSpan duration;
        public DateTime timestamp;
        private HashSet<String> viewers;
        private HashSet<String> editors;
        public String title, description, notes;
        public List<EventResult> results;

        public Event(String creator, EventResult parent = null) {
            this.creator = creator;
            this.parent = parent;
            this.timestamp = DateTime.UtcNow;
        }

        public virtual bool canView(String player) {
            return (player == this.creator) || (this.viewers == null) || (this.viewers.Contains(player));
        }

        public virtual bool canEdit(String player) {
            return (player == this.creator) || (this.editors == null) || (this.editors.Contains(player));
        }

        public virtual void apply(Campaign c) {
            foreach (EventResult res in this.results) { res.apply(c); }
        }

        public virtual void revert(Campaign c) {
            foreach (EventResult res in this.results) { res.revert(c); }
        }
    }

    abstract class EventResult {
        public String creator;
        public Event parent;
        public String summary;
        public Event subEvent;

        public EventResult(String creator, Event parent, String summary) {
            this.creator = creator;
            this.parent = parent;
            this.summary = summary;
        }

        public abstract void apply(Campaign c);
        public abstract void revert(Campaign c);
    }

    class CharacterAddResult : EventResult {
        public Character character;
        public String charId;

        public CharacterAddResult(String creator, Event parent, String summary, Character character) : base(creator, parent, summary) {
            this.character = character;
        }

        public override void apply(Campaign c) {
            this.charId = c.addCharacter(this.character);
        }

        public override void revert(Campaign c) {
            c.removeCharacter(this.charId);
        }
    }

    class CharacterRemoveResult : EventResult {
        public Character character;
        public String charId;

        public CharacterRemoveResult(String creator, Event parent, String summary, String charId) : base(creator, parent, summary) {
            this.charId = charId;
        }

        public override void apply(Campaign c) {
            this.character = c.getCharacter(this.charId);
            c.removeCharacter(this.charId);
        }

        public override void revert(Campaign c) {
            this.charId = c.addCharacter(this.character);
        }
    }

    abstract class AttributeMod {
        public String key;

        public AttributeMod(String key) {
            this.key = key;
        }

        public abstract void apply(Character c);
        public abstract void revert(Character c);
    }

    class AttributeAdjustment : AttributeMod {
        public object offset;

        public AttributeAdjustment(String key, object offset) : base(key) {
            this.offset = offset;
        }

        public override void apply(Character c) {
            c.adjustAttribute(this.key, this.offset);
        }

        public override void revert(Character c) {
            c.adjustAttribute(this.key, this.offset, true);
        }
    }

    class AttributeReplacement : AttributeMod {
        public object value;
        public Attribute.Type type;
        public Attribute oldValue;

        public AttributeReplacement(String key, object value, Attribute.Type type) : base(key) {
            this.value = value;
            this.type = type;
        }

        public override void apply(Character c) {
            this.oldValue = c.getRawAttribute(this.key);
            c.setAttribute(this.key, this.value, this.type);
        }

        public override void revert(Character c) {
            c.setRawAttribute(this.key, this.oldValue);
        }
    }

    class CharacterEditResult : EventResult {
        public String charId;
        public List<AttributeMod> modifications;

        public CharacterEditResult(String creator, Event parent, String summary, String charId, List<AttributeMod> mods) : base(creator, parent, summary) {
            this.charId = charId;
            this.modifications = mods;
        }

        public override void apply(Campaign c) {
            Character ch = c.getCharacter(this.charId);
            foreach (AttributeMod m in this.modifications) {
                m.apply(ch);
            }
        }

        public override void revert(Campaign c) {
            Character ch = c.getCharacter(this.charId);
            foreach (AttributeMod m in this.modifications) {
                m.revert(ch);
            }
        }
    }

    //add/remove/edit party resources
    //add/remove/edit todo

    class EventBucket {
        public List<Event> events;
        public List<Event> virtualEvents;

        public void addEvent(Event e, bool isVirtual = false) {
            this.getList(isVirtual).Add(e);
        }

        public void insertEvent(int i, Event e, bool isVirtual = false) {
            this.getList(isVirtual).Insert(i, e);
        }

        public void removeEvent(int i, bool isVirtual = false) {
            this.getList(isVirtual).RemoveAt(i);
        }

        public void moveEvent(int fromIdx, int toIdx, bool wasVirtual = false, bool isVirtual = false) {
            if ((fromIdx == toIdx) && (wasVirtual == isVirtual)) { return; }
            Event e = this.getList(wasVirtual)[fromIdx];
            this.removeEvent(fromIdx, wasVirtual);
            this.insertEvent(toIdx, e, isVirtual);
        }

        protected List<Event> getList(bool isVirtual) {
            if (isVirtual) {
                if (this.virtualEvents == null) { this.virtualEvents = new List<Event>(); }
                return this.virtualEvents;
            }
            else {
                if (this.events == null) { this.events = new List<Event>(); }
                return this.events;
            }
        }
    }
}
