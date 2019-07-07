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

using System;

namespace CsvTools
{
  public class ConfigurationException : ApplicationException
  {
    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception inner) : base(message, inner)
    {
    }
  }

  public class ConversionException : ApplicationException
  {
    public ConversionException(string message) : base(message)
    {
    }

    public ConversionException(string message, Exception inner) : base(message, inner)
    {
    }
  }

  public class EncryptionException : ApplicationException
  {
    public EncryptionException(string message) : base(message)
    {
    }

    public EncryptionException(string message, Exception inner) : base(message, inner)
    {
    }
  }

  public class FileException : ApplicationException
  {
    public FileException(string message) : base(message)
    {
    }

    public FileException(string message, Exception inner) : base(message, inner)
    {
    }
  }

  public class FileReaderException : ApplicationException
  {
    public FileReaderException(string message) : base(message)
    {
    }

    public FileReaderException(string message, Exception inner) : base(message, inner)
    {
    }
  }

  public class FileWriterException : ApplicationException
  {
    public FileWriterException(string message) : base(message)
    {
    }

    public FileWriterException(string message, Exception inner) : base(message, inner)
    {
    }
  }
}