/*
 * Copyright 2002-2015 Drew Noakes
 *
 *    Modified by Yakov Danilov <yakodani@gmail.com> for Imazen LLC (Ported from Java to C#)
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this filePath except in compliance with the License.
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

using System.IO;
using Com.Drew.Lang;
using Com.Drew.Metadata.File;
using Com.Drew.Metadata.Photoshop;
using JetBrains.Annotations;

namespace Com.Drew.Imaging.Psd
{
    /// <summary>Obtains metadata from Photoshop's PSD files.</summary>
    /// <author>Drew Noakes https://drewnoakes.com</author>
    public static class PsdMetadataReader
    {
        /// <exception cref="System.IO.IOException"/>
        [NotNull]
        public static Metadata.Metadata ReadMetadata([NotNull] string filePath)
        {
            var metadata = new Metadata.Metadata();
            using (Stream stream = new FileStream(filePath, FileMode.Open))
                new PsdReader().Extract(new SequentialStreamReader(stream), metadata);
            new FileMetadataReader().Read(filePath, metadata);
            return metadata;
        }

        [NotNull]
        public static Metadata.Metadata ReadMetadata([NotNull] Stream stream)
        {
            var metadata = new Metadata.Metadata();
            new PsdReader().Extract(new SequentialStreamReader(stream), metadata);
            return metadata;
        }
    }
}
