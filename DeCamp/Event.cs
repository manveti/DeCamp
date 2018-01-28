using System;
using System.Collections.Generic;

namespace DeCamp {
    public class Event {
        public readonly String type;
        public String owner;
        public EventResult parent;
        public TimeSpan duration;
        public DateTime timestamp;
        public HashSet<String> viewers;
        public HashSet<String> editors;
        public String title, description, notes;
        public bool isVirtual;
        public List<EventResult> results;

        public Event(String type, String creator, EventResult parent = null) {
            this.type = type;
            this.owner = creator;
            this.parent = parent;
            this.timestamp = DateTime.UtcNow;
        }

        public virtual bool canView(String player) {
            return (player == this.owner) || (player == Campaign.gmKey) || (this.viewers == null) || (this.viewers.Contains(player));
        }

        public virtual bool canViewNotes(String player) {
            return (player == this.owner);
        }

        public virtual bool canEdit(String player) {
            return (player == this.owner) || (player == Campaign.gmKey) || (this.editors == null) || (this.editors.Contains(player));
        }

        public virtual bool canAssign(String player) {
            return (player == Campaign.gmKey);
        }

        public virtual bool canClaim(String player) {
            return (this.owner == null);
        }

        public virtual bool canSetPermissions(String player) {
            return (player == this.owner) || (player == Campaign.gmKey);
        }

        public virtual void apply(CampaignState s, bool doVirtual = false) {
            if ((this.isVirtual) && (!doVirtual)) { return; }
            if (this.results == null) { return; }
            foreach (EventResult res in this.results) {
                res.apply(s);
                if (res.subEvent != null) { res.subEvent.apply(s, doVirtual); }
            }
        }

        public virtual void revert(CampaignState s, bool doVirtual = false) {
            if ((this.isVirtual) && (!doVirtual)) { return; }
            if (this.results == null) { return; }
            foreach (EventResult res in this.results) {
                if (res.subEvent != null) { res.subEvent.revert(s, doVirtual); }
                res.revert(s);
            }
        }

        public virtual void addResult(EventResult r) {
            if (this.results == null) { this.results = new List<EventResult>(); }
            this.results.Add(r);
        }

        //insert, move, delete
    }

    public abstract class EventResult {
        public readonly String type;
        public String creator;
        public Event parent;
        public String summary;
        public Event subEvent;

        public EventResult(String type, String creator, Event parent, String summary) {
            this.type = type;
            this.creator = creator;
            this.parent = parent;
            this.summary = summary;
        }

        public abstract void apply(CampaignState s);
        public abstract void revert(CampaignState s);
    }

    public class CharacterAddResult : EventResult {
        public Character character;
        public String charId;

        public CharacterAddResult(String type, String creator, Event parent, String summary, Character character) : base(type, creator, parent, summary) {
            this.character = character;
        }

        public override void apply(CampaignState s) {
            this.charId = s.addCharacter(this.character);
        }

        public override void revert(CampaignState s) {
            s.removeCharacter(this.charId);
        }
    }

    public class CharacterRemoveResult : EventResult {
        public Character character;
        public String charId;

        public CharacterRemoveResult(String type, String creator, Event parent, String summary, String charId) : base(type, creator, parent, summary) {
            this.charId = charId;
        }

        public override void apply(CampaignState s) {
            this.character = s.getCharacter(this.charId);
            s.removeCharacter(this.charId);
        }

        public override void revert(CampaignState s) {
            this.charId = s.addCharacter(this.character);
        }
    }

    public abstract class AttributeMod {
        public String key;

        public AttributeMod(String key) {
            this.key = key;
        }

        public abstract void apply(Character c);
        public abstract void revert(Character c);
    }

    public class AttributeAdjustment : AttributeMod {
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

    public class AttributeReplacement : AttributeMod {
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

    public class CharacterEditResult : EventResult {
        public String charId;
        public List<AttributeMod> modifications;

        public CharacterEditResult(String type, String creator, Event parent, String summary, String charId, List<AttributeMod> mods) : base(type, creator, parent, summary) {
            this.charId = charId;
            this.modifications = mods;
        }

        public override void apply(CampaignState s) {
            Character c = s.getCharacter(this.charId);
            foreach (AttributeMod m in this.modifications) {
                m.apply(c);
            }
        }

        public override void revert(CampaignState s) {
            Character c = s.getCharacter(this.charId);
            foreach (AttributeMod m in this.modifications) {
                m.revert(c);
            }
        }
    }

    //add/remove/edit party resources
    //add/remove/edit todo
}
