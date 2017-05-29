using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prototype1
{
    /// <summary>
    /// Interface for any class that may wishes to request an Active Interaction to occur
    /// </summary>
    interface IActiveInterfaceRequestEventProvider
    {
        event EventHandler<ActiveInterfaceRequestEventArgs> RequestActiveInterface;
    }

    /// <summary>
    /// Event arguments to request and Active Interaction
    /// </summary>
    public class ActiveInterfaceRequestEventArgs : EventArgs
    {
        private Point _origin;

        /// <summary>
        /// The origin point of the point. this will dictate the monitor that the Active interaction will start on.
        /// This should be a point within the global reference (in relation to the entire window)
        /// </summary>
        public Point Origin { get { return _origin; } }

        private string _menuItem;

        /// <summary>
        /// Can specify a specific activity to start on here, this must be the name of the activity
        /// </summary>
        public string MenuItem { get { return _menuItem; } }

        private bool _result;

        /// <summary>
        /// The results of the request, true if the request was granted else false
        /// </summary>
        public bool Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public ActiveInterfaceRequestEventArgs(Point origin, string menuItem = "")
        {
            _origin = origin;
            _menuItem = menuItem;
        }

    }
}
