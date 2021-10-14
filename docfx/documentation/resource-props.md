# Accessing Resource Properties

The properties of a resource may be read or modified using the property accessor methods on [IResource](xref:Firefly.CloudFormationParser.IResource). This permits the modification of resources that would be associated with making a CloudFormation Package.

## Property Path Syntax

To reference a particular property, use a path syntax as follows.

```yaml
  ConfigRole: # Role for config service, allowing access to S3 bucket and SNS
    Type: "AWS::IAM::Role"
    Properties:
      AssumeRolePolicyDocument:
        Version: "2012-10-17"
        Statement:
          - Effect: "Allow"
            Principal:
              Service: "config.amazonaws.com"
            Action:
              - "sts:AssumeRole"
```

If we wanted to get to value of the `Action` in the policy document above, the path syntax would be

`AssumeRolePolicyDocument.Statement.0.Action.0`

That is to say, for mapping entries use the key name and for list entries, use the zero-based index.

## Property Accessor Methods

* [IResource.GetResourcePropertyValue](xref:Firefly.CloudFormationParser.IResource.GetResourcePropertyValue(System.String)) - Gets the value of a property at the given path.
* [IResource.UpdateResourceProperty](xref:Firefly.CloudFormationParser.IResource.UpdateResourceProperty(System.String,System.Object)) - Replaces the value of a propery at the given path.

