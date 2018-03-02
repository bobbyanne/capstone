using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSidePizzariaBLL.Models;

namespace SpaceSidePizzariaBLL
{
    public class OrderBLO
    {
        // Find the user with the most orders.
        public long GetMostValuableCustomer(List<OrderBO> allOrders)
        {
            return allOrders
                    .GroupBy(orderBO => orderBO.UserID)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .First();
        }
        
        // 2.) Find the driver with the most delivery runs
        public List<string> GetMostValuableDrivers(List<OrderBO> allOrders)
        {
            return allOrders.Select(orderBO => orderBO)
                            .Where(orderBO => orderBO.DriverID != null)
                            .GroupBy(orderBO => orderBO.DriverFirstName)
                            .OrderByDescending(x => x.Count())
                            .Select(x => x.Key)
                            .Take(3)
                            .ToList();
        }

        //public DateTime GetDayOfMonthSoldMost(List<OrderBO> allOrders)
        //{
        //    return allOrders
        //        .Select(orderBO => orderBO)
        //        .Where(orderBO => orderBO.OrderDate.Month == DateTime.Now.Month)
        //        .GroupBy(orderBO => orderBO.OrderDate.Day)
        //        .OrderByDescending(group => group.Count())
        //        .Select(group => group.Key)
        //        .First();
        //}
    }
}
