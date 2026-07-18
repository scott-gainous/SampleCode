using System;
using System.Data;

namespace ContosoOrders.Web.Code.Dal
{
    public class DashboardSummary
    {
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OpenOrderCount { get; set; }
        public decimal RevenueToDate { get; set; }
    }

    public static class DashboardDal
    {
        public static DashboardSummary GetSummary()
        {
            DataTable table = SqlHelper.ExecuteDataTable("dbo.usp_Dashboard_Summary");
            DataRow row = table.Rows[0];
            DashboardSummary summary = new DashboardSummary();
            summary.CustomerCount = (int)row["CustomerCount"];
            summary.ProductCount = (int)row["ProductCount"];
            summary.OpenOrderCount = (int)row["OpenOrderCount"];
            summary.RevenueToDate = (decimal)row["RevenueToDate"];
            return summary;
        }
    }
}
