using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InteractionCoaxer
{

    public delegate void ModelHandler<TICoaxerModel>(ICoaxerModel sender, EventArgs e);

    /// <summary>
    /// Interface for all Classes that wish to observe a Class that implements
    /// the ICoaxerModel Interface
    /// </summary>
    public interface ICoaxerModelObserver
    {
        /// <summary>
        /// Called when ever the model is updated in order for the observer to react to the new model values
        /// </summary>
        /// <param name="model">Model that was updated updated</param>
        /// <param name="e">Event arguments of update</param>
        void ModelUpdated(ICoaxerModel model, EventArgs e);
    }

    /// <summary>
    /// Interface defining Views for the Coaxer MVC
    /// </summary>
    public interface ICoaxerModel
    {
        /// <summary>
        /// Add observer to list of observers to be notified when model is updated
        /// </summary>
        /// <param name="observer">Object that wishes to be notified when model changes</param>
        void AddObserver(ICoaxerModelObserver observer);


        void decodeAndUpdate(byte[] data);

        byte[] encode();
    }

    /// <summary>
    /// Base class for all Coaxer Views
    /// </summary>
    public abstract class CoaxerViewBase : Canvas, ICoaxerModelObserver
    {
        protected CoaxerViewBase()
        {
            IsHitTestVisible = false;
        }

        public abstract void ModelUpdated(ICoaxerModel model, EventArgs e);

        public abstract ICoaxerModel getModel();
    }

}
