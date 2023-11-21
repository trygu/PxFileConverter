# PxFileConverter

`PxTools.FileConverter` is a simple console application designed for the conversion of `.px` files into various serialization formats. Built on top of the PCAxis framework, it offers the transformation of statistical data with ease and precision, now with added support for Amazon S3.

## Features

- **Dynamic Serialization**: Convert `.px` files into various formats including JSON, Excel, Parquet, and more.
- **S3 Support**: Directly read from and write to AWS S3 buckets.
- **List Available Serializers**: Quickly view the supported serialization formats.
- **Command Line Driven**: Intuitive command line arguments for easy operations.

## Prerequisites

- .NET Core 3.1 or later
- AWS account and credentials for S3 access (if using S3 features)

## Dependencies

- `CommandLineParser` version `2.9.1`
- `PcAxis.Core` version `1.2.3`
- `PcAxis.Serializers` version `1.2.2`
- `PxTools.Bucket` version `0.0.2`

## Installation

1. Clone the repository:
   ```
   git clone https://github.com/trygu/PxTools.FileConverter.git
   ```
2. Navigate to the project directory:
   ```
   cd PxTools.FileConverter
   ```
3. Restore the NuGet packages:
   ```
   dotnet restore
   ```

## Usage

**List Available Serializers**:
```
app -l
```
or
```
app --list
```

**Convert `.px` File**:
```
app -i input.px -o output.json -t json
```
or
```
app --input input.px --output output.json --serializer json
```

**Using S3 for Input/Output**:
```
app -i s3://bucketname/input.px -o s3://bucketname/output.json -t json -e endpoint -k accessKey -s secretKey
```

## Parameters

- `-l` or `--list`: Lists the available serializers.
- `-i` or `--input`: Specifies the path to the input `.px` file or S3 URI.
- `-o` or `--output`: Specifies the path for the output serialized file or S3 URI.
- `-t` or `--serializer`: Specifies the type of serializer to use (e.g., `json`).
- `-e` or `--endpoint`: (Optional for S3) Specifies the S3 service endpoint URL.
- `-k` or `--accessKey`: (Optional for S3) Specifies the AWS access key.
- `-s` or `--secretKey`: (Optional for S3) Specifies the AWS secret key.
- `-x` or `--sessionToken`: (Optional for S3) Specifies the AWS session token.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)
