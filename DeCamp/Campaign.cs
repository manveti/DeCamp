using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class CampaignState {
        public Timestamp timestamp;
        private Dictionary<String, Character> party;
        //loot
        //todo
        //journal

        public CampaignState(Timestamp timestamp) {
            this.timestamp = timestamp;
            this.party = new Dictionary<string, Character>();
        }

        private CampaignState() { }

        public CampaignState copy() {
            return new CampaignState() {
                timestamp = this.timestamp,
                party = this.party.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.copy())
            };
        }

        public ICollection<String> getParty() {
            return this.party.Keys;
        }

        public Character getCharacter(String key) {
            return this.party[key];
        }

        public String addCharacter(Character c) {
            String key = c.name;
            for (int i = 0; this.party.ContainsKey(key); i++) {
                key = c.name + i;
            }
            this.party[key] = c;
            return key;
        }

        public void removeCharacter(String key) {
            if (!this.party.ContainsKey(key)) { return; }
            this.party.Remove(key);
        }
    }

    class Campaign {
        public const String gmKey = "__GM__";

        public String name;
        private Dictionary<String, Player> players;
        private String gm;
        public readonly String calendarName;
        private readonly Calendar calendar;
        private readonly String rulesetName;
        private readonly Ruleset ruleset;
        private Timestamp now;
        private CampaignState state;
        private SortedDictionary<Timestamp, List<Event>> timeline;

        public Campaign(String name, String gm, String calendar, String ruleset) {
            this.name = name;
            this.players = new Dictionary<string, Player>();
            this.gm = this.addPlayer(gm);
            this.calendarName = calendar;
            this.calendar = Calendars.newCalendar(this.calendarName);
            this.rulesetName = ruleset;
            this.ruleset = Rulesets.newRuleset(this.rulesetName);
            this.now = this.calendar.defaultTimestamp();
            this.state = new CampaignState(this.now);
            this.timeline = new SortedDictionary<Timestamp, List<Event>>();
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

        public String getGm() {
            return this.gm;
        }

        //change gm
        //future: change calendar

        public Ruleset getRuleset() {
            return this.ruleset;
        }

        //future: change ruleset

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

        public ICollection<String> getParty() {
            return this.state.getParty();
        }

        public Character getCharacter(String key) {
            return this.state.getCharacter(key);
        }

        //other state stuff

        public void advanceTo(Timestamp t) {
            if (this.state.timestamp >= t) { return; }
            foreach (Timestamp ts in this.timeline.Keys) {
                if (ts <= this.state.timestamp) { continue; }
                if (ts > t) { break; }
                foreach (Event e in this.timeline[ts]) { e.apply(this.state); }
                this.state.timestamp = ts;
            }
        }

        public void rewindTo(Timestamp t) {
            if (this.state.timestamp <= t) { return; }
            Timestamp[] timestamps = this.timeline.Keys.ToArray();
            timestamps.Reverse();
            foreach (Timestamp ts in timestamps) {
                if (ts > this.state.timestamp) { continue; }
                if (ts <= t) { break; }
                Event[] events = this.timeline[ts].ToArray();
                events.Reverse();
                foreach (Event e in events) { e.revert(this.state); }
                this.state.timestamp = ts;
            }
        }

        public void addEvent(Timestamp t, Event e) {
            if (this.state.timestamp > t) {
                this.rewindTo(t);
            }
            if (!this.timeline.ContainsKey(t)) { this.timeline[t] = new List<Event>(); }
            this.timeline[t].Add(e);
            e.apply(this.state);
            if (this.now < t) { this.now = t; }
            if (this.state.timestamp < this.now) {
                this.advanceTo(this.now);
            }
        }

        //other timeline stuff

        private void handleTimeChange() {
            if (this.now > this.state.timestamp) { this.advanceTo(this.now); }
            if (this.now < this.state.timestamp) { this.rewindTo(this.now); }
        }
    }
}
