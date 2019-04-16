﻿using System.Collections.Generic;
using System.IO;
using Microsoft.ML.SamplesUtils;

namespace Microsoft.ML.Samples.Dynamic.DataOperations
{
    public class SaveAndLoadFromBinary
    {
        public static void Example()
        {
            MLContext mlContext = new MLContext();

            // Get a small dataset as an IEnumerable.
            IEnumerable<DatasetUtils.SampleTemperatureData> enumerableOfData = DatasetUtils.GetSampleTemperatureData(5);

            // Load dataset into an IDataView. 
            IDataView data = mlContext.Data.LoadFromEnumerable(enumerableOfData);

            // Creating a FileStream object to create a file and use             
            // the stream to create a binary file.
            using (FileStream stream = new FileStream("./sample-temp-data.idv", FileMode.Create))
            {
                mlContext.Data.SaveAsBinary(data, stream);
            }

            // Load a binary file by file path.
            IDataView binaryData = mlContext.Data.LoadFromBinary("./sample-temp-data.idv");
        }
    }
}