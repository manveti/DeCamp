using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GUIx;

namespace DeCamp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Campaign campaign;

        public MainWindow() {
            InitializeComponent();
        }

        // File menu handlers
        public void newCampaign(object sender, RoutedEventArgs e) {
/////
//
            //prompt for existing unsaved campaign if necessary
//
/////
            QueryPrompt[] prompts = {
                new QueryPrompt("Campaign Name:", QueryType.STRING, "New Campaign"),
                new QueryPrompt("Calendar:", QueryType.LIST, Calendars.defaultCalendar, values: Calendars.getCalendars().ToArray()),
                new QueryPrompt("Rule Set:", QueryType.LIST, Rulesets.defaultRuleset, values: Rulesets.getRulesets().ToArray())
            };
            object[] values = SimpleDialog.askCompound("New Campaign", prompts, this);
            if (values == null) { return; }
/////
//
            this.campaign = new Campaign((String)values[0], "me", (String)values[1], (String)values[2]);
//
/////
            this.showCampaign();
        }

        //other menu handlers

        // Timeline handlers
        public void adjustDate(object sender, RoutedEventArgs e) {
            int amount = 1;
            Calendar.Interval unit;
            String lbl = (String)(((Button)sender).Content);
            if (lbl.Length != 2) { return; }
            if ((lbl[0] != '+') && (lbl[0] != '-')) { return; }
            switch (lbl[1]) {
            case '?':
                String[] intervals = new String[] { "year(s)", "month(s)", "week(s)", "day(s)", "hour(s)", "minute(s)", "second(s)" }; // time omitted
                QueryPrompt[] prompts = {
                    new QueryPrompt("", QueryType.INT, amount, 0),
                    new QueryPrompt("", QueryType.LIST, "month(s)", values: intervals)
                };
                object[] values = SimpleDialog.askCompound((lbl[0] == '+' ? "Advance" : "Rewind"), prompts, this);
                if (values == null) { return; }
                amount = (int)(values[0]);
                int idx = Array.IndexOf(intervals, values[1]);
                if ((idx < 0) || (idx >= intervals.Length)) { return; }
                if (idx >= (int)(Calendar.Interval.time)) { idx += 1; } // adjust idx because time was omitted from intervals
                unit = (Calendar.Interval)idx;
                break;
            case 'Y':
                unit = Calendar.Interval.year;
                break;
            case 'M':
                unit = Calendar.Interval.month;
                break;
            case 'D':
                unit = Calendar.Interval.day;
                break;
            case 'H':
                unit = Calendar.Interval.hour;
                break;
            default:
                return;
            }
            if (lbl[0] == '-') { amount = -amount; }
            this.campaign.adjustTimestamp(amount, unit);
            this.showCampaign();
        }

        public void setDate(object sender, RoutedEventArgs e) {
            Timestamp t = Calendars.askTimestamp(this.campaign.calendarName, "Date", this.campaign.getTimestamp(), this);
            if (t == null) { return; }
            this.campaign.setTimestamp(t);
            this.showCampaign();
        }

        //other timeline handlers
        //party handlers
        //journal handlers

        private void showCampaign() {
            if (this.campaign == null) { return; }
            this.Title = this.campaign.name + " - Aide de Campaign";
            this.timestampBox.Content = this.campaign.getTimestamp().toString(true, true);
            //timeline
            //party
            //journal
        }
    }
}
