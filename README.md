# Firefly.CloudFormationParser

[![Build status](https://ci.appveyor.com/api/projects/status/710rkxeyw1inj39w/branch/master?svg=true)](https://ci.appveyor.com/project/fireflycons/firefly-cloudformationparser/branch/master)


![Nuget](https://img.shields.io/nuget/v/Firefly.CloudFormationParser)

**WORK IN PROGESS**

There are so many questions on Stack Overflow and other sites related to the parsing of CloudFormation Templates in .NET. This is a problem I really wanted to solve once and for all, as I have several other repos here that have half-cocked CloudFormation support, and this package will be gradually integrated into them.

I set out to solve the [five main issues](https://fireflycons.github.io/Firefly.CloudFormationParser/documentation/gory-details.html) I see with parsing CloudFormation effectively, plus I wanted to be able to understand the dependency relationship between objects declared in a template. Should you for instance want to create a CloudFormation Linter, most of what you would need is here.

Using this library, templates may be parsed from a number of sources, currently:

* A string
* A file
* A stream
* From a deployed CloudFormation Stack
* From a template stored in S3

## Targets

- [![.NET Standard](https://img.shields.io/badge/.NET%20Standard-%3E%3D%202.0-blue.svg)](#)

Supports Source Link (using dedicated symbol packages)

To get it working you need to:
- Uncheck option "Enable Just My Code"
- Add the NuGet symbol server (*https://symbols.nuget.org/download/symbols*)
- Check option "Enable Source Link support"

## Next Steps

* View the [documentation](https://fireflycons.github.io/Firefly.CloudFormationParser/)