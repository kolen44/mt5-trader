using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace mtapi.mt5
{
    /// <summary>
    /// Quotes and trades sessions of the symbol
    /// </summary>
    public class SymbolSessions : IEqualityComparer<SymbolSessions>, IEqualityComparer
    {
        public List<Session>[] Quotes;
        public List<Session>[] Trades;


        public static bool AreEqual(SymbolSessions ss1, SymbolSessions ss2)
        {
            // Check if the Quotes arrays are structurally identical
            if (!AreSessionListsEqual(ss1.Quotes, ss2.Quotes))
            {
                return false;
            }

            // Check if the Trades arrays are structurally identical
            if (!AreSessionListsEqual(ss1.Trades, ss2.Trades))
            {
                return false;
            }

            return true;
        }

        private static bool AreSessionListsEqual(List<Session>[] lists1, List<Session>[] lists2)
        {
            // Check if both arrays are null or have the same length
            if (lists1 == null || lists2 == null)
            {
                return lists1 == lists2; // Both should be null to be considered equal
            }

            if (lists1.Length != lists2.Length)
            {
                return false;
            }

            // Check each list in the arrays
            for (int i = 0; i < lists1.Length; i++)
            {
                var list1 = lists1[i];
                var list2 = lists2[i];

                if (!AreSessionListsEqual(list1, list2))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreSessionListsEqual(List<Session> list1, List<Session> list2)
        {
            // Check if both lists are null or have the same count
            if (list1 == null || list2 == null)
            {
                return list1 == list2; // Both should be null to be considered equal
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            // Check each Session object in the lists
            for (int i = 0; i < list1.Count; i++)
            {
                if (!AreSessionsEqual(list1[i], list2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreSessionsEqual(Session s1, Session s2)
        {
            // Compare the public fields of the Session objects
            return s1.StartTime == s2.StartTime &&
                   s1.EndTime == s2.EndTime;
            // Extend equality checks here depending on additional fields.
        }

        public new bool Equals(object x, object y)
        {
            return AreEqual((SymbolSessions)x, (SymbolSessions)y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(SymbolSessions x, SymbolSessions y)
        {
            return AreEqual((SymbolSessions)x, (SymbolSessions)y);
        }

        public int GetHashCode(SymbolSessions obj)
        {
            return obj.GetHashCode();
        }
    }
}
