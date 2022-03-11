using System;
using Android.App;
using Android.Runtime;

namespace Changa.Droid
{
    [Application]
    [MetaData("com.google.android.maps.v2.API_KEY",
              Value = Variables.GOOGLE_MAPS_ANDROID_API_KEY)]
    public class MyApp : Application
    {
        public MyApp(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
    }
}