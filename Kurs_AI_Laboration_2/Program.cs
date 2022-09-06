using System;
using System.Drawing;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Microsoft.Extensions.Configuration;

namespace Kurs_AI_Laboration_2
{
    class Program
    {

        private static ComputerVisionClient cvClient;
        //static CustomVisionPredictionClient prediction_client;
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                //string prediction_endpoint = configuration["PredictionEndpoint"];
                //string prediction_key = configuration["PredictionKey"];
                //Guid project_id = Guid.Parse(configuration["ProjectID"]);
                //string model_name = configuration["ModelName"];

                Console.WriteLine("Do you want to analyze a local file or a image URL?");
                Console.WriteLine("1: Local file");
                Console.WriteLine("2: URL");
                string choice = Console.ReadLine();

                Console.WriteLine("Add image to analyze: ");
                string imageToAnalyze = Console.ReadLine();

                // Get image
                //string imageFile = @"C:\Users\Blank\source\repos\Kurs_AI_Laboration_2\Kurs_AI_Laboration_2\Labb-2-Images\Saxophone\saxophone3.jpg";
                string imageFile = $"{imageToAnalyze}";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }

                // Authenticate Computer Vision client
                Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials credentials = new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(cogSvcKey);
                cvClient = new ComputerVisionClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };

                // Authenticate a client for the prediction API
                //prediction_client = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(prediction_key))
                //{
                //    Endpoint = prediction_endpoint
                //};

                //Analyze image
                switch (choice)
                {
                    case "1":
                        await AnalyzeImage(imageFile);
                        break;
                    case "2":
                        await AnalyzeImageUrl(cvClient, imageFile);
                        //await ImageUrl(prediction_client, imageFile, project_id, model_name);
                        break;
                    default:
                        break;
                }

                //Get thumbnail
                await GetThumbnail(imageFile);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task AnalyzeImageUrl(ComputerVisionClient cvClient, string imageUrl)
        {
            {
                Console.WriteLine($"Analyzing image...");

                // Creating a list that defines the features to be extracted from the image. 

                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                     VisualFeatureTypes.Description,
                     VisualFeatureTypes.Tags,
                     VisualFeatureTypes.Categories,
                     VisualFeatureTypes.Brands,
                     VisualFeatureTypes.Objects,
                     VisualFeatureTypes.Adult
                };

                ImageAnalysis results = await cvClient.AnalyzeImageAsync(imageUrl, visualFeatures: features);

                //Get image captions
                foreach (var caption in results.Description.Captions)
                {
                    Console.WriteLine($"Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
                }

                // Image tags and their confidence score
                Console.WriteLine("Tags:");
                foreach (var tag in results.Tags)
                {
                    Console.WriteLine($"{tag.Name} {tag.Confidence}");
                }
                Console.WriteLine();
            }
        }

        static async Task AnalyzeImage(string imageFile)
        {
            Console.WriteLine($"Analyzing image...");

            // Specify features to be retrieved
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                 VisualFeatureTypes.Description,
                 VisualFeatureTypes.Tags,
                 VisualFeatureTypes.Categories,
                 VisualFeatureTypes.Brands,
                 VisualFeatureTypes.Objects,
                 VisualFeatureTypes.Adult
            };

            // Get image analysis
            using (var imageData = File.OpenRead(imageFile))
            {
                var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);

                // get image captions
                foreach (var caption in analysis.Description.Captions)
                {
                    Console.WriteLine($"Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
                }
                // Get image tags
                if (analysis.Tags.Count > 0)
                {
                    Console.WriteLine("Tags:");
                    foreach (var tag in analysis.Tags)
                    {
                        Console.WriteLine($" -{tag.Name} (confidence: {tag.Confidence.ToString("P")})");
                    }
                }
            }
        }

        static async Task GetThumbnail(string imageFile)
        {
            Console.WriteLine("Generating thumbnail");

            // Generate a thumbnail
            using (var imageData = File.OpenRead(imageFile))
            {
                // Get thumbnail data
                var thumbnailStream = await cvClient.GenerateThumbnailInStreamAsync(100, 100, imageData, true);
                // Save thumbnail image
                string thumbnailFileName = @"C:\Users\Blank\source\repos\Kurs_AI_Laboration_2\Kurs_AI_Laboration_2\thumbnail.jpg";
                using (Stream thumbnailFile = File.Create(thumbnailFileName))
                {
                    thumbnailStream.CopyTo(thumbnailFile);
                }
                Console.WriteLine($"Thumbnail saved in {thumbnailFileName}");
            }

        }
        //static async Task ImageUrl(CustomVisionPredictionClient predictionApi, string imageUrl, Guid projectId, string modelName)
        //{
        //    Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImageUrl url = new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImageUrl(imageUrl);
        //    var fileStream = File.OpenRead(imageUrl);
        //    var result = predictionApi.ClassifyImageUrl(projectId, modelName, url);
        //    //var result = await predictionApi.ClassifyImageAsync(projectId, modelName, fileStream);
        //    foreach (var predictions in result.Predictions)
        //    {
        //        Console.WriteLine($"Tag - {predictions.TagName}. Probability - {predictions.Probability}");
        //    }
        //}


    }
}