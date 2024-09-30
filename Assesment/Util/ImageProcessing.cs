using Assesment.Models.Dtos;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Drawing;
namespace Assesment.Util
{
    public class ImageProcessing
    {
        // Set the paths to your YOLO model files
        string cfgPath = @"C:/yolo/yolov3.cfg";
        string weightsPath = @"C:/yolo/yolov3_2000.weights";
        string labelsPath = @"C:/yolo/labels.name";
        private readonly Net yolo;

        public ImageProcessing()
        {
            // Load YOLO model
            yolo = DnnInvoke.ReadNetFromDarknet(cfgPath, weightsPath);
            yolo.SetPreferableBackend(Emgu.CV.Dnn.Backend.OpenCV);
            yolo.SetPreferableTarget(Target.Cpu);
        }

        public async Task<ObjectDetectionResult> PerformObjectDetection(string imagePath)
        {
            // Load the uploaded image
            Mat image = CvInvoke.Imread(imagePath);
            // Load the class labels
            var classLabels = File.ReadAllLines(labelsPath);
            var blob = DnnInvoke.BlobFromImage(image, 1 / 255.0, new Size(416, 416), new MCvScalar(), true, false);
            yolo.SetInput(blob);

            // Forward pass through the YOLO network
            // Forward pass to get output from YOLO
            var output = new VectorOfMat();
            yolo.Forward(output, yolo.UnconnectedOutLayersNames);

            // Parse the detections
            var detectedObjects = ParseYOLOOutput(output, image, classLabels);

            // Calculate match percentage (e.g., % of objects detected that match the YOLO classes)
            var matchPercentage = CalculateMatchPercentage(detectedObjects, classLabels);

            return new ObjectDetectionResult
            {
                DetectedObjects = detectedObjects,
                MatchPercentage = matchPercentage
            };


        }


        public List<DetectedObject> ParseYOLOOutput(VectorOfMat output, Mat image, string[] classLabels)
        {
            List<Rectangle> boxes = new List<Rectangle>();
            List<float> confidences = new List<float>();
            List<int> classIds = new List<int>();
            float confidenceThreshold = 0.5f;
            float nmsThreshold = 0.4f;

            for (int i = 0; i < output.Size; i++)
            {
                Mat mat = output[i];
                for (int j = 0; j < mat.Rows; j++)
                {
                    // Use Ptr<float>() to get a pointer to the current row
                    IntPtr rowPtr = mat.Row(j).DataPointer;
                    float[] row = new float[mat.Cols];
                    System.Runtime.InteropServices.Marshal.Copy(rowPtr, row, 0, mat.Cols);

                    // Extract values from the row array
                    float centerX = row[0] * image.Width;
                    float centerY = row[1] * image.Height;
                    float width = row[2] * image.Width;
                    float height = row[3] * image.Height;

                    // Calculate the top-left corner of the bounding box
                    int left = (int)(centerX - width / 2);
                    int top = (int)(centerY - height / 2);

                    // Get the confidence scores (confidence starts at index 5 in YOLO output)
                    float[] scores = row.Skip(5).ToArray(); // Skip first 5 elements
                    float maxVal = scores.Max();
                    int classId = Array.IndexOf(scores, maxVal);

                    if (maxVal > confidenceThreshold)
                    {
                        boxes.Add(new Rectangle(left, top, (int)width, (int)height));
                        confidences.Add(maxVal);
                        classIds.Add(classId); // Class ID based on the highest confidence score
                    }
                }
            }

            List<DetectedObject> detectedObjects = new List<DetectedObject>();
            for (int idx = 0; idx < boxes.Count; idx++)
            {
                string label = classLabels[classIds[idx]];
                detectedObjects.Add(new DetectedObject
                {
                    ClassId = classIds[idx],
                    Label = label,
                    Confidence = confidences[idx],
                    BoundingBox = boxes[idx]
                });
            }

            return detectedObjects;
        }

        public double CalculateMatchPercentage(List<DetectedObject> detectedObjects, string[] classLabels)
        {
            int totalClasses = classLabels.Length;
            int detectedClasses = detectedObjects.Select(o => o.ClassId).Distinct().Count();

            // Matching percentage: detected unique classes vs total possible classes
            double matchPercentage = ((double)detectedClasses / totalClasses) * 100;

            return matchPercentage;
        }


    }
}
