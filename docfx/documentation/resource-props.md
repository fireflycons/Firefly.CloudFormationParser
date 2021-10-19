# Property Manipulation

Using this library it is possible to read, change or create properties on resources prior to re-serializing the template. This is especially useful when implementing functionality such as `aws cloudformation package` where you would package local files to S3 and then adjust the resource to point to the uploaded S3 object.

## Methods for Property Manipulation

* [IResource.GetPropertyValue](xref:Firefly.CloudFormationParser.IResource.GetResourcePropertyValue(System.String)) - Gets the value at the given property path. This may be a scalar value if the property is a leaf node in the resource declaration, or a dictionary or list if the requested property has descendants. If it is a leaf node and there is an intrinsic at the node, then the library attempts to evaluate the result of the intrinsic. The evaluation will depend on values provided for template parameters. If the property is not found, then `null` is returned.
* [IResource.UpdateResourceProperty](xref:Firefly.CloudFormationParser.IResource.UpdateResourceProperty(System.String,System.Object)) - Sets the value of the property at the given property path. The value of the property at the given path is replaced with the value you provide. This value may be a scalar or a complete object graph. If the given path does not resolve, then object graph is added for the unresolved path segments before the value is placed at the new leaf node.

## Property Path Syntax

Paths are expressed in dotted notation, and list elements with a zero-based index e.g.

```yaml
  MyLambda:
    Type: AWS::Serverless::Function
    Properies:
      VpcConfig:
        SecurityGroupIds:
          - sg-123
          - sg-456
```
`VpcConfig.SecurityGroupIds` as a path will return a `List<object>` containing the security group IDs

```yaml
  iamRole:
    Type: AWS::IAM::Role
    Properties:
      Path: /
      AssumeRolePolicyDocument:
        Statement:
          - Effect: Allow
            Action: sts:AssumeRole
            Principal:
              Service:
                - lambda.amazonaws.com
      Policies:
        - PolicyName: ADConnector
          PolicyDocument:
            Version: 2012-10-17
            Statement:
              - Sid: CreateAdConnectorEc2Resources
                Effect: Allow
                Action:
                  - ec2:DescribeSubnets
                Resource: '*'
                Condition:
                  Bool:
                    aws:ViaAWSService: true
              - Sid: DeleteAdConnectorEc2Resources
                Effect: Allow
                Action:
                  - ec2:DeleteSecurityGroup
                Resource: '*'
                Condition:
                  Bool:
                    aws:ViaAWSService: true
```
* `Polices.0.PolicyDocument.Statement.0.Sid` will return the value `CreateAdConnectorEc2Resources`
* `Polices.0.PolicyDocument.Statement` will return a `List<object>` containing all the statements.

```yaml
Parameters:
  Param1:
    Type: String
    Value: MyValue

Resources:
  SSMParameter:
    Type: AWS::SSM::Parameter
    Properties:
      Type: String
      Name: MyName
      Value: !Ref Param1
```
`Value` will return `MyValue`. If an alternate value for parameter `Param1` is set by the [deserializer builder](xref:Firefly.CloudFormationParser.Serialization.Settings.DeserializerSettingsBuilder.WithParameterValues(System.Collections.Generic.IDictionary{System.String,System.Object})) then that value will be returned.


