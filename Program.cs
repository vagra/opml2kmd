using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace opml2kmd
{
    class Program
    {
        private static List<string> list = null;
        private static int level = 0;
        private static bool first = false;

        private static string content = "";

        private static StreamWriter sw = null;

        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine("arg[{0}]: {1}", i, args[i]);
            }

            GetOpmlFiles(args);

            foreach (string inPath in list)
            {
                var pattern = @".opml$";
                var outPath = Regex.Replace(inPath, pattern, ".md");

                Console.WriteLine(inPath);
                Console.WriteLine(outPath);

                ReadFromOpml(inPath);
                CreateMdFile(outPath);

                Parse();
                CloseMdFile();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static void GetOpmlFiles(string[] paths)
        {
            list = new List<string>();

            foreach (string path in paths)
            {
                FileAttributes attrbutes = File.GetAttributes(path);

                if (attrbutes.HasFlag(FileAttributes.Directory))
                {
                    var di = new DirectoryInfo(path);

                    var options = new EnumerationOptions();
                    options.RecurseSubdirectories = true;

                    foreach (var fi in di.EnumerateFiles("*.opml", options))
                    {
                        list.Add(fi.FullName);
                    }
                }
                else
                {
                    var fi = new FileInfo(path);

                    if (fi.Extension.Equals(".opml"))
                    {
                        list.Add(fi.FullName);
                    }
                }
            }
        }

        static void ReadFromOpml(string path)
        {
            try
            {
                content = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                throw (e);
            }

            content = content.Replace("&nbsp;", " ");
        }

        static void CreateMdFile(string path)
        {
            try
            {
                sw = new StreamWriter(path);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        static void CloseMdFile()
        {
            sw.Flush();
            sw.Close();
        }

        static void Parse()
        {
            var doc = new XmlDocument();

            try
            {
                doc.LoadXml(content);
            }
            catch (Exception e)
            {
                throw (e);
            }

            var opml = doc.SelectSingleNode("/opml");
            if (opml == null)
            {
                throw new Exception("[ERROR]: input file does not have a 'opml' node.");
            }

            var title = opml.SelectSingleNode("//head/title");
            if (title == null)
            {
                throw new Exception("[ERROR]: input file does not have a 'title' node.");
            }

            ParseTitle(title);

            var body = opml.SelectSingleNode("//body");

            if (body == null)
            {
                throw new Exception("[ERROR]: input file does not have a 'body' node.");
            }

            ParseNode(body);
        }

        static void ParseTitle(XmlNode node)
        {
            level = 1;
            Console.WriteLine("{0}, {1}", node.Name, node.InnerText);

            string title = "#1 " + node.InnerText;

            sw.WriteLine(title);

        }


        static void ParseNode(XmlNode node)
        {
            if (node.Name.Equals("outline"))
            {
                if (first)
                {
                    level = 2;
                    first = false;
                }

                for (int i = 0; i < level; i++)
                {
                    Console.Write("  ");
                }

                Console.WriteLine("{0}, {1}, {2}", level, node.Name, node.Attributes["text"].Value);

                if (node.HasChildNodes)
                {
                    sw.WriteLine("#{0} {1}", level, node.Attributes["text"].Value);
                    sw.WriteLine();
                }
                else
                {
                    sw.WriteLine("{0}", node.Attributes["text"].Value);
                    sw.WriteLine();
                }
            }

            level++;
            foreach (XmlNode child in node.ChildNodes)
            {
                ParseNode(child);
            }
            level--;
        }

    }
}
