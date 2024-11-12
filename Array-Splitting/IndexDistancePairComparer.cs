using System;
using System.Collections.Generic;

namespace Array_Splitting
{
    public class IndexDistancePairComparer : IComparer<IndexDistancePair>
    {
        public int Compare(IndexDistancePair x, IndexDistancePair y)
        {
            // Sort primarily by distance
            int distanceComparison = x.Distance.CompareTo(y.Distance);

            // If distances are equal, sort by index to ensure uniqueness
            return distanceComparison == 0 ? x.Index.CompareTo(y.Index) : distanceComparison;
        }
    }
}
