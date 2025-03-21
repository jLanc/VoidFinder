using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VoidFinder.Services
{
    public class DarkSkyFinder
    {
        private const double STANDARD_EXTINCTION = 0.2;
        private const double EARTH_RADIUS = 6371.0;
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Gets the elevation at a specific latitude and longitude using an elevation API.
        /// </summary>
        public static async Task<double> GetElevationAsync(double latitude, double longitude)
        {
            //string apiKey = "YOUR_ELEVATION_API_KEY";  // Replace with your API key
            string url = $"https://api.open-elevation.com/api/v1/lookup?locations={latitude},{longitude}";

            HttpResponseMessage response = await client.GetAsync(url);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            JObject data = JObject.Parse(jsonResponse);
            var results = data["results"];
            if (results == null || !results.HasValues || results[0]?["elevation"] == null)
            {
                throw new InvalidOperationException("Invalid or missing elevation data in the API response.");
            }
            return results[0]["elevation"].Value<double>();
        }

        /// <summary>
        /// Estimates sky brightness using real elevation data, without considering light pollution.
        /// </summary>
        public static async Task<double> EstimateSkyBrightnessAsync(double latitude, double longitude)
        {
            double altitude = await GetElevationAsync(latitude, longitude);

            double baseBrightness = 21.0;  // Base brightness in mag/arcsec²
            double altitudeFactor = Math.Exp(-altitude / 2000.0);  // Factor to decrease brightness based on altitude

            return baseBrightness - (altitudeFactor);  // Just altitude factor without pollution adjustment
        }

        /// <summary>
        /// Finds the best dark-sky location within a given radius using real data (without light pollution data).
        /// </summary>
        public static async Task<(double, double)> FindDarkSkyLocationAsync(double latitude, double longitude, double radiusKm)
        {
            double bestLatitude = latitude;
            double bestLongitude = longitude;
            double bestBrightness = double.MaxValue;

            for (double latOffset = -0.5; latOffset <= 0.5; latOffset += 0.1)
            {
                for (double lonOffset = -0.5; lonOffset <= 0.5; lonOffset += 0.1)
                {
                    double newLat = latitude + latOffset;
                    double newLon = longitude + lonOffset;

                    if (GetDistance(latitude, longitude, newLat, newLon) > radiusKm)
                        continue;

                    double brightness = await EstimateSkyBrightnessAsync(newLat, newLon);
                    if (brightness > bestBrightness)
                    {
                        bestBrightness = brightness;
                        bestLatitude = newLat;
                        bestLongitude = newLon;
                    }
                }
            }

            return (bestLatitude, bestLongitude);
        }

        private static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            lat1 = ToRadians(lat1);
            lat2 = ToRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EARTH_RADIUS * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
