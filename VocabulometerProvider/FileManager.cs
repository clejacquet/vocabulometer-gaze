using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace VocabulometerProvider
{
    public class FileManager
    {
        //Return a list of gaze from a JSON file (the file path is took in parameter)
        public static List<Gaze> getDataFromFile(string filePath)
        {
            List<Gaze> parsedData = JsonConvert.DeserializeObject<List<Gaze>>(File.ReadAllText(filePath));
            return parsedData;
        }

        //Save in the file "fixations.json" the list of gaze took in parameter
        public static void saveDataInFile(List<Gaze> dataToSave)
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;

                string filePath = Path.GetFullPath("fixations.json");

                using (StreamWriter sw = new StreamWriter(@filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, dataToSave);
                }

                Console.WriteLine("Data saved in Json file !");
            }
            catch (Exception ex) { Console.WriteLine("Error when trying to save data" + ex); }
        }

    }
}
