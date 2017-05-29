using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Class to hold and create a set of Activity Objects from an xml file
    /// </summary>
    class ActivitySet : ObservableCollection<Activity>
    {
        public ActivitySet()
        {
            XmlDocument activityDoc = new XmlDocument();
            activityDoc.Load(@"Activities/ActivitySet.xml");
            XmlNode activityList = activityDoc.ChildNodes.Item(1);
            foreach (XmlNode activity in activityList.ChildNodes)
            {
                Add(new Activity(activity));
            }
        }

    }
}
