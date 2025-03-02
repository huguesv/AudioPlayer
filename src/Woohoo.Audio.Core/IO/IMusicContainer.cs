// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.IO;

using System.Collections.Generic;
using System.IO;

public interface IMusicContainer
{
    string ContainerPath { get; }

    IEnumerable<string> EnumerateFilesByExtension(string extension);

    bool FileExists(string fileName);

    long GetFileSize(string fileName);

    string ReadFileText(string fileName);

    byte[] ReadFileBytes(string fileName);

    Stream OpenFileStream(string fileName);
}
