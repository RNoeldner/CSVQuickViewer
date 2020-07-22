using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class RealignColumnsTest
  {
    [TestMethod]
    public async Task AllFormatsPipeReaderAsync()
    {
      var setting = new CsvFile(UnitTestInitializeCsv.GetTestPath("RealignColumn.txt"))
      {
        HasFieldHeader = true,
        FileFormat = { FieldDelimiter = "\t" },
        TryToSolveMoreColumns = true,
        AllowRowCombining = true,
        SkipEmptyLines = false
      };

      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var test = new CsvFileReader(setting, processDisplay))
      {
        await test.OpenAsync(processDisplay.CancellationToken);

        // first five rows are good.
        await test.ReadAsync(processDisplay.CancellationToken); // Line 2
        await test.ReadAsync(processDisplay.CancellationToken); // Line 3
        await test.ReadAsync(processDisplay.CancellationToken); // Line 4
        await test.ReadAsync(processDisplay.CancellationToken); // Line 5
        await test.ReadAsync(processDisplay.CancellationToken); // Line 6

        // Issue row Column 3 = Text|6
        await test.ReadAsync(processDisplay.CancellationToken); // Line 6
        Assert.AreEqual("Text\tF", test.GetValue(3));

        await test.ReadAsync(processDisplay.CancellationToken); // Line 7
        await test.ReadAsync(processDisplay.CancellationToken); // Line 8
        await test.ReadAsync(processDisplay.CancellationToken); // Line 9
        await test.ReadAsync(processDisplay.CancellationToken); // Line 10

        await test.ReadAsync(processDisplay.CancellationToken); // Line 11
        Assert.AreEqual("Memo: A long text, \t multiple words 11", test.GetValue(5));
        await test.ReadAsync(processDisplay.CancellationToken); // Line 12

        await test.ReadAsync(processDisplay.CancellationToken); // Line 13
        await test.ReadAsync(processDisplay.CancellationToken); // Line 14
        await test.ReadAsync(processDisplay.CancellationToken); // Line 15
        await test.ReadAsync(processDisplay.CancellationToken); // Line 16
        await test.ReadAsync(processDisplay.CancellationToken); // Line 17
        Assert.AreEqual("Memo: A long text\nmultiple words 17", test.GetValue(5));
      }
    }

    [TestMethod]
    public void DirectTest()
    {
      var goodLines = new List<string>(
        new[]
        {
          "03.04.2014|-22477|-12086.66|Text A|-11654.13|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|1BD10E34-7D66-481B-A7E3-AE817B5BEE02|12|14:26:58|CET",
          "23.05.2014|22435|12454.21|Text B|-20412.43|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|5D1E034E-506B-4EFB-A99F-29B2E4652D4E|13||",
          "|1175|-11621.02|Text C|-7157.88|Memo: A long text, multiple words 3|TRUE|99281DDB-10BF-42E2-8E9D-B9E89F57FB02|15|08:42:27|CET",
          "03.04.2014|4068|27374.82|Text D|-1078.97|Memo: A long text, multiple words 4|FALSE|C5213108-64D9-4842-AB37-807716694AEB|16|05:18:02|PST",
          "11.05.2013||-7515.08|Text E|-21323.06|Memo: A long text, multiple words 5|FALSE|B36CFE01-E52E-44A6-B3ED-62BF584D3735|17||",
          "30.07.2012|-16602|-2124.27|Text G|-13112.11|Memo: A long text, multiple words 7|TRUE|B73BEE7B-A4E7-4763-97DA-4F7E4CAF9D65|20|14:03:01|",
          "|17829|20200.53|Text H|-20223.31|Memo: A long text, multiple words 8|FALSE|02B72B47-A36B-4FFC-BE6F-1141A0FE7694|22|23:51:27|PST",
          "03.04.2014|22444|-10457.26|Text J|-8431.18|Memo: A long text, multiple words 9|TRUE|8820AE20-0A45-4817-A223-F122F6ACDA66|23|23:21:24|MST",
          "23.05.2014|-28016|-25425.4|Text K|12071.84|Memo: A long text, multiple words 10|FALSE|928D80F7-3C7C-47C3-9049-ABD4D1DAEDEB|24|16:00:29|",
          "03.04.2014|-13733|9859.87|Text M|-20742.91|Memo: A long text, multiple words 12|TRUE|57379EB7-7B87-4401-9DA9-D083B74B6938|27|05:22:31|",
          "|-12172|1226.88|Text N|4207.36|Memo: A long text, multiple words 13|TRUE|92B9979B-F3C8-4077-B8DF-872BAC4161B3|28|01:21:54'|",
          "17.10.2014|-25103|-31988.63|Text O|-21643.83|Memo: A long text, multiple words 14|TRUE|4FE3F3D4-D604-4DBE-AA20-78DF0D01B8C9|30|01:36:54|",
          "30.07.2012|30461|20645.77|Text P|-7776.66|Memo: A long text, multiple words 15|FALSE|72C7B497-7765-4366-9517-CC20FF8634E5|31|00:48:20|EST",
          "09.09.2012|-27972|8027.03|Text Q|4193.08|Memo: A long text, multiple words 16|FALSE|78C344BB-7496-4B5D-98C9-06279287D5AC|32|23:07:06|",
          "19.03.2014|-15345|-16196.45|Text S|23967.22|Memo: A long text, multiple words 18|TRUE|FF2D5E1F-D36C-48A9-9C80-AE729DC7E9AC|37|23:55:21|",
          "03.04.2014|-17615|13440.35|Text T|728.93|Memo: A long text, multiple words 19|||38||",
          "11.05.2013|-4999|-3224.07|Text U|10000.58|Memo: A long text, multiple words 20|FALSE|777ECB98-A80B-470F-B44C-19F797A202DC|39|11:00:11|",
          "17.10.2014|-22360|23429.85|Text V||Memo: A long text, multiple words 21|FALSE|F9642041-D4B6-4965-BDA5-0BFBAF0DC7BF|40|03:32:05|",
          "19.03.2020|4526|1110,506785|Text W|4733|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|4C317A2D-1C0D-35EB-7577-9BBC58F05459|41|21:39:31|",
          "23.02.2020|3044|5847,658701|Text W|1929|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|32F6A714-992B-90FF-36EB-BB9C145A1B81|42|20:28:55|",
          "02.04.2020|-92|2210,58581|Text W|592|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|A3ABA6EA-2003-300B-0687-D97A516E6AB6|43|12:43:23|PST",
          "19.05.2020|-661|6040,844481|Text W|-798|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|580451C4-321B-5C60-2574-F92B5A6E0026|44|13:56:36|",
          "29.02.2020|4384|6177,66725|Text W|1586|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|D91AF6D6-32FE-07F0-3ED0-4BABF8D57188|45|01:25:00|PST",
          "27.04.2020|70|-2930,330254|Text W|535|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|8DDBB845-7EBF-95CB-2D09-1F80664B66DA|46|13:45:29|",
          "13.04.2020|2075|-2394,025278|Text W|4480|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|DC5A87AE-6D9C-3B33-0A7F-B28D031616F9|47|06:29:39|PST",
          "09.12.2019|3952|-358,0813425|Text W|3469|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|F29D5A9C-49E3-0F70-6ED9-CF9E07431188|48|12:53:55|",
          "16.03.2020|1730|-2478,520494|Text W|-943|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|F51C5FA2-3029-3009-6B9D-7860B48E0DBE|49|21:28:07|",
          "22.01.2020|-313|2263,623557|Text W|3074|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|071D5480-38C8-662A-739E-A92DEAF6A1BA|50|06:16:10|",
          "23.05.2020|408|-305,9681625|Text W|4740|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|768B3B21-29A6-068C-8152-CB411B6025DF|51|01:06:21|PST",
          "15.12.2019|2252|-2534,310099|Text W|2056|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|A5BD56A6-3C10-2305-01CA-EC2C74E61D27|52|18:21:07|",
          "02.03.2020|770|3755,03542|Text W|2269|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|80640A85-9AC1-5004-7426-68141B5B0F55|53|17:57:54|PST",
          "22.11.2019|1801|-1961,307353|Text W|4619|Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean|TRUE|C806AD74-01FA-5D95-3F28-4D1F10E47FFD|54|07:43:52|",
          "27.02.2020|1511|-469,6989025|Text W|519|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|29AC2AB2-705A-2DFF-6403-723375A0A329|55|16:02:07|",
          "01.02.2020|183|3112,997111|Text W|2729|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|0DFDC09B-4247-391E-93F7-FE5F44460253|56|16:56:12|",
          "14.01.2020|1198|5928,843772|Text W|787|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|BDC353EE-984E-2803-421D-16C58BC102F3|57|10:50:22|",
          "06.03.2020|3740|2900,702837|Text W|4449|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|E0750EF9-8595-1188-A57C-F55C48493275|58|22:51:30|",
          "28.11.2019|1814|303,3034595|Text W|3515|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|CD26BDC4-0368-9E8B-3147-8F9102409A49|59|16:03:40|PST",
          "11.02.2020|2515|-1094,015166|Text W|-92|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|FALSE|493246D3-2475-6778-25E9-869A07E603C3|60|23:19:29|",
          "23.12.2019|-633|4469,447608|Text W|-567|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|49A4150C-5CE3-2592-0FC3-C8CE21872C08|61|10:38:31|",
          "18.11.2019|3403|-1959,463392|Text W|-336|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|29266832-9FE8-91DB-969C-70C17E5735E9|62|05:05:33|",
          "25.11.2019|1648|3525,107923|Text W|-34|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|A4DB81AE-299F-A2C0-4E2F-8DF1F13357C6|63|02:40:42|",
          "26.04.2020|3673|3747,552877|Text W|1440|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|BE0EF0D9-8304-1755-078F-DF40D8CA04D8|64|18:34:52|PST",
          "22.02.2020|725|5463,086516|Text W|91|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|EB9FC759-6683-5A5B-2464-6D64D58632E7|65|22:39:39|",
          "10.05.2020|4190|-462,6769369|Text W|2037|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|E2D36DD8-7BAB-6887-5443-4A235A6C5DA8|66|06:34:49|",
          "22.04.2020|4042|6316,735827|Text W|1626|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|F8A24F67-5E11-479C-169B-304560055D81|67|09:16:26|PST",
          "30.12.2019|3864|1105,45264|Text W|4435|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|BDE8578D-3AE7-59AE-9D56-1ABA255A218B|68|02:11:49|",
          "10.05.2020|1998|961,1318844|Text W|727|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|0F6DA8A3-861D-395B-A24E-11A378708070|69|17:20:46|",
          "22.02.2020|4927|2811,56184|Text W|-867|Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|5540E899-73AC-0D20-02DB-AB8443E22586|70|02:23:52|"
        });

      var badLines = new List<string>(
        new[]
        {
          "17.10.2014|-29015|28411.75|Text|F|30853.25|Memo: A long text, multiple words 6|TRUE|781670F6-81CA-4236-ADC4-C9203631B5C9|18|15:03:29|PST",
          "19.03.2014|6448|-34.45|Text L|-12491.85|Memo: A long text, | multiple words 11|FALSE|CD93A774-04F7-43F3-8928-80C08B6C59C2|25|21:57:25|",
          "03.04.2014|-9056|-11148.96|Text | R|3082.31|Memo: A long text|\nmultiple words 17|FALSE|D70210E4-B273-42E1-9C08-26FA2D727F7E|34|09:35:49|EST",
          "17.12.2014|-22360|23429.85|Text V||Memo: A long text, multiple words 21|FALSE|F9642041-D4B6-4965-BDA5-0BFBAF0DC7BF|40|03:32:05||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||",
          "27.04.2020|70|-2930,330254|Text W|535||Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit|TRUE|8DDBB845-7EBF-95CB-2D09-1F80664B66DA|46|13:45:29|"
        });

      var numColumns = goodLines[0].Split('|').Length;
      var test = new ReAlignColumns(numColumns);
      foreach (var line in goodLines)
        test.AddRow(line.Split('|'));
      var col = -1;
      Action<int, string> handle = (i, s) =>
      {
        col = i;
      };

      var result1 = test.RealignColumn(badLines[0].Split('|'), handle, badLines[0]);
      Assert.AreEqual("Text|F", result1[3], "Line 1");
      Assert.AreEqual(3, col);

      var result2 = test.RealignColumn(badLines[1].Split('|'), handle, badLines[1]);
      Assert.AreEqual("Memo: A long text, | multiple words 11", result2[5], "Line 2");

      var result3 = test.RealignColumn(badLines[2].Split('|'), handle, badLines[2]);
      Assert.AreEqual("Text | R", result3[3], "Line 3");
      Assert.AreEqual("Memo: A long text|\nmultiple words 17", result3[5], "Line 3");

      var result4 = test.RealignColumn(badLines[3].Split('|'), handle, badLines[3]);
      Assert.AreEqual(numColumns, result4.Length, "Line 4 - Lots of training columns");

      var result5 = test.RealignColumn(badLines[4].Split('|'), handle, badLines[4]);
      Assert.AreEqual("TRUE", result5[6], "Line 5 - Empty COlumn");
    }
  }
}