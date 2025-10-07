using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Devices.Sensors;


namespace GPSdemo3.ViewModels
{
    public class LocationViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private double? _latitude;
        private double? _longitude;
        private string _address;
        private string _statusMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand GetLocationCommand { get; }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                (GetLocationCommand as Command)?.ChangeCanExecute();
            }
        }

        public double? Latitude
        {
            get => _latitude;
            private set { _latitude = value; OnPropertyChanged(nameof(Latitude)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public double? Longitude
        {
            get => _longitude;
            private set { _longitude = value; OnPropertyChanged(nameof(Longitude)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public string Address
        {
            get => _address;
            private set { _address = value; OnPropertyChanged(nameof(Address)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public string DisplayLocation
        {
            get
            {
                if (IsBusy) return "Retrieving location...";
                if (!string.IsNullOrWhiteSpace(StatusMessage)) return StatusMessage;
                if (Latitude is null || Longitude is null) return "Tap 'Your Location' to fetch.";
                if (!string.IsNullOrWhiteSpace(Address))
                    return $"{Address}\n({Latitude:0.0000}, {Longitude:0.0000})";
                return $"({Latitude:0.0000}, {Longitude:0.0000})";
            }
        }

        public LocationViewModel()
        {
            GetLocationCommand = new Command(async () => await GetLocationAsync(), () => !IsBusy);
        }

        private async Task GetLocationAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = string.Empty;
                Address = string.Empty;

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    StatusMessage = "Location not available.";
                    return;
                }

                Latitude = location.Latitude;
                Longitude = location.Longitude;

                await ReverseGeocodeAsync(location.Latitude, location.Longitude);
            }
            catch (FeatureNotSupportedException)
            {
                StatusMessage = "Location not supported on this device.";
            }
            catch (PermissionException)
            {
                StatusMessage = "Location permission denied.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ReverseGeocodeAsync(double lat, double lon)
        {
            try
            {
                using var http = new HttpClient();
                var url =
                    $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&subscription-key={AzureMapsConfig.SubscriptionKey}";
                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    StatusMessage = "Reverse geocode failed.";
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var addressElem = doc.RootElement
                    .GetProperty("addresses")[0]
                    .GetProperty("address");

                var freeform = addressElem.TryGetProperty("freeformAddress", out var ff)
                    ? ff.GetString()
                    : null;

                Address = freeform ?? "Address unavailable";
            }
            catch
            {
                // Silently keep coordinates only
            }
        }

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Add this class to your project, ideally in a separate file, or at the top of LocationViewModel.cs for demonstration.
    // Replace "YOUR_AZURE_MAPS_SUBSCRIPTION_KEY" with your actual Azure Maps subscription key.
    public static class AzureMapsConfig
    {
        public static string SubscriptionKey { get; } = "YOUR_AZURE_MAPS_SUBSCRIPTION_KEY";
    }
}