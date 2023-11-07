# PxFileConverter

`PxTools.FileConverter` is a simple console application designed for the conversion of `.px` files into various serialization formats. Built on top of the PCAxis framework, it offers the transformation of statistical data with ease and precision.

## Features

- **Dynamic Serialization**: Convert `.px` files into various formats including JSON, Excel, Parquet, and more.
- **List Available Serializers**: Quickly view the supported serialization formats.
- **Command Line Driven**: Intuitive command line arguments for easy operations.

## Prerequisites

- .NET Core 3.1 or later

## Dependencies

- `CommandLineParser` version `2.9.1`
- `PcAxis.Core` version `1.2.3`
- `PcAxis.Serializers` version `1.2.1`

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
app -i input.px -o output.json -s json
```
or
```
app --input input.px --output output.json --serializer json
```

## Parameters

- `-l` or `--list`: Lists the available serializers.
- `-i` or `--input`: Specifies the path to the input `.px` file.
- `-o` or `--output`: Specifies the path for the output serialized file.
- `-s` or `--serializer`: Specifies the type of serializer to use (e.g., `json`).

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)
