namespace Firefly.CloudFormationParser.Utils
{
    using System.Collections.Generic;

    /// <summary>
    /// Helper class for regions and availability zones
    /// </summary>
    internal static class RegionInfo
    {
        /// <summary>
        /// Dictionary of available regions and number of AZs in each
        /// </summary>
        private static readonly IDictionary<string, int> Regions = new Dictionary<string, int>
                                                                       {
                                                                           { "ap-east-1", 3 },
                                                                           { "ap-northeast-1", 3 },
                                                                           { "ap-northeast-2", 4 },
                                                                           { "ap-northeast-3", 3 },
                                                                           { "ap-south-1", 3 },
                                                                           { "ap-southeast-1", 3 },
                                                                           { "ap-southeast-2", 3 },
                                                                           { "ca-central-1", 3 },
                                                                           { "eu-central-1", 3 },
                                                                           { "eu-north-1", 3 },
                                                                           { "eu-west-1", 3 },
                                                                           { "eu-west-2", 3 },
                                                                           { "eu-west-3", 3 },
                                                                           { "sa-east-1", 3 },
                                                                           { "us-east-1", 6 },
                                                                           { "us-east-2", 3 },
                                                                           { "us-west-1", 2 },
                                                                           { "us-west-2", 4 }
                                                                       };

        /// <summary>
        /// Gets a list of availability zones for given region
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns>List of availability zones</returns>
        public static IList<string> GetAZs(string region)
        {
            var numZones = Regions[region];
            var azs = new List<string>();

            for (var i = 0; i < numZones; ++i)
            {
                azs.Add($"{region}{(char)('a' + i)}");
            }

            return azs;
        }
    }
}