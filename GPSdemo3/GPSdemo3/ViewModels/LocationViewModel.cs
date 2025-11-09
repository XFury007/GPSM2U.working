using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
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
        private List<SchoolInfo> _nearestSchools;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand GetLocationCommand { get; }
        public ICommand FindNearestSchoolsCommand { get; }

        // New route commands
        public ICommand RouteToTableMountainCommand { get; }
        public ICommand RouteToVAWaterfrontCommand { get; }
        public ICommand RouteToCapePointCommand { get; }
        public ICommand RouteToBishopsCommand { get; }
        public ICommand RouteToRondeboschBoysCommand { get; }
        public ICommand RouteToRondeboschGirlsCommand { get; }
        public ICommand RouteToRustenburgGirlsCommand { get; }
        public ICommand RouteToWesterfordCommand { get; }
        public ICommand RouteToSACSCommand { get; }
        public ICommand RouteToWynbergBoysCommand { get; }
        public ICommand RouteToWynbergGirlsCommand { get; }
        public ICommand RouteToHerschelGirlsCommand { get; }
        public ICommand RouteToSeaPointHighCommand { get; }
        public ICommand RouteToCampsBayHighCommand { get; }
        public ICommand RouteToPinelandsHighCommand { get; }
        public ICommand RouteToTableViewHighCommand { get; }
        public ICommand RouteToMilnertonHighCommand { get; }
        public ICommand RouteToFairmontCommand { get; }
        public ICommand RouteToDFMalanCommand { get; }
        public ICommand RouteToTheSettlersCommand { get; }
        public ICommand RouteToBellvilleHighCommand { get; }
        public ICommand RouteToDurbanvilleHighCommand { get; }
        public ICommand RouteToGoodwoodCommand { get; }
        public ICommand RouteToBelharCommand { get; }
        public ICommand RouteToAbbottsCollegeMilnertonCommand { get; }
        public ICommand RouteToBishopLavisCommand { get; }
        public ICommand RouteToSaltRiverHighCommand { get; }
        public ICommand RouteToNoordhoekHighCommand { get; }
        public ICommand RouteToHoutBayHighCommand { get; }
        public ICommand RouteToChapmansPeakCommand { get; }
        public ICommand RouteToMuizenbergHighCommand { get; }

        public ICommand OpenRouteToSchoolCommand { get; }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(DisplayLocation));
                (GetLocationCommand as Command)?.ChangeCanExecute();
                (FindNearestSchoolsCommand as Command)?.ChangeCanExecute();
                (RouteToTableMountainCommand as Command)?.ChangeCanExecute();
                (RouteToVAWaterfrontCommand as Command)?.ChangeCanExecute();
                (RouteToCapePointCommand as Command)?.ChangeCanExecute();
                (RouteToTableMountainCommand as Command)?.ChangeCanExecute();
                (RouteToVAWaterfrontCommand as Command)?.ChangeCanExecute();

            }
        }

        public double? Latitude
        {
            get => _latitude;
            private set { if (_latitude == value) return; _latitude = value; OnPropertyChanged(nameof(Latitude)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public double? Longitude
        {
            get => _longitude;
            private set { if (_longitude == value) return; _longitude = value; OnPropertyChanged(nameof(Longitude)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public string Address
        {
            get => _address;
            private set { if (_address == value) return; _address = value; OnPropertyChanged(nameof(Address)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set { if (_statusMessage == value) return; _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); OnPropertyChanged(nameof(DisplayLocation)); }
        }

        public List<SchoolInfo> NearestSchools
        {
            get => _nearestSchools;
            private set
            {
                if (_nearestSchools == value) return;
                _nearestSchools = value;
                OnPropertyChanged(nameof(NearestSchools));
                OnPropertyChanged(nameof(HasNearestSchools));
            }
        }

        public bool HasNearestSchools => NearestSchools?.Count > 0;

        public string DisplayLocation
        {
            get
            {
                if (IsBusy) return "Retrieving location...";
                if (!string.IsNullOrWhiteSpace(StatusMessage)) return StatusMessage;
                if (Latitude is null || Longitude is null) return "Tap 'My Location' to fetch.";
                if (!string.IsNullOrWhiteSpace(Address))
                    return $"{Address}\n({Latitude:0.0000}, {Longitude:0.0000})";
                if (!string.IsNullOrWhiteSpace(Address))
                    return $"{Address}\n({Latitude:0.0000}, {Longitude:0.0000})";

                // Cape Town proximity hint
                if (Latitude is double lat && Longitude is double lon &&
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




        private List<SchoolInfo> GetAllSchools()
        {
            return new List<SchoolInfo>
            {
                new SchoolInfo("Bishops Diocesan College", -33.96878, 18.46837),
                new SchoolInfo("Rondebosch Boys' High School", -33.96806, 18.47639),
                new SchoolInfo("Rondebosch Girls' High School", -33.96790, 18.47600),
                new SchoolInfo("Rustenburg Girls' High School", -33.96417, 18.47778),
                new SchoolInfo("Westerford High School", -33.96365, 18.46895),
                new SchoolInfo("South African College Schools (SACS) High School", -33.97040, 18.46011),
                new SchoolInfo("Wynberg Boys' High School", -33.99639, 18.45889),
                new SchoolInfo("Wynberg Girls' High School", -33.99700, 18.45450),
                new SchoolInfo("Herschel Girls' School", -33.98090, 18.47020),
                new SchoolInfo("Sea Point High School", -33.91784, 18.39207),
                new SchoolInfo("Camps Bay High School", -33.94150, 18.38100),
                new SchoolInfo("Pinelands High School", -33.95450, 18.49740),
                new SchoolInfo("Table View High School", -33.83106, 18.49879),
                new SchoolInfo("Milnerton High School", -33.85320, 18.48820),
                new SchoolInfo("Fairmont High School", -33.81850, 18.65590),
                new SchoolInfo("Hoërskool D.F. Malan", -33.88810, 18.63650),
                new SchoolInfo("The Settlers High School", -33.87300, 18.61500),
                new SchoolInfo("Bellville High School", -33.88100, 18.63000),
                new SchoolInfo("Durbanville High School", -33.84860, 18.65720),
                new SchoolInfo("Goodwood High School", -33.89420, 18.59800),
                new SchoolInfo("Belhar Secondary School", -33.87260, 18.60210),
                new SchoolInfo("Abbotts College Milnerton", -33.84260, 18.49090),
                new SchoolInfo("Bishop Lavis High School", -33.89500, 18.59000),
                new SchoolInfo("Salt River High School", -33.92900, 18.44400),
                new SchoolInfo("Noordhoek High School", -34.06800, 18.38600),
                new SchoolInfo("Hout Bay High School", -34.04100, 18.35600)
            };
        }

        private async Task FindNearestSchoolsAsync()
        {
            try
            {
                IsBusy = true;
                NearestSchools = null;

                // Ensure we have current location
                if (Latitude is null || Longitude is null)
                {
                    await GetLocationAsync();
                }

                if (Latitude is null || Longitude is null)
                {
                    StatusMessage = "Unable to determine current location.";
                    return;
                }

                var schools = GetAllSchools();
                var currentLat = Latitude.Value;
                var currentLon = Longitude.Value;

                // Calculate distance to each school and sort
                foreach (var school in schools)
                {
                    school.DistanceKm = CalculateDistance(currentLat, currentLon, school.Latitude, school.Longitude);
                }

                // Get the 3 nearest schools
                NearestSchools = schools.OrderBy(s => s.DistanceKm).Take(3).ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error finding nearest schools: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("FindNearestSchoolsAsync error: " + ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula to calculate distance between two coordinates
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
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

        public async Task OpenRouteToSchoolAsync(SchoolInfo school)
        {
            await OpenAzureMapsRouteAsync(school.Name, school.Latitude, school.Longitude);
        }

        private async Task OpenAzureMapsRouteAsync(string destinationName, double destLat, double destLon)
        {
            try
            {
                // Ensure we have current location
                if (Latitude is null || Longitude is null)
                    await GetLocationAsync();

                if (Latitude is null || Longitude is null)
                {
                    StatusMessage = "Unable to determine current location.";
                    return;
                }

                var originLat = Latitude.Value.ToString(CultureInfo.InvariantCulture);
                var originLon = Longitude.Value.ToString(CultureInfo.InvariantCulture);
                var destLatStr = destLat.ToString(CultureInfo.InvariantCulture);
                var destLonStr = destLon.ToString(CultureInfo.InvariantCulture);

                // Example: "My Location" route, using V&A Waterfront as default destination
                if (string.IsNullOrWhiteSpace(destinationName))
                {
                    destinationName = "Wynberg High School";
                    destLatStr = "-33.9036";
                    destLonStr = "18.4204";
                }

                var uri = $"https://www.google.com/maps/dir/?api=1&origin={originLat},{originLon}&destination={destLatStr},{destLonStr}&travelmode=driving";
                await Launcher.OpenAsync(uri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OpenAzureMapsRouteAsync error: " + ex);
            }
        }

        // Add this method to the LocationViewModel class to fix CS0103
        private async Task<bool> EnsureLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            return status == PermissionStatus.Granted;
        }

        // Add this method to fix CS0103: The name 'ReverseGeocodeAsync' does not exist in the current context
        private async Task ReverseGeocodeAsync(double lat, double lon)
        {
            try
            {
                using var http = new HttpClient();
                var url =
                    $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={lat.ToString(CultureInfo.InvariantCulture)},{lon.ToString(CultureInfo.InvariantCulture)}&subscription-key={AzureMapsConfig.SubscriptionKey}";
                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return;

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

        public LocationViewModel()
        {
            GetLocationCommand = new Command(
                async () => await GetLocationAsync(),
                () => !IsBusy);

            FindNearestSchoolsCommand = new Command(
                async () => await FindNearestSchoolsAsync(),
                () => !IsBusy);

            OpenRouteToSchoolCommand = new Command<SchoolInfo>(
                async (school) => await OpenRouteToSchoolAsync(school),
                (school) => !IsBusy && school != null);

            RouteToBishopsCommand = new Command(
          async () => await OpenAzureMapsRouteAsync("Bishops Diocesan College", -33.96878, 18.46837),
          () => !IsBusy);

            RouteToRondeboschBoysCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Rondebosch Boys' High School", -33.96806, 18.47639),
                () => !IsBusy);

            RouteToRondeboschGirlsCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Rondebosch Girls' High School", -33.96790, 18.47600),
                () => !IsBusy);

            RouteToRustenburgGirlsCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Rustenburg Girls' High School", -33.96417, 18.47778),
                () => !IsBusy);

            RouteToWesterfordCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Westerford High School", -33.96365, 18.46895),
                () => !IsBusy);

            RouteToSACSCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("South African College Schools (SACS) High School", -33.97040, 18.46011),
                () => !IsBusy);

            RouteToWynbergBoysCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Wynberg Boys' High School", -33.99639, 18.45889),
                () => !IsBusy);

            RouteToWynbergGirlsCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Wynberg Girls' High School", -33.99700, 18.45450),
                () => !IsBusy);

            RouteToHerschelGirlsCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Herschel Girls' School", -33.98090, 18.47020),
                () => !IsBusy);

            RouteToSeaPointHighCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Sea Point High School", -33.91784, 18.39207),
                () => !IsBusy);

            RouteToCampsBayHighCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Camps Bay High School", -33.94150, 18.38100),
                () => !IsBusy);

            RouteToPinelandsHighCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Pinelands High School", -33.95450, 18.49740),
                () => !IsBusy);

            RouteToTableViewHighCommand = new Command(
                async () => await OpenAzureMapsRouteAsync("Table View High School", -33.83106, 18.49879),
                () => !IsBusy);
        }
        
        public class SchoolInfo
        {
            public string Name { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double DistanceKm { get; set; }

            public string DisplayText => $"{Name} ({DistanceKm:F2} km)";

            public SchoolInfo(string name, double latitude, double longitude)
            {
                Name = name;
                Latitude = latitude;
                Longitude = longitude;
            }
        }
    }

}