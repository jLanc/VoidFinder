using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

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
                    var result = DarkSkyFinder.FindDarkestSky(latitude, longitude, altitude, utcNow);

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
}
