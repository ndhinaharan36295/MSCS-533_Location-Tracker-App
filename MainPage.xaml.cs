using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using SQLite;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;

namespace LocationTrackerApp
{
    public partial class MainPage : ContentPage
    {
        // Database instance for storing location data
        private readonly LocationDatabase _locationDb;

        // Timer to periodically capture location
        private readonly Timer _locationTimer;

        // Observable collection to bind location data to the UI
        private readonly ObservableCollection<LocationModel> _locations;

        // Map control to display user location and pins
        private readonly MauiMap _map;

        public MainPage()
        {
            InitializeComponent();
            // Initialize the database and collection
            _locationDb = new LocationDatabase();
            _locations = new ObservableCollection<LocationModel>();

            // Initialize the map and set its properties
            _map = new MauiMap
            {
                IsShowingUser = true,       // Show the user's location
            };
            Content = _map;                 // Set the map as the content of the page

            // Set up the timer to capture location every 60 seconds
            _locationTimer = new Timer(60000); // 60 seconds
            _locationTimer.Elapsed += async (s, e) => await CaptureLocation();
            _locationTimer.Start();

            // Load previously stored locations
            LoadLocations();
        }

        // Method to capture the current location and save it to the database
        private async Task CaptureLocation()
        {
            try
            {
                // Get the last known location
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    // Create a new location model and save it
                    var loc = new LocationModel
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Timestamp = DateTime.Now
                    };
                    await _locationDb.SaveLocationAsync(loc);

                    // Log the saved location
                    Debug.WriteLine($"Saved Location: {loc.Latitude}, {loc.Longitude}");

                    // Update the UI with the new location and add a pin to the map
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _locations.Add(loc);    // Add the location to the observable collection
                        AddHeatMapPin(loc);     // Add a pin to the map for the location
                    });
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur while capturing the location
                Debug.WriteLine($"Error getting location: {ex.Message}");
            }
        }

        // Method to load previously stored locations from the database
        private async void LoadLocations()
        {
            // Retrieve all stored locations from the database
            var stored = await _locationDb.GetLocationsAsync();
            foreach (var loc in stored)
            {
                _locations.Add(loc);     // Add each location to the observable collection
                AddHeatMapPin(loc);      // Add a pin to the map for each location
            }
        }

        // Method to add a pin to the map for a given location
        private void AddHeatMapPin(LocationModel loc)
        {
            // Simulate heat intensity with pin color saturation (not true heatmap rendering)
            var pin = new Pin
            {
                Location = new Microsoft.Maui.Devices.Sensors.Location(loc.Latitude, loc.Longitude),     // Set pin location
                Label = $"Visited: {loc.Timestamp}"      // Set pin label with timestamp
            };
            _map.Pins.Add(pin);          // Add the pin to the map
        }
    }

    // Model class representing a location entry
    public class LocationModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }                  // Unique identifier for the location
        public double Latitude { get; set; }         // Latitude of the location
        public double Longitude { get; set; }        // Longitude of the location
        public DateTime Timestamp { get; set; }      // Timestamp of when the location was captured
    }

    // class for managing location data in the Database
    public class LocationDatabase
    {
        private readonly SQLiteAsyncConnection _db;

        public LocationDatabase()
        {
            // Define the database path and initialize the connection
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
            _db = new SQLiteAsyncConnection(dbPath);

            // Create the LocationModel table if it doesn't exist
            _db.CreateTableAsync<LocationModel>().Wait();
        }

        // Save a new location to the database
        public Task<int> SaveLocationAsync(LocationModel location)
        {
            return _db.InsertAsync(location);
        }

        // Retrieve all stored locations from the database
        public Task<List<LocationModel>> GetLocationsAsync()
        {
            return _db.Table<LocationModel>().ToListAsync();
        }
    }
}