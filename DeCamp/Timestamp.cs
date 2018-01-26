using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using GUIx;

namespace DeCamp {
    class TimeSpan {
        protected readonly Calendar calendar;
        public readonly Decimal value;

        public TimeSpan(Calendar calendar, Decimal value) {
            this.calendar = calendar;
            this.value = value;
        }

        public virtual String toString(bool precise = false) {
            return this.calendar.timeSpanToString(this, precise);
        }
    }

    class Timestamp : IComparable {
        public readonly Calendar calendar;
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

        public virtual String timeSpanToString(TimeSpan s, bool precise = false) {
            String retval = "";
            Decimal value = s.value;
            bool gotStep = false;
            if (value < 0) {
                retval += "-";
                value = -value;
            }
            Tuple<Interval, String>[] unitSpecs = new Tuple<Interval, String>[] {
                new Tuple<Interval, String>(Interval.year, "year"),
                new Tuple<Interval, String>(Interval.month, "month"),
                new Tuple<Interval, String>(Interval.week, "week"),
                new Tuple<Interval, String>(Interval.day, "day"),
                new Tuple<Interval, String>(Interval.hour, "hour"),
                new Tuple<Interval, String>(Interval.minute, "minute"),
                new Tuple<Interval, String>(Interval.second, "second"),
            };
            foreach (Tuple<Interval, String> us in unitSpecs) {
                Decimal len = this.getSpanLength(us.Item1);
                Decimal stepRaw = value / len;
                if ((gotStep) && (!precise)) {
                    if (stepRaw > 0) {
                        retval += String.Format(" {0:0.#} {1}", stepRaw, us.Item2);
                        if (stepRaw != 1) { retval += "s"; }
                    }
                    break;
                }
                ulong step = (ulong)stepRaw;
                if (step > 0) {
                    if (gotStep) { retval += " "; }
                    retval += String.Format("{0} {1}", step, us.Item2);
                    if (step != 1) { retval += "s"; }
                    value -= step * len;
                    gotStep = true;
                }
            }
            if (!gotStep) {
                retval = String.Format("{0} second", s.value);
                if (s.value != 1) { retval += "s"; }
            }
            return retval;
        }

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

        protected abstract Decimal getSpanLength(Interval unit);
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

        protected override Decimal getSpanLength(Interval unit) {
            Decimal retval = 1;
            switch (unit) {
            case Interval.year:
                retval *= this.yearDays;
                goto case Interval.day;
            case Interval.month:
                retval *= this.monthDays;
                goto case Interval.day;
            case Interval.week:
                retval *= this.weekDays;
                goto case Interval.day;
            case Interval.day:
                retval *= this.dayHours;
                goto case Interval.hour;
            case Interval.hour:
                retval *= this.hourMins;
                goto case Interval.minute;
            case Interval.minute:
                retval *= this.minuteSecs;
                goto case Interval.second;
            case Interval.second:
                break;
            default:
                throw new ArgumentException("Cannot get length of unit " + unit);
            }
            return retval;
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
            return this.newTimestamp(2458119m * this.dayHours * this.hourMins * this.minuteSecs, Interval.time);
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
        public class Month {
            public readonly String name;
            public readonly uint days;
            public readonly bool isVirtual;

            public Month(String name, uint days, bool isVirtual = false) {
                this.name = name;
                this.days = days;
                this.isVirtual = isVirtual;
            }
        }

        public class Date {
            public long year;
            public uint month;
            public uint day;

            public Date(long year, uint month, uint day) {
                this.year = year;
                this.month = month;
                this.day = day;
            }
        }

        public Month[] months;
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
            if ((precision >= Interval.month) && (month > 0)) {
                for (month -= 1; month > 0; month -= 1) {
                    value += this.months[month - 1].days * dayLength;
                }
            }
            if (precision == Interval.week) { value += week * this.getIntervalLength(value, Interval.week); }
            if (precision >= Interval.day) { value += (day - 1) * dayLength; }
            if (precision >= Interval.time) { value += hour * this.getIntervalLength(value, Interval.hour); }
            if (precision >= Interval.minute) { value += minute * this.getIntervalLength(value, Interval.minute); }
            if (precision >= Interval.second) { value += second * this.getIntervalLength(value, Interval.second); }
            return this.newTimestamp(value, precision);
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

        protected override Decimal getSpanLength(Interval unit) {
            Decimal retval = 1;
            uint dayCount = 0, monthCount = 0;
            switch (unit) {
            case Interval.year:
                foreach (Month month in this.months) {
                    dayCount += month.days;
                }
                retval *= dayCount;
                goto case Interval.day;
            case Interval.month:
                foreach (Month month in this.months) {
                    dayCount += month.days;
                    if (!month.isVirtual) { monthCount += 1; }
                }
                if (monthCount == 0) { monthCount = (uint)(this.months.Length); }
                retval *= dayCount / monthCount;
                goto case Interval.day;
            case Interval.week:
                retval *= this.days.Length;
                goto case Interval.day;
            case Interval.day:
                retval *= 24;
                goto case Interval.hour;
            case Interval.hour:
                retval *= 60;
                goto case Interval.minute;
            case Interval.minute:
                retval *= 60;
                goto case Interval.second;
            case Interval.second:
                break;
            default:
                throw new ArgumentException("Cannot get length of unit " + unit);
            }
            return retval;
        }

        public virtual String getWeekday(Timestamp t) {
            return null;
        }

        public virtual Date getDate(Timestamp t) {
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

        public virtual int getTime(Timestamp t) {
            Decimal dayLength = this.getIntervalLength(t.value, Interval.day);
            Decimal retval = t.value % dayLength;
            if (retval < 0) { retval += dayLength; }
            return Decimal.ToInt32(retval);
        }

        public virtual Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
            Decimal l = 0;
            switch (unit) {
            case Interval.year:
                foreach (Month month in this.months) {
                    l += month.days;
                }
                return l * this.getSpanLength(Interval.day);
            case Interval.month:
                Date d = this.getDate(this.newTimestamp(value));
                d.month -= 1;
                if (!forwards) { d.month -= 1; }
                if (d.month < 0) { d.month += (uint)(this.months.Length); }
                return this.months[d.month].days * this.getSpanLength(Interval.day);
            case Interval.week:
            case Interval.day:
            case Interval.hour:
            case Interval.minute:
            case Interval.second:
                return value * this.getSpanLength(unit);
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

        protected override Decimal getSpanLength(Interval unit) {
            if (unit == Interval.month) { return 28 * this.getSpanLength(Interval.day); }
            return base.getSpanLength(unit);
        }

        public override String getWeekday(Timestamp t) {
            Date d = this.getDate(t);
            if (this.months[d.month - 1].isVirtual) { return null; }
            return this.days[(d.day - 1) % this.days.Length];
        }

        public override Date getDate(Timestamp t) {
            Date d = base.getDate(t);
            if (d.year >= 0) {
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

        public override String getWeekday(Timestamp t) {
            Date d = this.getDate(t);
            return this.days[(d.day - 1) % this.days.Length];
        }

        public override Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
            if (unit == Interval.month) { return 28 * this.getIntervalLength(value, Interval.day, forwards); }
            return base.getIntervalLength(value, unit, forwards);
        }

        protected override bool intervalLengthConstant(Interval unit) {
            return true;
        }
    }

    abstract class LeapCalendar : SimpleCalendar {
        public uint? leapMonth;
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
            if (precision >= Interval.minute) { value += minute * this.getIntervalLength(value, Interval.minute); }
            if (precision >= Interval.second) { value += second * this.getIntervalLength(value, Interval.second); }
            return this.newTimestamp(value, precision);
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

        protected override Decimal getSpanLength(Interval unit) {
            if (unit == Interval.year) { return this.getCycleLength() / this.cycleYears; }
            if (unit == Interval.month) { return this.getCycleLength() / (this.cycleYears * this.months.Length); }
            return base.getSpanLength(unit);
        }

        public virtual bool isLeapYear(long year) {
            return false;
        }

        public virtual Decimal getCycleLength() {
            if (this.cycleLength == null) {
                this.cycleLength = (this.cycleYears * this.getBaseYearDays() + this.cycleLeapDays) * this.getSpanLength(Interval.day);
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

        public override Date getDate(Timestamp t) {
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

        public override Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
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

        protected override Decimal getSpanLength(Interval unit) {
            if (unit == Interval.month) { return 30 * this.getSpanLength(Interval.day); }
            return base.getSpanLength(unit);
        }

        public override bool isLeapYear(long year) {
            return (year % 4) == 0;
        }

        public override Decimal getIntervalLength(Decimal value, Interval unit, bool forwards = true) {
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

    class GregorianCalendar : LeapCalendar {
        public GregorianCalendar() {
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
            this.days = new String[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            this.dateFormat = "{1} {0}, {2}";
            this.leapMonth = 2;
            this.cycleYears = 400;
            this.cycleLeapDays = 97;
        }

        public override Timestamp defaultTimestamp() {
            return this.newTimestamp(2018, 1, 0, 1, 12, 0, 0, Interval.time);
        }

        public override string getWeekday(Timestamp t) {
            int day = Decimal.ToInt32(Math.Floor(t.value / this.getIntervalLength(t.value, Interval.day) - 1) % this.days.Length);
            if (day < 0) { day += this.days.Length; }
            return this.days[day];
        }

        public override bool isLeapYear(long year) {
            // for historical accuracy, we'd need this (which would make the cycle invalid for years between 45 BCE and 7 CE)
            //if (year < 8) {
            //    return (year >= -45) && (year <= -9) && (year % 3 == 0);
            //}
            if ((year % 4) != 0) { return false; }
            if ((year % 400) == 0) { return true; }
            return (year % 100) != 0;
        }
    }


    abstract class DatePickerDialog : Window {
        protected Calendar calendar;
        protected Grid dateGrid, precisionGrid;
        protected ComboBox precisionBox;
        public bool valid;

        public DatePickerDialog(Calendar calendar) {
            this.calendar = calendar;
            this.valid = false;
        }

        public virtual void populateDialog(String title, Timestamp t) {
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Title = title;
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            this.dateGrid = new Grid();
            this.populateDateGrid();
            Grid.SetRow(this.dateGrid, 0);
            Grid.SetColumn(this.dateGrid, 0);
            g.Children.Add(this.dateGrid);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            this.precisionGrid = new Grid();
            this.populatePrecisionGrid();
            Grid.SetRow(this.precisionGrid, 1);
            Grid.SetColumn(this.precisionGrid, 0);
            g.Children.Add(this.precisionGrid);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            Grid butGrid = new Grid();
            butGrid.HorizontalAlignment = HorizontalAlignment.Right;
            butGrid.ColumnDefinitions.Add(new ColumnDefinition());
            butGrid.ColumnDefinitions.Add(new ColumnDefinition());
            butGrid.RowDefinitions.Add(new RowDefinition());
            Button okBut = new Button();
            okBut.Content = "OK";
            okBut.Click += this.doOk;
            Grid.SetRow(okBut, 0);
            Grid.SetColumn(okBut, 0);
            butGrid.Children.Add(okBut);
            Button cancelBut = new Button();
            cancelBut.Content = "Cancel";
            cancelBut.Click += this.doCancel;
            Grid.SetRow(cancelBut, 0);
            Grid.SetColumn(cancelBut, 1);
            butGrid.Children.Add(cancelBut);
            Grid.SetRow(butGrid, 2);
            Grid.SetColumn(butGrid, 0);
            g.Children.Add(butGrid);
            this.setDefaultValues(t);
            this.Content = g;
        }

        protected abstract void populateDateGrid();

        protected virtual void populatePrecisionGrid() {
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.precisionGrid.ColumnDefinitions.Add(cd);
            this.precisionGrid.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.precisionGrid.RowDefinitions.Add(rd);
            Label promptBox = new Label();
            promptBox.Content = "Precision:";
            Grid.SetRow(promptBox, 0);
            Grid.SetColumn(promptBox, 0);
            this.precisionGrid.Children.Add(promptBox);
            this.precisionBox = new ComboBox();
            this.precisionBox.Items.Add("Year");
            this.precisionBox.Items.Add("Month");
            this.precisionBox.Items.Add("Week");
            this.precisionBox.Items.Add("Day");
            this.precisionBox.Items.Add("Time");
            this.precisionBox.Items.Add("Hour");
            this.precisionBox.Items.Add("Minute");
            this.precisionBox.Items.Add("Second");
            Grid.SetRow(this.precisionBox, 0);
            Grid.SetColumn(this.precisionBox, 1);
            this.precisionGrid.Children.Add(this.precisionBox);
        }

        protected virtual void setDefaultValues(Timestamp t) {
            this.precisionBox.SelectedIndex = (int)(t.precision);
        }

        private void doOk(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void doCancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public abstract Timestamp getTimestamp();

        protected virtual Calendar.Interval getPrecision() {
            return (Calendar.Interval)(this.precisionBox.SelectedIndex);
        }
    }

    class DayPickerDialog : DatePickerDialog {
        protected SpinBox dateBox;

        public DayPickerDialog(Calendar calendar) : base(calendar) { }

        protected override void populateDateGrid() {
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.dateGrid.ColumnDefinitions.Add(cd);
            this.dateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.dateGrid.RowDefinitions.Add(rd);
            Label promptBox = new Label();
            promptBox.Content = "Date:";
            Grid.SetRow(promptBox, 0);
            Grid.SetColumn(promptBox, 0);
            this.dateGrid.Children.Add(promptBox);
            this.dateBox = new SpinBox();
            Grid.SetRow(this.dateBox, 0);
            Grid.SetColumn(this.dateBox, 1);
            this.dateGrid.Children.Add(this.dateBox);
        }

        protected override void setDefaultValues(Timestamp t) {
            base.setDefaultValues(t);
            this.dateBox.Value = (double)(t.value);
        }

        public override Timestamp getTimestamp() {
            return this.calendar.newTimestamp((Decimal)(this.dateBox.Value), this.getPrecision());
        }
    }

    class JulianDatePickerDialog : DayPickerDialog {
        public JulianDatePickerDialog(Calendar calendar) : base(calendar) { }

        protected override void setDefaultValues(Timestamp t) {
            base.setDefaultValues(t);
            this.dateBox.Value = (double)(t.value / (24 * 60 * 60));
        }

        public override Timestamp getTimestamp() {
            return this.calendar.newTimestamp(((Decimal)(this.dateBox.Value)) * 24 * 60 * 60, this.getPrecision());
        }
    }

    class DayTimePickerDialog : DayPickerDialog {
        protected ComboBox hourBox, minBox, secBox;

        public DayTimePickerDialog(Calendar calendar) : base(calendar) { }

        protected override void populateDateGrid() {
            base.populateDateGrid();
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.dateGrid.RowDefinitions.Add(rd);
            Label promptBox = new Label();
            promptBox.Content = "Time:";
            Grid.SetRow(promptBox, 1);
            Grid.SetColumn(promptBox, 0);
            this.dateGrid.Children.Add(promptBox);
            Grid timeGrid = new Grid();
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            timeGrid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            timeGrid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            timeGrid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            timeGrid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            timeGrid.ColumnDefinitions.Add(cd);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            timeGrid.RowDefinitions.Add(rd);
            this.hourBox = new ComboBox();
            for (int i = 0; i < 24; i++) { this.hourBox.Items.Add(String.Format("{0:D2}", i)); }
            Grid.SetRow(this.hourBox, 0);
            Grid.SetColumn(this.hourBox, 0);
            timeGrid.Children.Add(this.hourBox);
            Label l = new Label();
            l.Content = ":";
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 1);
            timeGrid.Children.Add(l);
            this.minBox = new ComboBox();
            for (int i = 0; i < 60; i++) { this.minBox.Items.Add(String.Format("{0:D2}", i)); }
            Grid.SetRow(this.minBox, 0);
            Grid.SetColumn(this.minBox, 2);
            timeGrid.Children.Add(this.minBox);
            l = new Label();
            l.Content = ":";
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 3);
            timeGrid.Children.Add(l);
            this.secBox = new ComboBox();
            for (int i = 0; i < 60; i++) { this.secBox.Items.Add(String.Format("{0:D2}", i)); }
            Grid.SetRow(this.secBox, 0);
            Grid.SetColumn(this.secBox, 4);
            timeGrid.Children.Add(this.secBox);
            Grid.SetRow(timeGrid, 1);
            Grid.SetColumn(timeGrid, 1);
            this.dateGrid.Children.Add(timeGrid);
        }

        protected override void setDefaultValues(Timestamp t) {
            base.setDefaultValues(t);
            Decimal value = t.value; // value is in seconds
            this.secBox.SelectedIndex = Decimal.ToInt32(((value % 60) + 60) % 60); // double-modulus because c#'s "%n" can yield -(n-1) to (n-1)
            value = Math.Floor(value / 60); // value now in minutes
            this.minBox.SelectedIndex = Decimal.ToInt32(((value % 60) + 60) % 60);
            value = Math.Floor(value / 60); // value now in hours
            this.hourBox.SelectedIndex = Decimal.ToInt32(((value % 24) + 24) % 24);
            value = Math.Floor(value / 24); // value now in days
            this.dateBox.Value = (double)(value);
        }

        public override Timestamp getTimestamp() {
            Decimal value = (Decimal)(this.dateBox.Value) * 24; // value is in hours
            value += this.hourBox.SelectedIndex;
            value *= 60; // value now in minutes
            value += this.minBox.SelectedIndex;
            value *= 60; // value now in seconds
            value += this.secBox.SelectedIndex;
            return this.calendar.newTimestamp(value, this.getPrecision());
        }
    }

    class SimpleDatePickerDialog : DatePickerDialog {
        public enum DateOrder { DMY, MDY, YMD };

        DateOrder order;
        protected SpinBox yearBox;
        protected ComboBox monthBox, dayBox, hourBox, minBox, secBox;

        public SimpleDatePickerDialog(Calendar calendar, DateOrder order = DateOrder.DMY) : base(calendar) {
            this.order = order;
        }

        protected override void populateDateGrid() {
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.dateGrid.ColumnDefinitions.Add(cd);
            this.dateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.dateGrid.RowDefinitions.Add(rd);
            Label l = new Label();
            l.Content = "Date:";
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 0);
            this.dateGrid.Children.Add(l);
            Grid g = new Grid();
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            this.yearBox = new SpinBox();
            Grid.SetRow(this.yearBox, 0);
            Grid.SetColumn(this.yearBox, (this.order == DateOrder.YMD ? 0 : 2));
            g.Children.Add(this.yearBox);
            this.monthBox = new ComboBox();
            SimpleCalendar cal = (SimpleCalendar)(this.calendar);
            int maxDays = 0;
            for (int i = 0; i < cal.months.Length; i++) {
                this.monthBox.Items.Add(cal.months[i].name);
                if (cal.months[i].days > maxDays) { maxDays = (int)(cal.months[i].days); }
            }
            this.monthBox.SelectionChanged += this.monthChanged;
            Grid.SetRow(this.monthBox, 0);
            Grid.SetColumn(this.monthBox, (this.order == DateOrder.MDY ? 0 : 1));
            g.Children.Add(this.monthBox);
            this.dayBox = new ComboBox();
            for (int i = 0; i < maxDays; i++) {
                this.dayBox.Items.Add(String.Format("{0:D2}", i + 1));
            }
            Grid.SetRow(this.dayBox, 0);
            Grid.SetColumn(this.dayBox, (int)(this.order));
            g.Children.Add(this.dayBox);
            Grid.SetRow(g, 0);
            Grid.SetColumn(g, 1);
            this.dateGrid.Children.Add(g);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.dateGrid.RowDefinitions.Add(rd);
            l = new Label();
            l.Content = "Time:";
            Grid.SetRow(l, 1);
            Grid.SetColumn(l, 0);
            this.dateGrid.Children.Add(l);
            g = new Grid();
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            this.hourBox = new ComboBox();
            for (int i = 0; i < 24; i++) { this.hourBox.Items.Add(String.Format("{0:D2}", i)); }
            Grid.SetRow(this.hourBox, 0);
            Grid.SetColumn(this.hourBox, 0);
            g.Children.Add(this.hourBox);
            l = new Label();
            l.Content = ":";
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 1);
            g.Children.Add(l);
            this.minBox = new ComboBox();
            for (int i = 0; i < 60; i++) { this.minBox.Items.Add(String.Format("{0:D2}", i)); }
            Grid.SetRow(this.minBox, 0);
            Grid.SetColumn(this.minBox, 2);
            g.Children.Add(this.minBox);
            l = new Label();
            l.Content = ":";
            Grid.SetRow(l, 0);
            Grid.SetColumn(l, 3);
            g.Children.Add(l);
            this.secBox = new ComboBox();
            for (int i = 0; i < 60; i++) { this.secBox.Items.Add(String.Format("{0:D2}", i)); }
            Grid.SetRow(this.secBox, 0);
            Grid.SetColumn(this.secBox, 4);
            g.Children.Add(this.secBox);
            Grid.SetRow(g, 1);
            Grid.SetColumn(g, 1);
            this.dateGrid.Children.Add(g);
        }

        protected override void setDefaultValues(Timestamp t) {
            base.setDefaultValues(t);
            SimpleCalendar.Date d = ((SimpleCalendar)(this.calendar)).getDate(t);
            this.yearBox.Value = d.year;
            this.monthBox.SelectedIndex = (int)(d.month - 1);
            this.dayBox.SelectedIndex = (int)(d.day - 1);
            int s = ((SimpleCalendar)(this.calendar)).getTime(t); // time of day in seconds
            this.secBox.SelectedIndex = s % 60;
            s /= 60; // s now in minutes
            this.minBox.SelectedIndex = s % 60;
            s /= 60; // s now in hours
            this.hourBox.SelectedIndex = s % 24; // s should already be in [0, 24), but one mod is cheap in user time scale
        }

        public override Timestamp getTimestamp() {
            long year = (long)(this.yearBox.Value);
            uint month = (uint)(this.monthBox.SelectedIndex) + 1, day = (uint)(this.dayBox.SelectedIndex) + 1;
            uint hour = (uint)(this.hourBox.SelectedIndex), min = (uint)(this.minBox.SelectedIndex), sec = (uint)(this.secBox.SelectedIndex);
            return this.calendar.newTimestamp(year, month, 0, day, hour, min, sec, this.getPrecision());
        }

        protected virtual void monthChanged(object sender, RoutedEventArgs e) {
            SimpleCalendar cal = (SimpleCalendar)(this.calendar);
            int month = this.monthBox.SelectedIndex;
            if ((month < 0) || (month >= cal.months.Length)) { return; }
            int day = this.dayBox.SelectedIndex;
            if (day >= cal.months[month].days) { day = (int)(cal.months[month].days) - 1; }
            this.dayBox.Items.Clear();
            for (int i = 0; i < cal.months[month].days; i++) {
                this.dayBox.Items.Add(String.Format("{0:D2}", i + 1));
            }
            if (day >= 0) { this.dayBox.SelectedIndex = day; }
        }
    }

    class LeapDatePickerDialog : SimpleDatePickerDialog {
        public LeapDatePickerDialog(Calendar calendar, DateOrder order = DateOrder.DMY) : base(calendar, order) { }

        protected override void populateDateGrid() {
            base.populateDateGrid();
            this.yearBox.ValueChanged += this.monthChanged;
        }

        protected override void monthChanged(object sender, RoutedEventArgs e) {
            LeapCalendar cal = (LeapCalendar)(this.calendar);
            long year = (long)(this.yearBox.Value);
            int month = this.monthBox.SelectedIndex;
            if ((month < 0) || (month >= cal.months.Length)) { return; }
            int monthDays = (int)(cal.months[month].days);
            if ((month + 1 == cal.leapMonth) && (cal.isLeapYear(year))) { monthDays += 1; }
            int day = this.dayBox.SelectedIndex;
            if (day >= monthDays) { day = monthDays - 1; }
            this.dayBox.Items.Clear();
            for (int i = 0; i < monthDays; i++) {
                this.dayBox.Items.Add(String.Format("{0:D2}", i + 1));
            }
            if (day >= 0) { this.dayBox.SelectedIndex = day; }
        }
    }


    static class Calendars {
        public const String defaultCalendar = "Campaign Date";

        private class CalImpl {
            public Func<Calendar> calendar;
            public Func<Calendar, DatePickerDialog> picker;

            public CalImpl(Func<Calendar> calendar, Func<Calendar, DatePickerDialog> picker) {
                this.calendar = calendar;
                this.picker = picker;
            }
        }

        private static SortedDictionary<String, CalImpl> calendars = new SortedDictionary<string, CalImpl>() {
            { "Greyhawk", new CalImpl( () => new GreyhawkCalendar(), (cal) => new SimpleDatePickerDialog(cal) ) },
            { "Eberron", new CalImpl( () => new EberronCalendar(), (cal) => new SimpleDatePickerDialog(cal) ) },
            { "Forgotten Realms", new CalImpl( () => new FRCalendar(), (cal) => new LeapDatePickerDialog(cal, SimpleDatePickerDialog.DateOrder.MDY) ) },
            { "Gregorian", new CalImpl( () => new GregorianCalendar(), (cal) => new LeapDatePickerDialog(cal, SimpleDatePickerDialog.DateOrder.MDY) ) },
            { "Julian", new CalImpl( () => new JulianCalendar(), (cal) => new JulianDatePickerDialog(cal) ) },
            { defaultCalendar, new CalImpl( () => new CampaignCalendar(), (cal) => new DayTimePickerDialog(cal) ) }
        };

        public static ICollection<String> getCalendars() {
            return calendars.Keys;
        }

        private static CalImpl getCalImpl(String calendar) {
            if (calendars.ContainsKey(calendar)) { return calendars[calendar]; }
            return calendars[defaultCalendar];
        }

        public static Calendar newCalendar(String calendar) {
            return getCalImpl(calendar).calendar.Invoke();
        }

        public static Timestamp askTimestamp(String calendar, String title, Timestamp t, Window owner = null) {
            DatePickerDialog dlg = getCalImpl(calendar).picker.Invoke(t.calendar);
            if (owner != null) {
                dlg.Owner = owner;
            }
            dlg.populateDialog(title, t);
            dlg.ShowDialog();
            if (!dlg.valid) { return null; }
            return dlg.getTimestamp();
        }
    }
}
