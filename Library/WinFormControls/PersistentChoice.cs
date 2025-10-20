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
#nullable enable

using System;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Class to store dialog choices in a persistent way
  /// </summary>
  public sealed class PersistentChoice
  {
    public PersistentChoice(in DialogResult option)
    {
      if (option != DialogResult.Yes && option != DialogResult.No)
        throw new ArgumentOutOfRangeException(nameof(option), option, @"Only Yes and No are supported");
      DialogResult = option;
    }

    public bool Chosen { get; set; }

    public DialogResult DialogResult { get; private set; }

    public int NumRecs { get; private set; }

    public void ProcessedOne() => NumRecs--;

    public void Reset(int counter)
    {
      NumRecs = counter;
      Chosen = false;
    }
  }
}
