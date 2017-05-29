using Prototype1.Activity_Classes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for ActivityMatrix.xaml
    /// </summary>
    public partial class ActivityMatrix : UserControl, INotifyPropertyChanged
    {
        public ActivityMatrix()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Search tag property to use to filter activities being viewed in matrix
        /// </summary>
        public string SearchTag
        {
            get
            {
                return GetValue(SearchTagProperty) as string;
            }
            set
            {
                SetValue(SearchTagProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for SearchTag to make it bindable in Xaml
        /// </summary>
        public static readonly DependencyProperty SearchTagProperty =
                DependencyProperty.Register("SearchTag",
                                            typeof(string),
                                            typeof(ActivityMatrix),
                                            new PropertyMetadata(new PropertyChangedCallback(OnSearchTagPropertyChanged)));

        /// <summary>
        /// list of filter tags to be used to filter activities being viewed in matrix
        /// </summary>
        public ObservableCollection<string> FilterTags
        {
            get
            {
                return GetValue(FilterTagsProperty) as ObservableCollection<string>;
            }
            set
            {
                SetValue(FilterTagsProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for FilterTags to make it bindable in Xaml
        /// </summary>
        public static readonly DependencyProperty FilterTagsProperty =
                DependencyProperty.Register("FilterTags",
                                            typeof(ObservableCollection<string>),
                                            typeof(ActivityMatrix),
                                            new PropertyMetadata(new PropertyChangedCallback(OnFilterTagsPropertyChanged)));


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// When ever filter tags property changes a new collection is created, there we need to listen to the 
        /// collection changed property of the new collection, remove the old collection and raise the event to
        /// cause filtering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnFilterTagsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sl = sender as ActivityMatrix;
            if (sl != null)
            {
                var oldCollection = e.OldValue as INotifyCollectionChanged;
                var newCollection = e.NewValue as INotifyCollectionChanged;

                if (oldCollection != null)
                {
                    oldCollection.CollectionChanged -= sl.NewCollection_CollectionChanged;
                }

                if (newCollection != null)
                {
                    newCollection.CollectionChanged += sl.NewCollection_CollectionChanged; ;
                }

                sl.RaiseValueChangedEvent(e);
            }
        }

        /// <summary>
        /// Need to raise event to cause filtering to occur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnSearchTagPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sl = sender as ActivityMatrix;
            sl.NotifyPropertyChanged("SearchTag");
            sl.RaiseValueChangedEvent(e);
        }

        /// <summary>
        /// causes collection view source to refresh and filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            (Resources["FilterActivities"] as CollectionViewSource).View.Refresh();
        }

        /// <summary>
        /// causes collection view source to refresh and filter
        /// </summary>
        /// <param name="e"></param>
        private void RaiseValueChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            (Resources["FilterActivities"] as CollectionViewSource).View.Refresh();
        }

        /// <summary>
        /// Filter the set of activities in the matrix based on the current filter tags and search tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            Activity activity = e.Item as Activity;

            if (FilterTags != null && FilterTags.Count > 0)
            {
                //In order to be displayed, the activity must have the tags of all the select filter values
                if (!(FilterTags.Select(i => i.ToUpper()).Intersect(activity.Tags.Select(i => i.ToUpper())).Count() == FilterTags.Count))
                {
                    e.Accepted = false;
                }
            }
            if (e.Accepted && SearchTag != null && SearchTag.Length > 0)
            {
                string upperSearchTag = SearchTag.ToUpper();

                //The activity will also be displayed only if any of is information matches the search tag
                if (!
                    (activity.Name.ToUpper().StartsWith(upperSearchTag) ||
                    activity.Type.ToUpper().StartsWith(upperSearchTag) ||
                    activity.Tags.Where(tag => tag.ToUpper().StartsWith(upperSearchTag)).Count() > 0 ||
                    activity.Description.ToUpper().Contains(upperSearchTag)))
                {
                    e.Accepted = false;
                }
            }

            if (e.Accepted)
            {

            }
            else
            {
                //TODO: find a way to animate the activities when they get filtered out
                Storyboard fadeOut = FindResource("FadeOut") as Storyboard;
                for (int i = 0; i < Activities.Items.Count; i++)
                {
                    FrameworkElement uiElement = (FrameworkElement)Activities.ItemContainerGenerator.ContainerFromIndex(i);
                    fadeOut.Begin(uiElement);
                }
            }

        }

        /// <summary>
        /// Stop window from moving with scroll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerticalScrollViewer_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void VerticalScrollViewer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ActiveAppSpace.resetTimer = true;
        }
    }
}