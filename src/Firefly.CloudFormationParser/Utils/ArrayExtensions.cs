namespace Firefly.CloudFormationParser.Utils
{
    using System;

    /// <summary>
    /// Extension methods for <see cref="Array"/> objects
    /// </summary>
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Perform an action on all members of an array
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="action">The action.</param>
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0)
            {
                return;
            }

            ArrayTraverse walker = new ArrayTraverse(array);
            
            do
            {
                action(array, walker.Position);
            }
            while (walker.Step());
        }

        private class ArrayTraverse
        {
            private readonly int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                this.maxLengths = new int[array.Rank];

                for (var i = 0; i < array.Rank; ++i)
                {
                    this.maxLengths[i] = array.GetLength(i) - 1;
                }

                this.Position = new int[array.Rank];
            }

            public int[] Position { get; }

            public bool Step()
            {
                for (var i = 0; i < this.Position.Length; ++i)
                {
                    if (this.Position[i] < this.maxLengths[i])
                    {
                        this.Position[i]++;
                        for (var j = 0; j < i; j++)
                        {
                            this.Position[j] = 0;
                        }

                        return true;
                    }
                }

                return false;
            }
        }
    }
}