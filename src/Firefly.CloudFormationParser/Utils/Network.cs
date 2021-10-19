namespace Firefly.CloudFormationParser.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an entire network with subnet divisions.
    /// Used in <c>Fn::Cidr</c> calculations.
    /// </summary>
    internal class Network
    {
        /// <summary>
        /// The overall network
        /// </summary>
        private readonly IPRange network;

        /// <summary>
        /// The subnets
        /// </summary>
        private readonly List<IPRange> subnets = new List<IPRange>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Network"/> class.
        /// </summary>
        /// <param name="cidr">CIDR for this network.</param>
        public Network(string cidr)
        {
            this.network = new IPRange(cidr);
        }

        /// <summary>
        /// Gets a collection of subnets from this network.
        /// </summary>
        /// <param name="numberOfSubnets">The number of CIDRs to generate.</param>
        /// <param name="cidrBits">The number of subnet bits for the CIDR. For example, specifying a value "8" for this parameter will create a CIDR with a mask of "/24".</param>
        /// <returns>A list of CIDR address blocks.</returns>
        public List<string> GetSubnets(int numberOfSubnets, int cidrBits)
        {
            var requestedSize = 32 - (uint)cidrBits;
            var result = new List<string>();

            for (var i = 0; i < numberOfSubnets; ++i)
            {
                result.Add(this.NextSubnet(requestedSize));
            }

            return result;
        }

        /// <summary>
        /// Gets the next available subnet with the given prefix length in this network.
        /// </summary>
        /// <param name="requestedPrefix">The requested prefix.</param>
        /// <returns>CIDR address of the discovered subnet.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Can't fit a /{requestedPrefix} subnet in a /{this.network.Prefix} network.
        /// or
        /// Not enough space for a /{requestedPrefix} in {this.network}
        /// </exception>
        private string NextSubnet(uint requestedPrefix)
        {
            if (requestedPrefix < this.network.Prefix)
            {
                throw new InvalidOperationException(
                    $"Can't fit a /{requestedPrefix} subnet in a /{this.network.Prefix} network.");
            }

            for (var @base = this.network.Base;
                 @base < this.network.Top;
                 @base += (uint)Math.Pow(2, 32 - requestedPrefix))
            {
                var attempt = new IPRange(@base, requestedPrefix);

                if (attempt.Top > this.network.Top)
                {
                    break;
                }

                if (!this.subnets.Any(s => attempt.Overlaps(s)))
                {
                    this.subnets.Add(attempt);
                    return attempt.ToCidr();
                }
            }

            throw new InvalidOperationException($"Not enough space for a /{requestedPrefix} in {this.network}");
        }

        /// <summary>
        /// Represents a range of IP addresses in a network, e.g as specified by a CIDR.
        /// Used in <c>Fn::Cidr</c> calculations.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private class IPRange
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="IPRange"/> class.
            /// </summary>
            /// <param name="cidr">The CIDR of the network.</param>
            public IPRange(string cidr)
            {
                var parts = cidr.Split('/');

                this.Base = IPToNum(parts[0]);
                this.Prefix = uint.Parse(parts[1]);
                this.Top = (uint)(this.Base + Math.Pow(2, 32 - this.Prefix));
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="IPRange"/> class.
            /// </summary>
            /// <param name="base">The base address of the network.</param>
            /// <param name="prefix">The network prefix.</param>
            public IPRange(uint @base, uint prefix)
            {
                this.Base = @base;
                this.Prefix = prefix;
                this.Top = @base + (uint)Math.Pow(2, 32 - prefix);
            }

            /// <summary>
            /// Gets the base address of the network as an integer.
            /// </summary>
            /// <value>
            /// The base.
            /// </value>
            public uint Base { get; }

            /// <summary>
            /// Gets the network prefix.
            /// </summary>
            /// <value>
            /// The prefix.
            /// </value>
            public uint Prefix { get; }

            /// <summary>
            /// Gets the top address of the network.
            /// </summary>
            /// <value>
            /// The top.
            /// </value>
            public uint Top { get; }

            /// <summary>
            /// Determines whether this range overlaps another
            /// </summary>
            /// <param name="other">The other.</param>
            /// <returns><c>true</c> if ranges overlap; else <c>false</c></returns>
            public bool Overlaps(IPRange other)
            {
                return (this.Top > other.Base && this.Top < other.Top)
                       || (this.Base >= other.Base && this.Base < other.Top)
                       || (other.Top > this.Base && other.Top < this.Top)
                       || (other.Base >= this.Base && other.Base < this.Top);
            }

            /// <summary>
            /// Converts this range to a CIDR
            /// </summary>
            /// <returns>CIDR notation.</returns>
            public string ToCidr()
            {
                return $"{NumToIP(this.Base)}/{this.Prefix}";
            }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return this.ToCidr();
            }

            /// <summary>
            /// Converts dotted quad IP address to integer.
            /// </summary>
            /// <param name="ip">The IP address.</param>
            /// <returns>Integer IP address</returns>
            // ReSharper disable once StyleCop.SA1616
            // ReSharper disable once InconsistentNaming
            private static uint IPToNum(string ip)
            {
                return ip.Split('.').Select(uint.Parse).Aggregate((a, b) => (a * 256) + b);
            }

            /// <summary>
            /// Converts integer IP address to dotted quad.
            /// </summary>
            /// <param name="num">The integer address.</param>
            /// <returns>Dotted quad IP address</returns>
            // ReSharper disable once StyleCop.SA1616
            // ReSharper disable once InconsistentNaming
            private static string NumToIP(uint num)
            {
                return string.Join(
                    ".",
                    new[]
                        {
                            (num & 0xFF000000) / 0x1000000, 
                            (num & 0x00FF0000) / 0x10000, 
                            (num & 0x0000FF00) / 0x100,
                            num & 0x000000FF
                        }
                        .Select(u => u.ToString()));
            }
        }
    }
}