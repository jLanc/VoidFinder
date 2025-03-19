using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using VoidFinder.Services;

namespace VoidFinder
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            FindDarkestSky(); // Auto-run on load
        }

        private async void OnFindDarkSkyClicked(object sender, EventArgs e)
        {
            await FindDarkestSky();
        }

        private async Task FindDarkestSky()
        {
            try
            {
                // Get GPS Location
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High));
                }

                if (location != null)
                {
                    double latitude = location.Latitude;
                    double longitude = location.Longitude;
                    double altitude = location.Altitude ?? 0;

                    LocationLabel.Text = $"Location: {latitude:F4}, {longitude:F4}, Alt: {altitude}m";

                    // Get current UTC time
                    DateTime utcNow = DateTime.UtcNow;

                    // Compute Darkest Sky
                    double skyBrightness = await DarkSkyFinder.EstimateSkyBrightnessAsync(latitude, longitude);
                    Console.WriteLine($"Estimated sky brightness: {skyBrightness:F2} mag/arcsec²");

                    var (darkLat, darkLon) = await DarkSkyFinder.FindDarkSkyLocationAsync(latitude, longitude, 50);
                    Console.WriteLine($"Best dark-sky location within 50km: {darkLat:F4}, {darkLon:F4}");

                    // Convert to RA/DEC using time and observer's latitude
                    var result = CelestialCoordinateFinder.ComputeRaDec(darkLat, darkLon, utcNow);

                    if (result.azimuth != -1)
                    {
                        DarkSkyLabel.Text = $"Darkest RA/DEC: {result.ra:F2}h, {result.dec:F2}°\n" +
                                            $"Visible at: {result.altitude:F2}° Alt, {result.azimuth:F2}° Az";
                    }
                    else
                    {
                        DarkSkyLabel.Text = "No visible dark sky region found.";
                    }
                }
                else
                {
                    LocationLabel.Text = "Unable to get location.";
                }
            }
            catch (Exception ex)
            {
                DarkSkyLabel.Text = $"Error: {ex.Message}";
            }
        }
    }

    public static class CelestialCoordinateFinder
    {
        public static (double ra, double dec, double altitude, double azimuth) ComputeRaDec(double lat, double lon, DateTime utcNow)
        {
            // Simplified RA/DEC Calculation (Placeholder - Replace with precise astronomy library)
            double siderealTime = (utcNow.TimeOfDay.TotalHours + lon / 15.0) % 24.0;
            double ra = siderealTime;
            double dec = lat; // Simplified for zenith approximation

            // Compute Altitude & Azimuth (Assuming Observer's Position)
            double altitude = 90 - Math.Abs(lat - dec);
            double azimuth = (ra % 360) / 15.0 * 24.0;

            return (ra, dec, altitude, azimuth);
        }
    }
}
