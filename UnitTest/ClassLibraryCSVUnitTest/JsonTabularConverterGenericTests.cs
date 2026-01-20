using CsvTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CsvTools.Tests;

[TestClass]
public class JsonTabularConverterTests
{
  [TestMethod]
  public void DiscoverColumns_ShouldHandleArrayOfObjects()
  {
    // Arrange
    var array = new[] {
            new JObject
            {
                ["items"] = new JArray(
                    new JObject { ["id"] = 1, ["name"] = "Item1" },
                    new JObject { ["id"] = 2, ["name"] = "Item2" }
                )
            }
        };

    // Act
    var columns = array.DiscoverColumns();

    // Assert
    Assert.IsTrue(columns.Any(c => c.HeaderName == "items.ids"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "items.names"));
  }

  [TestMethod]
  public void DiscoverColumns_ShouldHandleArrayOfPrimitives()
  {
    // Arrange
    var array = new[] { new JObject { ["tags"] = new JArray("tag1", "tag2") } };

    // Act
    var columns = array.DiscoverColumns();

    // Assert
    Assert.AreEqual(1, columns.Count);
    Assert.AreEqual("tags", columns[0].HeaderName);
    Assert.AreEqual(typeof(string), columns[0].PropertyType); // primitive array concatenated as string
  }

  [TestMethod]
  public void DiscoverColumns_ShouldHandleScalars()
  {
    // Arrange
    var array = new List<JObject>
        {
            new JObject { ["id"] = 1, ["name"] = "Alice", ["active"] = true },
            new JObject { ["id"] = 2, ["name"] = "Bob", ["active"] = false }
        };

    // Act
    var columns = array.DiscoverColumns();

    // Assert
    Assert.AreEqual(3, columns.Count);
    Assert.IsTrue(columns.Any(c => c.HeaderName == "id"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "name"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "active"));
  }
  [TestMethod]
  public void PickObjectProperties_ShouldPrioritizeIdThen_IdThenOthers()
  {
    // Arrange
    var obj1 = new JObject { ["id"] = 1, ["code"] = "X1", ["name"] = "Alice" };
    var obj2 = new JObject { ["user_id"] = 42, ["title"] = "Manager" };

    // Act
    var props = typeof(JsonTabularConverter)
        .GetMethod("PickObjectProperties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
        .Invoke(null, new object[] { 3, new JObject[] { obj1, obj2 } }) as IReadOnlyCollection<JProperty>;

    var propNames = props.Select(p => p.Name).ToList();

    // Assert priority: 'id' first, then 'user_id', then others
    Assert.AreEqual("id", propNames[0]);
    Assert.AreEqual("code", propNames[1]);
    Assert.IsTrue(propNames.Contains("name") || propNames.Contains("title") || propNames.Contains("user_id"));
  }

  [TestMethod]
  [DataRow("UserV5.json")]
  [DataRow("Emp.json")]
  [DataRow("larger.json")]
  [DataRow("Jason1.json")]
  [DataRow("Jason2.json")] 
  [DataRow("Jason3.json")]
  [DataRow("Jason4.json")]
  [DataRow("Array.json")]
  [DataRow("LogFile.json")]
  public void ProcessJsonFile_AsTable_ShouldReturnExpectedColumns(string fileName)
  {
    // Arrange
    var filePath = fileName.FullPath(UnitTestStatic.ApplicationDirectory);
    Assert.IsTrue(File.Exists(filePath), $"Test JSON file not found: {fileName}");

    var rows = new List<IReadOnlyCollection<string>>();
    using var reader = new StreamReader(filePath);
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row), ',', 3, UnitTestStatic.Token);

    // Assert
    Assert.IsTrue(columns.Count > 0, $"No columns discovered for {fileName}");
    Assert.IsTrue(rows.Count > 0, $"No rows written for {fileName}");

    // Optional logging
    Logger.Information($"Columns for {fileName}: {string.Join(", ", columns.Select(c => c.HeaderName))}");
    Logger.Information($"First row: {string.Join(", ", rows[0])}");

    // Validate expected columns per file
    var expectedColumns = fileName.ToLowerInvariant() switch
    {
      "larger.json" => new[] { "object_id", "course_code", "lo_hours" },
      "userv5.json" => new[] { "externalId", "userId", "email", "avatar.tinies", "learningTopics.topic_ids" },
      "jason1.json" => new[] { "id", "websiteId", "enabled", "starred", "tags" },
      "jason2.json" => new[] { "date", "level", "message" },
      "jason3.json" => new[] { "id", "title" },
      "jason4.json" => new[] { "time", "level", "message" },
      _ => Array.Empty<string>()
    };

    foreach (var col in expectedColumns)
    {
      Assert.IsTrue(columns.Any(c => c.HeaderName.Equals(col, StringComparison.OrdinalIgnoreCase)),
          $"Expected column '{col}' not found in {fileName}");
    }
  }

  [TestMethod]
  public void StreamRows_ShouldHandleArraysOfObjectsWithIdentityProperties()
  {
    // Arrange
    var json = @"{
            ""items"": [
                { ""id"": 1, ""name"": ""Item1"" },
                { ""id"": 2, ""name"": ""Item2"" }
            ]
        }";

    var rows = new List<IReadOnlyCollection<string>>();

    using var reader = new StringReader(json);

    // Act
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row));

    // Assert
    Assert.AreEqual(2, rows.Count); // each object in array yields a row
    Assert.IsTrue(columns.Any(c => c.HeaderName == "id"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "name"));

    var row1 = rows[0].ToArray();
    var row2 = rows[1].ToArray();

    int idIndex = columns.ToList().FindIndex(c => c.HeaderName == "id");
    int nameIndex = columns.ToList().FindIndex(c => c.HeaderName == "name");

    Assert.AreEqual("1", row1[idIndex]);
    Assert.AreEqual("Item1", row1[nameIndex]);
    Assert.AreEqual("2", row2[idIndex]);
    Assert.AreEqual("Item2", row2[nameIndex]);
  }

  [TestMethod]
  public void StreamRows_ShouldHandleEmptyArrays()
  {
    // Arrange
    var json = @"{ ""tags"": [] }";

    var rows = new List<IReadOnlyCollection<string>>();

    using var reader = new StringReader(json);

    // Act
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row));

    // Assert
    Assert.AreEqual(0, rows.Count);
  }

  [TestMethod]
  public void StreamRows_ShouldHandleMultipleTopLevelObjects()
  {
    // Arrange: JSON with multiple top-level objects
    var json = @"{
            ""collection"": { ""id"": ""1"", ""title"": ""Blog"" },
            ""collection2"": { ""id"": ""2"", ""title"": ""Blog2"" }
        }";

    var rows = new List<IReadOnlyCollection<string>>();

    using var reader = new StringReader(json);

    // Act
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row));

    // Assert
    Assert.AreEqual(2, rows.Count);
    Assert.IsTrue(columns.Any(c => c.HeaderName == "id"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "title"));

    var row1 = rows[0].ToArray();
    var row2 = rows[1].ToArray();

    int idIndex = columns.ToList().FindIndex(c => c.HeaderName == "id");
    int titleIndex = columns.ToList().FindIndex(c => c.HeaderName == "title");

    Assert.AreEqual("1", row1[idIndex]);
    Assert.AreEqual("Blog", row1[titleIndex]);
    Assert.AreEqual("2", row2[idIndex]);
    Assert.AreEqual("Blog2", row2[titleIndex]);
  }

  [TestMethod]
  public void StreamRows_ShouldHandleNestedObjects()
  {
    // Arrange: JSON with nested object
    var json = @"{
            ""collection"": {
                ""id"": ""1"",
                ""title"": ""Blog"",
                ""address"": { ""homepage"": true, ""fullUrl"": ""/blog/"" }
            }
        }";

    var rows = new List<IReadOnlyCollection<string>>();

    using var reader = new StringReader(json);

    // Act
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row));

    // Assert
    Assert.AreEqual(1, rows.Count);
    Assert.IsTrue(columns.Any(c => c.HeaderName == "id"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "title"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "address.homepages")); // Pluralized

    var row = rows[0].ToArray();
    int homepageIndex = columns.ToList().FindIndex(c => c.HeaderName == "address.homepages");

    Assert.AreEqual("true", row[homepageIndex]);
  }

  [TestMethod]
  public void StreamRows_ShouldHandleRootScalarsAsMetadata()
  {
    // Arrange: JSON with only scalars
    var json = @"{ ""time"": ""2020-01-01"", ""level"": ""Info"" }";

    var rows = new List<IReadOnlyCollection<string>>();

    using var reader = new StringReader(json);

    // Act
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row));

    // Assert
    Assert.AreEqual(1, rows.Count); // root object is yielded
    Assert.AreEqual("2020-01-01", metadata["time"].Value);
    Assert.AreEqual("Info", metadata["level"].Value);

    Assert.IsTrue(columns.Any(c => c.HeaderName == "time"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "level"));
  }

  [TestMethod]
  public void WriteRows_ShouldGenerateExpectedValues()
  {
    // Arrange
    var array = new List<JObject>
        {
            new JObject
            {
                ["id"] = 1,
                ["name"] = "Alice",
                ["tags"] = new JArray("t1", "t2"),
                ["active"] = true,
                ["created"] = DateTime.Parse("2026-01-12T08:00:00")
            }
        };

    var rows = new List<IReadOnlyCollection<string>>();

    // Act
    using var reader = new StringReader(new JArray(array).ToString());
    var (columns, metadata) = reader.StreamRows(row => rows.Add(row));

    // Assert
    Assert.AreEqual(1, rows.Count);
    var row = rows[0].ToArray();

    int idIndex = columns.ToList().FindIndex(c => c.HeaderName == "id");
    int nameIndex = columns.ToList().FindIndex(c => c.HeaderName == "name");
    int tagsIndex = columns.ToList().FindIndex(c => c.HeaderName == "tags");
    int activeIndex = columns.ToList().FindIndex(c => c.HeaderName == "active");
    int createdIndex = columns.ToList().FindIndex(c => c.HeaderName == "created");

    Assert.AreEqual("1", row[idIndex]);
    Assert.AreEqual("Alice", row[nameIndex]);
    Assert.AreEqual("t1, t2", row[tagsIndex]);
    Assert.AreEqual("true", row[activeIndex]);
    Assert.AreEqual("2026-01-12T08:00:00", row[createdIndex]);
  }
}
