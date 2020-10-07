using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace ADN_Verification
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        IFaceClient client = new FaceClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY"))) { Endpoint = Environment.GetEnvironmentVariable("FACE_ENDPOINT") };

        public MainWindow()
        {
            InitializeComponent();
        }


        private async void btnUpload_Click(object sender, RoutedEventArgs e)
        {

            

            var openFrame = new Microsoft.Win32.OpenFileDialog { Filter = "JPEG Image(*.jpg)|*.jpg" };
            openFrame.Multiselect = true;
            var result = openFrame.ShowDialog(this);

            if (!(bool)result)
                return;

            //Get image 1
            var filePath = openFrame.FileNames[0];
            var fileUri = new Uri(filePath);
            var bitMapSource = new BitmapImage();


            bitMapSource.BeginInit();
            bitMapSource.CacheOption = BitmapCacheOption.None;
            bitMapSource.UriSource = fileUri;
            bitMapSource.EndInit();
            FaceImage0.Source = bitMapSource;

            //Get image 2
            var filePath1 = openFrame.FileNames[1];
            var fileUri1 = new Uri(filePath1);
            var bitMapSource1 = new BitmapImage();

            bitMapSource1.BeginInit();
            bitMapSource1.CacheOption = BitmapCacheOption.None;
            bitMapSource1.UriSource = fileUri1;
            bitMapSource1.EndInit();
            FaceImage1.Source = bitMapSource;

            //Analyze individual images
            string face0 = await MakeAnalysisRequest(filePath);
            string face1 = await MakeAnalysisRequest(filePath1);

            JArray jsonArray0 = JArray.Parse(face0);
            JArray jsonArray1 = JArray.Parse(face1);

            dynamic data = JObject.Parse(jsonArray0[0].ToString());
            dynamic data1 = JObject.Parse(jsonArray1[0].ToString());

            //Get face id of images
            string faceID = data.faceId;
            string faceID1 = data1.faceId;

            Console.WriteLine(await SimilarFaces(faceID, faceID1));

        }


        static async Task<string> MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY"));

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uri = Environment.GetEnvironmentVariable("FACE_ENDPOINT") + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                return contentString;
            }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }




        static async Task<string> SimilarFaces(string faceID1, string faceID2)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("FACE_SUBSCRIPTION_KEY"));

            // Assemble the URI for the REST API Call.
            string uri = Environment.GetEnvironmentVariable("FACE_ENDPOINT2");

            string body = "{\"faceId1\": " + "\"" + faceID1 + "\" ," + "\"faceId2\": " + "\"" + faceID2 + "\"}";

            HttpResponseMessage response = await client.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/json"));

            // Get the JSON response.
            string contentString = await response.Content.ReadAsStringAsync();

            return contentString;

        }


    }
}
