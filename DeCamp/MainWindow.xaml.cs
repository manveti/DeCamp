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
                new QueryPrompt("Calendar:", QueryType.LIST, values: Calendar.getCalendars().ToArray()),
                new QueryPrompt("Rule Set:", QueryType.LIST, values: new String[]{ "D&D 3.5" })
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
        //timeline handlers
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
