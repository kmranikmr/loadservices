// This file defines the JsonReaderHelper class, which provides utility functions for parsing
// and extracting field information from C# class definitions in a JSON-like structure using
// the Roslyn API.
//
// Classes:
// - JsonReaderHelper: Contains methods for parsing class definitions and properties, mapping 
//   them to FieldInfo objects, and determining their data types.
//
// Methods:
// - GetFieldInfos: Parses the provided text to extract class and property definitions, returning 
//   a list of FieldInfo objects.
// - GetProps: Recursively extracts properties from a given class, mapping them to FieldInfo objects.
// - GetDataTypeFromString: Converts string representations of data types to DataType enums.
//
// Fields:
// - NativeDataTypes: A set of strings representing native data types (e.g., "string", "int").


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DataAnalyticsPlatform.Shared
{
    public static class JsonReaderHelper
    {
        public static List<FieldInfo> GetFieldInfos(string text)
        {
            List<FieldInfo> fieldInfos = new List<FieldInfo>();


            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(text);


            //var root = tree.GetRoot();

            //var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();


            var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            Dictionary<string, ClassDeclarationSyntax> classDict = new Dictionary<string, ClassDeclarationSyntax>();


            ClassDeclarationSyntax mainClass = null;

            foreach (var member in classes)
            {

                string className = member.Identifier.ValueText;

                classDict.Add(className, member);

                if (mainClass == null)
                {
                    mainClass = member;
                }

            }

            return GetProps(mainClass, classDict, FieldInfo.GetEmptyFieldInfo());

        }

        public static List<FieldInfo> GetProps(ClassDeclarationSyntax syn, Dictionary<string, ClassDeclarationSyntax> classDict, FieldInfo parentFi)
        {
            var members = syn.DescendantNodes().OfType<PropertyDeclarationSyntax>();

            List<FieldInfo> res = new List<FieldInfo>();

            foreach (PropertyDeclarationSyntax item in members)
            {
                FieldInfo fi = null;

                if (item.Type is IdentifierNameSyntax a)
                {
                    fi = new FieldInfo(item.Identifier.ValueText, DataType.Object);
                    fi.SetMap(parentFi);
                    fi.InnerFields = GetProps(classDict[a.Identifier.ValueText], classDict, fi);

                }
                else if (item.Type is ArrayTypeSyntax atx)
                {
                    if (atx.ElementType is IdentifierNameSyntax ins)
                    {
                        fi = new FieldInfo(item.Identifier.ValueText, DataType.ObjectArray);
                        fi.IsArray = true;
                        fi.SetMap(parentFi);
                        fi.InnerFields = GetProps(classDict[ins.Identifier.ValueText], classDict, fi);
                    }
                    else if (atx.ElementType is PredefinedTypeSyntax pts)
                    {
                        fi = new FieldInfo(item.Identifier.ValueText, GetDataTypeFromString(pts.Keyword.ValueText, true));
                        fi.IsArray = true;
                        fi.SetMap(parentFi);
                    }


                }
                else if (item.Type is GenericNameSyntax gns && gns.Identifier.ValueText == "Dictionary")
                {
                    fi = new FieldInfo(item.Identifier.ValueText, DataType.Dict);
                    fi.SetMap(parentFi);
                }
                else if (item.Type is PredefinedTypeSyntax b)
                {
                    fi = new FieldInfo(item.Identifier.ValueText, GetDataTypeFromString(b.Keyword.ValueText));
                    fi.SetMap(parentFi);
                }



                res.Add(fi);
            }
            return res;
        }

        public static DataType GetDataTypeFromString(string dt, bool isArray = false)
        {
            if (!isArray)
            {
                if (dt == "string")
                    return DataType.String;
                if (dt == "int")
                    return DataType.Int;
                if (dt == "char")
                    return DataType.Char;
                if (dt == "float")
                    return DataType.Double;
                if (dt == "bool")
                    return DataType.Boolean;
                if (dt == "long")
                    return DataType.Long;
            }
            else if (isArray)
            {
                if (dt == "string")
                    return DataType.StringArray;
                else
                    return DataType.ObjectArray;
            }
            return DataType.Object;
        }

        public static HashSet<string> NativeDataTypes { get; set; } = new HashSet<string>() { "string", "int", "char", "short" };
    }
}
