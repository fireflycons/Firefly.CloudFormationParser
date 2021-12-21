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
