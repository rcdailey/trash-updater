// using System;
// using System.Collections.Generic;
// using System.IO;
// using Common;
// using FluentAssertions;
// using NSubstitute;
// using NUnit.Framework;
// using Serilog;
// using Trash.Radarr;
// using Trash.Radarr.CustomFormat;
// using Trash.Radarr.CustomFormat.Guide;
//
// namespace Trash.Tests.Radarr.CustomFormat
// {
//     [TestFixture]
//     [Parallelizable(ParallelScope.All)]
//     public class RadarrCustomFormatUpdaterTest
//     {
//         private class Context
//         {
//             public ResourceDataReader ResourceData { get; } =
//                 new(typeof(RadarrCustomFormatUpdaterTest), "Data");
//         }
//
//         [Test]
//         public void ParseMarkdown_Preview_CorrectBehavior()
//         {
//             var context = new Context();
//
//             var testJsonList = new List<string>
//             {
//                 context.ResourceData.ReadData("ImportableCustomFormat1.json"),
//                 context.ResourceData.ReadData("ImportableCustomFormat2.json")
//             };
//
//             var logger = Substitute.For<ILogger>();
//             var guideParser = Substitute.For<ICustomFormatGuideParser>();
//             var updater = new CustomFormatUpdater(logger, guideParser);
//
//             guideParser.ParseMarkdown(Arg.Any<string>()).Returns(testJsonList);
//
//             var args = Substitute.For<IRadarrCommand>();
//             args.Preview.Returns(true);
//             var config = new RadarrConfiguration();
//
//             var output = new StringWriter();
//             Console.SetOut(output);
//
//             updater.Process(args, config);
//
//             var expectedOutput = new List<string>
//             {
//                 // language=regex
//                 @"Surround Sound\s+43bb5f09c79641e7a22e48d440bd8868",
//                 // language=regex
//                 @"DTS-HD/DTS:X\s+4eb3c272d48db8ab43c2c85283b69744"
//             };
//
//             foreach (var expectedLine in expectedOutput)
//             {
//                 output.ToString().Should().MatchRegex(expectedLine);
//             }
//         }
//     }
// }
