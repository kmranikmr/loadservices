using System.Collections.Generic;

namespace DataAnalyticsPlatform.Shared.DataAccess
{
    public static class BulkHelper
    {
        public static string GetWriteStatement(this ImporterInfo importerInfo, string schemaName)
        {
            List<string> columnNames = new List<string>();

            foreach (var item in importerInfo.Columns)
            {
                columnNames.Add(item.Item1);
            }

            return $"COPY {schemaName}.{importerInfo.TableName} ({string.Join(",", columnNames)}) FROM STDIN (FORMAT BINARY)";
        }

        public static string GetCreateTableStatement(this ImporterInfo importerInfo, string schemaName)
        {
            List<string> columnNameWithdataTypes = new List<string>();

            foreach (var item in importerInfo.Columns)
            {
                columnNameWithdataTypes.Add(string.Format(@"{0} {1}", item.Item1, item.Item2));
            }

            return string.Format("Create table if not exists " + schemaName + ".{0}({1});", importerInfo.TableName,
                              string.Join(",", columnNameWithdataTypes));
        }
    }
}
