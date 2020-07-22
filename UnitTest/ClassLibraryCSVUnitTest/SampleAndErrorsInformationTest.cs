/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace CsvTools.Tests
{
  [TestClass]
  public class SampleAndErrorsInformationTest
  {
    //private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void Defaults()
    {
      var test = new SampleAndErrorsInformation();
      Assert.AreEqual(-1, test.NumErrors);
      Assert.AreEqual(0, test.Errors.Count);
      Assert.AreEqual(0, test.Samples.Count);
    }

    [TestMethod]
    public void AddError()
    {
      var test = new SampleAndErrorsInformation();
      test.Errors.Add(new SampleRecordEntry(10, "ErrorText"));
      Assert.AreEqual(1, test.NumErrors);
      Assert.AreEqual(0, test.Samples.Count);
      test.Errors = new ObservableCollection<SampleRecordEntry>
      {
        new SampleRecordEntry(1, true), new SampleRecordEntry(2, true), new SampleRecordEntry(3, true)
      };
      Assert.AreEqual(3, test.NumErrors);
      Assert.AreEqual(0, test.Samples.Count);
    }

    [TestMethod]
    public void AddSamples()
    {
      var test = new SampleAndErrorsInformation();
      test.Samples.Add(new SampleRecordEntry(20, true));
      Assert.AreEqual(1, test.Samples.Count);
      test.Samples.Add(new SampleRecordEntry(20, false));
      Assert.AreEqual(2, test.Samples.Count);
      test.Samples = new ObservableCollection<SampleRecordEntry>
      {
        new SampleRecordEntry(1, true), new SampleRecordEntry(2, true), new SampleRecordEntry(3, true)
      };
      Assert.AreEqual(3, test.Samples.Count);
    }

    [TestMethod]
    public void Clone()
    {
      var test = new SampleAndErrorsInformation();
      test.Errors.Add(new SampleRecordEntry(10, "ErrorText"));

      test.Samples.Add(new SampleRecordEntry(20, true));
      test.Samples.Add(new SampleRecordEntry(20, false));

      var test2 = test.Clone();
      Assert.IsTrue(test.Equals(test2));
      Assert.AreEqual(test.Errors[0], test2.Errors[0]);
      Assert.AreEqual(test.Samples[0], test2.Samples[0]);
    }

    [TestMethod]
    public void Equals()
    {
      var test = new SampleAndErrorsInformation();
      test.Errors.Add(new SampleRecordEntry(10, "ErrorText"));

      test.Samples.Add(new SampleRecordEntry(20, true));
      test.Samples.Add(new SampleRecordEntry(20, false));

      var test2 = test.Clone();
      Assert.IsTrue(test.Equals(test));
      Assert.IsTrue(test.Equals(test2));
      Assert.IsFalse(test.Equals(null));
    }
  }
}