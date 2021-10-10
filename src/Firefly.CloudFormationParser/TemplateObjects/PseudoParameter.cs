namespace Firefly.CloudFormationParser.TemplateObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Pseudo parameters are parameters that are predefined by AWS CloudFormation. 
    /// </summary>
    /// <seealso cref="Firefly.CloudFormationParser.TemplateObjects.Parameter" />
    public class PseudoParameter : Parameter
    {
        /// <summary>
        /// Valid pseudo parameter names
        /// </summary>
        private static readonly string[] ValidNames =
            {
                "AWS::AccountId", 
                "AWS::NotificationARNs", 
                "AWS::NoValue", 
                "AWS::Partition",
                "AWS::Region",
                "AWS::StackId",
                "AWS::StackName",
                "AWS::URLSuffix"
            };

        /// <summary>
        /// Creates a pseudo parameter with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A new <see cref="PseudoParameter"/></returns>
        /// <exception cref="System.ArgumentException">Invalid pseudo parameter '<i>name</i>', - name</exception>
        public static PseudoParameter Create(string name)
        {
            if (!ValidNames.Contains(name))
            {
                throw new ArgumentException($"Invalid pseudo parameter '{name}',", nameof(name));
            }

            var param = new PseudoParameter { Name = name, Type = "String" };
            param.SetCurrentValue(new Dictionary<string, object> { { name, $"Unresolved {name}" } });
            
            return param;
        }
    }
}