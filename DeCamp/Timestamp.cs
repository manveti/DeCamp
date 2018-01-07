using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    abstract class Timestamp {
        public enum Interval { year, month, week, day, time, hour, minute, second };

        protected Interval precision;

        public abstract String toString(bool date = true, bool time = false);
        public abstract void adjust(int amount, Interval unit = Interval.second);
        public abstract void set(int value, Interval unit);

        public Timestamp copy() {
            return (Timestamp)this.MemberwiseClone();
        }

        public void setPrecision(Interval p) {
            this.precision = p;
        }
    }

    abstract class SimpleDate : Timestamp {
        protected class Month {
            public String name;
            public int days;
            public bool isVirtual;

            public Month(String name, int days, bool isVirtual = false) {
                this.name = name;
                this.days = days;
                this.isVirtual = isVirtual;
            }
        }

        protected int year;
        protected int time;
        protected Month[] months;
        protected String dateFormat;

        public override String toString(bool date = true, bool time = false) {
            String retval = "";
            if (this.precision <= Interval.day) { time = false; }
            if (time == false) { date = true; }
            if (date) {
                String weekday = this.getWeekday();
                if (weekday != null) { retval += weekday + ", "; }
                Tuple<int, int> d = this.getDate();
                retval += String.Format(this.dateFormat, d.Item2, this.months[d.Item1 - 1].name, this.year);
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

        public override void adjust(int amount, Interval unit = Interval.second) {
            if ((unit == Interval.week) || (unit == Interval.time)) { return; } // no default implementation for week; can't add time of day
            if (unit == Interval.year) {
                this.year += amount;
                return;
            }
            if (unit == Interval.month) {
                // convert months to days, then fall through to logic for day, hour, minute, and second
                int month = this.getDate().Item1 - 1, days = 0;
                for (; amount > 0; amount--) {
                    days += this.months[month].days;
                    month += 1;
                    if (month >= this.months.Length) { month = 0; }
                }
                for (; amount < 0; amount++) {
                    month -= 1;
                    if (month < 0) { month = this.months.Length - 1; }
                    days -= this.months[month].days;
                }
                amount = days;
                unit = Interval.day;
            }
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
            if (unit <= Interval.day) { amount *= 24; } // convert days to hours
            if (unit <= Interval.hour) { amount *= 60; } // convert hours to minutes
            if (unit <= Interval.minute) { amount *= 60; } // convert minutes to seconds
            this.time += amount;
            // normalize
            yearLength *= this.getDayLength(); // convert days to seconds
            while (this.time >= yearLength) {
                this.year += 1;
                this.time -= yearLength;
            }
            while (this.time < 0) {
                this.year -= 1;
                this.time += yearLength;
            }
        }

        public override void set(int value, Interval unit) {
            Tuple<int, int> d;
            int tail;

            switch (unit) {
            case Interval.year:
                this.year = value;
                break;
            case Interval.month:
                d = this.getDate();
                this.setDate(value, d.Item2);
                break;
            case Interval.day:
                d = this.getDate();
                this.setDate(d.Item1, value);
                break;
            case Interval.time:
                // fall through to hour case
            case Interval.hour:
                tail = this.time % this.getDayLength();
                this.time -= tail;
                this.time += value * 60 * 60 + (tail % (60 * 60));
                break;
            case Interval.minute:
                tail = this.time % (60 * 60);
                this.time -= tail;
                this.time += value * 60 + (tail % 60);
                break;
            case Interval.second:
                this.time -= (this.time % 60);
                this.time += value;
                break;
            }
            if (this.precision < unit) { this.precision = unit; }
        }

        protected virtual String getWeekday() {
            return null;
        }

        protected virtual Tuple<int, int> getDate() {
            int month = 0, date = this.getDayOfYear();
            while ((month < this.months.Length) && (date > this.months[month].days)) {
                date -= this.months[month].days;
                month += 1;
            }
            return new Tuple<int, int>(month + 1, date);
        }

        protected virtual void setDate(int month, int date) {
            while (month > 0) {
                month -= 1;
                date += this.months[month].days;
            }
            this.setDayOfYear(date);
        }

        protected virtual int getDayOfYear() {
            return (this.time / this.getDayLength()) + 1;
        }

        protected virtual void setDayOfYear(int date) {
            this.time = (date - 1) * this.getDayLength() + this.getTime();
        }

        protected virtual int getTime() {
            return this.time % this.getDayLength();
        }

        protected virtual void setTime(int time) {
            this.time -= this.getTime();
            this.time += time;
        }

        protected virtual void setTime(int hour, int minute, int second) {
            this.setTime(hour * 60 * 60 + minute * 60 + second);
        }

        protected virtual int getYearLength() {
            int retval = 0;
            foreach (Month month in this.months) {
                retval += month.days;
            }
            return retval;
        }

        protected virtual int getDayLength() {
            return 24 * 60 * 60;
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
            this.months = new Month[] { new Month("", int.MaxValue) };
            this.dateFormat = "Day {0}";
        }

        public override void adjust(int amount, Interval unit = Interval.second) {
            if (unit == Interval.time) { return; } // can't add time of day
            if (unit == Interval.year) {
                amount *= 365; // treat year as shorthand for 365 days
                unit = Interval.day;
            }
            if (unit == Interval.month) {
                amount *= 30; // treat month as shorthand for 30 days
                unit = Interval.day;
            }
            if (unit == Interval.week) {
                amount *= 7; // treat week as shortand for 7 days
                unit = Interval.day;
            }
            if (unit <= Interval.day) {
                this.date += amount;
                return;
            }
            if (unit <= Interval.hour) { amount *= 60; } // convert hours to minutes
            if (unit <= Interval.minute) { amount *= 60; } // convert minutes to seconds
            this.time += amount;
            // normalize
            int dayLength = this.getDayLength();
            while (this.time >= dayLength) {
                this.date += 1;
                this.time -= dayLength;
            }
            while (this.time < 0) {
                this.date -= 1;
                this.time += dayLength;
            }
        }

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
            this.months = new Month[]{ new Month("Needfest", 7, true),
                                        new Month("Fireseek", 28),
                                        new Month("Readying", 28),
                                        new Month("Coldeven", 28),
                                        new Month("Growfest", 7, true),
                                        new Month("Planting", 28),
                                        new Month("Flocktime", 28),
                                        new Month("Wealsun", 28),
                                        new Month("Richfest", 7, true),
                                        new Month("Reaping", 28),
                                        new Month("Goodmonth", 28),
                                        new Month("Harvester", 28),
                                        new Month("Brewfest", 7, true),
                                        new Month("Patchwall", 28),
                                        new Month("Ready'reat", 28),
                                        new Month("Sunsebb", 28)};
            this.dateFormat = "{0} {1}, {2} CY";
            this.days = new String[]{ "Starday", "Sunday", "Moonday", "Godsday", "Waterday", "Earthday", "Freeday"};
        }

        protected override String getWeekday() {
            Tuple<int, int> date = this.getDate();
            if (this.months[date.Item1 - 1].isVirtual) { return null; }
            return this.days[(date.Item2 - 1) % this.days.Length];
        }

        public override void adjust(int amount, Interval unit = Interval.second) {
            if (unit == Interval.year) {
                // deal with the fact that there's no year 0 in common reckoning
                if ((this.year < 0) && (this.year + amount >= 0)) { this.year += 1; }
                if ((this.year > 0) && (this.year + amount <= 0)) { this.year -= 1; }
                this.year += amount;
                return;
            }
            if (unit == Interval.month) {
                // convert months to days, then fall through to logic for day, hour, minute, and second
                int month = this.getDate().Item1 - 1, days = 0;
                for (; amount > 0; amount--) {
                    if (this.months[month].isVirtual) {
                        // skip over festivals
                        days += this.months[month].days;
                        month += 1;
                        if (month >= this.months.Length) { month = 0; } // no end-of-year festival, so we should never need this
                    }
                    days += this.months[month].days;
                    month += 1;
                    if (month >= this.months.Length) { month = 0; }
                }
                for (; amount < 0; amount++) {
                    month -= 1;
                    if (month < 0) { month = this.months.Length - 1; }
                    days -= this.months[month].days;
                    if (this.months[month].isVirtual) {
                        // skip over festivals
                        month -= 1;
                        if (month < 0) { month = this.months.Length - 1; }
                        days -= this.months[month].days;
                    }
                }
                amount = days;
                unit = Interval.day;
            }
            if (unit == Interval.week) {
                amount *= this.days.Length;
                unit = Interval.day;
            }
            base.adjust(amount, unit);
        }
    }

    class EberronDate : SimpleDate {
        String[] days;

        public EberronDate() {
            this.year = 998;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Month[]{ new Month("Zarantyr", 28),
                                        new Month("Olarune", 28),
                                        new Month("Therendor", 28),
                                        new Month("Eyre", 28),
                                        new Month("Dravago", 28),
                                        new Month("Nymm", 28),
                                        new Month("Lharvion", 28),
                                        new Month("Barrakas", 28),
                                        new Month("Rhaan", 28),
                                        new Month("Sypheros", 28),
                                        new Month("Aryth", 28),
                                        new Month("Vult", 28)};
            this.dateFormat = "{0} {1}, {2} YK";
            this.days = new String[] { "Sul", "Mol", "Zol", "Wir", "Zor", "Far", "Sar" };
        }

        protected override String getWeekday() {
            return this.days[(this.getDayOfYear() - 1) % this.days.Length];
        }

        public override void adjust(int amount, Interval unit = Interval.second) {
            if (unit == Interval.month) {
                amount *= 28; // all months have 28 days
                unit = Interval.day;
            }
            if (unit == Interval.week) {
                amount *= this.days.Length;
                unit = Interval.day;
            }
            base.adjust(amount, unit);
        }
    }

    class FRDate : SimpleDate {
        public FRDate() {
            this.year = 1491;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Month[]{ new Month("Hammer", 30),
                                        new Month("Midwinter", 1, true),
                                        new Month("Alturiak", 30),
                                        new Month("Ches", 30),
                                        new Month("Tarsakh", 30),
                                        new Month("Greengrass", 1, true),
                                        new Month("Mirtul", 30),
                                        new Month("Kythorn", 30),
                                        new Month("Flamerule", 30),
                                        new Month("Midsummer", 1, true),
                                        new Month("Shieldmeet", 0, true),
                                        new Month("Eleasis", 30),
                                        new Month("Eleint", 30),
                                        new Month("Highharvestide", 1, true),
                                        new Month("Marpenoth", 30),
                                        new Month("Uktar", 30),
                                        new Month("Feast of the Moon", 1, true),
                                        new Month("Nightal", 30)};
            this.dateFormat = "{1} {0}, {2} DR";
        }

        protected override Tuple<int, int> getDate() {
            int oldDays = this.months[10].days;
            if (this.isLeapYear()) {
                this.months[10].days = 1;
            }
            Tuple<int, int> retval = base.getDate();
            this.months[10].days = oldDays;
            return retval;
        }

        protected override int getYearLength() {
            return 365 + (this.isLeapYear() ? 1 : 0);
        }

        protected bool isLeapYear() {
            return (this.year % 4) == 0;
        }

        public override void adjust(int amount, Interval unit = Interval.second) {
            if (unit == Interval.time) { return; } // can't add time of day
            if (unit == Interval.year) {
                this.year += amount;
                return;
            }
            if ((unit == Interval.month) || (unit == Interval.week)) {
                Tuple<int, int> d = this.getDate();
                int month = d.Item1 - 1, date = d.Item2;
                bool wasFestival = (this.months[month].isVirtual), wasMidsummer = false;
                if (wasFestival) {
                    // treat festivals as the fist of the following month
                    month += 1;
                    if (this.months[month].isVirtual) {
                        wasMidsummer = true;
                        month += 1;
                    }
                }
                if (unit == Interval.week) {
                    // convert weeks to months, adjusting date by remaining tendays
                    int extra = amount % 3;
                    if (extra < 0) {
                        extra += 3;
                        amount -= 3;
                    }
                    amount /= 3;
                    if (extra > 0) {
                        date += extra * 10;
                        if (wasFestival) {
                            date -= 1;
                            wasFestival = false;
                            wasMidsummer = false;
                        }
                    }
                }
                // adjust month by amount, adjusting this.year as necessary along the way
                for (; amount > 0; amount--) {
                    month += 1;
                    if (month >= this.months.Length) {
                        month = 0;
                        this.year += 1;
                    }
                    while (this.months[month].isVirtual) { month += 1; }
                }
                for (; amount < 0; amount++) {
                    month -= 1;
                    if (month < 0) {
                        month = this.months.Length - 1;
                        this.year -= 1;
                    }
                    while (this.months[month].isVirtual) { month -= 1; }
                }
                // if we started on a festival and can end on one, do so
                if ((wasFestival) && (month > 0) && (this.months[month - 1].isVirtual)) {
                    month -= 1;
                    // if we started on midsummer and can end on it (i.e. amount was a multiple of 12), do so
                    if ((wasMidsummer) && (month > 0) && (this.months[month - 1].isVirtual)) { month -= 1; }
                }
                this.setDate(month, date);
                return;
            }
            // we're not falling through from longer spans here, so we won't worry about overflow
            if (unit <= Interval.day) { amount *= 24; } // convert days to hours
            if (unit <= Interval.hour) { amount *= 60; } // convert hours to minutes
            if (unit <= Interval.minute) { amount *= 60; } // convert minutes to seconds
            this.time += amount;
            // normalize
            while (this.time >= this.getYearLength() * this.getDayLength()) {
                this.time -= this.getYearLength() * this.getDayLength();
                this.year += 1;
            }
            while (this.time < 0) {
                this.year -= 1;
                this.time += this.getYearLength() * this.getDayLength();
            }
        }

        protected override void setDate(int month, int date) {
            while (month > 0) {
                month -= 1;
                date += this.months[month].days;
                if ((this.months[month].days == 0) && (this.isLeapYear())) {
                    date += 1;
                }
            }
            this.setDayOfYear(date);
        }
    }

    class GregorianDate : SimpleDate {
        String[] days;

        public GregorianDate() {
            this.year = 2018;
            this.time = 12 * 60 * 60;
            this.precision = Interval.time;
            this.months = new Month[]{ new Month("January", 31),
                                        new Month("February", 28),
                                        new Month("March", 31),
                                        new Month("April", 30),
                                        new Month("May", 31),
                                        new Month("June", 30),
                                        new Month("July", 31),
                                        new Month("August", 31),
                                        new Month("September", 30),
                                        new Month("October", 31),
                                        new Month("November", 30),
                                        new Month("December", 31)};
            this.dateFormat = "{1} {0}, {2}";
            this.days = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        }

        protected override Tuple<int, int> getDate() {
            int oldDays = this.months[1].days;
            if (this.isLeapYear()) {
                this.months[1].days = 29;
            }
            Tuple<int, int> retval = base.getDate();
            this.months[1].days = oldDays;
            return retval;
        }

        protected override int getYearLength() {
            return 365 + (this.isLeapYear() ? 1 : 0);
        }

        protected bool isLeapYear() {
            if (this.year < 8) {
                return ((this.year >= -45) && (this.year <= -9) && (this.year % 3 == 0));
            }
            if ((this.year % 4) != 0) { return false; }
            if ((this.year % 400) == 0) { return true; }
            return (this.year % 100) != 0;
        }

        protected override String getWeekday() {
            int year = this.year, day = this.getDayOfYear() % 7;
            if (year < 8) {
/////
//
                //fix up wonky leap years and deal with the fact that there's no 0 CE
//
/////
            }
            while (year < 2001) { year += 400; } // 365 days per non-leap year means day increases by 1 (365 % 7) per year, plus leap days
            while (year >= 2401) { year -= 400; } // 97 leap days per 400 years, so day increases by 497 days (=> 0 days) every 400 years
            // now that year is between 2001 and 2400, we'll get a little more involved to translate it to 2001
            while (year > 2100) {
                year -= 100; // 24 leap years for every 100 years between 2001 and 2400...
                day = (day + 124) % 7; // ...so day increases by 124 (=> 5) days every 100 years
            }
            // year is now between 2001 and 2100
            while (year > 2004) {
                year -= 4; // 1 leap year every 4 years between 2001 and 2100...
                day = (day + 5) % 7; // ...so day increases by 5 days every 4 years
            }
            // year is now between 2001 and 2004
            while (year > 2001) {
                year -= 1; // no leap years between 2001 and 2004...
                day = (day + 1) % 7; // ...so day increases by 1 day every year
            }
            return this.days[day]; // Jan 1, 2001 was a Monday, so day 1 is this.days[1]
        }

        public override void adjust(int amount, Interval unit = Interval.second) {
            if (unit == Interval.time) { return; } // can't add time of day
            if (unit == Interval.year) {
                // deal with the fact that there's no year 0 in common era
                if ((this.year < 0) && (this.year + amount >= 0)) { this.year += 1; }
                if ((this.year > 0) && (this.year + amount <= 0)) { this.year -= 1; }
                this.year += amount;
                return;
            }
            if (unit == Interval.month) {
                Tuple<int, int> d = this.getDate();
                int month = d.Item1 - 1, date = d.Item2;
                // adjust month by amount, adjusting this.year as necessary along the way
                for (; amount > 0; amount--) {
                    month += 1;
                    if (month >= this.months.Length) {
                        month = 0;
                        this.year += 1;
                    }
                }
                for (; amount < 0; amount++) {
                    month -= 1;
                    if (month < 0) {
                        month = this.months.Length - 1;
                        this.year -= 1;
                    }
                }
                this.setDate(month, date);
                return;
            }
            // we're not falling through from longer spans here, so we won't worry about overflow
            if (unit <= Interval.week) { amount *= 7; } // convert weeks to days
            if (unit <= Interval.day) { amount *= 24; } // convert days to hours
            if (unit <= Interval.hour) { amount *= 60; } // convert hours to minutes
            if (unit <= Interval.minute) { amount *= 60; } // convert minutes to seconds
            this.time += amount;
            // normalize
            while (this.time >= this.getYearLength() * this.getDayLength()) {
                this.time -= this.getYearLength() * this.getDayLength();
                this.year += 1;
            }
            while (this.time < 0) {
                this.year -= 1;
                this.time += this.getYearLength() * this.getDayLength();
            }
        }

        protected override void setDate(int month, int date) {
            while (month > 0) {
                month -= 1;
                date += this.months[month].days;
                if ((month == 1) && (this.isLeapYear())) {
                    date += 1;
                }
            }
            this.setDayOfYear(date);
        }
    }


    static class Calendar {
        private class CalImpl {
            public Func<Timestamp> timestamp;

            public CalImpl(Func<Timestamp> timestamp) {
                this.timestamp = timestamp;
            }
        }

        private static Dictionary<String, CalImpl> calendars = new Dictionary<string, CalImpl>() {
            { "Greyhawk", new CalImpl( () => new GreyhawkDate() ) },
            { "Eberron", new CalImpl( () => new EberronDate() ) },
            { "Forgotten Realms", new CalImpl( () => new FRDate() ) },
            { "Gregorian", new CalImpl( () => new GregorianDate() ) },
            { null, new CalImpl( () => new CampaignDate() ) }
        };

        public static ICollection<String> getCalendars() {
            return calendars.Keys;
        }

        public static Timestamp newTimestamp(String calendar) {
            return (calendars[calendar] ?? calendars[null]).timestamp.Invoke();
        }
    }
}
