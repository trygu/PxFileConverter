using PCAxis.Paxiom;
using PCAxis.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;

class Program
{
    // Command line options class
    public class Options
    {
        [Option('l', "list", Required = false, HelpText = "List available serializers.")]
        public bool List { get; set; }

        [Option('i', "input", Required = false, HelpText = "Input .px file path.")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file path.")]
        public string Output { get; set; }

        [Option('s', "serializer", Required = false, HelpText = "Serializer type.")]
        public string Serializer { get; set; }
    }

    private static class SerializerFactory
    {
        public static IPXModelStreamSerializer CreateSerializer(string serializerType)
        {
            var serializerTypes = GetAvailableSerializers();
            if (!serializerTypes.ContainsKey(serializerType.ToLower()))
            {
                throw new ArgumentException($"Unsupported format: {serializerType}");
            }

            var serializerInstance = (IPXModelStreamSerializer)Activator.CreateInstance(serializerTypes[serializerType.ToLower()]);

            if (serializerInstance is XlsxSerializer xlsxSerializer)
            {
                xlsxSerializer.DoubleColumn = DoubleColumnType.NoDoubleColumns;
                xlsxSerializer.InformationLevel = InformationLevelType.AllInformation;
            }

            return serializerInstance;
        }
    }

    private static Dictionary<string, Type> GetAvailableSerializers()
    {
        Assembly assembly = typeof(PCAxis.Serializers.Html5TableSerializer).Assembly;

        var serializerTypes = assembly.GetTypes()
                                      .Where(t => t.GetInterfaces().Contains(typeof(IPXModelStreamSerializer)))
                                      .ToDictionary(t => t.Name.Replace("Serializer", "").ToLower(), t => t);

        return serializerTypes;
    }

    private static void SerializeModel(string serializerType, string inputPath, string outputPath)
    {
        var builder = new PXFileBuilder();
        builder.SetPath(new Uri(inputPath, UriKind.Relative).ToString());
        builder.BuildForSelection();
        var selection = Selection.SelectAll(builder.Model.Meta);
        builder.BuildForPresentation(selection);
        var model = builder.Model;

        using (FileStream stream = new FileStream(new Uri(outputPath, UriKind.Relative).ToString(), FileMode.Create))
        {
            var serializerInstance = SerializerFactory.CreateSerializer(serializerType);
            try
            {
                serializerInstance!.Serialize(model, stream);
                Console.WriteLine($"Serialization to {serializerType} completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => HandleOptions(opts))
              .WithNotParsed(errors => HandleParseError(errors));
    }

    private static void HandleOptions(Options opts)
    {
        if (opts.List)
        {
            Console.WriteLine("Available serializers:");
            foreach (var type in GetAvailableSerializers().Keys)
            {
                Console.WriteLine(type);
            }
            return;
        }

        if (string.IsNullOrEmpty(opts.Input) || string.IsNullOrEmpty(opts.Output) || string.IsNullOrEmpty(opts.Serializer))
        {
            Console.WriteLine("Please provide valid input, output paths, and serializer type using -i, -o, and -s respectively.");
            return;
        }

        SerializeModel(opts.Serializer, opts.Input, opts.Output);
    }

    private static void HandleParseError(IEnumerable<Error> errors)
    {
        Console.WriteLine("Error parsing command line arguments.");
    }
}
