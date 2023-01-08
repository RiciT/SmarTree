using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace SmarTree
{
    class Connect
    {

        static void Main(string[] args)
        {
            int lastValue = -1;
            while (true)
            {
                Console.WriteLine("waiting 5 sec before next update");
                Thread.Sleep(5000);
                try
                {
                    string html = string.Empty;
                    string url = @"http://165.227.145.200/logs-41236/";

                    html = Get(url);
                    var deserializedTreeinfo = JsonConvert.DeserializeObject<List<JObject>>(html);

                    // deserializedTreeinfo.Select(onemessage =>
                    //{
                    //    var str = onemessage["stringMessage"].ToString();
                    //    var deserializedOneTree = JsonConvert.DeserializeObject<JObject>(str);
                    //    return new
                    //    {
                    //        treeid = int.Parse(deserializedOneTree["treeid"].ToString()),
                    //        moisture = deserializedOneTree["moisture"].ToString(),
                    //        timestamp = DateTime.Parse(deserializedOneTree["timestamp"].ToString()),
                    //    };
                    //})
                    //.OrderBy(elem3 => elem3.timestamp)
                    //.GroupBy(elem2 => elem2.treeid)
                    //.ToList()
                    //;

                    var onemessage = deserializedTreeinfo.Last();
                    var str = onemessage["stringMessage"].ToString();
                    var deserializedOneTree = JsonConvert.DeserializeObject<JObject>(str);
                    int moisture = int.Parse(deserializedOneTree["moisture"].ToString());

                    if (moisture == lastValue)
                    {
                        Console.WriteLine("Skipping update, same moisture data: " + moisture);
                        continue;
                    }
                    else
                    {
                        lastValue = moisture;
                    }

                    var dataobject = new
                    {
                        treeid = int.Parse(deserializedOneTree["treeid"].ToString()),
                        moisture = deserializedOneTree["moisture"].ToString(),
                        timestamp = DateTime.Parse(onemessage["timestamp"].ToString()),
                    };

                    string airtableURL = @"https://api.airtable.com/v0/appCOKZPTfpaZ0RkZ/Coordinates/rec1zePcwAjq2UNi7";
                    string postData = "{ \"fields\": { \"Moisture\": " + dataobject.moisture + "} }";
                    string contentType = "application/json";

                    var retdata = Post(airtableURL, postData, contentType, "PATCH");
                    //Console.WriteLine(retdata);
                    Console.WriteLine("Successfully updated the moisture data: " + moisture);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error while updating: {ex.Message}");
                }
            }
        }

        public static string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string Post(string uri, string data, string contentType, string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            WebHeaderCollection headerRequest = request.Headers;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;
            request.Method = method;
            headerRequest.Add("Authorization: Bearer keyCpSbvruShOPMWj");
            using (Stream requestBody = request.GetRequestStream())
            {
                requestBody.Write(dataBytes, 0, dataBytes.Length);
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
