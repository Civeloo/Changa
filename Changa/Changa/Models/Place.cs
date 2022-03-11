using System;
using System.Collections.Generic;
using System.Text;

namespace Changa.Models
{
    public class Place
    {
        public string Id { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string AdminArea { get; set; }
        public string CountryCode { get; set; }
        public string Locality { get; set; }
        public string SubAdminArea { get; set; }
        public string SubLocality { get; set; }
        public string PostalCode { get; set; }
    }
}
