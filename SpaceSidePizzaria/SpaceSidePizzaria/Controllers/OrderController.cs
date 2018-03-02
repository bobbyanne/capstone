using System;
using System.Collections.Generic;
using SpaceSidePizzariaDAL;
using SpaceSidePizzaria.Models;
using SpaceSidePizzaria.Custom;
using SpaceSidePizzariaDAL.Models;
using SpaceSidePizzaria.ViewModels;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using SpaceSidePizzariaBLL.Models;
using SpaceSidePizzariaBLL;

namespace SpaceSidePizzaria.Controllers
{
    [SessionRoleFilter("Role", 1, 2, 3)]
    public class OrderController : CustomController
    {
        OrderDAO _orderDAO;
        OrderBLO _orderBLO;
        PizzaDAO _pizzaDAO;
        PizzaBLO _pizzaBLO;
        UserDAO _userDAO;

        public OrderController()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;

            _orderDAO = new OrderDAO(connectionString);
            _pizzaDAO = new PizzaDAO(connectionString);
            _userDAO = new UserDAO(connectionString);
            _pizzaBLO = new PizzaBLO();
            _orderBLO = new OrderBLO();
        }

        /************* HELPER METHODS BEGIN ***************/

        private void FillSelectListItems(OrderPO orderPO)
        {
            List<string> textValues = new List<string> { "Prepping", "Cooking", "Completed" };

            orderPO.StatusSelectListItemsPickup = new List<SelectListItem>();
            orderPO.StatusSelectListItemsDelivery = new List<SelectListItem>();

            foreach (string elm in textValues)
            {
                orderPO.StatusSelectListItemsDelivery.Add(new SelectListItem { Text = elm, Value = elm });
                orderPO.StatusSelectListItemsPickup.Add(new SelectListItem { Text = elm, Value = elm });
            }

            if (orderPO.IsDelivery)
            {
                orderPO.StatusSelectListItemsDelivery.Add(new SelectListItem { Text = "Ready For Delivery", Value = "Ready For Delivery" });
                orderPO.StatusSelectListItemsDelivery.Find(item => item.Value == orderPO.Status).Selected = true;
            }
            else
            {
                orderPO.StatusSelectListItemsPickup.Add(new SelectListItem { Text = "Ready For Pickup", Value = "Ready For Pickup" });
                orderPO.StatusSelectListItemsPickup.Find(item => item.Value == orderPO.Status).Selected = true;
            }
        }

        /************** HELPER METHODS END ****************/

        // TODO: Create an Index View
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Renders all of the pending orders for the admin.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1)]
        public ActionResult ViewPendingOrders()
        {
            ActionResult response = null;

            try
            {
                // Build a list of orderPOs to pass along to the View.
                List<OrderPO> orderPOList = new List<OrderPO>();

                foreach (OrderDO orderDO in _orderDAO.GetPendingOrders())
                {
                    OrderPO orderPO = Mapping.OrderMapper.OrderDOtoOrderPO(orderDO);

                    // If the status is not equal to "En Route" then fill the
                    // select list items so the admin can change them.
                    if (orderDO.Status != "En Route")
                    {
                        FillSelectListItems(orderPO);
                    }
                    else { /* Don't fill the select list items */ }

                    orderPOList.Add(orderPO);
                }

                // Pass in the list pending orders to the Views
                response = View(orderPOList);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
                else { /* response was assigned */ }
            }

            return response;
        }

        /// <summary>
        /// Renders the details of an order.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1, 2, 3)]
        public ActionResult OrderDetails(long ID)
        {
            ActionResult response = null;

            try
            {
                OrderDO orderDO = _orderDAO.GetOrderByID(ID);

                if (orderDO != null)  // If that order exists
                {
                    // Map the orderDO we got earlier to a orderPO
                    OrderPO orderPO = Mapping.OrderMapper.OrderDOtoOrderPO(orderDO);

                    // If this user's ID in session is not the same as the UserID on the order
                    // AND the role is 3 (User).
                    // -- Allow the Driver's and Admins to view other person's order details but
                    // -- don't allow the other users to view other user's order details.
                    if (orderPO.UserID != GetSessionUserID() && GetSessionRole() == 3)
                    {
                        response = RedirectingPage("You don't have permissions to view this page.", "../../Account/Login");
                    }
                    else
                    { 
                        // Get all the pizzas associtated with this order
                        List<PizzaPO> pizzaPOList =
                            Mapping
                            .PizzaMapper
                            .PizzaDOListToPizzaPOList(_pizzaDAO.GetPizzasByOrderID(ID));

                        // Create the view model for 1 order and a list of pizzas
                        PizzaOrderVM pizzaOrderVM = new PizzaOrderVM();
                        pizzaOrderVM.Order = orderPO;

                        pizzaOrderVM.Pizzas = pizzaPOList;

                        // Pass in the view model
                        response = View(pizzaOrderVM);
                    }
                }
                else
                {   // The order couldn't be found .
                    
                    // If the current user is an Admin then show that the order doesn't exist
                    if (GetSessionRole() == 1)
                    {
                        response = RedirectingPage("Order does not exist", "../");
                    }
                    else // Don't show anyone else that the order doesn't exist.
                    {
                        response = RedirectToAction("Home", "Index");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }

        [HttpGet]
        public ActionResult PreviousOrders()
        {
            ActionResult response = null;

            try
            {
                List<OrderPO> orderPOList = Mapping
                    .OrderMapper
                    .OrderDOsToOrderPOs(_orderDAO.GetOrdersByUserID(GetSessionUserID()));

                response = View(orderPOList);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }
        /// <summary>
        /// Finds all the orders with the the current user's ID.
        /// </summary>
        [HttpGet]
        public ActionResult MyOrders()
        {
            ActionResult response = null;

            try
            {
                // Get all of the orders for the current user.
                List<OrderPO> orderPOList =
                    Mapping
                    .OrderMapper
                    .OrderDOsToOrderPOs(_orderDAO.GetPendingOrders((GetSessionUserID())));

                // Pass the orders into the view.
                response = View(orderPOList);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }

        /// <summary>
        /// Allows the admin to change an order's status.
        /// </summary>
        [AjaxOnlyFilter]
        [HttpPost]
        [SessionRoleFilter("Role", 1)]
        public ActionResult ChangeStatus(int orderID, string status)
        {
            JsonResult jsonResult = null;

            try
            {
                // If the Admin completed an order.
                if (status == "Completed")
                {
                    if (_orderDAO.CompleteOrder(orderID))  // If the stored procedure was successfull
                    {
                        jsonResult = new JsonResult
                        {
                            Data = new { message = "Successfully completed order# " + orderID }
                        };
                    }
                    else // The stored procedure wasn't successful.
                    {
                        jsonResult = new JsonResult
                        {
                            Data = new { message = "Could not complete order." }
                        };
                    }
                }
                else  // If the status wansn't "Complete"
                {
                    if (_orderDAO.UpdateOrderStatus(orderID, status))  // If the update was successfull
                    {
                        jsonResult = new JsonResult
                        {
                            Data = new { message = "Successfully updated Order#" + orderID + " to status: " + status }
                        };
                    }
                    else  // The update was successfull
                    {
                        jsonResult = new JsonResult
                        {
                            Data = new { message = "Could not update Order" }
                        };
                    }
                }
            }
            catch (Exception exception)
            {
                jsonResult = new JsonResult
                {
                    Data = new { message = "An error occured while submitting the status, please contact IT." }
                };
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (jsonResult == null)
                {
                    jsonResult = new JsonResult
                    {
                        Data = new { message = "There was a problem submitting the status please try again." }
                    };
                }
            }

            return jsonResult;
        }

        /// <summary>
        /// Deletes a pizza form an order.
        /// </summary>
        [HttpGet]
        public ActionResult DeleteFromOrder(long ID)
        {
            ActionResult response = null;
            int rowsAffected = 0;

            try
            {
                PizzaDO pizzaDO = _pizzaDAO.ViewPizzaByID(ID);

                if (pizzaDO != null)
                {
                    PizzaPO pizzaPO = Mapping.PizzaMapper.PizzaDOtoPizzaPO(pizzaDO);

                    if (pizzaPO.OrderID == null)
                    {
                        // Thats a prefab pizza and that shouldn't be deleted from this action.
                        if (GetSessionRole() == 1)
                        {
                            TempData["ErrorMessage"] = "You must delete that pizza from this page.";
                            response = RedirectToAction("PrefabPizzas", "Pizza");
                        }
                    }
                    else
                    {
                        // Get the order that this pizza is associated with the pizza.
                        // Use this later to update the new total for the order.
                        OrderPO orderPO =
                            Mapping
                            .OrderMapper
                            .OrderDOtoOrderPO(_orderDAO.GetOrderByID((long)pizzaPO.OrderID));

                        if (GetSessionRole() == 1)
                        {
                            rowsAffected = _pizzaDAO.DeletePizza(ID);
                        }
                        else
                        {
                            // Check to make sure that the current user is associated with the pizza's order.
                            if (GetSessionUserID() == orderPO.UserID)
                            {
                                // The user is deleting their own pizza, so that's okay.
                                rowsAffected = _pizzaDAO.DeletePizza(ID);
                                response = RedirectToAction("OrderDetails", "Order", new { ID = orderPO.OrderID });
                            }
                            else
                            {
                                Logger.Log("WARNING", "PizzaController", "DeletePizza",
                                    "User #" + GetSessionUserID() + " tried to delete someone elses pizza");
                                response = RedirectingPage("You do not have enough permissions to change a customers order.", "../../");
                            }
                        }

                        if (rowsAffected > 0)
                        {
                            // Recalculate the total for the order.

                            // Get all of the pizza associated with this order
                            List<PizzaBO> pizzaBOList =
                                Mapping
                                .PizzaMapper
                                .PizzaDOListToPizzaBOList(_pizzaDAO.GetPizzasByOrderID(orderPO.OrderID));

                            // Calculate the new total
                            decimal newTotal = _pizzaBLO.GetCostOfPizzas(pizzaBOList);

                            // Update the order's total.
                            _orderDAO.UpdateOrderTotal(orderPO.OrderID, newTotal);
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "That pizza doesn't exists.";
                }
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }

        /// <summary>
        /// Delete an existing order.
        /// </summary>
        [HttpGet]
        public ActionResult DeleteOrder(long ID)
        {
            ActionResult response = null;

            try
            {
                int rowsAffected = 0;

                // Get the order the user is trying to delete.
                OrderPO orderPO = Mapping.OrderMapper.OrderDOtoOrderPO(_orderDAO.GetOrderByID(ID));

                // If an Admin is trying to delete an order or if the order is in fact the user's,
                // then delete the order.
                if (GetSessionRole() == 1 || orderPO.UserID == GetSessionUserID())
                {
                    // Delete the order and capture the number of rows affected.
                    rowsAffected = _orderDAO.DeleteOrder(ID);

                    response = RedirectingPage("Order deleted", "../../");
                }
                else
                {
                    // The user doesn't have permissions to delete this order.
                    response = RedirectingPage("You do not have permissions to delete this order.", "");
                }
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    TempData["ErrorMessage"] = "There was a problem deleting your order, " +
                        "please try again later.";
                    response = RedirectToAction("MyOrders", "Order");
                }
                else { /* The response was not null. */}
            }

            return response;
        }

        /// <summary>
        /// Find and show all of the deliveries that have the status "Ready To Deliver".
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1, 2)]
        public ActionResult ReadyDeliveries()
        {
            ActionResult response = null;

            try
            {
                // Find all the deliveries that are "Ready".
                List<OrderPO> deliveryList =
                    Mapping
                    .OrderMapper
                    .OrderDOsToOrderPOs(_orderDAO.GetReadyDeliveryOrders());

                response = View(deliveryList);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                } else { /* The response wasn't null. */ }
            }

            return response;
        }

        /// <summary>
        /// Updates a delivery order's status to "En Route" and sets the DriverID
        /// on the order to the current usersID.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1, 2)]
        public ActionResult TakeDelivery(long ID)
        {
            ActionResult response = null;

            try
            {
                // Get the order the user wants to update.
                OrderPO orderPO =
                    Mapping
                    .OrderMapper
                    .OrderDOtoOrderPO(_orderDAO.GetOrderByID(ID));

                // If this order is in fact a delivery order.
                if (orderPO.IsDelivery)
                {
                    // Assign the current user to the order's DriverID.
                    _orderDAO.AssignDriverToOrder(ID, GetSessionUserID());

                    TempData["SuccessMessage"] = "Successfully added order# " + ID + " to your current deliveries.";

                    response = RedirectToAction("ReadyDeliveries", "Order");
                }
                else  // Otherwise this order is not a delivery order.
                {
                    TempData["ErrorMessage"] = "Could not add that order to you current deliveries.";

                    Logger.Log("INFO", "OrderController", "TakeDelivery",
                        "User# " + GetSessionUserID() + " tried to checkout an order that was " +
                        "not a delivery order.");
                }
            }
            catch (Exception exception)
            {
                TempData["ErrorMessage"] = "An error occured while adding that order to your current deliveries.";
                response = RedirectToAction("ReadyDeliveries", "Order");
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }

        /// <summary>
        /// Updates a delivery order's status to "Complete".
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1, 2)]
        public ActionResult CompleteDelivery(long ID)
        {
            ActionResult response = null;

            try
            {
                // Get the order the user is wanting to delete.
                OrderDO orderDOtoComplete = _orderDAO.GetOrderByID(ID);

                if (orderDOtoComplete != null) // If the order exists.
                {
                    // Map the orderDO to an orderPO
                    OrderPO orderPOtoComplete = Mapping.OrderMapper.OrderDOtoOrderPO(orderDOtoComplete);

                    // If the order is in fact a delivery order.
                    if (orderPOtoComplete.IsDelivery)
                    {
                        // Complete the order.
                        _orderDAO.CompleteOrder(ID);

                        TempData["SuccessMessage"] = "Order Complete";
                    }
                    else // The order the user is trying to delete is not a delivery order.
                    {
                        TempData["ErrorMessage"] = "That order# " + ID + " is not a delivery order. The order may have been changed to carryout.";
                    }
                }
                else  // The order doesn't exist.
                {
                    TempData["ErrorMessage"] = "Order# " + ID + " doesn't exist.  The order may have been deleted.";
                }

                response = RedirectToAction("MyDeliveries", "Order");

                return response;
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                } else { /* The response was not null */ }
            }

            return response;
        }

        /// <summary>
        /// Views the current user's delivery orders.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1, 2)]
        public ActionResult MyDeliveries()
        {
            ActionResult response = null;

            try
            {
                // Get all of the user's delivery orders.
                List<OrderPO> orderPOList =
                    Mapping
                    .OrderMapper
                    .OrderDOsToOrderPOs(_orderDAO.GetEnRouteDeliveryOrders(GetSessionUserID()));

                // Build the google maps http request based on the address on the orders
                if (orderPOList.Count > 0)
                {
                    // Get the strores address from the config file.
                    string storeAddress = ConfigurationManager.AppSettings["storeAddress"];
                    string googleMapDirections = "https://www.google.com/maps/dir/" + storeAddress + "/";

                    // Loop through all of the orders and create an http request to google maps.
                    orderPOList
                        .Select(orderPO => orderPO.BuyerAddress)
                        .Distinct()
                        .ToList()
                        .ForEach(address => googleMapDirections += address + "/");

                    TempData["Directions"] = googleMapDirections;
                }

                response = View(orderPOList);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }

        /// <summary>
        /// Gets all of the completed orders and passes them to the view.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1)]
        public ActionResult CompletedOrders()
        {
            ActionResult response = null;

            try
            {
                // Get all of the completed orders from the database.
                List<OrderPO> completedOrders =
                    Mapping
                    .OrderMapper
                    .OrderDOsToOrderPOs(_orderDAO.GetOrdersByStatus("Completed"));

                // Pass the completed orders list to the view.
                response = View(completedOrders);
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }

        /// <summary>
        /// Removes a delivery from a drivers order.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1, 2)]
        public ActionResult RemoveDelivery(long ID)
        {
            ActionResult response = null;

            try
            {
                // Get the order the using is trying to delete.
                OrderDO orderDOtoRemove = _orderDAO.GetOrderByID(ID);

                if (orderDOtoRemove != null) // If that order exists.
                {
                    // Map the order to a OrderPO.
                    OrderPO orderPOtoRemove = Mapping.OrderMapper.OrderDOtoOrderPO(orderDOtoRemove);

                    // If the current user is the driver on the order.
                    if (orderDOtoRemove.DriverID == GetSessionUserID() || GetSessionRole() == 1)
                    {
                        _orderDAO.RemoveDeliveryFromDriver(ID);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "You MAY NOT remove an order that doesn't belong to you.";
                        Logger.Log("WARNING", "OrderController", "RemoveDelivery",
                            "User ID: " + GetSessionUserID() + " tried to delete " + orderDOtoRemove.BuyerName + "'s order");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "That order doesn't exist.";
                }

                response = RedirectToAction("MyDeliveries", "Order");
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                } else { /* response was not null */ }
            }

            return response;
        }

        /// <summary>
        /// Meaningful calculation.
        /// </summary>
        [HttpGet]
        [SessionRoleFilter("Role", 1)]
        public ActionResult Stats()
        {
            ActionResult response = null;

            try
            {
                // TODO: Do the orders need to be complete???

                // Get every single order in the DB
                List<OrderBO> allOrders =
                    Mapping
                    .OrderMapper
                    .OrderDOsToOrderBOs(_orderDAO.GetAllOrders());

                // Get the ID of the most valuable customer.
                long valuableCustomerID = _orderBLO.GetMostValuableCustomer(allOrders);

                // Find that customer in the DB and pull back the data.
                UserPO valuableCustomer = Mapping
                    .UserMapper
                    .UserDOtoUserPO(_userDAO.GetUserByID(valuableCustomerID));

                TempData["TotalOrders"] = allOrders.Count;
                TempData["ValuableCustomer"] = valuableCustomer;
                TempData["ValuableDrivers"] = _orderBLO.GetMostValuableDrivers(allOrders);

                response = View();
            }
            catch (Exception exception)
            {
                Logger.LogExceptionNoRepeats(exception);
            }
            finally
            {
                if (response == null)
                {
                    response = RedirectToAction("Index", "Home");
                }
            }

            return response;
        }
    }
}