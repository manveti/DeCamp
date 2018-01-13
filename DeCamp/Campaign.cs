using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Campaign {
        private Dictionary<String, Player> players;
        private String gm;
        private String calendar;
        private String ruleset;
        private Dictionary<String, Character> party;
        private Timestamp now;
        //timeline
        //todo
        //journal

        //constructor
        //...

        public Timestamp getTimestamp() {
            return this.now.copy();
        }

        public void adjustTimestamp(int amount, Timestamp.Interval unit = Timestamp.Interval.second) {
            this.now.adjust(amount, unit);
            this.handleTimeChange();
        }

        public void setTimestamp(int year, int month, int week, int day, int time, int hour, int minute, int second, Timestamp.Interval precision) {
            this.now.set(year, month, week, day, time, hour, minute, second, precision);
            this.handleTimeChange();
        }

        //...

        private void handleTimeChange() {
            //update timeline, todo, etc. to deal with the fact that the time just changed
        }
    }
}
