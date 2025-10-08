using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using GPSdemo3.Configuration;

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
                OnPropertyChanged(nameof(DisplayLocation)); // Ensure label updates when busy starts/ends
                (GetLocationCommand as Command)?.ChangeCanExecute();
            }
        }

        public double? Latitude
        {
            get => _latitude;
            private set
            {
                if (_latitude == value) return;
                _latitude = value;
                OnPropertyChanged(nameof(Latitude));
                OnPropertyChanged(nameof(DisplayLocation));
            }
        }

        public double? Longitude
        {
            get => _longitude;
            private set
            {
                if (_longitude == value) return;
                _longitude = value;
                OnPropertyChanged(nameof(Longitude));
                OnPropertyChanged(nameof(DisplayLocation));
            }
        }

        public string Address
        {
            get => _address;
            private set
            {
                if (_address == value) return;
                _address = value;
                OnPropertyChanged(nameof(Address));
                OnPropertyChanged(nameof(DisplayLocation));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
                OnPropertyChanged(nameof(DisplayLocation));
            }
        }

        public string DisplayLocation
        {
            get
            {
                if (IsBusy) return "Retrieving location...";
                if (!string.IsNullOrWhiteSpace(StatusMessage)) return StatusMessage;
                if (Latitude is null || Longitude is null) return "Tap 'My Location' to fetch.";

                // Cape Town proximity (only if we have no resolved address)
                if (string.IsNullOrWhiteSpace(Address) &&
                    Latitude is double lat && Longitude is double lon &&
                    Math.Abs(lat - (-33.9249)) < 0.1 &&
                    Math.Abs(lon - 18.4241) < 0.1)
                {
                    return $"Cape Town, South Africa\n({lat:0.0000}, {lon:0.0000})";
                }

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

                if (!await EnsureLocationPermissionAsync())
                    return;

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                Location location = null;

                try
                {
                    location = await Geolocation.GetLocationAsync(request);
                }
                catch (FeatureNotEnabledException)
                {
                    StatusMessage = "Location services disabled.";
                }
                catch (Exception ex)
                {
                    StatusMessage = "Failed to get active location.";
                    System.Diagnostics.Debug.WriteLine("Geolocation.GetLocationAsync error: " + ex);
                }

                if (location == null)
                {
                    try
                    {
                        location = await Geolocation.GetLastKnownLocationAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("GetLastKnownLocationAsync error: " + ex);
                    }
                }

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
                System.Diagnostics.Debug.WriteLine("GetLocationAsync unexpected: " + ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> EnsureLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                StatusMessage = "Location permission denied.";
                return false;
            }
            return true;
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
                    System.Diagnostics.Debug.WriteLine("Reverse geocode HTTP status: " + response.StatusCode);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("addresses", out var addresses) ||
                    addresses.GetArrayLength() == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Reverse geocode: no addresses element.");
                    return;
                }

                var addressElem = addresses[0].GetProperty("address");
                var freeform = addressElem.TryGetProperty("freeformAddress", out var ff)
                    ? ff.GetString()
                    : null;

                if (!string.IsNullOrWhiteSpace(freeform))
                    Address = freeform;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ReverseGeocode failed: " + ex);
            }
        }

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}