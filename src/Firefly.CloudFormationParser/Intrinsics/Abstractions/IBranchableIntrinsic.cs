namespace Firefly.CloudFormationParser.Intrinsics.Abstractions
{
    /// <summary>
    /// Where an intrinsic can return one path from a selection depending on conditions, gets the selected branch
    /// </summary>
    public interface IBranchableIntrinsic
    {
        /// <summary>
        /// Gets the branch.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>Selected item based on template or intrinsic conditions</returns>
        object GetBranch(ITemplate? template);
    }
}