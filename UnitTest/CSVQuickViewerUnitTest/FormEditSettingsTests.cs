using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormEditSettingsTests
  {
    [TestMethod]
    [Timeout(4000)]
    public void FormEditSettings()
    {
      UnitTestStaticForms.ShowForm(() =>
        new FormEditSettings(new ViewSettings(), new CsvFileDummy(), new List<string>(), (int?) null));
    }
  }
}