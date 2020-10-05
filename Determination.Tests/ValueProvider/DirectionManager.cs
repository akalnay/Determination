////////////////////////////////////////////////////////
// Copyright (c) Alejandro Kalnay                     //
// License: GNU GPLv3                                 //
////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace Determination.Tests
{
    public enum Direction { Up = 1, Right, Down, Left }

    /// <summary>
    /// This class is just for demonstration purposes.  The <see cref="GetNextRandomDirection"/> method
    /// returns a random <see cref="Direction"/> value when invoked.
    /// </summary>
    public sealed class DirectionManager
    {
        private static readonly Random _RANDOM = new Random();

        /// <summary>
        /// Returns a random <see cref="Direction"/>
        /// </summary>
        /// <returns>A random <see cref="Direction"/></returns>
        public static Direction GetNextRandomDirection() => (Direction)_RANDOM.Next((int)Min<Direction>(), Count<Direction>() + 1);

        private static T Min<T>() where T : struct, Enum => Enum.GetValues(typeof(T)).Cast<T>().Min();

        private static int Count<T>() where T : struct, Enum => Enum.GetNames(typeof(T)).Length;
    }
}
