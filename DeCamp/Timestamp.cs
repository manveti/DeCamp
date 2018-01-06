using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    abstract class Timestamp {
        public enum Interval { year, month, week, day, time, hour, minute, second };

        public abstract String toString(bool date = true, bool time = false);
        public abstract void add(int amount, Interval unit = Interval.second);

        public Timestamp copy() {
            return (Timestamp)this.MemberwiseClone();
        }
    }

    abstract class SimpleDate : Timestamp {
        protected int year;
        protected int time;
        protected Interval precision;
        protected Tuple<String, int>[] months;
        protected String dateFormat;

        public override String toString(bool date = true, bool time = false) {
            String retval = "";
            if (this.precision <= Interval.day) { time = false; }
            if (time == false) { date = true; }
            if (date) {
                String weekday = this.getWeekday();
                if (weekday != null) { retval += weekday + ", "; }
                Tuple<int, int> d = this.getDate();
                retval += String.Format(this.dateFormat, d.Item2, this.months[d.Item1 - 1].Item1, this.year);
            }
            if (date && time) { retval += " "; }
            if (time) {
                int hours = this.time;
                int seconds = hours % 60;
                hours /= 60;
                int minutes = hours % 60;
                hours /= 60;
                hours %= 24;
                if (this.precision == Interval.time) {
                    retval += this.getFuzzyTime(hours);
                }
                else {
                    retval += String.Format("{0:D2}", hours);
                    if (this.precision >= Interval.minute) { retval += String.Format(":{0:D2}", minutes); }
                    if (this.precision >= Interval.second) { retval += String.Format(":{0:D2}", seconds); }
                }
            }
            return retval;
        }

        public override void add(int amount, Interval unit = Interval.second) {
            if ((unit == Interval.week) || (unit == Interval.time)) { return; } // no default implementation for week; can't add time of day
            if (unit == Interval.year) {
                this.year += amount;
                return;
            }
            if (unit == Interval.month) {
                // convert months to days, then fall through to logic for day, hour, minute, and second
                int month = this.getDate().Item1 - 1, days = 0;
                for (; amount > 0; amount--) {
                    days += this.months[month].Item2;
                    month += 1;
                    if (month >= this.months.Length) { month = 0; }
                }
                for (; amount < 0; amount++) {
                    month -= 1;
                    if (month < 0) { month = this.months.Length - 1; }
                    days -= this.months[month].Item2;
                }
                amount = days;
                unit = Interval.day;
            }
            if (unit <= Interval.day) { amount *= 24; } // convert days to hours
            // strip off extra years at this point to avoid overflow when we convert to seconds; we'll still need to normalize later, though
            int yearLength = this.getYearLength();
            while (amount >= yearLength) {
                this.year += 1;
                amount -= yearLength;
            }
            while (amount <= -yearLength) {
                this.year -= 1;
                amount += yearLength;
            }
            if (unit <= Interval.hour) { amount *= 60; } // convert hours to minutes
            if (unit <= Interval.minute) { amount *= 60; } // convert minutes to seconds
            this.time += amount;
            yearLength *= 24 * 60 * 60; // convert days to seconds
            while (this.time >= yearLength) {
                this.year += 1;
                this.time -= yearLength;
            }
            while (this.time < 0) {
                this.year -= 1;
                this.time += yearLength;
            }
        }

        protected virtual String getWeekday() {
            return null;
        }

        protected virtual Tuple<int, int> getDate() {
            int month = 0, date = this.getDayOfYear();
            while ((month < this.months.Length) && (date > this.months[month].Item2)) {
                date -= this.months[month].Item2;
                month += 1;
            }
            return new Tuple<int, int>(month + 1, date);
        }

        protected virtual int getDayOfYear() {
            return (this.time / 86400) + 1;
        }

        protected virtual int getYearLength() {
            int retval = 0;
            foreach (Tuple<String, int> month in this.months) {
                retval += month.Item2;
            }
            return retval;
        }

        protected virtual String getFuzzyTime(int hours) {
            if (hours < 1) { return "Midnight"; }
            if (hours < 6) { return "Early Morning"; }
            if (hours < 11) { return "Morning"; }
            if (hours < 12) { return "Late Morning"; }
            if (hours < 13) { return "Noon"; }
            if (hours < 17) { return "Afternoon"; }
            if (hours < 21) { return "Evening"; }
            return "Night";
        }
    }

    class CampaignDate : SimpleDate {
        protected int date;

        public CampaignDate() {
            this.date = 1;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Tuple<String, int>[] { new Tuple<string, int>("", int.MaxValue) };
            this.dateFormat = "Day {0}";
        }

        //override add: ditch year logic and add date logic

        protected override int getDayOfYear() {
            return this.date;
        }
    }

    class GreyhawkDate : SimpleDate {
        String[] days;

        public GreyhawkDate() {
            this.year = 591;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Tuple<String, int>[]{ new Tuple<String, int>("Needfest", 7),
                                                    new Tuple<String, int>("Fireseek", 28),
                                                    new Tuple<String, int>("Readying", 28),
                                                    new Tuple<String, int>("Coldeven", 28),
                                                    new Tuple<String, int>("Growfest", 7),
                                                    new Tuple<String, int>("Planting", 28),
                                                    new Tuple<String, int>("Flocktime", 28),
                                                    new Tuple<String, int>("Wealsun", 28),
                                                    new Tuple<String, int>("Richfest", 7),
                                                    new Tuple<String, int>("Reaping", 28),
                                                    new Tuple<String, int>("Goodmonth", 28),
                                                    new Tuple<String, int>("Harvester", 28),
                                                    new Tuple<String, int>("Brewfest", 7),
                                                    new Tuple<String, int>("Patchwall", 28),
                                                    new Tuple<String, int>("Ready'reat", 28),
                                                    new Tuple<String, int>("Sunsebb", 28)};
            this.dateFormat = "{0} {1}, {2} CY";
            this.days = new String[]{ "Starday", "Sunday", "Moonday", "Godsday", "Waterday", "Earthday", "Freeday"};
        }

        protected override String getWeekday() {
            Tuple<int, int> date = this.getDate();
            if (this.months[date.Item1 - 1].Item2 < 28) { return null; }
            return this.days[(date.Item2 - 1) % this.days.Length];
        }

        //override add: handle festivals and weeks; fix for lack of year 0 in common reckoning, so -1 goes straight to 1
    }

    class EberronDate : SimpleDate {
        String[] days;

        public EberronDate() {
            this.year = 998;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Tuple<String, int>[]{ new Tuple<String, int>("Zarantyr", 28),
                                                    new Tuple<String, int>("Olarune", 28),
                                                    new Tuple<String, int>("Therendor", 28),
                                                    new Tuple<String, int>("Eyre", 28),
                                                    new Tuple<String, int>("Dravago", 28),
                                                    new Tuple<String, int>("Nymm", 28),
                                                    new Tuple<String, int>("Lharvion", 28),
                                                    new Tuple<String, int>("Barrakas", 28),
                                                    new Tuple<String, int>("Rhaan", 28),
                                                    new Tuple<String, int>("Sypheros", 28),
                                                    new Tuple<String, int>("Aryth", 28),
                                                    new Tuple<String, int>("Vult", 28)};
            this.dateFormat = "{0} {1}, {2} YK";
            this.days = new String[] { "Sul", "Mol", "Zol", "Wir", "Zor", "Far", "Sar" };
        }

        protected override String getWeekday() {
            return this.days[(this.getDayOfYear() - 1) % this.days.Length];
        }

        //override add: handle weeks
    }

    class FRDate : SimpleDate {
        public FRDate() {
            this.year = 1491;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Tuple<String, int>[]{ new Tuple<String, int>("Hammer", 28),
                                                    new Tuple<String, int>("Midwinter", 1),
                                                    new Tuple<String, int>("Alturiak", 28),
                                                    new Tuple<String, int>("Ches", 28),
                                                    new Tuple<String, int>("Tarsakh", 28),
                                                    new Tuple<String, int>("Greengrass", 1),
                                                    new Tuple<String, int>("Mirtul", 28),
                                                    new Tuple<String, int>("Kythorn", 28),
                                                    new Tuple<String, int>("Flamerule", 28),
                                                    new Tuple<String, int>("Midsummer", 1),
                                                    new Tuple<String, int>("Shieldmeet", 0),
                                                    new Tuple<String, int>("Eleasis", 28),
                                                    new Tuple<String, int>("Eleint", 28),
                                                    new Tuple<String, int>("Highharvestide", 1),
                                                    new Tuple<String, int>("Marpenoth", 28),
                                                    new Tuple<String, int>("Uktar", 28),
                                                    new Tuple<String, int>("Feast of the Moon", 1),
                                                    new Tuple<String, int>("Nightal", 28)};
            this.dateFormat = "{1} {0}, {2} DR";
        }

        protected override Tuple<int, int> getDate() {
            Tuple<String, int> shieldmeet = this.months[10];
            if ((this.year % 4) == 0) {
                this.months[10] = new Tuple<string, int>("Shieldmeet", 1);
            }
            Tuple<int, int> retval = base.getDate();
            this.months[10] = shieldmeet;
            return retval;
        }

        protected override int getYearLength() {
            return 365 + ((this.year % 4) == 0 ? 1 : 0);
        }

        //override add: handle festivals and leap years
    }


    static class Calendar {
        public static Timestamp newTimestamp(String calendar) {
            switch (calendar) {
            case "Greyhawk": return new GreyhawkDate();
            case "Eberron": return new EberronDate();
            case "Forgotten Realms": return new FRDate();
            }
            return new CampaignDate();
        }
    }
}
