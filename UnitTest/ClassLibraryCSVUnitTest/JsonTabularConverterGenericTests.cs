using CsvTools;
using EIHControlCenter.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace CsvTools.Tests;

[TestClass]
public class JsonTabularConverterTests
{
  [TestMethod]
  public void DiscoverColumns_ShouldHandleScalars()
  {
    // Arrange
    var array = new JArray(
        new JObject { ["id"] = 1, ["name"] = "Alice", ["active"] = true },
        new JObject { ["id"] = 2, ["name"] = "Bob", ["active"] = false }
    );

    // Act
    var columns = array.DiscoverColumns().ToList();

    // Assert
    Assert.AreEqual(3, columns.Count);
    Assert.IsTrue(columns.Any(c => c.HeaderName == "id"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "name"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "active"));
  }

  [TestMethod]
  public void DiscoverColumns_ShouldHandleArrayOfPrimitives()
  {
    // Arrange
    var array = new JArray(
        new JObject { ["tags"] = new JArray("tag1", "tag2") }
    );

    // Act
    var columns = array.DiscoverColumns().ToList();

    // Assert
    Assert.AreEqual(1, columns.Count);
    Assert.AreEqual("tags", columns[0].HeaderName);
    Assert.AreEqual(typeof(string), columns[0].PropertyType); // primitive array concatenated as string
  }

  [TestMethod]
  public void DiscoverColumns_ShouldHandleArrayOfObjects()
  {
    // Arrange
    var array = new JArray(
        new JObject
        {
          ["items"] = new JArray(
                new JObject { ["id"] = 1, ["name"] = "Item1" },
                new JObject { ["id"] = 2, ["name"] = "Item2" }
            )
        }
    );

    // Act
    var columns = array.DiscoverColumns().ToList();

    // Assert
    Assert.IsTrue(columns.Any(c => c.HeaderName == "items.id"));
    Assert.IsTrue(columns.Any(c => c.HeaderName == "items.name"));
  }

  [TestMethod]
  public void PickObjectProperties_ShouldPrioritizeIdThen_IdThenOthers()
  {
    // Arrange
    var obj1 = new JObject
    {
      ["id"] = 1,
      ["code"] = "X1",
      ["name"] = "Alice"
    };
    var obj2 = new JObject
    {
      ["user_id"] = 42,
      ["title"] = "Manager"
    };

    // Act
    var props = typeof(JsonTabularConverter)
        .GetMethod("PickObjectProperties", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
        .Invoke(null, new object[] { 3, new JObject[] { obj1, obj2 } }) as IReadOnlyCollection<JProperty>;

    var propNames = props.Select(p => p.Name).ToList();

    // Assert priority: 'id' first, then 'user_id', then others
    Assert.AreEqual("id", propNames[0]);
    Assert.AreEqual("user_id", propNames[1]);
    Assert.IsTrue(propNames.Contains("code") || propNames.Contains("name") || propNames.Contains("title"));
  }

  [TestMethod]
  public void WriteRows_ShouldGenerateExpectedValues()
  {
    // Arrange
    var array = new JArray(
        new JObject
        {
          ["id"] = 1,
          ["name"] = "Alice",
          ["tags"] = new JArray("t1", "t2"),
          ["active"] = true,
          ["created"] = DateTime.Parse("2026-01-12T08:00:00")
        }
    );

    var columns = array.DiscoverColumns().ToList();
    var rows = new List<IReadOnlyCollection<string>>();

    // Act
    int count = JsonTabularConverter.WriteRows(array, columns, r => rows.Add(r), CancellationToken.None);

    // Assert
    Assert.AreEqual(1, count);
    Assert.AreEqual(rows[0].Count, columns.Count);
    var row = rows[0].ToArray();

    // Validate some known values
    int idIndex = columns.FindIndex(c => c.HeaderName == "id");
    int nameIndex = columns.FindIndex(c => c.HeaderName == "name");
    int tagsIndex = columns.FindIndex(c => c.HeaderName == "tags");
    int activeIndex = columns.FindIndex(c => c.HeaderName == "active");
    int createdIndex = columns.FindIndex(c => c.HeaderName == "created");

    Assert.AreEqual("1", row[idIndex]);
    Assert.AreEqual("Alice", row[nameIndex]);
    Assert.AreEqual("t1, t2", row[tagsIndex]);
    Assert.AreEqual("true", row[activeIndex]);
    Assert.AreEqual("2026-01-12T08:00:00", row[createdIndex]);
  }



  [TestMethod]
  [DataRow("UserV5.json")]
  [DataRow("larger.json")]
  public void ProcessJsonFile_AsTable_ShouldReturnExpectedColumns(string fileName)
  {

    // Arrange
    var filePath = fileName.FullPath(UnitTestStatic.ApplicationDirectory);
    Assert.IsTrue(File.Exists(filePath), $"Test JSON file not found: {fileName}");

    var jsonText = File.ReadAllText(filePath);
    var jsonObj = JObject.Parse(jsonText);

    var columns = new List<JsonTabularConverter.JsonColumn>();
    var rows = new List<IReadOnlyCollection<string>>();


    // Act
    int rowCount = jsonObj.WriteJsonArrayAsTable(
        columns,
        fileName.Contains("larger", StringComparison.OrdinalIgnoreCase) ? "pl" : fileName.Contains("user", StringComparison.OrdinalIgnoreCase) ? "profiles" : "data",
        writeHeader: true,
        handleOneRow: row => rows.Add(row),
        token: CancellationToken.None
    );

    // Assert
    Assert.IsTrue(columns.Count > 0, "No columns were discovered.");
    Assert.IsTrue(rowCount > 0, "No rows were written.");

    // Optional: output first row for debugging
    Logger.Information($"Columns for {fileName}:\n{string.Join(", ", columns.Select(c => c.HeaderName))}");
    if (rows.Count>0)
      Logger.Information($"First data row:\n{string.Join(", ", rows[1])}");

    // Optional: check specific columns exist
    var expectedColumns = fileName.Contains("larger", StringComparison.OrdinalIgnoreCase) ? new[] { "object_id", "course_code", "lo_hours" } : 
                          fileName.Contains("user", StringComparison.OrdinalIgnoreCase) ? new[] { "externalId", "userId", "email", "avatar.tiny", "learningTopics.topic_id" } : Array.Empty<string>(); 
    foreach (var col in expectedColumns)
    {
      Assert.IsTrue(columns.Any(c => c.HeaderName.Equals(col, StringComparison.OrdinalIgnoreCase)),
          $"Expected column '{col}' not found in {fileName}");
    }
  }
}
