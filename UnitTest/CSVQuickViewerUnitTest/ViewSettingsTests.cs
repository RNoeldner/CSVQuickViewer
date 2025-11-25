/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Collections.Generic;

namespace CsvTools.Tests;

[TestClass]
public class ViewSettingsTests
{

  [TestMethod]
  public void PropertiesTest()
  {
    var test1 = new ViewSettings();

    Assert.IsFalse(test1.StoreSettingsByFile);
    test1.StoreSettingsByFile = true;
    Assert.IsTrue(test1.StoreSettingsByFile);

    var test1FillGuessSettings = new FillGuessSettings(true);
    test1.FillGuessSettings = test1FillGuessSettings;

    Assert.IsTrue(test1FillGuessSettings.Equals(test1.FillGuessSettings));
  }

  [TestMethod]
  public void SerializeCheckPropsFillGuessSettingsTest()
  {
    var test1 = new FillGuessSettings(true);
    UnitTestStatic.RunSerializeAllProps(test1);
  }

  [TestMethod]
  public void SerializeViewSettingsTest()
  {
    var test1 = new ViewSettings { AllowJson = false, HtmlStyle = new HtmlStyle("Dummy") };
    var output = UnitTestStatic.RunSerialize(test1);
    Assert.AreEqual(test1.AllowJson, output.AllowJson);
    Assert.AreEqual(test1.HtmlStyle.Style, output.HtmlStyle.Style);
  }

  [TestMethod]
  public void SerializeCheckViewSettingsTest()
  {
    var test1 = new ViewSettings { AllowJson = false, HtmlStyle = new HtmlStyle("Dummy") };
    // JsonIgnore
    var ignore = new List<string>() { 
      nameof(ViewSettings.InitialFolder), 
      nameof(ViewSettings.DefaultInspectionResult), 
      nameof(ViewSettings.WriteSetting), 
      nameof(ViewSettings.DurationTimeSpan) };

    UnitTestStatic.RunSerializeAllProps(test1, ignore);
  }

}