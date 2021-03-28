using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static HashSet<string> NativeDataTypes { get; set; }  = new HashSet<string>() { "string", "int", "char", "short" };
    }
}
