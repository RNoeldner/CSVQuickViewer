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
using System;

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
      test.Errors.Add(new SampleRecordEntry(10, error: "ErrorText"));
      Assert.AreEqual(1, test.NumErrors);
      Assert.AreEqual(0, test.Samples.Count);
      test.Errors.AddRangeNoClone(new[] { new SampleRecordEntry(1), new SampleRecordEntry(2), new SampleRecordEntry(3) });
      Assert.AreEqual(4, test.NumErrors);
      Assert.AreEqual(0, test.Samples.Count);
    }

    [TestMethod]
    public void AddSamples()
    {
      var test = new SampleAndErrorsInformation();
      test.Samples.Add(new SampleRecordEntry(1));
      Assert.AreEqual(1, test.Samples.Count);
      test.Samples.Add(new SampleRecordEntry(20, false));
      Assert.AreEqual(2, test.Samples.Count);
      test.Samples.AddRangeNoClone(new[] { new SampleRecordEntry(1), new SampleRecordEntry(2), new SampleRecordEntry(3) });
      Assert.AreEqual(4, test.Samples.Count);
    }

    [TestMethod]
    public void Clone()
    {
      var test = new SampleAndErrorsInformation();
      test.Errors.Add(new SampleRecordEntry(10, error: "ErrorText"));

      test.Samples.Add(new SampleRecordEntry(20));
      test.Samples.Add(new SampleRecordEntry(20, false));

      var test2 = (SampleAndErrorsInformation) test.Clone();
      Assert.IsTrue(test.Equals(test2), "Equals");
      Assert.AreEqual(test.Errors[0], test2.Errors[0]);
      Assert.AreEqual(test.Samples[0], test2.Samples[0]);
    }

    [TestMethod]
    public void CopyTo()
    {
      var test = new SampleAndErrorsInformation(5, new[] { new SampleRecordEntry(10, error: "ErrorText") },
        new[] { new SampleRecordEntry(20), new SampleRecordEntry(21, false) });

      var test2 = new SampleAndErrorsInformation(2, Array.Empty<SampleRecordEntry>(),
        new[] { new SampleRecordEntry(100) });
      test.CopyTo(test2);
      Assert.IsTrue(test.Equals(test2), "Equals");
      Assert.AreEqual(test.Errors[0], test2.Errors[0]);
      Assert.AreEqual(test.Samples[0], test2.Samples[0]);
    }

    [TestMethod]
    public void Equals()
    {
      var test = new SampleAndErrorsInformation();
      test.Errors.Add(new SampleRecordEntry(10, error: "ErrorText"));

      test.Samples.Add(new SampleRecordEntry(20));
      test.Samples.Add(new SampleRecordEntry(20, false));

      var test2 = (SampleAndErrorsInformation) test.Clone();
      Assert.IsTrue(test.Equals(test));
      Assert.IsTrue(test.Equals(test2));
      Assert.IsFalse(test.Equals(null));
    }
  }
}