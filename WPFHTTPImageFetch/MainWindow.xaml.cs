using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WPFHTTPImageFetch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        string url =    "https://script.google.com/macros/s/AKfycbw8laScKBfxda2Wb0g63gkYDBdy8NWNxINoC4xDOwnCQ3JMFdruam1MdmNmN4wI5k4/exec";
        string url2 = "https://www.google.com";
        string id = "1y2S2kaDWGZrAWDaxjQZZSSgY29Ad-oVE";
        string tempImageString ="";
        BitmapImage imageControl = new BitmapImage();


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.WriteDebug("LoadImageButton_Click called");
            string originalContent = LoadImageButton.Content.ToString();
            LoadImageButton.Content = "Loading...";

            //set up the URL with the query parameters
            var queryParams = new Dictionary<string, string>
            {
                { "id", id }
            };

            string fullUrl = QueryStringHelper.BuildUrlWithQueryStringUsingStringConcat(url, queryParams);

            Helpers.WriteDebug("Full URL: " + fullUrl);

            tempImageString = await ReadStringFromUriAsync(fullUrl);
            Helpers.WriteDebug("Image data received: " );
            LoadImageButton.Content = "Loaded";


            //todo convert the string to a byte array and then to an image
            byte[] imageBytes = Convert.FromBase64String(tempImageString);
            Helpers.WriteDebug("Image bytes converted from base64 string.");
            
             imageControl = LoadImage(imageBytes);

            ImagePlaceholder.Source = imageControl;
            LoadImageButton.Content = originalContent;
        }


        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public async Task<string> ReadStringFromUriAsync(string url)
        {
            Helpers.WriteDebug($"Entering Task HTTPClientKludge({url}...");
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("You must supply a url to interrogate for this function to work.");

            Uri uri;
            Helpers.WriteDebug($"Attempting to create Uri from {url}...");
            // Attempt to create a Uri from the provided URL.
            try
            {
                string response = await _httpClient.GetStringAsync(url);
                return response;
            }
            catch  (HttpRequestException e)
            {
                Helpers.WriteDebug($"HttpRequestException: {e.Message}");
                throw new InvalidOperationException("An error occurred while making the HTTP request.", e);
            }
            catch (System.Exception e)
            {
                Helpers.WriteDebug($"Exception: {e.Message}");
                throw new InvalidOperationException("An unexpected error occurred.", e);
            }


           
        }

     

        public static class QueryStringHelper
        {
            public static string BuildUrlWithQueryStringUsingStringConcat(
                string basePath, Dictionary<string, string> queryParams)
            {
                var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                return $"{basePath}?{queryString}";
            }
        }

    }
}