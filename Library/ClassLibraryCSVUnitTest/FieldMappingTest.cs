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

namespace CsvTools.Tests
{
  [TestClass]
  public class FieldMappingTest
  {
    private readonly Mapping m_FieldMapping = new Mapping();

    [TestMethod]
    public void Equals()
    {
      var notEqual = new Mapping();
      notEqual.FileColumn = m_FieldMapping.FileColumn + "a";
      notEqual.TemplateField = m_FieldMapping.TemplateField;

      var equal = new Mapping();
      equal.FileColumn = m_FieldMapping.FileColumn;
      equal.TemplateField = m_FieldMapping.TemplateField;

      Assert.IsTrue(m_FieldMapping.Equals(equal));
      Assert.IsFalse(m_FieldMapping.Equals(notEqual));
    }

    [TestMethod]
    public void EqualsNull()
    {
      Assert.IsFalse(m_FieldMapping.Equals(null));
    }

    [TestInitialize]
    public void Init()
    {
      m_FieldMapping.FileColumn = "A";
      m_FieldMapping.TemplateField = "B";
    }
  }
}