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
using System;
using System.Collections.Generic;

namespace CsvTools.Tests;

[TestClass]
public class BiDirectionalDictionaryTests
{
  [TestMethod]
  public void Add_DuplicateKey_ShouldThrow()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    dict.Add(1, "A");
    Assert.Throws<ArgumentException>(() => dict.Add(1, "B"));
  }

  [TestMethod]
  public void Add_DuplicateValue_ShouldThrow()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    dict.Add(1, "A");
    Assert.Throws<ArgumentException>(() => dict.Add(2, "A"));
  }

  [TestMethod]
  public void AddAndGetByValue_ShouldWork()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    dict.Add(1, "A");
    dict.Add(2, "B");
    Assert.AreEqual(1, dict.GetByValue("A"));
    Assert.AreEqual(2, dict.GetByValue("B"));
  }

  [TestMethod]
  public void BiDirectionalDictionaryAddException1()
  {
    var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
    Assert.AreEqual(2, bi.Count);
    Assert.Throws<ArgumentException>(() => bi.Add(3, 2));
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
  public void BiDirectionalDictionaryCtorDirectory()
  {
    var init = new Dictionary<int, int> { { 1, 1 }, { 2, 2 } };
    var bi = new BiDirectionalDictionary<int, int>(init);
    Assert.AreEqual(2, bi.Count);
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
    Assert.Throws<KeyNotFoundException>(() => _ = bi.GetByValue(13));
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
  public void Clear_ShouldRemoveAll()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    dict.Add(1, "A");
    dict.Add(2, "B");
    dict.Clear();
    Assert.AreEqual(0, dict.Count);
    Assert.IsFalse(dict.TryGetByValue("A", out _));
    Assert.IsFalse(dict.TryGetByValue("B", out _));
  }

  [TestMethod]
  public void ClearTest()
  {
    var bi = new BiDirectionalDictionary<int, int> { { 1, 1 }, { 2, 2 } };
    bi.Clear();
    Assert.AreEqual(0, bi.Count);
  }

  [TestMethod]
  public void Constructor_FromDictionary_DuplicateValue_ShouldThrow()
  {
    var source = new Dictionary<int, string> { { 1, "A" }, { 2, "A" } };
    Assert.Throws<ArgumentException>(() => { var dict = new BiDirectionalDictionary<int, string>(source); });
  }

  [TestMethod]
  public void Constructor_FromDictionary_ShouldCopy()
  {
    var source = new Dictionary<int, string> { { 1, "A" }, { 2, "B" } };
    var dict = new BiDirectionalDictionary<int, string>(source);
    Assert.AreEqual(1, dict.GetByValue("A"));
    Assert.AreEqual(2, dict.GetByValue("B"));
  }

  [TestMethod]
  public void GetByValue_NotFound_ShouldThrow()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    Assert.Throws<KeyNotFoundException>(() => dict.GetByValue("X"));
  }

  [TestMethod]
  public void Remove_ShouldRemoveBothDirections()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    dict.Add(1, "A");
    dict.Remove(1);
    Assert.IsFalse(dict.ContainsKey(1));
    Assert.IsFalse(dict.TryGetByValue("A", out _));
  }

  [TestMethod]
  public void TryAdd_ShouldReturnFalseOnDuplicate()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    Assert.IsTrue(dict.TryAdd(1, "A"));
    Assert.IsFalse(dict.TryAdd(1, "B"));
    Assert.IsFalse(dict.TryAdd(2, "A"));
  }

  [TestMethod]
  public void TryGetByValue_ShouldReturnTrueIfExists()
  {
    var dict = new BiDirectionalDictionary<int, string>();
    dict.Add(1, "A");
    Assert.IsTrue(dict.TryGetByValue("A", out var key));
    Assert.AreEqual(1, key);
    Assert.IsFalse(dict.TryGetByValue("B", out _));
  }
}