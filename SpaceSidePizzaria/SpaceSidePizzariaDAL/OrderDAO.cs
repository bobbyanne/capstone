using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SpaceSidePizzariaDAL.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceSidePizzariaDAL.Models;

namespace SpaceSidePizzariaDAL
{
    public class OrderDAO
    {
        private readonly string _connectionString;

        public OrderDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Inserts a new order in the DB and returns the primary key of the newly
        /// created order.
        /// </summary>
        public long CreateOrder(OrderDO newOrder)
        {
            long incrementedID = -1;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("CREATE_ORDER", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@UserID", newOrder.UserID);
                sqlCommand.Parameters.AddWithValue("@Status", newOrder.Status);
                sqlCommand.Parameters.AddWithValue("@IsDelivery", newOrder.IsDelivery);
                sqlCommand.Parameters.AddWithValue("@OrderDate", newOrder.OrderDate);
                sqlCommand.Parameters.AddWithValue("@Paid", newOrder.Paid);
                sqlCommand.Parameters.AddWithValue("@Total", newOrder.Total);

                sqlConnection.Open();

                incrementedID = long.Parse(sqlCommand.ExecuteScalar().ToString());
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return incrementedID;
        }

        /// <summary>
        /// Gets a OrderDo by an ID.
        /// </summary>
        public OrderDO GetOrderByID(long orderID)
        {
            OrderDO orderDO = null;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ORDER_BY_ORDERID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                if (orderTable.Rows.Count > 0)
                {
                    orderDO = OrderDataTableMapping.DataRowToOrderDO(orderTable.Rows[0]);
                }
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderDO;
        }

        /// <summary>
        /// Gets every order in the DB.
        /// </summary>
        public List<OrderDO> GetAllOrders()
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ALL_ORDERS", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets any order that has the "status" value passed in to this method.
        /// </summary>
        public List<OrderDO> GetOrdersByStatus(string status)
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ORDERS_BY_STATUS", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@Status", status);

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every delivery order in the DB.
        /// </summary>
        public List<OrderDO> GetDeliveryOrders()
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ALL_DELIVERY_ORDERS", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every delivery order in the DB with the status of "Ready For Delivery".
        /// </summary>
        public List<OrderDO> GetReadyDeliveryOrders()
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_READY_DELIVERY_ORDERS", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every delivery order for a specific driver.
        /// </summary>
        public List<OrderDO> GetEnRouteDeliveryOrders(long driverID)
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ENROUTE_ORDERS_BY_DRIVERID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@DriverID", driverID);

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every order that a user has in the DB.
        /// </summary>
        public List<OrderDO> GetOrdersByUserID(long userID)
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ORDERS_BY_USERID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@UserID", userID);

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every order that a user has in the DB with a specific status.
        /// </summary>
        public List<OrderDO> GetOrdersByUserID(long userID, string status)
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ORDERS_BY_STATUS_AND_USERID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@Status", status);
                sqlCommand.Parameters.AddWithValue("@UserID", userID);

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every pending order in the DB.
        /// </summary>
        public List<OrderDO> GetPendingOrders()
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_ALL_PENDING_ORDERS", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Gets every pending order for in the DB for a specific user.
        /// </summary>
        public List<OrderDO> GetPendingOrders(long userID)
        {
            List<OrderDO> orderList = new List<OrderDO>();

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;
            SqlDataAdapter adapter = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("OBTAIN_PENDING_ORDERS_BY_USERID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@UserID", userID);

                adapter = new SqlDataAdapter(sqlCommand);

                DataTable orderTable = new DataTable();

                sqlConnection.Open();

                adapter.Fill(orderTable);

                orderList = OrderDataTableMapping.DataTableToOrderDOs(orderTable);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand != null)
                {
                    sqlCommand.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return orderList;
        }

        /// <summary>
        /// Updates the "DriverID" and and the "Status" columns in the database.
        /// </summary>
        public bool RemoveDeliveryFromDriver(long orderID)
        {
            bool success = false;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("REMOVE_DELIVERY_FROM_DRIVER", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);

                sqlConnection.Open();

                success = sqlCommand.ExecuteNonQuery() > 0;
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return success;
        }

        /// <summary>
        /// Updates the "Status" column on a order to "Complete".
        /// </summary>
        public bool CompleteOrder(long orderID)
        {
            bool success = false;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("COMPLETE_ORDER", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);

                sqlConnection.Open();

                success = sqlCommand.ExecuteNonQuery() > 0;
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return success;
        }

        /// <summary>
        /// Updates an order's "Total" column to a new value.
        /// </summary>
        public bool UpdateOrderTotal(long orderID, decimal newTotal)
        {
            bool success = false;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("UPDATE_ORDER_TOTAL_BY_ORDERID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);
                sqlCommand.Parameters.AddWithValue("@Total", newTotal);

                sqlConnection.Open();

                success = sqlCommand.ExecuteNonQuery() == 1;
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return success;
        }

        /// <summary>
        /// Updates an order's status to the supplied status.
        /// </summary>
        public bool UpdateOrderStatus(long orderID, string status)
        {
            bool success = false;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("UPDATE_ORDER_STATUS", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);
                sqlCommand.Parameters.AddWithValue("@Status", status);

                sqlConnection.Open();

                success = sqlCommand.ExecuteNonQuery() == 1;
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return success;
        }

        /// <summary>
        /// Attempts to delete an order in the DB.
        /// </summary>
        public int DeleteOrder(long orderID)
        {
            int rowsAffected = 0;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("DELETE_ORDER_BY_ID", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);

                sqlConnection.Open();

                rowsAffected = sqlCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Logger.Log("INFO", "OrderDAO", "DeleteOrder",
                        "Deleted Order #" + orderID + " Rows Affected: " + rowsAffected);
                }
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Updates the "DriverID", "Status" columns.
        /// </summary>
        public bool AssignDriverToOrder(long orderID, long driverID)
        {
            bool success = false;

            SqlConnection sqlConnection = null;
            SqlCommand sqlCommand = null;

            try
            {
                sqlConnection = new SqlConnection(_connectionString);

                sqlCommand = new SqlCommand("ASSIGN_DRIVER_TO_ORDER", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@OrderID", orderID);
                sqlCommand.Parameters.AddWithValue("@DriverID", driverID);

                sqlConnection.Open();

                success = sqlCommand.ExecuteNonQuery() == 1;
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
                throw exception;
            }
            finally
            {
                if (sqlConnection == null)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                if (sqlCommand == null)
                {
                    sqlCommand.Dispose();
                }
            }

            return success;
        }
    }
}
