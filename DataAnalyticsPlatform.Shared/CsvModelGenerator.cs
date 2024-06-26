using DataAnalyticsPlatform.Shared.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataAnalyticsPlatform.Shared
{

    public class CsvModelGenerator
    {
        private CodeDomProvider provider;
        public CsvModelGenerator()
        {
            provider = CodeDomProvider.CreateProvider("C#");
        }

        //public Task<string> x = Task<string>.Factory.StartNew( (file_name)=>
        //{
        //    return ClassGenerator(file_name.ToString());
        //});
        public bool CheckDateTime(string dateString)
        {
            string[] formats = { "M/dd/yy", "MM/dd/yy", "MM/dd/yyyy", "MM/d/yy", "M/d/yy" };
            DateTime parsedDateTime;

            return DateTime.TryParseExact(dateString, formats, null,
                                           DateTimeStyles.None, out parsedDateTime);
        }
        public string CheckandGetName(string name)
        {
            if (!provider.IsValidIdentifier(name))
            {
                return name + "0";
            }
            return name;
        }
        public string ClassGenerator(string filePath, ref string className, string delimiter = ",", string classAttribute = "", string propertyAttribute = "", CsvReaderConfiguration csvReaderConfiguration = null)
        {

            if (string.IsNullOrWhiteSpace(propertyAttribute) == false)
                propertyAttribute += "\n\t";
            if (string.IsNullOrWhiteSpace(propertyAttribute) == false)
                classAttribute += "\n";

            char _delimiter = ',';
            if (delimiter.Contains("\\"))
            {
                _delimiter = '\t';//hack fo rtab
            }
            else
            {
                _delimiter = delimiter.ToCharArray()[0];
            }
            List<string> lines = (List<string>)File.ReadLines(filePath).Skip((int)csvReaderConfiguration?.skipLines).Take(10).ToList();
            string[] columnNames = lines.First().Split(_delimiter).Select(str => str.Trim()).ToArray();
            string[] data = lines.Skip(1).ToArray();

            className = Path.GetFileNameWithoutExtension(filePath);
            className = Regex.Replace(className, @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
            // use StringBuilder for better performance
            string code = String.Format("{0}public partial class {1} {{ \n", classAttribute, className);
            code += String.Format($"public {className}()");
            code += "{ Init();}";
            code += "partial void Init();";
            for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
            {
                var col = columnNames[columnIndex];
                bool dateBool = CheckDateTime(col);
                string columnName = "";
                if (!dateBool)
                {
                    columnName = Regex.Replace(columnNames[columnIndex], @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
                    columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());
                }
                else
                {
                    columnName = "day" + col.Replace("/", "_");
                }
                columnName = CheckandGetName(columnName);
                if (string.IsNullOrEmpty(columnName))
                    columnName = "Column" + (columnIndex + 1);
                if (Char.IsDigit(columnName[0]))
                {
                    columnName = "col_" + columnName;
                }
                code += "\t" + GetVariableDeclaration(_delimiter, data, columnIndex, columnName, propertyAttribute) + "\n\n";
            }

            code += "}\n";
            return code;
        }

        public List<string> skip10Lines(string path)
        {
            int count = 0;
            List<string> lines = new List<string>();
            foreach (var line in File.ReadLines(path))
            {
                if (count == 10)
                    break;
                lines.Add(line);
                count++;
            }
            return lines;
        }
        public List<FieldInfo> GetAllFields(string filePath, ref string className, string delimiter = ",", string classAttribute = "", string propertyAttribute = "", CsvReaderConfiguration csvReaderConfiguration = null)
        {
            try
            {
                List<FieldInfo> listOfFieldInfo = new List<FieldInfo>();

                char _delimiter = ',';
                if (delimiter.Contains("\\"))
                {
                    _delimiter = '\t';//hack fo rtab
                }
                else
                {
                    _delimiter = delimiter.ToCharArray()[0];
                }
                // char _delimiter = delimiter.ToCharArray()[0];
                if (File.Exists(filePath))
                {
                    long length = new System.IO.FileInfo(filePath).Length;
                    Console.WriteLine(filePath + "exists " + " " + length + " " + (int)csvReaderConfiguration?.skipLines);
                }

                List<string> lines = skip10Lines(filePath);//(List<string>)File.ReadLines(filePath).Skip((int)csvReaderConfiguration?.skipLines).Take(10).ToList();
                if (lines == null) { Console.WriteLine("null lines"); }
                Console.WriteLine(lines.Count);
                string[] columnNames = lines.First().Split(_delimiter).Select(str => str.Trim()).ToArray();
                string[] data = lines.Skip(1).ToArray();
                Console.WriteLine("COlumnNames Length " + columnNames.Length);
                for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
                {
                    var col = columnNames[columnIndex];
                    bool dateBool = CheckDateTime(col);
                    string columnName = "";
                    if (dateBool)
                    {
                        columnName = "day" + col.Replace("/", "_");
                    }
                    else
                    {
                        columnName = Regex.Replace(columnNames[columnIndex], @"[\s\.\-]", string.Empty, RegexOptions.IgnoreCase);
                        columnName = new string(columnName.Where(c => Char.IsLetter(c) || Char.IsDigit(c) || c == '_').ToArray());

                    }

                    columnName = CheckandGetName(columnName);
                    var displayName = Regex.Replace(columnNames[columnIndex], @"[\s]", string.Empty, RegexOptions.IgnoreCase);

                    if (string.IsNullOrEmpty(columnName))
                        columnName = "Column" + (columnIndex + 1);
                    if (Char.IsDigit(columnName[0]))
                    {
                        columnName = "col_" + columnName;
                    }
                    //var dataType = GetVariableDeclaration(data, columnIndex, columnName, propertyAttribute);

                    FieldInfo fi = new FieldInfo(columnName, GetDatatype(_delimiter, data, columnIndex, columnName, propertyAttribute));
                    fi.DisplayName = displayName;
                    listOfFieldInfo.Add(fi);
                    Console.WriteLine("DisplayName " + fi.DisplayName);
                }

                return listOfFieldInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static string[] SplitCSVReg(string input)
        {
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] Fields = CSVParser.Split(input);
            return Fields;
        }
        static Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
        public static string[] SplitCSV(string input)
        {

            List<string> list = new List<string>();
            string curr = null;
            foreach (Match match in csvSplit.Matches(input))
            {
                curr = match.Value;
                if (0 == curr.Length)
                {
                    list.Add("");
                }

                list.Add(curr.TrimStart(','));
            }

            return list.ToArray();
        }

        public string GetVariableDeclaration(char delimiter, string[] data, int columnIndex, string columnName, string attribute = null)
        {
            try
            {
                //SplitCSV(data[columnIndex]);/
                string[] columnValues = data.Select(line => SplitCSVReg(line)[columnIndex].Trim()).ToArray();
                // string[] columnValues = data.Select(line => line.Split(delimiter)[columnIndex].Trim()).ToArray();  //data.Select(line => line.Split(delimiter)[columnIndex].Trim()).ToArray();
                string typeAsString;


                if (AllDoubleValues(columnValues))
                {
                    typeAsString = "double";
                }
                else if (AllIntValues(columnValues))
                {
                    typeAsString = "int";
                }

                else if (AllDateTimeValues(columnValues))
                {
                    typeAsString = "DateTime";
                }
                else
                {
                    typeAsString = "string";
                }

                string declaration = String.Format("{0}public {1} {2} {{ get; set; }}", attribute, typeAsString, columnName);
                return declaration;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public DataType GetDatatype(char delimiter, string[] data, int columnIndex, string columnName, string attribute = null)
        {
            string[] columnValues = data.Select(line => line.Split(delimiter)[columnIndex].Trim()).ToArray();
            string typeAsString;

            if (AllDoubleValues(columnValues))
            {
                return DataType.Double;
            }
            else if (AllIntValues(columnValues))
            {
                return DataType.Int;
            }

            else if (AllDateTimeValues(columnValues))
            {
                return DataType.DateTime;
            }
            else
            {
                return DataType.String;
            }
        }

        public bool AllDoubleValues(string[] values)
        {
            double d;
            return values.All(val => double.TryParse(val, out d));
        }

        public bool AllIntValues(string[] values)
        {
            int d;
            return values.All(val => int.TryParse(val, out d));
        }

        public bool AllDateTimeValues(string[] values)
        {
            DateTime d;
            return values.All(val => DateTime.TryParse(val, out d));
        }

        // add other types if you need...

    }
}
