﻿using System;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms;

namespace Samples.Dynamic
{
    public static class CustomMappingSaveAndLoad
    {
        // This example shows how to define and apply a custom mapping of input
        // columns to output columns with a contract name. The contract name is
        // used in the CustomMappingFactoryAttribute that decorates the custom
        // mapping action. The pipeline containing the custom mapping can then be
        // saved to disk, and it can be loaded back after the assembly containing
        // the custom mapping action is registered.
        public static void Example()
        {
            // Create a new ML context, for ML.NET operations. It can be used for
            // exception tracking and logging, as well as the source of randomness.
            var mlContext = new MLContext();

            // Get a small dataset as an IEnumerable and convert it to an IDataView.
            var samples = new List<InputData>
            {
                new InputData { Age = 26 },
                new InputData { Age = 35 },
                new InputData { Age = 34 },
                new InputData { Age = 28 },
            };
            var data = mlContext.Data.LoadFromEnumerable(samples);

            // Custom transformations can be used to transform data directly, or as
            // part of a pipeline of estimators. The contractName must be provided
            // in order for a pipeline containing a CustomMapping estimator to be
            // saved and loaded back. The contractName must be the same as in the
            // CustomMappingFactoryAttribute used to decorate the custom action
            // defined by the user.
            var pipeline = mlContext.Transforms.CustomMapping(new
                IsUnderThirtyCustomAction().GetMapping(), contractName:
                "IsUnderThirty");

            var transformer = pipeline.Fit(data);

            // To save and load the CustomMapping estimator, the assembly in which
            // the custom action is defined needs to be registered in the
            // environment. The following registers the assembly where
            // IsUnderThirtyCustomAction is defined.    
            // This is necessary only in versions v1.5-preview2 and earlier
            mlContext.ComponentCatalog.RegisterAssembly(typeof(
                IsUnderThirtyCustomAction).Assembly);

            // Now the transform pipeline can be saved and loaded through the usual
            // MLContext method. 
            mlContext.Model.Save(transformer, data.Schema, "customTransform.zip");
            var loadedTransform = mlContext.Model.Load("customTransform.zip", out
                var inputSchema);

            // Now we can transform the data and look at the output to confirm the
            // behavior of the estimator. This operation doesn't actually evaluate
            // data until we read the data below.
            var transformedData = loadedTransform.Transform(data);

            var dataEnumerable = mlContext.Data.CreateEnumerable<TransformedData>(
                transformedData, reuseRowObject: true);

            Console.WriteLine("Age\tIsUnderThirty");
            foreach (var row in dataEnumerable)
                Console.WriteLine($"{row.Age}\t {row.IsUnderThirty}");

            // Expected output:
            // Age      IsUnderThirty
            // 26       True
            // 35       False
            // 34       False
            // 28       True
        }

        // The custom action needs to implement the abstract class
        // CustomMappingFactory, and needs to have attribute
        // CustomMappingFactoryAttribute with argument equal to the contractName
        // used to define the CustomMapping estimator which uses the action.
        [CustomMappingFactoryAttribute("IsUnderThirty")]
        private class IsUnderThirtyCustomAction : CustomMappingFactory<InputData,
            CustomMappingOutput>
        {
            // We define the custom mapping between input and output rows that will
            // be applied by the transformation.
            public static void CustomAction(InputData input, CustomMappingOutput
                output) => output.IsUnderThirty = input.Age < 30;

            public override Action<InputData, CustomMappingOutput> GetMapping()
                => CustomAction;
        }

        // Defines only the column to be generated by the custom mapping
        // transformation in addition to the columns already present.
        private class CustomMappingOutput
        {
            public bool IsUnderThirty { get; set; }
        }

        // Defines the schema of the input data.
        private class InputData
        {
            public float Age { get; set; }
        }

        // Defines the schema of the transformed data, which includes the new column
        // IsUnderThirty.
        private class TransformedData : InputData
        {
            public bool IsUnderThirty { get; set; }
        }
    }
}
