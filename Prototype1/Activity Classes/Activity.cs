using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Class to hold all information relavent to the system about an Activity
    /// </summary>
    class Activity
    {
        private string _name;
        private string _type;
        private BitmapImage _icon;
        private List<string> _tags;
        private string _description;
        private List<string> _files;
        private string _url;
        private int _height;
        private int _width;

        /// <summary>
        /// Name of the Activity
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Type of the activity. Either: Game, Media
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Icon of the Activity
        /// </summary>
        public BitmapImage Icon
        {
            get { return _icon; }
        }

        /// <summary>
        /// Tags for Activity. To be used to filter and search for activities related to certain interests.
        /// The values within this list should be contained with the Filters.xml file
        /// </summary>
        public List<string> Tags
        {
            get
            {
                return _tags;
            }
        }

        /// <summary>
        /// Description of the Activity. Should be no more than 140 characters.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }
        /// <summary>
        /// Description of the Activity. Should be no more than 140 characters.
        /// </summary>
        public string Url
        {
            get
            {
                return _url;
            }
        }
        /// <summary>
        /// Description of the Activity. Should be no more than 140 characters.
        /// </summary>
        public int Width
        {
            get
            {
                return _width;
            }
        }
        /// <summary>
        /// Description of the Activity. Should be no more than 140 characters.
        /// </summary>
        public int Height
        {
            get
            {
                return _height;
            }
        }

        /// <summary>
        /// Files associated with the Activity
        /// If Game, there should only be one file.
        /// If Media, will contain the list of all media files to be shown in slide show
        /// </summary>
        public List<string> Files
        {
            get { return _files; }
        }

        /// <summary>
        /// CReates an Activity Object from an XMLNode
        /// </summary>
        /// <param name="activity"></param>
        public Activity(XmlNode activity)
        {
            XmlNode name = activity.SelectSingleNode("Name");
            XmlNode imageLocation = activity.SelectSingleNode("ImageLocation");
            XmlNode type = activity.SelectSingleNode("Type");
            XmlNode tags = activity.SelectSingleNode("Tags");
            XmlNode description = activity.SelectSingleNode("Description");
            XmlNode files = activity.SelectSingleNode("Files");

            _name = name.InnerText.Trim();

            _type = type.InnerText.Trim();

            _icon = new BitmapImage();
            _icon.BeginInit();
            _icon.UriSource = new Uri("pack://siteoforigin:,,,/Activities/" + _type + "/" + _name + "/" + imageLocation.InnerText.Trim());
            _icon.EndInit();

            _tags = new List<string>();
            foreach (XmlNode tag in tags)
            {
                _tags.Add(tag.InnerText.Trim());
            }

            _description = description.InnerText.Trim();

            if (_description.Length > 140)
            {
                //this should not happen
            }

            _files = new List<string>();
            foreach (XmlNode file in files.ChildNodes)
            {
                _files.Add(file.InnerText.Trim());
            }

            if (_type.ToUpper().Equals("MEDIA"))
            {
                XmlNode Resolution = activity.SelectSingleNode("Resolution");
                XmlNode Wdth = Resolution.SelectSingleNode("Width");
                XmlNode Hght = Resolution.SelectSingleNode("Height");
                _width = Convert.ToInt32(Wdth.InnerText.Trim());
                _height = Convert.ToInt32(Hght.InnerText.Trim());
            }
            if (_type.ToUpper().Equals("BROWSER"))
            {
                XmlNode url = activity.SelectSingleNode("Url");
                _url = url.InnerText.Trim();
            }

            if (_files.Count < 1 || 
                (_type.ToUpper().Equals("GAME") && _files.Count > 1))
            {
                // this should not happen
            }

        }
    }
}
