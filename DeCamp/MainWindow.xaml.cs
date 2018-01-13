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
            //prompt for existing campaign
            //prompt for campaign name, calendar, ruleset, etc.
            this.campaign = new Campaign("Test Campaign", "me", "Greyhawk", "ruleset");
            this.campaign.adjustTimestamp(1, Timestamp.Interval.hour);
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
