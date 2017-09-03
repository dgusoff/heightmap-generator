using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace HeightmapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            double mapWidthInMiles = 5.0;
            double mapWidthInDegrees = mapWidthInMiles / 69;

            double westLongitude = -78.450771;
            double northLatitude = 40.019914; // bedford PA

            double eastLongitude = westLongitude + mapWidthInDegrees;
            double southLatitude = northLatitude - mapWidthInDegrees;
            int numSubdivisions = 1024;
            double distanceBetweenPonts = (northLatitude - southLatitude) / (numSubdivisions - 1);
            Console.WriteLine($"Distance Between Points is {distanceBetweenPonts.ToString()}.");
            string requestUrlTemplate = "http://dev.virtualearth.net/REST/v1/Elevation/Polyline?points={0},{1},{0},{2}&samples={3}&key={4}";
            string key = "-- you need to provide a Bing API key --";

            List<int> heights = new List<int>();

            int maxHeight = 0;
            int minHeight = 29000;

            for(var i = 0; i < numSubdivisions; i++)
            {
                Console.WriteLine("---------------" + i + "-------------------------------");
                double lat = southLatitude + (distanceBetweenPonts * i);
                double long1 = westLongitude;
                double long2 = eastLongitude;

                string url = string.Format(requestUrlTemplate, lat, long1, long2, numSubdivisions, key);

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                    }
                    else
                    {
                        Encoding enc = System.Text.Encoding.GetEncoding(0);
                        StreamReader loResponseStream = new StreamReader(response.GetResponseStream(), enc);

                        string Response = loResponseStream.ReadToEnd();
                        dynamic jsonData = JsonConvert.DeserializeObject(Response);

                        foreach (var height in jsonData.resourceSets[0].resources[0].elevations)
                        {
                            heights.Add(Convert.ToInt32(height));
                            if (height > maxHeight) maxHeight = height;
                            if (height < minHeight) minHeight = height;
                            Console.Write(height + ",");
                        }
                        Console.WriteLine($"Max height is {maxHeight}, min height is {minHeight}.");
                        Console.WriteLine($"Height differnce is {maxHeight - minHeight}");
                    }
                }
            }
            Console.WriteLine($"heights has {heights.Count} members.");

            CreateHeightmapFile(heights, numSubdivisions);
            
        }

        public static void CreateHeightmapFile(List<int> heights, int width)
        {
            Bitmap bmp = new Bitmap(width, width);
            for (int i = 0; i < width + 1; i++)
            {
                for (int j = 0; j < width + 1; j++)
                {
                    if (i < width && j < width)
                    {
                        int index = j + (width * i);
                        int byteValue = heights[index];

                        if (byteValue < 0)
                        {
                            bmp.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                        }
                        else
                        {
                            bmp.SetPixel(i, j, Color.FromArgb(255, byteValue, byteValue, byteValue));
                        }
                    }
                }
            }
            bmp.Save(@"C:\git\graphics\terragen\generatedTerrain.png", ImageFormat.Png);
        }
    }   
}
