using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvDataWriterPgpFilesTest
  {
    

    [TestMethod]
    public async Task WritePGPAsync()
    {
      var fullPathPlain = Path.Combine(UnitTestStatic.ApplicationDirectory, "BasicCSV1.txt");
      var fullPath = fullPathPlain + ".pgp";
      FileSystemUtils.FileDelete(fullPathPlain);
      FileSystemUtils.FileDelete(fullPath);
      UnitTestInitialize.SetApplicationPGPSetting();

      var writer = new CsvFileWriter("id", fullPath, true, ValueFormat.Empty,
        codePageId: 65001,
        byteOrderMark: true,
        columnDefinition: null,
        identifierInContainer: null,
        header: null,
        footer: null,
        fileSettingDisplay: "",
        newLine: RecordDelimiterTypeEnum.Crlf,
        fieldDelimiterChar: ',',
        fieldQualifierChar: '\"',
        escapePrefixChar: char.MinValue,
        newLinePlaceholder: "",
        delimiterPlaceholder: "",
        qualifierPlaceholder: "",
        qualifyAlways: false,
        qualifyOnlyIfNeeded: true, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, sourceTimeZone: System.TimeZoneInfo.Local.Id,
        publicKey: PGPKeyTestHelper.cPublic, unencrypted: false);
      using var dt = UnitTestStaticData.GetDataTable();
      using var reader = new DataTableWrapper(dt);
      var res = await writer.WriteAsync(reader, UnitTestStatic.Token);
      Assert.AreEqual(dt.Rows.Count, Convert.ToInt32(res));
      Assert.IsTrue(FileSystemUtils.FileExists(fullPath));
      Assert.IsFalse(FileSystemUtils.FileExists(fullPathPlain));

      FileSystemUtils.FileDelete(fullPathPlain);
      FileSystemUtils.FileDelete(fullPath);
    }

    [TestMethod]
    public async Task WritePGPAsyncKeepUnencrypted()
    {
      var fullPathPlain = Path.Combine(UnitTestStatic.ApplicationDirectory, "BasicCSV2.txt");
      var fullPath = fullPathPlain + ".pgp";
      FileSystemUtils.FileDelete(fullPathPlain);
      FileSystemUtils.FileDelete(fullPath);
      UnitTestInitialize.SetApplicationPGPSetting();

      var writer = new CsvFileWriter("id", fullPath, true, ValueFormat.Empty,
        codePageId: 65001,
        byteOrderMark: true,
        columnDefinition: null,
        identifierInContainer: null,
        header: null,
        footer: null,
        fileSettingDisplay: "",
        newLine: RecordDelimiterTypeEnum.Crlf,
        fieldDelimiterChar: ',',
        fieldQualifierChar: '\"',
        escapePrefixChar: char.MinValue,
        newLinePlaceholder: "",
        delimiterPlaceholder: "",
        qualifierPlaceholder: "",
        qualifyAlways: false,
        qualifyOnlyIfNeeded: true,
        timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, sourceTimeZone: System.TimeZoneInfo.Local.Id,
        publicKey: PGPKeyTestHelper.cPublic, unencrypted: true);

      using var dt = UnitTestStaticData.GetDataTable();
      using var reader = new DataTableWrapper(dt);
      var res = await writer.WriteAsync(reader, UnitTestStatic.Token);
      Assert.AreEqual(dt.Rows.Count, Convert.ToInt32(res));

      Assert.IsTrue(FileSystemUtils.FileExists(fullPath));
      Assert.IsTrue(FileSystemUtils.FileExists(fullPathPlain));

      FileSystemUtils.FileDelete(fullPathPlain);
      FileSystemUtils.FileDelete(fullPath);
    }
  }
}