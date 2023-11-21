using PCAxis.Paxiom;
using PCAxis.Serializers;
using System.Reflection;
using CommandLine;
using PxTools.Bucket.Client;

class Program
{
    public class Options
    {
        [Option('l', "list", Required = false, HelpText = "List available serializers.")]
        public bool List { get; set; }

        [Option('i', "input", Required = false, HelpText = "Input .px file path.")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file path.")]
        public string Output { get; set; }

        [Option('t', "serializer", Required = false, HelpText = "Serializer type.")]
        public string Serializer { get; set; }

        [Option('k', "accessKey", Required = false, HelpText = "AWS access key.")]
        public string AccessKey { get; set; }

        [Option('s', "secretKey", Required = false, HelpText = "AWS secret key.")]
        public string SecretKey { get; set; }

        [Option('e', "endpoint", Required = false, HelpText = "S3 service endpoint URL.")]
        public string Endpoint { get; set; }

        [Option('x', "sessionToken", Required = false, HelpText = "AWS session token.")]
        public string SessionToken { get; set; }
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
        builder.SetPath(inputPath);
        builder.BuildForSelection();
        var selection = Selection.SelectAll(builder.Model.Meta);
        builder.BuildForPresentation(selection);
        var model = builder.Model;

        using (FileStream stream = new FileStream(outputPath, FileMode.Create))
        {
            var serializerInstance = SerializerFactory.CreateSerializer(serializerType);
            serializerInstance!.Serialize(model, stream);
            Console.WriteLine($"Serialization to {serializerType} completed.");
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
            Console.WriteLine("Please provide valid input, output paths, and serializer type using -i, -o, and -t respectively.");
            return;
        }

        // Initialize BucketClient if needed
        BucketClient bucketClient = null;
        if (IsS3Uri(opts.Input) || IsS3Uri(opts.Output))
        {
            bucketClient = new BucketClient(opts.Endpoint, opts.AccessKey, opts.SecretKey, false, null, null, opts.SessionToken);
        }

        // Process input path
        string localInputPath = opts.Input;
        if (IsS3Uri(opts.Input))
        {
            localInputPath = DownloadFromS3(bucketClient, opts.Input);
        }

        // Set a temporary output path if output is an S3 URI
        string localOutputPath = IsS3Uri(opts.Output) ? Path.GetTempFileName() : opts.Output;

        // Process the file
        SerializeModel(opts.Serializer, localInputPath, localOutputPath);

        // Upload to S3 if output is an S3 URI
        if (IsS3Uri(opts.Output))
        {
            UploadToS3(bucketClient, opts.Output, localOutputPath);
        }

        // Clean up local files if necessary
        if (IsS3Uri(opts.Input))
        {
            File.Delete(localInputPath);
        }
        if (IsS3Uri(opts.Output))
        {
            File.Delete(localOutputPath);
        }
    }

    private static string DownloadFromS3(BucketClient bucketClient, string s3Uri)
    {
        (string bucketName, string objectKey) = ParseS3Uri(s3Uri);
        string localPath = Path.GetTempFileName();
        using (var fileStream = File.Create(localPath))
        {
            bucketClient.ReadFile(objectKey, stream => stream.CopyTo(fileStream), bucketName);
        }
        return localPath;
    }

    private static void UploadToS3(BucketClient bucketClient, string s3Uri, string localPath)
    {
        (string bucketName, string objectKey) = ParseS3Uri(s3Uri);
        using (var fileStream = File.OpenRead(localPath))
        {
            bucketClient.UploadFile(objectKey, fileStream, bucketName);
        }
    }

    private static (string, string) ParseS3Uri(string s3Uri)
    {
        var uri = new Uri(s3Uri);
        string bucket = uri.Host;
        string key = uri.AbsolutePath.TrimStart('/');
        return (bucket, key);
    }

    private static bool IsS3Uri(string path)
    {
        return !string.IsNullOrEmpty(path) && path.StartsWith("s3://");
    }

    private static void HandleParseError(IEnumerable<Error> errors)
    {
        Console.WriteLine("Error parsing command line arguments.");
    }
}
