using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.GoogleMaps;

namespace Changa.Services
{
    public class MapService:IMapService
    {
        private readonly ReactivePropertySlim<Map> _myMap = new ReactivePropertySlim<Xamarin.Forms.GoogleMaps.Map>();
        public ReadOnlyReactivePropertySlim<Map> MyMap { get; }

        public MapService()
        {
            MyMap = _myMap.ToReadOnlyReactivePropertySlim();
        }

        public Map MapToLocation(string loc)
        {
            var map = new Map();
            if (loc != null)
            { 
                string[] latLong = loc.Split(';');
                var lat = Convert.ToDouble(latLong[0]);
                var lon = Convert.ToDouble(latLong[1]);
                var pos = new Position(lat, lon);

                map.InitialCameraUpdate = CameraUpdateFactory.NewCameraPosition(new CameraPosition(
                  pos,  // latlng
                  16d, // zoom
                  30d, // rotation
                  60d)); // tilt

                map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMeters(10)));
                var pin = new Pin() { Label = "here", Position = pos };
                map.Pins.Add(pin);
            }
            _myMap.Value = map;
            return map;
        }

    }
}
