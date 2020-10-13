/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EchelonScriptCompiler.Utilities {
    public struct StringSpanSwitch<TValue>
        where TValue : Delegate {
        private struct Entry {
            #region ================== Instance fields

            public int    HashCode; // Lower 31 bits of hash code, -1 if unused
            public string Key;      // Key of entry
            public TValue Value;    // Value of entry

            #endregion

            #region ================== Constructors

            public Entry (string key, int hashCode, TValue value) {
                Key = key;
                HashCode = hashCode;
                Value = value;
            }

            #endregion
        }

        #region ================== Instance fields

        private StringComparison comparisonMode;

        private Entry [] entries;
        private TValue defaultCase;

        #endregion

        #region ================== Constructors

        public StringSpanSwitch (StringComparison compareMode, TValue defCase, IEnumerable<(string Key, TValue Value)> cases) {
            comparisonMode = compareMode;
            defaultCase = defCase;

            using var entriesList = new StructPooledList<Entry> ();
            foreach (var c in cases) {
                var hashCode = c.Key.GetHashCode (comparisonMode);

                foreach (var entry in entriesList) {
                    if (entry.HashCode == hashCode && entry.Key.Equals (c.Key, comparisonMode))
                        throw new ArgumentException ("Cannot have duplicate cases.", nameof (cases));
                }

                entriesList.Add (new Entry (c.Key, hashCode, c.Value));
            }

            entries = entriesList.ToArray ();
        }

        public StringSpanSwitch (StringComparison compareMode, IEnumerable<(string Key, TValue Value)> cases)
            : this (compareMode, null, cases) { }

        public StringSpanSwitch (TValue defCase, IEnumerable<(string Key, TValue Value)> cases)
            : this (StringComparison.Ordinal, defCase, cases) { }

        public StringSpanSwitch (IEnumerable<(string Key, TValue Value)> cases)
            : this (StringComparison.Ordinal, null, cases) { }

        #endregion

        #region ================== Instance methods

        public TValue Switch (ReadOnlySpan<char> value) {
            var hashCode = string.GetHashCode (value, comparisonMode);

            foreach (var entry in entries) {
                if (entry.HashCode == hashCode && entry.Key.Equals (entry.Key, comparisonMode))
                    return entry.Value;
            }

            return defaultCase;
        }

        #endregion
    }
}
