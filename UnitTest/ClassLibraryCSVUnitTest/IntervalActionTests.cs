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
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class IntervalActionTests
  {
    [TestMethod]
    public void Defaults()
    {
      var test = new IntervalAction();

      var test2 = IntervalAction.ForProgress(null);
      Assert.IsNull(test2);

      var test3 = IntervalAction.ForProgress(new Progress<ProgressInfo>());
      Assert.IsNotNull(test3);
    }

    [TestMethod]
    public void IntervalActionError()
    {
      var intervalAction = new IntervalAction();
      intervalAction.Invoke(() => throw new ObjectDisposedException("dummy"));
    }

    [TestMethod]
    public void OtherInvokeMethods()
    {
      new IntervalAction().Invoke(new Progress<ProgressInfo>(), "Test2", 100);
    }

    [TestMethod]
    public async Task InvokeTestRapid()
    {
      long setValue = -1;
      var called = 0;
      var intervalAction = new IntervalAction();
      // first call should always go through
      intervalAction.Invoke(() => { setValue = 666; called++; });
      Assert.AreEqual(666L, setValue);
      Assert.AreEqual(1, called);

      // rapid call should be swallowed
      intervalAction.Invoke(() => { setValue = 669; called++; });
      Assert.AreEqual(666L, setValue);
      Assert.AreEqual(1, called);

      // wait for some time
      await Task.Delay(TimeSpan.FromSeconds(.3d).Milliseconds);

      // now the value should be set
      intervalAction.Invoke(() => { setValue = 669; called++; });
      Assert.AreEqual(669L, setValue);
      Assert.AreEqual(2, called);
    }
  }
}
