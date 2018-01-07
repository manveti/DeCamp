using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Event {
        public Timestamp start, end;
        public DateTime timestamp;
        //access
        public String title, description;
        //...

        public Event() {
            //...
            this.timestamp = DateTime.UtcNow;
            //...
        }
    }
}
