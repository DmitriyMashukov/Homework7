using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOtusProject
{
    internal interface IToDoReportService
    {
        (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId);
    }
}
