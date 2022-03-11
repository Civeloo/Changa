using System;
using System.Collections.Specialized;
using System.Linq;
using AiForms.Renderers;
using AiForms.Renderers.iOS;
using Xamarin.Forms;
using Changa.Controls;
using Changa.iOS.Renderers;

[assembly: ExportRenderer(typeof(CustomGridCollectionView), typeof(CustomGridCollectionViewRenderer))]
namespace Changa.iOS.Renderers
{
    public class CustomGridCollectionViewRenderer : GridCollectionViewRenderer
    {
        protected override void UpdateItems(NotifyCollectionChangedEventArgs e, int section, bool resetWhenGrouped, bool forceReset = false)
        {
            if (!Control.IndexPathsForVisibleItems.Any() ||
               (e.Action == NotifyCollectionChangedAction.Remove && Control.IndexPathsForVisibleItems.Count() == 1))
            {
                forceReset = true;
            }
            base.UpdateItems(e, section, resetWhenGrouped, forceReset);
        }
    }
}
