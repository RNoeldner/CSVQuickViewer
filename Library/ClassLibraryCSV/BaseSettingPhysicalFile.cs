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

using JetBrains.Annotations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CsvTools
{
	[DebuggerDisplay("File: {ID} {m_FileName} ({ColumnCollection.Count()} Columns)")]
	public abstract class BaseSettingPhysicalFile : BaseSettings, IFileSettingPhysicalFile
	{
		private string m_FileName;
		private long m_FileSize;
		private string m_FullPath = string.Empty;
		private bool m_FullPathInitialized;
		private string m_IdentifierInContainer = string.Empty;
		private string m_RemoteFileName = string.Empty;
		private bool m_ThrowErrorIfNotExists = true;
		private string m_ColumnFile = string.Empty;
		private string m_Passphrase = string.Empty;
		private string m_Recipient = string.Empty;
		private bool m_KeepUnencrypted = false;

		public BaseSettingPhysicalFile(string fileName)
		{
			m_FileName = FileNameFix(fileName);
		}

		/// <summary>
		///   Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[XmlAttribute]
		[DefaultValue("")]
		public virtual string FileName
		{
			[NotNull]
			get => m_FileName;
			[CanBeNull]
			set
			{
				var newVal = FileNameFix(value);
				if (m_FileName.Equals(newVal, StringComparison.Ordinal))
					return;
				var oldValue = m_FileName;
				m_FileName = newVal;
				NotifyPropertyChanged(nameof(FileName));
				NotifyPropertyChangedString(nameof(FileName), oldValue, newVal);
			}
		}

		/// <summary>
		///   Gets or sets the date the file when it was read
		/// </summary>
		/// <value>The consecutive empty rows.</value>
		[XmlAttribute]
		[DefaultValue(0)]
		public virtual long FileSize
		{
			get => m_FileSize;

			set
			{
				if (value == m_FileSize)
					return;
				m_FileSize = value;
				NotifyPropertyChanged(nameof(FileSize));
			}
		}

		[XmlIgnore]
		[NotNull]
		public virtual string FullPath
		{
			get
			{
				if (m_FullPathInitialized) return m_FullPath ?? FileName;
				m_FullPath = FileName.FullPath(RootFolder);
				if (m_FullPath.Length==0)
					m_FullPath = string.Empty;
				else
					m_FullPathInitialized = true;
				return m_FullPath;
			}
		}

		/// <summary>
		///   Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[XmlAttribute]
		[DefaultValue("")]
		public virtual string ColumnFile
		{
			[NotNull]
			get => m_ColumnFile;
			[CanBeNull]
			set => m_ColumnFile = value ?? string.Empty;
		}

		/// <summary>
		///   Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[XmlAttribute]
		[DefaultValue("")]
		public virtual string IdentifierInContainer
		{
			[NotNull]
			get => m_IdentifierInContainer;
			[CanBeNull]
			set
			{
				var newVal = value ?? string.Empty;
				if (m_IdentifierInContainer.Equals(newVal, StringComparison.Ordinal))
					return;

				m_IdentifierInContainer = newVal;
				NotifyPropertyChanged(nameof(IdentifierInContainer));
			}
		}

		/// <summary>
		///   The identified to find this specific instance
		/// </summary>
		[XmlIgnore]
		[NotNull]
		public override string InternalID => string.IsNullOrEmpty(ID) ? FileName : ID;

		/// <summary>
		///   Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[XmlAttribute]
		[DefaultValue("")]
		public virtual string RemoteFileName
		{
			[NotNull]
			get => m_RemoteFileName;
			[CanBeNull]
			set
			{
				var newVal = value ?? string.Empty;
				if (m_RemoteFileName.Equals(newVal, StringComparison.Ordinal))
					return;

				m_RemoteFileName = newVal;
				NotifyPropertyChanged(nameof(RemoteFileName));
			}
		}

		[XmlIgnore]
		[DefaultValue("")]
		public string RootFolder { get; set; } = string.Empty;

		/// <summary>
		///   Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
		[XmlAttribute]
		[DefaultValue(true)]
		public virtual bool ThrowErrorIfNotExists
		{
			get => m_ThrowErrorIfNotExists;

			set
			{
				if (m_ThrowErrorIfNotExists.Equals(value))
					return;
				m_ThrowErrorIfNotExists = value;
				NotifyPropertyChanged(nameof(ThrowErrorIfNotExists));
			}
		}

		public void ResetFullPath() => m_FullPathInitialized = false;

		protected override void BaseSettingsCopyTo(BaseSettings other)
		{
			if (other == null)
				return;
			base.BaseSettingsCopyTo(other);

			if (other is IFileSettingPhysicalFile fileSettingPhysicalFile)
			{
				fileSettingPhysicalFile.FileSize = FileSize;
				fileSettingPhysicalFile.ColumnFile = ColumnFile;
				fileSettingPhysicalFile.FileName = FileName;
				fileSettingPhysicalFile.RemoteFileName = RemoteFileName;
				fileSettingPhysicalFile.IdentifierInContainer = IdentifierInContainer;
				fileSettingPhysicalFile.ThrowErrorIfNotExists = ThrowErrorIfNotExists;
				fileSettingPhysicalFile.Passphrase = Passphrase;
				fileSettingPhysicalFile.Recipient = Recipient;
				fileSettingPhysicalFile.KeepUnencrypted = KeepUnencrypted;
			}
		}

		protected override bool BaseSettingsEquals(BaseSettings other)
		{
			if (other is IFileSettingPhysicalFile fileSettingPhysicalFile)
			{
				if (!string.Equals(fileSettingPhysicalFile.FileName, FileName, StringComparison.OrdinalIgnoreCase))
					return false;

				if (fileSettingPhysicalFile.RemoteFileName != RemoteFileName ||
						fileSettingPhysicalFile.ThrowErrorIfNotExists != ThrowErrorIfNotExists)
					return false;

				if (fileSettingPhysicalFile.IdentifierInContainer != IdentifierInContainer ||
						fileSettingPhysicalFile.FileSize != FileSize)
					return false;

				if (!fileSettingPhysicalFile.Passphrase.Equals(Passphrase, StringComparison.Ordinal) ||  !fileSettingPhysicalFile.Recipient.Equals(Recipient, StringComparison.OrdinalIgnoreCase) || fileSettingPhysicalFile.KeepUnencrypted != KeepUnencrypted)
					return false;

				if (!string.Equals(fileSettingPhysicalFile.ColumnFile, ColumnFile, StringComparison.OrdinalIgnoreCase))
					return false;
			}
			return base.BaseSettingsEquals(other);
		}

		/// <summary>
		///   Recipient for a outbound PGP encryption
		/// </summary>
		[XmlAttribute]
		[DefaultValue("")]
		public virtual string Recipient
		{
			[NotNull]
			get => m_Recipient;
			[CanBeNull]
			set
			{
				var newVal = (value ?? string.Empty).Trim();
				if (m_Recipient.Equals(newVal, StringComparison.Ordinal))
					return;
				m_Recipient = newVal;
				NotifyPropertyChanged(nameof(Recipient));
			}
		}

		/// <summary>
		///   Passphrase for Decryption, will not be stored
		/// </summary>
		[XmlIgnore]
		[DefaultValue("")]
		public virtual string Passphrase
		{
			[NotNull]
			get => m_Passphrase;
			[CanBeNull]
			set => m_Passphrase = (value ?? string.Empty).Trim();
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool KeepUnencrypted
		{
			get => m_KeepUnencrypted; set
			{
				if (m_KeepUnencrypted==value)
					return;
				m_KeepUnencrypted=value;
				NotifyPropertyChanged(nameof(KeepUnencrypted));
			}
		}

		[NotNull]
		private static string FileNameFix([CanBeNull] string value)
		{
			var newVal = value ?? string.Empty;
			if (newVal.StartsWith(".\\", StringComparison.Ordinal))
				newVal = newVal.Substring(2);
			return newVal;
		}
	}
}