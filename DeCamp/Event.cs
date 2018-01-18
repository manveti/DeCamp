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

        public bool canView(String player) {
            return (player == this.creator) || (this.viewers == null) || (this.viewers.Contains(player));
        }

        public bool canEdit(String player) {
            return (player == this.creator) || (this.editors == null) || (this.editors.Contains(player));
        }
    }

    class EventResult {
        public String creator;
        public Event parent;
        public String summary;
        public Event subEvent;

        public EventResult(String creator, Event parent) {
            this.creator = creator;
            this.parent = parent;
        }
    }

    //add/edit/remove character
    //add/edit/remove party resources
    //add/edit/remove todo

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
