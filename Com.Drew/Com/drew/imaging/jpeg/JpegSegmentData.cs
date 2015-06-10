/*
 * Copyright 2002-2015 Drew Noakes
 *
 *    Modified by Yakov Danilov <yakodani@gmail.com> for Imazen LLC (Ported from Java to C#)
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * More information about this project is available at:
 *
 *    https://drewnoakes.com/code/exif/
 *    https://github.com/drewnoakes/metadata-extractor
 */

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sharpen;

namespace Com.Drew.Imaging.Jpeg
{
    /// <summary>Holds a collection of JPEG data segments.</summary>
    /// <remarks>
    /// Holds a collection of JPEG data segments.  This need not necessarily be all segments
    /// within the JPEG. For example, it may be convenient to store only the non-image
    /// segments when analysing metadata.
    /// <para />
    /// Segments are keyed via their <see cref="JpegSegmentType"/>. Where multiple segments use the
    /// same segment type, they will all be stored and available.
    /// <para />
    /// Each segment type may contain multiple entries. Conceptually the model is:
    /// <c>Map&lt;JpegSegmentType, Collection&lt;byte[]&gt;&gt;</c>. This class provides
    /// convenience methods around that structure.
    /// </remarks>
    /// <author>Drew Noakes https://drewnoakes.com</author>
    public sealed class JpegSegmentData
    {
        [NotNull]
        private readonly Dictionary<byte, IList<byte[]>> _segmentDataMap = new Dictionary<byte, IList<byte[]>>(10);

        // TODO key this on JpegSegmentType rather than Byte, and hopefully lose much of the use of 'byte' with this class
        /// <summary>Adds segment bytes to the collection.</summary>
        /// <param name="segmentType">the type of the segment being added</param>
        /// <param name="segmentBytes">the byte array holding data for the segment being added</param>
        public void AddSegment(byte segmentType, [NotNull] byte[] segmentBytes)
        {
            GetOrCreateSegmentList(segmentType).Add(segmentBytes);
        }

        /// <summary>Gets the set of JPEG segment type identifiers.</summary>
        public IEnumerable<JpegSegmentType> GetSegmentTypes()
        {
            ICollection<JpegSegmentType> segmentTypes = new HashSet<JpegSegmentType>();
            foreach (var segmentTypeByte in _segmentDataMap.Keys)
            {
                var segmentType = JpegSegmentType.FromByte(segmentTypeByte);
                if (segmentType == null)
                {
                    throw new InvalidOperationException(string.Format("Should not have a segmentTypeByte that is not in the enum: 0x{0:X}", segmentTypeByte));
                }
                segmentTypes.Add(segmentType);
            }
            return segmentTypes;
        }

        /// <summary>Gets the first JPEG segment data for the specified type.</summary>
        /// <param name="segmentType">the JpegSegmentType for the desired segment</param>
        /// <returns>a byte[] containing segment data or null if no data exists for that segment</returns>
        [CanBeNull]
        public byte[] GetSegment([NotNull] JpegSegmentType segmentType)
        {
            return GetSegment(segmentType.ByteValue);
        }

        /// <summary>Gets segment data for a specific occurrence and type.</summary>
        /// <remarks>
        /// Gets segment data for a specific occurrence and type.  Use this method when more than one occurrence
        /// of segment data for a given type exists.
        /// </remarks>
        /// <param name="segmentType">identifies the required segment</param>
        /// <param name="occurrence">the zero-based index of the occurrence</param>
        /// <returns>the segment data as a byte[], or null if no segment exists for the type &amp; occurrence</returns>
        [CanBeNull]
        public byte[] GetSegment([NotNull] JpegSegmentType segmentType, int occurrence)
        {
            return GetSegment(segmentType.ByteValue, occurrence);
        }

        /// <summary>Gets segment data for a specific occurrence and type.</summary>
        /// <remarks>
        /// Gets segment data for a specific occurrence and type.  Use this method when more than one occurrence
        /// of segment data for a given type exists.
        /// </remarks>
        /// <param name="segmentType">identifies the required segment</param>
        /// <param name="occurrence">the zero-based index of the occurrence</param>
        /// <returns>the segment data as a byte[], or null if no segment exists for the type &amp; occurrence</returns>
        [CanBeNull]
        public byte[] GetSegment(byte segmentType, int occurrence = 0)
        {
            var segmentList = GetSegmentList(segmentType);
            return segmentList != null && segmentList.Count > occurrence ? segmentList[occurrence] : null;
        }

        /// <summary>Returns all instances of a given JPEG segment.</summary>
        /// <remarks>Returns all instances of a given JPEG segment.  If no instances exist, an empty sequence is returned.</remarks>
        /// <param name="segmentType">a number which identifies the type of JPEG segment being queried</param>
        /// <returns>zero or more byte arrays, each holding the data of a JPEG segment</returns>
        [NotNull]
        public IEnumerable<byte[]> GetSegments([NotNull] JpegSegmentType segmentType)
        {
            return GetSegments(segmentType.ByteValue);
        }

        /// <summary>Returns all instances of a given JPEG segment.</summary>
        /// <remarks>Returns all instances of a given JPEG segment.  If no instances exist, an empty sequence is returned.</remarks>
        /// <param name="segmentType">a number which identifies the type of JPEG segment being queried</param>
        /// <returns>zero or more byte arrays, each holding the data of a JPEG segment</returns>
        [NotNull]
        public IEnumerable<byte[]> GetSegments(byte segmentType)
        {
            return GetSegmentList(segmentType) ?? Enumerable.Empty<byte[]>();
        }

        [CanBeNull]
        private IList<byte[]> GetSegmentList(byte segmentType)
        {
            IList<byte[]> list;
            return _segmentDataMap.TryGetValue(segmentType, out list) ? list : null;
        }

        [NotNull]
        private IList<byte[]> GetOrCreateSegmentList(byte segmentType)
        {
            IList<byte[]> segmentList;
            if (!_segmentDataMap.TryGetValue(segmentType, out segmentList))
            {
                segmentList = new List<byte[]>();
                _segmentDataMap[segmentType] = segmentList;
            }
            return segmentList;
        }

        /// <summary>Returns the count of segment data byte arrays stored for a given segment type.</summary>
        /// <param name="segmentType">identifies the required segment</param>
        /// <returns>the segment count (zero if no segments exist).</returns>
        public int GetSegmentCount([NotNull] JpegSegmentType segmentType)
        {
            return GetSegmentCount(segmentType.ByteValue);
        }

        /// <summary>Returns the count of segment data byte arrays stored for a given segment type.</summary>
        /// <param name="segmentType">identifies the required segment</param>
        /// <returns>the segment count (zero if no segments exist).</returns>
        public int GetSegmentCount(byte segmentType)
        {
            var segmentList = GetSegmentList(segmentType);
            return segmentList == null ? 0 : segmentList.Count;
        }

        /// <summary>Removes a specified instance of a segment's data from the collection.</summary>
        /// <remarks>
        /// Removes a specified instance of a segment's data from the collection.  Use this method when more than one
        /// occurrence of segment data exists for a given type exists.
        /// </remarks>
        /// <param name="segmentType">identifies the required segment</param>
        /// <param name="occurrence">the zero-based index of the segment occurrence to remove.</param>
        public void RemoveSegmentOccurrence([NotNull] JpegSegmentType segmentType, int occurrence)
        {
            RemoveSegmentOccurrence(segmentType.ByteValue, occurrence);
        }

        /// <summary>Removes a specified instance of a segment's data from the collection.</summary>
        /// <remarks>
        /// Removes a specified instance of a segment's data from the collection.  Use this method when more than one
        /// occurrence of segment data exists for a given type exists.
        /// </remarks>
        /// <param name="segmentType">identifies the required segment</param>
        /// <param name="occurrence">the zero-based index of the segment occurrence to remove.</param>
        public void RemoveSegmentOccurrence(byte segmentType, int occurrence)
        {
            IList<byte[]> segmentList;
            if (_segmentDataMap.TryGetValue(segmentType, out segmentList))
            {
                segmentList.RemoveAt(occurrence);
            }
        }

        /// <summary>Removes all segments from the collection having the specified type.</summary>
        /// <param name="segmentType">identifies the required segment</param>
        public void RemoveSegment([NotNull] JpegSegmentType segmentType)
        {
            RemoveSegment(segmentType.ByteValue);
        }

        /// <summary>Removes all segments from the collection having the specified type.</summary>
        /// <param name="segmentType">identifies the required segment</param>
        public void RemoveSegment(byte segmentType)
        {
            Collections.Remove(_segmentDataMap, segmentType);
        }

        /// <summary>Determines whether data is present for a given segment type.</summary>
        /// <param name="segmentType">identifies the required segment</param>
        /// <returns>true if data exists, otherwise false</returns>
        public bool ContainsSegment([NotNull] JpegSegmentType segmentType)
        {
            return ContainsSegment(segmentType.ByteValue);
        }

        /// <summary>Determines whether data is present for a given segment type.</summary>
        /// <param name="segmentType">identifies the required segment</param>
        /// <returns>true if data exists, otherwise false</returns>
        public bool ContainsSegment(byte segmentType)
        {
            return _segmentDataMap.ContainsKey(segmentType);
        }
    }
}
