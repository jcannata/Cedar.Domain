namespace Cedar.Domain.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KellermanSoftware.CompareNetObjects;

    internal class MessageEqualityComparer : IEqualityComparer<object>
    {
        public static readonly MessageEqualityComparer Instance = new MessageEqualityComparer();
        private readonly CompareLogic _compareLogic;

        public MessageEqualityComparer()
        {
            _compareLogic = new CompareLogic {Config = {TreatStringEmptyAndNullTheSame = true, MaxDifferences = 50}};
        }

        new public bool Equals(object x, object y)
        {
            var result = _compareLogic.Compare(x, y);

            if(result.ExceededDifferences)
            {
                var type = x == null ? null : x.GetType();
                Console.WriteLine("Warning while comparing objects of type {1} exceeded maximum number of {0}", _compareLogic.Config.MaxDifferences, type);
            }

            if(!result.AreEqual)
            {

                var type = x == null ? null:x.GetType();
                Console.WriteLine("Found differences while comparing objects of type: \n\t\t - {0} " + string.Join("\n\t\t - ", result.Differences.Select(dif => dif.ToString())), type);
            }

            return result.AreEqual;
        }

        public int GetHashCode(object obj)
        {
            return 0;
        }
    }
}