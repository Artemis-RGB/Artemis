// I don't want all of MoreLINQ (https://github.com/morelinq/MoreLINQ) so we'll borrow just this

#region License and Terms

// MoreLINQ - Extensions to LINQ to Objects
// Copyright (c) 2008 Jonathan Skeet. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    /// <summary>
    ///     A static class providing <see cref="IEnumerable{T}" /> extensions
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        /// <summary>
        ///     Returns the index of the provided element inside the read only collection
        /// </summary>
        /// <typeparam name="T">The type of element to find</typeparam>
        /// <param name="self">The collection to search in</param>
        /// <param name="elementToFind">The element to find</param>
        /// <returns>If found, the index of the element to find; otherwise -1</returns>
        public static int IndexOf<T>(this IReadOnlyCollection<T> self, T elementToFind)
        {
            int i = 0;
            foreach (T element in self)
            {
                if (Equals(element, elementToFind))
                    return i;
                i++;
            }

            return -1;
        }
    }
}