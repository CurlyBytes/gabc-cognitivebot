﻿using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Training;
using Microsoft.Cognitive.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrainCustomVision
{
    class Program
    {
        private static List<string> gitarImages;

        private static List<string> candleImages;        

        static void Main(string[] args)
        {
            // Add your training key from the settings page of the portal
            string trainingKey = "d34ef84918894544889d7136f32d4e67";

            // Create the Api, passing in the training key
            TrainingApi trainingApi = new TrainingApi() { ApiKey = trainingKey };

            // Getting Project
            Console.WriteLine("Getting project");
            var project = trainingApi.GetProject(new Guid("06d4aeff-fb9d-453f-8912-5b2e89d1f1d4"));

            // Make two tags in the new project
            var gitarTag = trainingApi.CreateTag(project.Id, "Gitars");
            var candleTag = trainingApi.CreateTag(project.Id, "Candles");

            // Add some images to the tags
            Console.WriteLine("\tUploading images");
            LoadImagesFromDisk();
     
            var imageFiles = gitarImages.Select(img => new ImageFileCreateEntry(Path.GetFileName(img), File.ReadAllBytes(img))).ToList();
            trainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch(imageFiles, new List<Guid>() { gitarTag.Id }));

            imageFiles = candleImages.Select(img => new ImageFileCreateEntry(Path.GetFileName(img), File.ReadAllBytes(img))).ToList();
            trainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch(imageFiles, new List<Guid>() { candleTag.Id }));

            // Now there are images with tags start training the project
            Console.WriteLine("\tTraining");
            var iteration = trainingApi.TrainProject(project.Id);

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Thread.Sleep(1000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIteration(project.Id, iteration.Id);
            }

            // The iteration is now trained. Make it the default project endpoint
            iteration.IsDefault = true;
            trainingApi.UpdateIteration(project.Id, iteration.Id, iteration);
            Console.WriteLine("Done!\n");
           
            Console.ReadKey();
        }

        private static void LoadImagesFromDisk()
        {
            // this loads the images to be uploaded from disk into memory
            gitarImages = Directory.GetFiles(@"..\..\..\Images\Gitar").ToList();
            candleImages = Directory.GetFiles(@"..\..\..\Images\Candle").ToList();
        }
    }
}
