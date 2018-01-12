using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Event {
        public String creator;
        public Timestamp start, end;
        public DateTime timestamp;
        private HashSet<String> viewers;
        private HashSet<String> editors;
        public String title, description;
        //...

        public Event() {
            //...
            this.timestamp = DateTime.UtcNow;
            //...
        }

        public bool canView(String player) {
            return (player == this.creator) || (this.viewers == null) || (this.viewers.Contains(player));
        }

        public bool canEdit(String player) {
            return (player == this.creator) || (this.editors == null) || (this.editors.Contains(player));
        }
    }
}
