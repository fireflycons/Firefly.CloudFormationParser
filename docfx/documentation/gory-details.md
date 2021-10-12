# The Gory Details

[YamlDotNet](https://github.com/aaubry/YamlDotNet) as many of you will know, is extremely sparse on documentation! That gave this project a very steep learning curve. I found that pretty much what you want from YamlDotNet is actually there as you work through each particular problem. The further into this library I got, I came across situations where I thought _"I wonder how you do this?"_ only to find that with more digging, there was some interface that could be implemented to override some default functionality, both within the serialization and deserialization phases.

In order to get to grips with how YamlDotNet works, and what it is capable of, I resorted to cloning the latest release and adding it directly as a project reference in the solution file for this package so I could easily debug through it as the project progressed.

CloudFormation is a complicated beast. There are many articles in Stack Overflow and the like, giving examples of how to deserialize a custom tag to an object but all those articles are too simplistic when it comes to CloudFormation. Specifically we have to solve the following problems associated with [CloudFormation intrinsics](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference.html).

## Five Issues

1. An intrinsic may be declared in long or short form <sup>[1](#1-long-and-short-form)</sup>
1. An intrinsic may have other intrinsics nested within it, either immediately, or way down within a nested dictionary structure that is an argument to the intrinsic <sup>[2](#2-nesting-intrinsics)</sup>.
1. The same intrinsic may have several different arrangements of properties (`!Sub` and `!GetAZs` notably) <sup>[2](#2-nesting-intrinsics) [3](#3-differing-properties)</sup>.
1. A key in the schema may, given the context, be interpreted as an intrinsic in one part of a template, and not in others. `Condition:` (long form) when found in the `Conditions` section of a template should be parsed as a [Condition](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/intrinsic-function-reference-condition.html) intrinsic, but when found as a [condition statement](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_elements_condition.html) in a policy document, it is _not_ an intrinsic <sup>[4](#4-contextual-differences)</sup>.
5. Short form intrinsics with a single argument of another intrinsic. This sends the default parser into meltdown! it sees the next event in the parse stream tagged with the same tag and enters a recursion resulting in a stack overflow. In the example below <sup>[5](#5-intrinsic-with-intrinsic-as-argument)</sup>, the parser expects `!Base64` to be created from the current parsing event and the next parsing event is a mapping to enter `Fn::Sub:`, however that mapping event carries the `Base64` tag. Calling the `Func<IParser, Type, object?> nestedObjectDeserializer` argument passed to our `INodeDeserializer` implementation to parse this re-enters the same code for parsing `Base64` until the stack blows.

# Solving Deserialization

Firstly, how to parse the short form intrinsics which are expressed as YAML tags. If these intrinsics were simple objects with consistent property sets, then a simple [TypeConverter](https://www.google.com/search?q=yamldotnet+type+converter) implementation would do. However, this does not address issues 2, 3 and 4 above. The way to attack this is to provide an implementation of [INodeTypeResolver](https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Serialization/INodeTypeResolver.cs) to select implementations of [INodeDeserializer](https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Serialization/INodeDeserializer.cs) for each intrinsic to get more control of the deserialization process. The `Deserialize` method of `INodeDeserializer` which we implement is passed a `Func` argument which can be called to recursively deserialize the properties of the current intrinsic, thus it addresses issues 2 and 3 from the list above.

Now to the fourth issue. I found that it would be necessary to be able to pass information to my implementation of `INodeTypeResolver` to indicate where in the template the parse currently is. I did this using an implementation of [IObjectFactory](https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Serialization/IObjectFactory.cs) that can see when a concrete template section (Parameters, Conditions, Resources etc) is being created. The object factory then fires an event that connects to the `INodeTypeResolver` implementation so that the resolver can decide whether a key such as `Condition:` when buried deep within a resource's properties should be deserialized as intrinsic, or just passed to the default deserializer which will create a `Dictionary<object, object>`. In the case of `Condition`, if a resource is being created the latter of the two strategies should be employed.

The final issue, that of a stack overflow. Here I employed a read-ahead technique, buffering the complex object that follows (in the case of the example given it is a mapping, but can also be a sequence). I then remove the tag from the mapping or sequence start event and pass the buffer which is an implementation of [IParser](https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Core/IParser.cs) to the `Func` for nested object deserialization, and all is good.

## Long or Short Form?

As some may be well aware, YAML is a superset of JSON, thus YamlDotNet will parse JSON with little fuss. In the case of CloudFormation, the syntax of JSON requires that all intrinsics are coded long-form. Thus, when parsing JSON or long form syntax in YAML, the resulting object graph is exactly the same. Now the node deserializers become slightly ineffective in this situation. The `INodeTypeResolver` knows it needs to return an intrinsic if it sees the long form name as a key, however the key has already been consumed by the parser, therefore the object graph ends up containing a `KeyValuePair` that can look like `{"Ref", RefIntrinsic }` where what we actually want at the node where this KVP is, is just the intrinsic implementation.

Therefore the [Deserialize](xref:Firefly.CloudFormationParser.TemplateObjects.Template.Deserialize(Firefly.CloudFormationParser.Serialization.Settings.IDeserializerSettings)) method performs a second pass, walking the object graph fixing up these references.


# Solving Serialization

This was a little more straight forward than deserialization. In this case we can use implementations of [IYamlTypeConverter](https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Serialization/IYamlTypeConverter.cs) and override the `WriteYaml` method. Having said that, it was still necessary to find a way to serialize deep nests of objects that may be properties of the intrinsic currently being serialized. Now YamlDotNet provides [IValueSerializer](https://github.com/aaubry/YamlDotNet/blob/master/YamlDotNet/Serialization/IValueSerializer.cs) which is somewhat analogous to the `Func` argument passed to node deserializers, however it is not handily passed to you as a method argument on `WriteYaml`. Therefore it is necessary to create a default `ValueSerializer` and pass it to each `IYamlTypeConverter` implementation prior to the serialization run. Thus, when serializing the arguments of an intrinsic, where they are not a scalar or another intrinsic, they can be passed to the value serializer and the recursion of the object graph continues.

# Example Syntax

## 1 Long and Short Form

```yaml
Resources:
  Type: AWS::SSM::Parameter
  Properties:
    Type: String
    Name: !Ref ShortName
    Value:
      Ref: LongValue
```

## 2 Nesting Intrinsics

Note location of the two `!Ref` intrinsics...
```yaml
# --8<---
    - !If
      - SecretsManagerDomainCredentialsSecretsKMSKeyCondition
      - PolicyName: KMSKeyForSecret
        PolicyDocument:
          Version: 2012-10-17
          Statement:
            - Effect: Allow
              Action: kms:Decrypt
              Resource: !Ref SecretsManagerDomainCredentialsSecretsKMSKey
    - !Ref AWS::NoValue
```

## 3 Differing properties

### Sub with single argument

```yaml
  SomeProperty: !Sub blah-${ParamOrResourceReference}
```
### Sub with two arguments

Note also the nested `!GetAtt`

```yaml
  SomeProperty: !Sub
    - blah-${ParamOrResourceReference}-${LocalRef1}-${LocalRef2)
    - LocalRef1: value
      LocalRef2: !GetAtt
        - SomeResource
        - Property
```

## 4 Contextual Differences

Note the differing contexts for `Condition:`

```yaml
Conditions:
  Cond1: !Equals
    - 1
    - 1
  Cond2: !And
    - Condition: Cond1
    - !Equals
      - 1
      - 1

# --8<---
        PolicyDocument:
          Version: 2012-10-17
          Statement:
            - Effect: Allow
              Action: kms:Decrypt
              Resource: !Ref SecretsManagerDomainCredentialsSecretsKMSKey
              Condition:
                Bool:
                  aws:ViaAWSService: true
```

## 5 Intrinsic with intrinsic as argument

Here we get a potential for stack overflow. The tag `!Base64` is not a scalar value of `Value:`, but a tag applied to the `MappingStart` event that opens the object declaration with the property `Fn::Sub`

```yaml
    Properties:
      Type: String
      Value: !Base64
          Fn::Sub: '${Param1}'
```
