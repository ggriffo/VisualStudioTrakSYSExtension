using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrakSYSSynchronization
{
    public class Synchronization
    {
        public List<ResultItem> GetScripts(string connectionString, int scriptGroupType, DateTime lastVSUpdate)
        {
            using (var context = new TrakSYSSynchronization.EDBEntities(connectionString))
            {
                string sqlQuery = $@"
                    SELECT sg.Name GroupName, s.[Name] ScriptName, s.Script, MAX(ad.AuditDateTime) AS MaxAuditDateTime
                        FROM tScriptGroup sg
                            JOIN tScript s ON sg.ID = s.ScriptGroupID
                            JOIN tAuditDetail ad ON ad.TableName = 'tScript' AND ad.EntityName = s.[Name]
                    WHERE s.Enabled = 1 AND sg.[Type] = {scriptGroupType}
                    GROUP BY sg.Name, s.[Name], s.Script
                    Having Max(AuditDateTime) > '{lastVSUpdate.ToString("yyyy-MM-dd HH:mm:ss")}'";

                List<ResultItem> result = context.Database.SqlQuery<ResultItem>(sqlQuery).ToList();

                return result;
            }
        }

        public class ResultItem
        {
            public string GroupName { get; set; }
            public string ScriptName { get; set; }
            public string Script { get; set; }
            public DateTimeOffset MaxAuditDateTime { get; set; }
        }
    }
}
