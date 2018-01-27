using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using GUIx;

namespace DeCamp {
    class Event {
        public readonly String type;
        public String owner;
        public EventResult parent;
        public TimeSpan duration;
        public DateTime timestamp;
        protected HashSet<String> viewers;
        protected HashSet<String> editors;
        public String title, description, notes;
        public bool isVirtual;
        public List<EventResult> results;

        public Event(String type, String creator, EventResult parent = null) {
            this.type = type;
            this.owner = creator;
            this.parent = parent;
            this.timestamp = DateTime.UtcNow;
        }

        public virtual bool canView(String player) {
            return (player == this.owner) || (player == Campaign.gmKey) || (this.viewers == null) || (this.viewers.Contains(player));
        }

        public virtual bool canViewNotes(String player) {
            return (player == this.owner) || (player == Campaign.gmKey);
        }

        public virtual bool canEdit(String player) {
            return (player == this.owner) || (player == Campaign.gmKey) || (this.editors == null) || (this.editors.Contains(player));
        }

        public virtual void apply(CampaignState s) {
            if (this.results == null) { return; }
            foreach (EventResult res in this.results) { res.apply(s); }
        }

        public virtual void revert(CampaignState s) {
            if (this.results == null) { return; }
            foreach (EventResult res in this.results) { res.revert(s); }
        }

        public virtual void addResult(EventResult r) {
            if (this.results == null) { this.results = new List<EventResult>(); }
            this.results.Add(r);
        }

        //insert, move, delete
    }

    abstract class EventResult {
        public readonly String type;
        public String creator;
        public Event parent;
        public String summary;
        public Event subEvent;

        public EventResult(String type, String creator, Event parent, String summary) {
            this.type = type;
            this.creator = creator;
            this.parent = parent;
            this.summary = summary;
        }

        public abstract void apply(CampaignState s);
        public abstract void revert(CampaignState s);
    }

    class CharacterAddResult : EventResult {
        public Character character;
        public String charId;

        public CharacterAddResult(String type, String creator, Event parent, String summary, Character character) : base(type, creator, parent, summary) {
            this.character = character;
        }

        public override void apply(CampaignState s) {
            this.charId = s.addCharacter(this.character);
        }

        public override void revert(CampaignState s) {
            s.removeCharacter(this.charId);
        }
    }

    class CharacterRemoveResult : EventResult {
        public Character character;
        public String charId;

        public CharacterRemoveResult(String type, String creator, Event parent, String summary, String charId) : base(type, creator, parent, summary) {
            this.charId = charId;
        }

        public override void apply(CampaignState s) {
            this.character = s.getCharacter(this.charId);
            s.removeCharacter(this.charId);
        }

        public override void revert(CampaignState s) {
            this.charId = s.addCharacter(this.character);
        }
    }

    abstract class AttributeMod {
        public String key;

        public AttributeMod(String key) {
            this.key = key;
        }

        public abstract void apply(Character c);
        public abstract void revert(Character c);
    }

    class AttributeAdjustment : AttributeMod {
        public object offset;

        public AttributeAdjustment(String key, object offset) : base(key) {
            this.offset = offset;
        }

        public override void apply(Character c) {
            c.adjustAttribute(this.key, this.offset);
        }

        public override void revert(Character c) {
            c.adjustAttribute(this.key, this.offset, true);
        }
    }

    class AttributeReplacement : AttributeMod {
        public object value;
        public Attribute.Type type;
        public Attribute oldValue;

        public AttributeReplacement(String key, object value, Attribute.Type type) : base(key) {
            this.value = value;
            this.type = type;
        }

        public override void apply(Character c) {
            this.oldValue = c.getRawAttribute(this.key);
            c.setAttribute(this.key, this.value, this.type);
        }

        public override void revert(Character c) {
            c.setRawAttribute(this.key, this.oldValue);
        }
    }

    class CharacterEditResult : EventResult {
        public String charId;
        public List<AttributeMod> modifications;

        public CharacterEditResult(String type, String creator, Event parent, String summary, String charId, List<AttributeMod> mods) : base(type, creator, parent, summary) {
            this.charId = charId;
            this.modifications = mods;
        }

        public override void apply(CampaignState s) {
            Character c = s.getCharacter(this.charId);
            foreach (AttributeMod m in this.modifications) {
                m.apply(c);
            }
        }

        public override void revert(CampaignState s) {
            Character c = s.getCharacter(this.charId);
            foreach (AttributeMod m in this.modifications) {
                m.revert(c);
            }
        }
    }

    //add/remove/edit party resources
    //add/remove/edit todo


    class EventDialog : Window {
        public Event evt;
        public Timestamp timestamp;
        protected readonly Campaign campaign;
        protected Grid mainGrid, parentGrid, descGrid, resGrid, adminGrid;
        protected TextBox startBox, endBox, durBox, tsBox;
        protected Entry titleBox, descBox, notesBox;
        public bool valid = false;

        public EventDialog(Campaign campaign, Event evt, Timestamp timestamp, String title, String player, Window owner = null) {
            this.evt = evt;
            this.timestamp = timestamp;
            this.campaign = campaign;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Title = title;
            if (owner != null) {
                this.Owner = owner;
            }
            ColumnDefinition cd;
            Label lbl;
            bool canEdit = this.evt.canEdit(player);
            this.mainGrid = new Grid();
            this.mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.mainGrid.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.mainGrid.ColumnDefinitions.Add(cd);
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.mainGrid.RowDefinitions.Add(rd);
            GroupBox grp = new GroupBox();
            this.parentGrid = new Grid();
            if (this.timestamp != null) {
                grp.Header = "Date/Time";
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
                grp.Header = "Parent";
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
            Grid g = new Grid();
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
            grp.Content = this.parentGrid;
            Grid.SetRow(grp, 0);
            Grid.SetColumn(grp, 0);
            Grid.SetColumnSpan(grp, 3);
            this.mainGrid.Children.Add(grp);
            rd = new RowDefinition();
            rd.Height = new GridLength(3, GridUnitType.Star);
            this.mainGrid.RowDefinitions.Add(rd);
            grp = new GroupBox();
            grp.Header = "Description";
            this.descGrid = new Grid();
            cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.descGrid.ColumnDefinitions.Add(cd);
            this.descGrid.ColumnDefinitions.Add(new ColumnDefinition());
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.descGrid.RowDefinitions.Add(rd);
            lbl = new Label();
            lbl.Content = "Title:";
            Grid.SetRow(lbl, 0);
            Grid.SetColumn(lbl, 0);
            this.descGrid.Children.Add(lbl);
            this.titleBox = new Entry();
            this.titleBox.Text = this.evt.title;
            Grid.SetRow(this.titleBox, 0);
            Grid.SetColumn(this.titleBox, 1);
            this.descGrid.Children.Add(this.titleBox);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.descGrid.RowDefinitions.Add(rd);
            lbl = new Label();
            lbl.Content = "Description:";
            Grid.SetRow(lbl, 1);
            Grid.SetColumn(lbl, 0);
            Grid.SetColumnSpan(lbl, 2);
            this.descGrid.Children.Add(lbl);
            this.descGrid.RowDefinitions.Add(new RowDefinition());
            this.descBox = new Entry();
            this.descBox.AcceptsReturn = true;
            this.descBox.MinLines = 5;
            this.descBox.TextWrapping = TextWrapping.Wrap;
            this.descBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.descBox.Text = this.evt.description;
            Grid.SetRow(this.descBox, 2);
            Grid.SetColumn(this.descBox, 0);
            Grid.SetColumnSpan(this.descBox, 2);
            this.descGrid.Children.Add(this.descBox);
/////
//
            //[notes label] if this.evt.canViewNotes(player)
            //[notes box] if this.evt.canViewNotes(player) (deal with notesBox if not canEdit)
//
/////
            grp.Content = this.descGrid;
            Grid.SetRow(grp, 1);
            Grid.SetColumn(grp, 0);
            Grid.SetColumnSpan(grp, 3);
            this.mainGrid.Children.Add(grp);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.mainGrid.RowDefinitions.Add(rd);
            GridSplitter spl = new GridSplitter();
            spl.HorizontalAlignment = HorizontalAlignment.Stretch;
            spl.VerticalAlignment = VerticalAlignment.Center;
            spl.Height = 2;
            Grid.SetRow(spl, 2);
            Grid.SetColumn(spl, 0);
            Grid.SetColumnSpan(spl, 3);
            this.mainGrid.Children.Add(spl);
            rd = new RowDefinition();
            rd.Height = new GridLength(1, GridUnitType.Star);
            this.mainGrid.RowDefinitions.Add(rd);
            grp = new GroupBox();
            grp.Header = "Results";
            this.resGrid = new Grid();
/////
//
            //interface for results: list, add, edit, remove
//
/////
            grp.Content = this.resGrid;
            Grid.SetRow(grp, 3);
            Grid.SetColumn(grp, 0);
            Grid.SetColumnSpan(grp, 3);
            this.mainGrid.Children.Add(grp);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.mainGrid.RowDefinitions.Add(rd);
            spl = new GridSplitter();
            spl.HorizontalAlignment = HorizontalAlignment.Stretch;
            spl.VerticalAlignment = VerticalAlignment.Center;
            spl.Height = 2;
            Grid.SetRow(spl, 4);
            Grid.SetColumn(spl, 0);
            Grid.SetColumnSpan(spl, 3);
            this.mainGrid.Children.Add(spl);
            rd = new RowDefinition();
            rd.Height = new GridLength(1, GridUnitType.Star);
            this.mainGrid.RowDefinitions.Add(rd);
            grp = new GroupBox();
            grp.Header = "Admin";
            this.adminGrid = new Grid();
/////
//
            //[owner label] [owner box] [claim button] [divider] [virtual checkbox]
            //view/edit permissions interface
//
/////
            grp.Content = this.adminGrid;
            Grid.SetRow(grp, 5);
            Grid.SetColumn(grp, 0);
            Grid.SetColumnSpan(grp, 3);
            this.mainGrid.Children.Add(grp);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            this.mainGrid.RowDefinitions.Add(rd);
            if (canEdit) {
                Button okBut = new Button();
                okBut.Content = "OK";
                okBut.Click += this.doOk;
                Grid.SetRow(okBut, 6);
                Grid.SetColumn(okBut, 1);
                this.mainGrid.Children.Add(okBut);
            }
            Button cancelBut = new Button();
            cancelBut.Content = "Cancel";
            cancelBut.Click += this.doCancel;
            Grid.SetRow(cancelBut, 6);
            Grid.SetColumn(cancelBut, 2);
            this.mainGrid.Children.Add(cancelBut);
            if (!canEdit) {
                tsBut.IsEnabled = false;
                this.titleBox.IsReadOnly = true;
                this.descBox.IsReadOnly = true;
/////
//
                //disable other edit stuff here
//
/////
            }
            this.Content = this.mainGrid;
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
            this.valid = true;
            this.Close();
        }

        protected void doCancel(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
