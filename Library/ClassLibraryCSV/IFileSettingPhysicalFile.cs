
using System;

namespace CsvTools
{
  public interface IFileSettingPhysicalFile : IFileSetting
  {
    /// <summary>
    ///  Gets or sets the name of the file, this value could be a relative path
    /// </summary>
    /// <value>The name of the file.</value>
    string FileName { get; set; }

    /// <summary>
    ///  The Size of the file in Byte
    /// </summary>
    long FileSize { get; set; }

    /// <summary>
    ///  Gets the full path of the Filename
    /// </summary>
    /// <value>
    ///  The full path for <see cref="FileName" />
    /// </value>
    string FullPath { get; }
  }
}
