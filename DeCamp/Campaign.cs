﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Campaign {
        public String name;
        private Dictionary<String, Player> players;
        private String gm;
        private readonly Calendar calendar;
        private readonly String ruleset;
        private Dictionary<String, Character> party;
        private Timestamp now;
        private SortedDictionary<Timestamp, Event> timeline;
        //todo
        //journal

        public Campaign(String name, String gm, String calendar, String ruleset) {
            this.name = name;
            this.players = new Dictionary<string, Player>();
            this.gm = this.addPlayer(gm);
            this.calendar = Calendars.newCalendar(calendar);
            this.ruleset = ruleset;
            this.party = new Dictionary<string, Character>();
            this.now = this.calendar.defaultTimestamp();
        }

        public ICollection<String> getPlayers() {
            return this.players.Keys;
        }

        public String addPlayer(String name) {
            String key = name;
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

        public void setTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Calendar.Interval precision) {
            this.now = this.calendar.newTimestamp(year, month, week, day, hour, minute, second, precision);
            this.handleTimeChange();
        }

        //...

        private void handleTimeChange() {
            //update timeline, todo, etc. to deal with the fact that the time just changed
        }
    }
}
