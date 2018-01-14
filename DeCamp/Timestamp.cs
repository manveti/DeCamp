using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class TimeSpan {
        protected readonly Calendar calendar;
        public readonly Decimal value;

        public TimeSpan(Calendar calendar, Decimal value) {
            this.calendar = calendar;
            this.value = value;
        }

        public virtual String toString() {
            return this.calendar.timeSpanToString(this);
        }
    }

    class Timestamp : IComparable {
        protected readonly Calendar calendar;
        public readonly Decimal value;
        public readonly Calendar.Interval precision;

        public Timestamp(Calendar calendar, Decimal value, Calendar.Interval precision = Calendar.Interval.second) {
            this.value = value;
            this.calendar = calendar;
            this.precision = precision;
        }

        public virtual int CompareTo(object obj) {
            if (obj == null) { return 1; }
            return this.value.CompareTo(((Timestamp)obj).value);
        }

        public override bool Equals(object obj) {
            if (obj == null) { return false; }
            return this.value.Equals(((Timestamp)obj).value);
        }

        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public static bool operator <(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) < 0;
        }

        public static bool operator <=(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) <= 0;
        }

        public static bool operator ==(Timestamp t1, Timestamp t2) {
            if (ReferenceEquals(t1, t2)) { return true; }
            return t1.CompareTo(t2) == 0;
        }

        public static bool operator !=(Timestamp t1, Timestamp t2) {
            if (ReferenceEquals(t1, t2)) { return true; }
            return t1.CompareTo(t2) != 0;
        }

        public static bool operator >=(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) >= 0;
        }

        public static bool operator >(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return false; }
            return t1.CompareTo(t2) > 0;
        }

        public static Timestamp operator +(Timestamp t, TimeSpan amount) {
            if ((t == null) || (amount == null)) { return null; }
            return t.calendar.newTimestamp(t.value + amount.value, t.precision);
        }

        public static Timestamp operator -(Timestamp t, TimeSpan amount) {
            if ((t == null) || (amount == null)) { return null; }
            return t.calendar.newTimestamp(t.value - amount.value, t.precision);
        }

        public static TimeSpan operator -(Timestamp t1, Timestamp t2) {
            if ((t1 == null) || (t2 == null)) { return null; }
            return t1.calendar.newTimeSpan(t1.value - t2.value);
        }

        public virtual String toString(bool date = true, bool time = false) {
            return this.calendar.timestampToString(this, date, time);
        }

        public virtual Timestamp add(long amount, Calendar.Interval unit = Calendar.Interval.second) {
            return this.calendar.add(this, amount, unit);
        }
    }

    abstract class Calendar {
        public enum Interval { year, month, week, day, time, hour, minute, second };

        public virtual TimeSpan newTimeSpan(Decimal value) {
            return new TimeSpan(this, value);
        }

        public virtual Timestamp newTimestamp(Decimal value, Interval precision = Interval.second) {
            return new Timestamp(this, value, precision);
        }

        public abstract Timestamp newTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Interval precision = Interval.second);

        public virtual Timestamp defaultTimestamp() {
            return this.newTimestamp(0);
        }

        public abstract String timeSpanToString(TimeSpan s);
        public abstract String timestampToString(Timestamp t, bool date = true, bool time = false);
        public abstract Timestamp add(Timestamp t, long amount, Interval unit = Interval.second);

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

    abstract class DayCalendar : Calendar {
        protected Decimal yearDays, monthDays, weekDays;
        protected int dayHours, hourMins, minuteSecs;
        protected String dateFormat;

        public DayCalendar() {
            this.yearDays = 365.2425m;
            this.monthDays = this.yearDays / 12;
            this.weekDays = 7;
            this.dayHours = 24;
            this.hourMins = 60;
            this.minuteSecs = 60;
            this.dateFormat = "Day {0}";
        }

        public override Timestamp newTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Interval precision = Interval.second) {
            Decimal value = 0;
            if (precision >= Interval.year) { value += Math.Floor(year * this.yearDays); }
            if (precision >= Interval.month) { value += Math.Floor(month * this.monthDays); }
            if (precision >= Interval.week) { value += week * this.weekDays; }
            if (precision >= Interval.day) { value += day; }
            value *= this.dayHours;
            if (precision >= Interval.time) { value += hour; }
            value *= this.hourMins;
            if (precision >= Interval.minute) { value += minute; }
            value *= this.minuteSecs;
            if (precision >= Interval.second) { value += second; }
            return this.newTimestamp(value, precision);
        }

        public override String timeSpanToString(TimeSpan s) {
/////
//
            throw new NotImplementedException();
//
/////
        }

        public override String timestampToString(Timestamp t, bool date = true, bool time = false) {
            String retval = "";
            if (t.precision <= Interval.day) { time = false; }
            if (time == false) { date = true; }
            if (date) {
                retval += String.Format(this.dateFormat, this.getDate(t));
            }
            if (date && time) { retval += " "; }
            if (time) {
                int hours = this.getTime(t);
                int seconds = hours % this.minuteSecs;
                hours /= this.minuteSecs;
                int minutes = hours % this.hourMins;
                hours /= this.hourMins;
                if (t.precision == Interval.time) {
                    retval += this.getFuzzyTime(hours);
                }
                else {
                    retval += String.Format("{0:D2}", hours);
                    if (t.precision >= Interval.minute) { retval += String.Format(":{0:D2}", minutes); }
                    if (t.precision >= Interval.second) { retval += String.Format(":{0:D2}", seconds); }
                }
            }
            return retval;
        }

        public override Timestamp add(Timestamp t, long amount, Interval unit = Interval.second) {
            Decimal value = t.value;
            switch (unit) {
            case Interval.year:
                value += amount * this.yearDays * this.dayHours * this.hourMins * this.minuteSecs;
                break;
            case Interval.month:
                value += amount * this.monthDays * this.dayHours * this.hourMins * this.minuteSecs;
                break;
            case Interval.week:
                value += amount * this.weekDays * this.dayHours * this.hourMins * this.minuteSecs;
                break;
            case Interval.day:
                value += amount * this.dayHours * this.hourMins * this.minuteSecs;
                break;
            case Interval.time:
                return null; // can't add time of day
            case Interval.hour:
                value += amount * this.hourMins * this.minuteSecs;
                break;
            case Interval.minute:
                value += amount * this.minuteSecs;
                break;
            case Interval.second:
                value += amount;
                break;
            }
            return this.newTimestamp(value, t.precision);
        }

        protected virtual Decimal getDate(Timestamp t) {
            return Math.Floor(t.value / (this.dayHours * this.hourMins * this.minuteSecs));
        }

        protected virtual int getTime(Timestamp t) {
            Decimal dayLength = this.dayHours * this.hourMins * this.minuteSecs;
            Decimal retval = t.value % dayLength;
            if (retval < 0) { retval += dayLength; }
            return Decimal.ToInt32(retval);
        }
    }

    class CampaignCalendar : DayCalendar {
        public override Timestamp defaultTimestamp() {
            return this.newTimestamp(0, 0, 0, 1, 12, 0, 0, Interval.time);
        }
    }

    class JulianCalendar : DayCalendar {
        public override Timestamp defaultTimestamp() {
            return this.newTimestamp(2458119.5m * this.dayHours * this.hourMins * this.minuteSecs, Interval.time);
        }

        public override String timestampToString(Timestamp t, bool date = true, bool time = false) {
            Decimal value = t.value / (this.dayHours * this.hourMins * this.minuteSecs);
            if (t.precision <= Interval.day) { time = false; }
            if (time == false) {
                date = true;
                value = Math.Floor(value);
            }
            else if (t.precision <= Interval.time) {
                value = Decimal.Truncate(value * 10) / 10;
            }
            if (date == false) {
                value = value % 1;
                if (value < 0) { value += 1; }
            }
            return String.Format("JD {0}", value);
        }
    }

    abstract class SimpleCalendar : Calendar {
        protected class Month {
            public String name;
            public uint days;
            public bool isVirtual;

            public Month(String name, uint days, bool isVirtual = false) {
                this.name = name;
                this.days = days;
                this.isVirtual = isVirtual;
            }
        }

        protected class Date {
            public long year;
            public uint month;
            public uint day;

            public Date(long year, uint month, uint day) {
                this.year = year;
                this.month = month;
                this.day = day;
            }
        }

        protected Month[] months;
        protected String[] days;
        protected String dateFormat;

        public SimpleCalendar() {
            this.dateFormat = "";
        }

        public override Timestamp newTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Interval precision = Interval.second) {
            Decimal value = 0, dayLength = this.getIntervalLength(value, Interval.day);
            while (month > this.months.Length) {
                year += 1;
                month -= (uint)(this.months.Length);
            }
            if (precision >= Interval.year) { value += year * this.getIntervalLength(value, Interval.year); }
            if (precision >= Interval.month) {
                for (month -= 1; month > 0; month -= 1) {
                    value += this.months[month].days * dayLength;
                }
            }
            if (precision == Interval.week) { value += week * this.getIntervalLength(value, Interval.week); }
            if (precision >= Interval.day) { value += (day - 1) * dayLength; }
            if (precision >= Interval.time) { value += hour * this.getIntervalLength(value, Interval.hour); }
            if (precision >= Interval.minute) { value += hour * this.getIntervalLength(value, Interval.minute); }
            if (precision >= Interval.second) { value += hour * this.getIntervalLength(value, Interval.second); }
            return this.newTimestamp(value, precision);
        }

        public override String timeSpanToString(TimeSpan s) {
/////
//
            throw new NotImplementedException();
//
/////
        }

        public override String timestampToString(Timestamp t, bool date = true, bool time = false) {
            String retval = "";
            if (t.precision <= Interval.day) { time = false; }
            if (time == false) { date = true; }
            if (date) {
                String weekday = this.getWeekday(t);
                if (weekday != null) { retval += weekday + ", "; }
                Date d = this.getDate(t);
                retval += String.Format(this.dateFormat, d.day, this.months[d.month - 1].name, d.year);
            }
            if (date && time) { retval += " "; }
            if (time) {
                int hours = this.getTime(t);
                int seconds = hours % 60;
                hours /= 60;
                int minutes = hours % 60;
                hours /= 60;
                if (t.precision == Interval.time) {
                    retval += this.getFuzzyTime(hours);
                }
                else {
                    retval += String.Format("{0:D2}", hours);
                    if (t.precision >= Interval.minute) { retval += String.Format(":{0:D2}", minutes); }
                    if (t.precision >= Interval.second) { retval += String.Format(":{0:D2}", seconds); }
                }
            }
            return retval;
        }

        public override Timestamp add(Timestamp t, long amount, Interval unit = Interval.second) {
            if (unit == Interval.time) { return null; } // can't add time of day
            if (amount == 0) { return t; }
            Decimal value = t.value;
            if (this.intervalLengthConstant(unit)) {
                value += amount * this.getIntervalLength(value, unit);
            }
            else {
                while (amount > 0) {
                    value += this.getIntervalLength(value, unit, true);
                    amount -= 1;
                }
                while (amount < 0) {
                    value -= this.getIntervalLength(value, unit, false);
                    amount += 1;
                }
            }
            return this.newTimestamp(value, t.precision);
        }

        protected virtual String getWeekday(Timestamp t) {
            return null;
        }

        protected virtual Date getDate(Timestamp t) {
            Decimal value = t.value;
            Decimal yearLength = this.getIntervalLength(t.value, Interval.year);
            Decimal dayLength = this.getIntervalLength(t.value, Interval.day);
            long year = Decimal.ToInt64(Math.Floor(t.value / yearLength));
            if (year < 0) { year -= 1; } // so that negative value => negative year, but still positive month and day
            value -= year * yearLength;
            uint month = 0;
            for (Decimal monthLength = this.months[month].days * dayLength; value >= monthLength; monthLength = this.months[month].days * dayLength) {
                value -= monthLength;
                month += 1;
            }
            uint day = Decimal.ToUInt32(Math.Floor(value / dayLength));
            return new Date(year, month + 1, day + 1);
        }

        protected virtual int getTime(Timestamp t) {
            Decimal dayLength = this.getIntervalLength(t.value, Interval.day);
            Decimal retval = t.value % dayLength;
            if (retval < 0) { retval += dayLength; }
            return Decimal.ToInt32(retval);
        }

        protected virtual Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
            Decimal l = 0;
            switch (unit) {
            case Interval.year:
                foreach (Month month in this.months) {
                    l += month.days;
                }
                return l * this.getIntervalLength(value, Interval.day, forwards);
            case Interval.month:
                Date d = this.getDate(this.newTimestamp(value));
                d.month -= 1;
                if (!forwards) { d.month -= 1; }
                if (d.month < 0) { d.month += (uint)(this.months.Length); }
                return this.months[d.month].days * this.getIntervalLength(value, Interval.day, forwards);
            case Interval.week:
                return 7 * this.getIntervalLength(value, Interval.day, forwards);
            case Interval.day:
                return 24 * this.getIntervalLength(value, Interval.hour, forwards);
            case Interval.hour:
                return 60 * this.getIntervalLength(value, Interval.minute, forwards);
            case Interval.minute:
                return 60 * this.getIntervalLength(value, Interval.second, forwards);
            case Interval.second:
                return 1;
            }
            return 0;
        }

        protected virtual bool intervalLengthConstant(Interval unit) {
            return unit != Interval.month;
        }
    }

    class GreyhawkCalendar : SimpleCalendar {
        public GreyhawkCalendar() {
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
            this.days = new String[] { "Starday", "Sunday", "Moonday", "Godsday", "Waterday", "Earthday", "Freeday" };
            this.dateFormat = "{0} {1}, {2} CY";
        }

        public override Timestamp newTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Interval precision = Interval.second) {
            if (year > 0) { year -= 1; } // there's no year 0 in common reckoning, so decrement positive years
            return base.newTimestamp(year, month, week, day, hour, minute, second, precision);
        }

        public override Timestamp defaultTimestamp() {
            return this.newTimestamp(591, 1, 0, 1, 12, 0, 0, Interval.time);
        }

        public override Timestamp add(Timestamp t, long amount, Interval unit = Interval.second) {
            if (unit == Interval.month) {
                // convert months to days, then fall through to base logic for days
                uint month = this.getDate(t).month - 1, days = 0;
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
                    if (month < 0) { month = (uint)(this.months.Length - 1); }
                    days -= this.months[month].days;
                    if (this.months[month].isVirtual) {
                        // skip over festivals
                        month -= 1;
                        if (month < 0) { month = (uint)(this.months.Length - 1); }
                        days -= this.months[month].days;
                    }
                }
                amount = days;
                unit = Interval.day;
            }
            return base.add(t, amount, unit);
        }

        protected override String getWeekday(Timestamp t) {
            Date d = this.getDate(t);
            if (this.months[d.month - 1].isVirtual) { return null; }
            return this.days[(d.day - 1) % this.days.Length];
        }

        protected override Date getDate(Timestamp t) {
            Date d = base.getDate(t);
            if (d.year > 0) {
                d.year += 1; // there's no year 0 in common reckoning, so increment non-negative years
            }
            return d;
        }
    }

    class EberronCalendar : SimpleCalendar {
        public EberronCalendar() {
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
            this.days = new String[] { "Sul", "Mol", "Zol", "Wir", "Zor", "Far", "Sar" };
            this.dateFormat = "{0} {1}, {2} YK";
        }

        public override Timestamp defaultTimestamp() {
            return this.newTimestamp(998, 1, 0, 1, 12, 0, 0, Interval.time);
        }

        protected override String getWeekday(Timestamp t) {
            Date d = this.getDate(t);
            return this.days[(d.day - 1) % this.days.Length];
        }

        protected override Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
            if (unit == Interval.month) { return 28 * this.getIntervalLength(value, Interval.day, forwards); }
            return base.getIntervalLength(value, unit, forwards);
        }

        protected override bool intervalLengthConstant(Interval unit) {
            return true;
        }
    }

    abstract class LeapCalendar : SimpleCalendar {
        protected uint? leapMonth;
        protected uint cycleYears;
        protected uint cycleLeapDays;
        protected Decimal? cycleLength;
        protected uint? baseYearDays;

        public override Timestamp newTimestamp(long year, uint month, uint week, uint day, uint hour, uint minute, uint second, Interval precision = Interval.second) {
            Decimal value = 0, dayLength = this.getIntervalLength(value, Interval.day);
            while (month > this.months.Length) {
                year += 1;
                month -= (uint)(this.months.Length);
            }
            bool wasLeapYear = this.isLeapYear(year);
            if (precision >= Interval.year) {
                if (this.intervalLengthConstant(Interval.year)) {
                    value += year * this.getIntervalLength(value, Interval.year);
                }
                else {
                    if (this.cycleYears > 0) {
                        while (year >= this.cycleYears) {
                            value += this.getCycleLength();
                            year -= this.cycleYears;
                        }
                        while (year < 0) {
                            value -= this.getCycleLength();
                            year += this.cycleYears;
                        }
                    }
                    while (year > 0) {
                        year -= 1;
                        value += this.getYearLength(year);
                    }
                    while (year < 0) {
                        value -= this.getYearLength(year);
                        year += 1;
                    }
                }
            }
            if (precision >= Interval.month) {
                if ((wasLeapYear) && (month > this.leapMonth)) {
                    value += dayLength;
                }
                for (month -= 1; month > 0; month -= 1) {
                    value += this.months[month - 1].days * dayLength;
                }
            }
            if (precision == Interval.week) { value += week * this.getIntervalLength(value, Interval.week); }
            if (precision >= Interval.day) { value += (day - 1) * dayLength; }
            if (precision >= Interval.time) { value += hour * this.getIntervalLength(value, Interval.hour); }
            if (precision >= Interval.minute) { value += hour * this.getIntervalLength(value, Interval.minute); }
            if (precision >= Interval.second) { value += hour * this.getIntervalLength(value, Interval.second); }
            return this.newTimestamp(value, precision);
        }

        public override String timeSpanToString(TimeSpan s) {
/////
//
            throw new NotImplementedException();
//
/////
        }

        public override Timestamp add(Timestamp t, long amount, Interval unit = Interval.second) {
            if ((unit == Interval.year) && (!this.intervalLengthConstant(Interval.year))) {
                Decimal value = t.value;
                Date d = this.getDate(t);
                if (this.cycleYears > 0) {
                    while (amount >= this.cycleYears) {
                        value += this.getCycleLength();
                        amount -= this.cycleYears;
                    }
                    while (amount < 0) {
                        value -= this.getCycleLength();
                        amount += this.cycleYears;
                    }
                }
                while (amount > 0) {
                    value += this.getYearLength(d.year + amount);
                    amount -= 1;
                }
                while (amount < 0) {
                    value -= this.getYearLength(d.year + amount);
                    amount += 1;
                }
                return this.newTimestamp(value, t.precision);
            }
            return base.add(t, amount, unit);
        }

        public virtual bool isLeapYear(long year) {
            return false;
        }

        public virtual Decimal getCycleLength() {
            if (this.cycleLength == null) {
                this.cycleLength = (this.cycleYears * this.getBaseYearDays() + this.cycleLeapDays) * this.getIntervalLength(0, Interval.day);
            }
            return (Decimal)(this.cycleLength);
        }

        protected virtual int getBaseYearDays() {
            if (this.baseYearDays == null) {
                this.baseYearDays = 0;
                foreach (Month m in this.months) {
                    this.baseYearDays += m.days;
                }
            }
            return (int)(this.baseYearDays);
        }

        protected virtual Decimal getYearLength(long year) {
            return (this.getBaseYearDays() + (this.isLeapYear(year) ? 1 : 0)) * this.getIntervalLength(0, Interval.day);
        }

        protected virtual Decimal getMonthLength(long year, uint month) {
            uint days = this.months[month - 1].days;
            if ((this.isLeapYear(year)) && (month == this.leapMonth)) { days += 1; }
            return days * this.getIntervalLength(0, Interval.day);
        }

        protected override Date getDate(Timestamp t) {
            Decimal value = t.value;
            long year = 0;
            if (this.cycleYears > 0) {
                while (value >= this.getCycleLength()) {
                    year += this.cycleYears;
                    value -= this.getCycleLength();
                }
                while (value < 0) {
                    year -= this.cycleYears;
                    value += this.getCycleLength();
                }
            }
            for (Decimal yl = this.getYearLength(year); value >= yl; yl = this.getYearLength(year)) {
                year += 1;
                value -= yl;
            }
            for (Decimal yl = this.getYearLength(year - 1); value < 0; yl = this.getYearLength(year - 1)) {
                year -= 1;
                value += yl;
            }
            uint month = 0;
            for (Decimal ml = this.getMonthLength(year, month + 1); value >= ml; ml = this.getMonthLength(year, month + 1)) {
                value -= ml;
                month += 1;
            }
            uint day = Decimal.ToUInt32(Math.Floor(value / this.getIntervalLength(t.value, Interval.day)));
            return new Date(year, month + 1, day + 1);
        }

        protected override Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
            if (unit <= Interval.month) {
                Date d = this.getDate(this.newTimestamp(value));
                uint days;
                if (unit == Interval.year) {
                    return this.getYearLength(d.year);
                }
                d.month -= 1;
                if (!forwards) { d.month = (uint)((d.month + this.months.Length - 1) % this.months.Length); }
                days = this.months[d.month].days;
                if ((d.month + 1) == this.leapMonth) { days += 1; }
                return days * this.getIntervalLength(value, Interval.day, forwards);
            }
            return base.getIntervalLength(value, unit, forwards);
        }

        protected override bool intervalLengthConstant(Interval unit) {
            return unit > Interval.month;
        }
    }

    class FRCalendar : LeapCalendar {
        public FRCalendar() {
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
            this.leapMonth = 11;
            this.cycleYears = 4;
            this.cycleLeapDays = 1;
        }

        public override Timestamp defaultTimestamp() {
            return this.newTimestamp(1491, 1, 0, 1, 12, 0, 0, Interval.time);
        }

        public override bool isLeapYear(long year) {
            return (year % 4) == 0;
        }

        protected override Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
            Date d;
            Decimal retval;
            if (unit == Interval.month) {
                d = this.getDate(this.newTimestamp(value));
                retval = this.getMonthLength(d.year, d.month);
                // skip over festivals
                while (this.months[d.month - 1].isVirtual) {
                    d.month += 1;
                    if (d.month > this.months.Length) { d.month = 1; } // no end-of-year festival, so we should never need this
                    retval += this.getMonthLength(d.year, d.month);
                }
                return retval;
            }
            if (unit == Interval.week) {
                d = this.getDate(this.newTimestamp(value));
                retval = 10 * this.getIntervalLength(value, Interval.day, forwards);
                // skip over festivals
                while (this.months[d.month - 1].isVirtual) {
                    d.month += 1;
                    if (d.month > this.months.Length) { d.month = 1; } // no end-of-year festival, so we should never need this
                    retval += this.getMonthLength(d.year, d.month);
                }
                return retval;
            }
            return base.getIntervalLength(value, unit, forwards);
        }

        protected override bool intervalLengthConstant(Interval unit) {
            return unit > Interval.week;
        }
    }

#if false
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
#endif


    static class Calendars {
        private const String defaultCalendar = "Campaign Date";

        private class CalImpl {
            public Func<Calendar> calendar;

            public CalImpl(Func<Calendar> calendar) {
                this.calendar = calendar;
            }
        }

        private static Dictionary<String, CalImpl> calendars = new Dictionary<string, CalImpl>() {
            { "Greyhawk", new CalImpl( () => new GreyhawkCalendar() ) },
            { "Eberron", new CalImpl( () => new EberronCalendar() ) },
            { "Forgotten Realms", new CalImpl( () => new FRCalendar() ) },
            //{ "Gregorian", new CalImpl( () => new GregorianCalendar() ) },
            { "Julian", new CalImpl( () => new JulianCalendar() ) },
            { defaultCalendar, new CalImpl( () => new CampaignCalendar() ) }
        };

        public static ICollection<String> getCalendars() {
            return calendars.Keys;
        }

        public static Calendar newCalendar(String calendar) {
            return (calendars.ContainsKey(calendar) ? calendars[calendar] : calendars[defaultCalendar]).calendar.Invoke();
        }
    }
}
