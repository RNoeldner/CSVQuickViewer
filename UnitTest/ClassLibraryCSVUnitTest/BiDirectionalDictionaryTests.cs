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
using System.Collections.Generic;

namespace CsvTools.Tests
{
  [TestClass]
  public class BiDirectionalDictionaryTests
  {
    [TestMethod]
    public void BiDirectionalDictionaryCount()

    {
      var bi = new BiDirectionalDictionary<int, int>();
      Assert.AreEqual(0, bi.Count);
      bi.Add(1, 1);
      Assert.AreEqual(1, bi.Count);
      bi.Add(2, 2);
      Assert.AreEqual(2, bi.Count);
      bi.Clear();
      Assert.AreEqual(0, bi.Count);
    }

    [TestMethod]
    public void BiDirectionalDictionaryInitSmaller()

    {
      var bi = new BiDirectionalDictionary<int, int>(1);
      Assert.AreEqual(0, bi.Count);
      bi.Add(1, 1);
      Assert.AreEqual(1, bi.Count);
      bi.Add(2, 2);
      Assert.AreEqual(2, bi.Count);
    }

    [TestMethod]
    public void BiDirectionalDictionaryCount2()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      Assert.AreEqual(2, bi.Count);
    }

    [TestMethod]
    public void BiDirectionalDictionaryCtorDirectory()

    {
      var init = new Dictionary<int, int> { { 1, 1 }, { 2, 2 } };
      var bi = new BiDirectionalDictionary<int, int>(init);
      Assert.AreEqual(2, bi.Count);
    }

    [TestMethod]
    public void BiDirectionalDictionaryXml()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      //TODO: ReadXml and WriteXml Test

      
    }

    [TestMethod]
    public void BiDirectionalDictionaryRemove()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      Assert.AreEqual(2, bi.Count);
      bi.Remove(1);  
      Assert.AreEqual(1, bi.Count);
    }

    [TestMethod]
    public void BiDirectionalDictionaryTryAdd()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      Assert.AreEqual(2, bi.Count);
      Assert.IsTrue(bi.TryAdd(3, 3));
      Assert.AreEqual(3, bi.Count);
      Assert.IsFalse(bi.TryAdd(1, 2));
      Assert.AreEqual(3, bi.Count);
    }

    [TestMethod]
    public void BiDirectionalDictionaryTryGetByValue()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 12 } };
      Assert.AreEqual(2, bi.Count);
      var found = bi.TryGetByValue(12, out var key);
      Assert.IsTrue(found);
      Assert.AreEqual(2, key);
    }

    [TestMethod]
    public void BiDirectionalDictionaryGetByValue()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 12 } };
      Assert.AreEqual(2, bi.Count);
      var key = bi.GetByValue(12);
      Assert.AreEqual(2, key);
    }

    [TestMethod]
    public void BiDirectionalDictionaryGetByValueException()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 12 } };
      Assert.AreEqual(2, bi.Count);
      var exception = false;
      try
      {
        _ = bi.GetByValue(13);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void BiDirectionalDictionaryAddException1()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      Assert.AreEqual(2, bi.Count);
      var exception = false;
      try
      {
        bi.Add(3, 2);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void BiDirectionalDictionaryAddException2()

    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      Assert.AreEqual(2, bi.Count);
      var exception = false;
      try
      {
        bi.Add(1, 3);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void TryAddTest()
    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 10 }, { 2, 20 }, { 3, 30 } };
      Assert.IsTrue(bi.TryAdd(100, 1000));
      Assert.IsFalse(bi.TryAdd(1, 15));
      bi.Remove(100);
    }

    [TestMethod]
    public void TryGetByValueTest()
    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 10 }, { 2, 20 }, { 3, 30 } };
      Assert.IsTrue(bi.TryGetByValue(10, out var key));
      Assert.AreEqual(1, key);
      Assert.IsFalse(bi.TryGetByValue(100, out _));
    }

    [TestMethod]
    public void GetByValueTest()
    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 10 }, { 2, 20 }, { 3, 30 } };
      Assert.AreEqual(1, bi.GetByValue(10));
      Assert.AreEqual(2, bi.GetByValue(20));
    }

    [TestMethod]
    public void ClearTest()
    {
      var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
      bi.Clear();
      Assert.AreEqual(0, bi.Count);
    }
  }
}