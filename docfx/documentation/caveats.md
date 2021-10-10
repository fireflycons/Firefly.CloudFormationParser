# Caveats and Omissions

As in things this does _not_ yet do, but may later.

## Serverless Application Model (SAM)

SAM templates imply resources that will be created when the template is expanded. This package currently does not handle references to the implied resources as it does not currently perform an in-memory expansion of the template (and won't unless I suddenly get the urge to transliterate a few thousand lines of Python), so they are simply ignored when the [dependency graph](./dependencies.md) is constructed.

When [ITemplate.IsSAMTemplate](xref:Firefly.CloudFormationParser.ITemplate.IsSAMTemplate) is `true`, relying on the dependency graph is not a good idea as this only knows about objects declared in the template and not objects that will actually exist once the template is deployed. You can however retrieve a processed template from a stack deployed with the SAM template to get the concrete resources and build a dependency graph from that.

## Other Macro Types

The library will likely never support custom macro expansions as these rely on third party code that won't be directly accessible to this library, and will always throw exceptions on templates where additional attributes for the macro are present at the resource level (i.e. sibling attribute to Type, Properties). Again, better to retrieve a processed template from CloudFormation.





