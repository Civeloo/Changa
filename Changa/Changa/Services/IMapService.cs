using Xamarin.Forms.GoogleMaps;
using Reactive.Bindings;
using System.Threading.Tasks;

namespace Changa.Services
{
    public interface IMapService
    {
        ReadOnlyReactivePropertySlim<Map> MyMap { get; }
        Map MapToLocation(string loc);
    }
}