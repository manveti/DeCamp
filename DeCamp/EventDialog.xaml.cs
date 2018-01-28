using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

using GUIx;

namespace DeCamp {
    /// <summary>
    /// Interaction logic for EventDialog.xaml
    /// </summary>
    public partial class EventDialog : Window {
        public Event evt;
        public Timestamp timestamp;
        protected readonly Campaign campaign;
        protected TextBox startBox, endBox, durBox, tsBox;
        protected Entry notesBox;
        public bool valid = false;

        public EventDialog(Campaign campaign, Event evt, Timestamp timestamp, String title, String player, Window owner = null) {
            InitializeComponent();
            this.evt = evt;
            this.timestamp = timestamp;
            this.campaign = campaign;
            this.Title = title;
            if (owner != null) {
                this.Owner = owner;
            }
            bool canEdit = this.evt.canEdit(player);
            ColumnDefinition cd;
            RowDefinition rd;
            Label lbl;
            Grid g;
            if (this.timestamp != null) {
                this.parentGrp.Header = "Date/Time";
                if (this.evt.duration == null) {
                    this.evt.duration = new TimeSpan(this.timestamp.calendar, 0);
                }
                cd = new ColumnDefinition();
                cd.Width = GridLength.Auto;
                this.parentGrid.ColumnDefinitions.Add(cd);
                this.parentGrid.ColumnDefinitions.Add(new ColumnDefinition());
                cd = new ColumnDefinition();
                cd.Width = GridLength.Auto;
                this.parentGrid.ColumnDefinitions.Add(cd);
                this.parentGrid.ColumnDefinitions.Add(new ColumnDefinition());
                rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                this.parentGrid.RowDefinitions.Add(rd);
                Button startBut = new Button();
                startBut.Content = "Start:";
                startBut.Click += this.setStart;
                Grid.SetRow(startBut, 0);
                Grid.SetColumn(startBut, 0);
                this.parentGrid.Children.Add(startBut);
                this.startBox = new TextBox();
                this.startBox.IsReadOnly = true;
                this.startBox.Text = (this.timestamp - this.evt.duration).toString(true, true);
                Grid.SetRow(this.startBox, 0);
                Grid.SetColumn(this.startBox, 1);
                this.parentGrid.Children.Add(this.startBox);
                Button endBut = new Button();
                endBut.Content = "End:";
                endBut.Click += this.setEnd;
                Grid.SetRow(endBut, 0);
                Grid.SetColumn(endBut, 2);
                this.parentGrid.Children.Add(endBut);
                this.endBox = new TextBox();
                this.endBox.IsReadOnly = true;
                this.endBox.Text = this.timestamp.toString(true, true);
                Grid.SetRow(this.endBox, 0);
                Grid.SetColumn(this.endBox, 3);
                this.parentGrid.Children.Add(this.endBox);
                rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                this.parentGrid.RowDefinitions.Add(rd);
                Button durBut = new Button();
                durBut.Content = "Duration:";
                durBut.Click += this.setDuration;
                Grid.SetRow(durBut, 1);
                Grid.SetColumn(durBut, 0);
                this.parentGrid.Children.Add(durBut);
                this.durBox = new TextBox();
                this.durBox.IsReadOnly = true;
                this.durBox.Text = this.evt.duration.toString(true);
                Grid.SetRow(this.durBox, 1);
                Grid.SetColumn(this.durBox, 1);
                Grid.SetColumnSpan(this.durBox, 3);
                this.parentGrid.Children.Add(this.durBox);
                if (!canEdit) {
                    startBut.IsEnabled = false;
                    endBut.IsEnabled = false;
                    durBox.IsEnabled = false;
                }
            }
            else {
                this.parentGrp.Header = "Parent";
                this.parentGrid.ColumnDefinitions.Add(new ColumnDefinition());
                rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                this.parentGrid.RowDefinitions.Add(rd);
                lbl = new Label();
                if (this.evt != null) {
                    lbl.Content = this.evt.parent.parent.title;
                }
                else {
                    lbl.Content = "Event is in vault";
                }
                Grid.SetRow(lbl, 0);
                Grid.SetColumn(lbl, 0);
                this.parentGrid.Children.Add(lbl);
            }
            g = new Grid();
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.parentGrid.RowDefinitions.Add(rd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            g.ColumnDefinitions.Add(cd);
            g.ColumnDefinitions.Add(new ColumnDefinition());
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            Button tsBut = new Button();
            tsBut.Content = "Timestamp:";
/////
//
            //button click handler
//
/////
            Grid.SetRow(tsBut, 0);
            Grid.SetColumn(tsBut, 0);
            g.Children.Add(tsBut);
            this.tsBox = new TextBox();
            this.tsBox.IsReadOnly = true;
            this.tsBox.Text = this.evt.timestamp.ToLocalTime().ToString();
            Grid.SetRow(this.tsBox, 0);
            Grid.SetColumn(this.tsBox, 1);
            g.Children.Add(this.tsBox);
            Grid.SetRow(g, this.parentGrid.RowDefinitions.Count);
            Grid.SetColumn(g, 0);
            Grid.SetColumnSpan(g, this.parentGrid.ColumnDefinitions.Count);
            this.parentGrid.Children.Add(g);
/////
//
            //this.descGrid:
            //[notes label] if this.evt.canViewNotes(player)
            //[notes box] if this.evt.canViewNotes(player) (deal with notesBox if not canEdit)
            //anything dynamic for this.resGrid
//
/////
            this.viewersLst.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            if (this.evt.viewers == null) {
                this.viewersLst.Items.Add("<Everyone>");
            }
            else {
                foreach (String p in this.evt.viewers) {
                    this.viewersLst.Items.Add(this.getPlayer(p));
                }
            }
            this.viewersLst.Items.Refresh();
            this.editorsLst.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            if (this.evt.editors == null) {
                this.editorsLst.Items.Add("<Everyone>");
            }
            else {
                foreach (String p in this.evt.editors) {
                    this.editorsLst.Items.Add(this.getPlayer(p));
                }
            }
            this.editorsLst.Items.Refresh();
            if (!canEdit) {
                tsBut.IsEnabled = false;
                this.titleBox.IsReadOnly = true;
                this.descBox.IsReadOnly = true;
/////
//
                //disable results edit stuff
//
/////
                this.virtBox.IsEnabled = false;
/////
//
                //disable other edit stuff
//
/////
                this.okBut.Visibility = Visibility.Hidden;
                this.cancelBut.Content = "Done";
            }
            if ((!this.evt.canAssign(player)) && (!this.evt.canClaim(player))) {
                this.ownerBut.IsEnabled = false;
            }
            if (!this.evt.canSetPermissions(player)) {
                this.viewerAddBut.IsEnabled = false;
                this.viewerRemBut.IsEnabled = false;
                this.viewerAllBut.IsEnabled = false;
                this.viewerNoneBut.IsEnabled = false;
                this.editorAddBut.IsEnabled = false;
                this.editorRemBut.IsEnabled = false;
                this.editorAllBut.IsEnabled = false;
                this.editorNoneBut.IsEnabled = false;
            }
        }

        protected void setStart(object sender, RoutedEventArgs e) {
            Timestamp t = Calendars.askTimestamp(this.campaign.calendarName, "Event Start", this.timestamp - this.evt.duration, this);
            if (t == null) { return; }
            this.startBox.Text = t.toString(true, true);
            this.evt.duration = this.timestamp - t;
            this.durBox.Text = this.evt.duration.toString(true);
        }

        protected void setEnd(object sender, RoutedEventArgs e) {
            Timestamp t = Calendars.askTimestamp(this.campaign.calendarName, "Event End", this.timestamp, this);
            if (t == null) { return; }
            Timestamp startTime = this.timestamp - this.evt.duration;
            this.timestamp = t;
            this.endBox.Text = this.timestamp.toString(true, true);
            this.evt.duration = this.timestamp - startTime;
            this.durBox.Text = this.evt.duration.toString(true);
        }

        protected void setDuration(object sender, RoutedEventArgs e) {
            TimeSpan s = Calendars.askTimeSpan("Event Duration", this.evt.duration, this);
            if (s == null) { return; }
            Timestamp startTime = this.timestamp - this.evt.duration;
            this.evt.duration = s;
            this.timestamp = startTime + this.evt.duration;
            this.endBox.Text = this.timestamp.toString(true, true);
            this.durBox.Text = this.evt.duration.toString(true);
        }

        protected void doOk(object sender, RoutedEventArgs e) {
            this.evt.title = this.titleBox.Text;
            this.evt.description = this.descBox.Text;
/////
//
            //this.evt.notes = this.notesBox.Text;
//
/////
            this.evt.isVirtual = (bool)(this.virtBox.IsChecked);
            this.valid = true;
            this.Close();
        }

        protected void doCancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public virtual String getPlayer(String key) {
            if (key == null) { return "<System>"; }
            if (key == Campaign.gmKey) { return "<GM>"; }
            return this.campaign.getPlayer(key).name;
        }
    }
}
