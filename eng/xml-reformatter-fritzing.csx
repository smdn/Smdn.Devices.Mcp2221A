#!/usr/bin/env dotnet-script
//
// requirements:
//   .NET Core or .NET Runtime
//
// usage:
//   1. install dotnet-script
//     dotnet tool install -g dotnet-script
//   2. run this script
//     ./xml-reformatter-fritzing.csx a.xml b.xml
//     dotnet script ./xml-reformatter-fritzing.csx a.xml b.xml

using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;

var settings = new XmlWriterSettings() {
  Encoding = new UTF8Encoding(false),
  Indent = true,
  IndentChars = " ",
  NewLineChars = "\n",
};

foreach (var file in Args) {
  Console.Write($"reformatting {file} ... ");

  var doc = XDocument.Load(file);

  // set background color
  const string styleBackgroundColor = "background-color: white;";
  var attrStyle = doc.Root.Attribute("style");

  doc.Root.Add(
    new XAttribute("style", styleBackgroundColor)
  );

  // add mergin
  const double viewBoxMargin = 8.0;
  var attrViewBox = doc.Root.Attribute("viewBox");
  var viewBox = attrViewBox.Value.Split(" ").Select(double.Parse).ToArray();

  attrViewBox.SetValue($"{viewBox[0] - viewBoxMargin} {viewBox[1] - viewBoxMargin} {viewBox[2] + viewBoxMargin * 2.0} {viewBox[3] + viewBoxMargin * 2.0}");

  using (var writer = XmlWriter.Create(file, settings)) {
    doc.Save(writer);
  }

  Console.WriteLine("done");
}