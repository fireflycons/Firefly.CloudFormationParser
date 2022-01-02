# v0.3.4 (not released)

* Fix - Issue with !Join evaluation when an argument is a ref to a list.

# v0.3.3

* Fix - Another accidental commenting out of a `[YamlIgnore]` attribute in the parameter class generated a back-reference to the entire template. YamlDotNet then renders this as an alias.

# v0.3.2

* Fix - An accidental commenting out of a `[YamlIgnore]` attribute in the parameter class generated a back-reference to the entire template. YamlDotNet then renders this as an alias. [Issue link](https://github.com/fireflycons/Firefly.CloudFormationParser/issues/4)

# v0.3.1

* Enhancement - Add an "ExtraData" property to intrinsics. This allows client applications to attach app specific payloads to intrinsics.

# v0.3.0

* Enhancement - Breaking Change. Complete rewrite of visitor implementation.

# v0.2.0

* Fix - Ensure cloudformation parameter cannot have value `null`.
* Enhancement - Add visiting for resource properties and mappings (needed by PSCloudFormation).
* Enhancement - Add `IBranchableIntrinsic` interface to intrinsics that return one value from a selection (`!If`, `!Select`).
* Enhancement - Make all intrinsic objects immutable.

# v0.1.1

* Fix - Fix incomplete Parameter.SetValue implementation so it validates AWS-specific parameter types. [Issue link](https://github.com/fireflycons/Firefly.CloudFormationParser/issues/3)

# v0.1.0

* Initial Release!
