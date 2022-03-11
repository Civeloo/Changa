using System;
using AiForms.Renderers;
using Xamarin.Forms;

namespace Changa.Controls
{
    public class CustomGridCollectionView : GridCollectionView
    {
        public CustomGridCollectionView()
        {
        }

        public CustomGridCollectionView(ListViewCachingStrategy cachingStrategy) : base(cachingStrategy)
        {
        }
    }
}
