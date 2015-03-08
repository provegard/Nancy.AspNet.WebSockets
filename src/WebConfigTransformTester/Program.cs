using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Web.XmlTransform;
using Microsoft.XmlDiffPatch;

namespace WebConfigTransformTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: program.exe <web config XML> <transformation XDT> <expected XML>");
            }
            var webConfigXml = File.ReadAllText(args[0]);
            var transformXml = File.ReadAllText(args[1]);

            var expectedDoc = new XmlDocument();
            expectedDoc.Load(args[2]);
            using (var document = new XmlTransformableDocument())
            {
                document.PreserveWhitespace = true;
                document.LoadXml(webConfigXml);

                using (var transform = new XmlTransformation(transformXml, false, null))
                {
                    if (transform.Apply(document))
                    {
                        var xmlWriterSettings = new XmlWriterSettings();
                        xmlWriterSettings.Indent = true;
                        xmlWriterSettings.IndentChars = "    ";

                        var diffBuilder = new StringBuilder();
                        bool bIdentical;
                        using (var diffgramWriter = XmlWriter.Create(diffBuilder, xmlWriterSettings))
                        {
                            var xmldiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder |
                                                      XmlDiffOptions.IgnoreNamespaces |
                                                      XmlDiffOptions.IgnorePrefixes);
                            bIdentical = xmldiff.Compare(expectedDoc.DocumentElement, document.DocumentElement,
                                diffgramWriter);
                        }
                        if (!bIdentical)
                        {
                            Console.Error.WriteLine("Actual differs from expected. Differences are:");
                            Console.Error.WriteLine(diffBuilder.ToString());
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Transformation failed for unkown reason");
                        Environment.Exit(2);
                    }
                }
            }
        }
    }
}
