using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Campaign {
        public const String gmKey = "__GM__";

        public String name;
        private Dictionary<String, Player> players;
        private String gm;
        public readonly String calendarName;
        private readonly Calendar calendar;
        private readonly String rulesetName;
        private Dictionary<String, Character> party;
        private Timestamp now;
        private SortedDictionary<Timestamp, EventBucket> timeline;
        //todo
        //journal

        public Campaign(String name, String gm, String calendar, String ruleset) {
            this.name = name;
            this.players = new Dictionary<string, Player>();
            this.gm = this.addPlayer(gm);
            this.calendarName = calendar;
            this.calendar = Calendars.newCalendar(this.calendarName);
            this.rulesetName = ruleset;
            this.party = new Dictionary<string, Character>();
            this.now = this.calendar.defaultTimestamp();
        }

        public ICollection<String> getPlayers() {
            return this.players.Keys;
        }

        public Player getPlayer(String key) {
            if (key == gmKey) { return this.getPlayer(this.gm); }
            return this.players[key];
        }

        public String addPlayer(String name) {
            String key = name;
            if (key == gmKey) { key += "0"; } // disallow gmKey
            for (int i = 0; this.players.ContainsKey(key); i++) {
                key = name + i;
            }
            this.players[key] = new Player(name);
            return key;
        }

        public void removePlayer(String key) {
            if (!this.players.ContainsKey(key)) { return; }
/////
//
            //traverse party, events, etc. to deal with references to player (change resource ownership to gm, remove access, etc.)
            //do something useful if player is gm (i.e. key==this.gm)
//
/////
            this.players.Remove(key);
        }

        //get/change gm
        //future: change calendar
        //future: change ruleset
        //get party (change through events)

        public Timestamp getTimestamp() {
            return this.now;
        }

        public void adjustTimestamp(int amount, Calendar.Interval unit = Calendar.Interval.second) {
            this.now = this.now.add(amount, unit);
            this.handleTimeChange();
        }

        public void setTimestamp(Timestamp t) {
            this.now = t;
            this.handleTimeChange();
        }

        public void setTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Calendar.Interval precision) {
            this.setTimestamp(this.calendar.newTimestamp(year, month, week, day, hour, minute, second, precision));
        }

        //...

        private void handleTimeChange() {
            //update timeline, todo, etc. to deal with the fact that the time just changed
        }
    }
}
