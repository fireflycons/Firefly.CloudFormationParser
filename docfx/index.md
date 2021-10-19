# Firefly.CloudFormationParser

There are so many questions on Stack Overflow and other sites related to the parsing of CloudFormation Templates in .NET. This is a problem I really wanted to solve once and for all, as I have several other repos here that have half-cocked CloudFormation support, and this package will be gradually integrated into them.

I set out to solve the [five main issues](./documentation/gory-details.md) I see with parsing CloudFormation effectively, plus I wanted to be able to understand the dependency relationship between objects declared in a template. Should you for instance want to create a CloudFormation Linter, most of what you would need is here.

Using this library, templates may be parsed from a number of sources, currently:

* A string
* A file
* A stream
* From a deployed CloudFormation Stack
* From a template stored in S3

<hr>

[Return to GitHub](https://github.com/fireflycons/Firefly.CloudFormationParser)