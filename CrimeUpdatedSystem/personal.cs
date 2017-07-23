using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Locations;
using Android.Util;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace CrimeUpdatedSystem
{

    [Activity(Label = "Personal Details Report")]
    public class personal : Activity, ILocationListener
    {
        static readonly string LogTag = "GetLocation";
        TextView _addressText;
        Location _currentLocation;
        LocationManager _locationManager;
        string name,surname;

        string storage;
        public Spinner spinner = null;

        string _locationProvider;
        TextView _locationText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.personalDetails);

            _addressText = FindViewById<TextView>(Resource.Id.address_2);
            _locationText = FindViewById<TextView>(Resource.Id.location_2);
            FindViewById<TextView>(Resource.Id.get_address_btn).Click += AddressButton_OnClick;

           var editName = FindViewById<EditText> (Resource.Id.name);
           var editSurname = FindViewById<EditText> (Resource.Id.surname);

           TextView failmsg = FindViewById<TextView>(Resource.Id.fldTxt);
            
            
             editName.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) => {      
               name = e.Text.ToString ();};

            editSurname.TextChanged += (object send, Android.Text.TextChangedEventArgs c) =>
            {
               surname = c.Text.ToString();
            };


            spinner = FindViewById<Spinner>(Resource.Id.crimeList);

            var items = new List<string>() { "Rape", "Robbery", "Hijack", "Harasment", "Fraud", "Kidnaping", "Bulgary", "Assult", "Murder" };
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, items);

            spinner.Adapter = adapter;

            Button button = FindViewById<Button>(Resource.Id.reportCrime);

            button.Click += delegate
            {
                
                storage = spinner.SelectedItem.ToString();

                if(_currentLocation!=null)
                {
                    try
                    {
                        MailMessage mail = new MailMessage();
                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                        mail.From = new MailAddress("215048262@student.uj.ac.za");
                        mail.To.Add("vhalili91@outlook.com");
                        mail.Subject = storage + "-" + name + "-" + surname;
                        mail.Body = _addressText.Text;
                        SmtpServer.Port = 587;
                        SmtpServer.Credentials = new System.Net.NetworkCredential("215048262@student.uj.ac.za", "Christopher@1991");
                        SmtpServer.EnableSsl = true;
                        ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true;
                        };
                        SmtpServer.Send(mail);
                        Toast.MakeText(Application.Context, "Crime Has been Reported", ToastLength.Short).Show();
                    }

                    catch (Exception ex)
                    {
                        Toast.MakeText(Application.Context, ex.ToString(), ToastLength.Long);
                    }
                }
                else
                {

                    failmsg.Visibility = ViewStates.Visible;
                }
               

            };


            InitializeLocationManager();
        }

        public void OnLocationChanged(Location location)
        {
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location.";
            }
            else
            {
                _locationText.Text = String.Format("{0},{1}", _currentLocation.Latitude, _currentLocation.Longitude);
            }
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Log.Debug(LogTag, "{0}, {1}", provider, status);
        }
        void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = String.Empty;
            }
            Log.Debug(LogTag, "Using " + _locationProvider + ".");
        }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
            Log.Debug(LogTag, "Listening for location updates using " + _locationProvider + ".");
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
            Log.Debug(LogTag, "No longer listening for location updates.");
        }

        async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            if (_currentLocation == null)
            {
                _addressText.Text = "Can't determine the current address.";
                return;
            }

            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList = geocoder.GetFromLocation(_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.Append(address.GetAddressLine(i))
                                 .AppendLine(",");
                }
                _addressText.Text = deviceAddress.ToString();
            }
            else
            {
                _addressText.Text = "Unable to determine the address.";
            }
        }
    }
}
