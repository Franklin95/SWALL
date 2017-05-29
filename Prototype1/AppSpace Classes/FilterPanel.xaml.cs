using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for FilterPanel.xaml
    /// </summary>
    public partial class FilterPanel : UserControl, INotifyPropertyChanged
    {
        private int screenWidth;
        int count = 0;
        public FilterPanel()
        {
            InitializeComponent();
            screenWidth = (int)grid.Width - 10;
            using (XmlReader reader = XmlReader.Create("Filters.xml"))
            {
                string attribute1 = null;
                string previousAttribute = null;

                while (reader.Read())
                {

                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {
                        // Get element name and switch on it.
                        switch (reader.Name)
                        {
                            case "FilterType":
                                attribute1 = reader["Name"];
                                if (attribute1 != null)
                                {
                                    Console.WriteLine("FilterType: " + attribute1);
                                }
                                count++;

                                break;
                        }
                    }
                }
                Console.Out.WriteLine("" + count);
            }
            using (XmlReader reader = XmlReader.Create("Filters.xml"))
            {
                string attribute1 = null;
                int track = 0;
                ComboBox cb = null;
                Grid newGrid = null;
                int gridLeft = 0;
                int gridRight = screenWidth / count;
                while (reader.Read())
                {

                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {
                        // Get element name and switch on it.
                        switch (reader.Name)
                        {
                            case "FilterType":
                                if (cb != null && newGrid != null)
                                {
                                    newGrid.Children.Add(cb);
                                    Console.Out.WriteLine("GridShown: " + gridLeft);
                                    gridLeft += screenWidth / count;
                                    gridRight += screenWidth / count;
                                }
                                attribute1 = reader["Name"];
                                if (attribute1 != null)
                                {
                                    Console.WriteLine("FilterType: " + attribute1);
                                }
                                newGrid = new Grid();
                                newGrid.HorizontalAlignment = HorizontalAlignment.Left;
                                newGrid.VerticalAlignment = VerticalAlignment.Top;
                                newGrid.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * 6 / 192;
                                newGrid.Width = screenWidth / count;
                                newGrid.Margin = new Thickness(gridLeft, 10, 10, 10);

                                grid.Children.Add(newGrid);
                                cb = new ComboBox();

                                cb.HorizontalAlignment = HorizontalAlignment.Left;
                                cb.VerticalAlignment = VerticalAlignment.Top;
                                cb.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * 5 / 192;
                                cb.Width = screenWidth / count;
                                cb.FontFamily = new FontFamily("Rockwell");
                                cb.FontSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * 1.5 / 192;
                                cb.Items.Add(attribute1);
                                cb.Text = attribute1;
                                track++;
                                break;
                            case "FilterValue":
                                // Detect this article element.
                                Console.WriteLine("Start <FilterValue> element.");
                                // Search for the attribute name on this current node.
                                CheckBox check = new CheckBox();
                                check.Checked += CheckBox_Checked;
                                check.Unchecked += CheckBox_Unchecked;
                                //check.Margin = new Thickness(1, 2, 3, 2);
                                check.VerticalAlignment = VerticalAlignment.Center;
                                //check.Padding = new Thickness(2, 0, 0, 0);
                                check.FontSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height * 2 / 192;
                                check.FontFamily = new FontFamily("Rockwell");
                                check.Foreground = Brushes.White;
                                string attribute = reader["Name"];
                                if (attribute != null)
                                {
                                    check.Content = attribute;
                                    cb.Items.Add(check);
                                    //cb.Items.Add(attribute); 
                                }
                                break;
                        }
                    }
                }
                if (cb != null && newGrid != null)
                {
                    newGrid.Children.Add(cb);
                    Console.Out.WriteLine("Last GridShown: " + gridLeft);
                    cb = null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private ObservableCollection<string> _filterTags = new ObservableCollection<string>();

        /// <summary>
        /// List of tags user has chosen to filter on
        /// </summary>
        public ObservableCollection<string> FiltersTags
        {
            get { return _filterTags; }
            set
            {
                _filterTags = value;
            }
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            _filterTags.Add(((CheckBox)sender).Content.ToString());
            Console.Out.WriteLine(((CheckBox)sender).Content.ToString());
            NotifyPropertyChanged("FiltersTags");
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            _filterTags.Remove(((CheckBox)sender).Content.ToString());
            NotifyPropertyChanged("FiltersTags");
        }

        /// <summary>
        /// Stop window from moving with scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tree_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
        }
    }
}
