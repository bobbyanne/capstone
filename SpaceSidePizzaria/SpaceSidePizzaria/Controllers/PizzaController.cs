using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpaceSidePizzariaDAL;
using SpaceSidePizzariaDAL.Models;
using SpaceSidePizzaria.Models;
using System.Configuration;
using System.IO;
using SpaceSidePizzaria.Custom;
using SpaceSidePizzariaBLL;
using SpaceSidePizzariaBLL.Models;

namespace SpaceSidePizzaria.Custom
{
    public class PizzaController : CustomController
    {
        private readonly PizzaDAO _pizzaDAO;
        private readonly PizzaBLO _pizzaBLO;
        private readonly OrderDAO _orderDAO;

        public PizzaController()
        {
            string connection = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;

            _pizzaDAO = new PizzaDAO(connection);
            _pizzaBLO = new PizzaBLO();
            _orderDAO = new OrderDAO(connection);
        }

        /******* HELPER METHODS *******/

        private PizzaPO PizzaWithSelectItemsFilled()
        {
            PizzaPO pizzaPO = new PizzaPO();

            pizzaPO.CrustSelectListItems = new List<SelectListItem>();
            pizzaPO.CrustSelectListItems.Add(new SelectListItem { Text = "Select a Crust Type", Value = null, Disabled = true });
            pizzaPO.CrustSelectListItems.Add(new SelectListItem { Text = "Hand Tossed", Value = "Hand Tossed" });

            pizzaPO.SizeSelectListItems = new List<SelectListItem>();
            pizzaPO.SizeSelectListItems.Add(new SelectListItem { Text = "Small 6 in.", Value = "6" });
            pizzaPO.SizeSelectListItems.Add(new SelectListItem { Text = "Medium 8 in.", Value = "8" });

            pizzaPO.ToppingsSelectListItems = new List<SelectListItem>();
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Select Toppings", Disabled = true });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Pepperoni", Value = "Pepperoni" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Sausage", Value = "Sausage" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Bacon", Value = "Bacon" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Mushroom", Value = "Mushroom" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Onion", Value = "Onion" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Pineapple", Value = "Pineapple" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Spinach", Value = "Spinach" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Jalapeno", Value = "Jalapeno" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Tomato", Value = "Tomato" });

            pizzaPO.SauceSelectListItems = new List<SelectListItem>();
            pizzaPO.SauceSelectListItems.Add(new SelectListItem { Text = "Tomato Sauce", Value = "Tomato" });
            pizzaPO.SauceSelectListItems.Add(new SelectListItem { Text = "Barbecue sauce", Value = "Barbecue" });
            pizzaPO.SauceSelectListItems.Add(new SelectListItem { Text = "Hummus", Value = "Hummus" });

            return pizzaPO;
        }

        private void FillPizzaSelectItems(PizzaPO pizzaPO)
        {
            pizzaPO.CrustSelectListItems = new List<SelectListItem>();
            pizzaPO.CrustSelectListItems.Add(new SelectListItem { Text = "Select a Crust Type", Value = null, Disabled = true });
            pizzaPO.CrustSelectListItems.Add(new SelectListItem { Text = "Hand Tossed", Value = "Hand Tossed" });

            pizzaPO.SizeSelectListItems = new List<SelectListItem>();
            pizzaPO.SizeSelectListItems.Add(new SelectListItem { Text = "Small 6 in.", Value = "6" });
            pizzaPO.SizeSelectListItems.Add(new SelectListItem { Text = "Medium 8 in.", Value = "8" });

            pizzaPO.ToppingsSelectListItems = new List<SelectListItem>();
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Select Toppings", Disabled = true });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Pepperoni", Value = "Pepperoni" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Sausage", Value = "Sausage" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Bacon", Value = "Bacon" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Mushroom", Value = "Mushroom" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Onion", Value = "Onion" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Pineapple", Value = "Pineapple" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Spinach", Value = "Spinach" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Jalapeno", Value = "Jalapeno" });
            pizzaPO.ToppingsSelectListItems.Add(new SelectListItem { Text = "Tomato", Value = "Tomato" });

            pizzaPO.SauceSelectListItems = new List<SelectListItem>();
            pizzaPO.SauceSelectListItems.Add(new SelectListItem { Text = "Tomato Sauce", Value = "Tomato" });
            pizzaPO.SauceSelectListItems.Add(new SelectListItem { Text = "Barbecue sauce", Value = "Barbecue" });
            pizzaPO.SauceSelectListItems.Add(new SelectListItem { Text = "Hummus", Value = "Hummus" });
        }

        private List<List<PizzaPO>> GeneratePizzaMatrix(List<PizzaPO> pizzaList)
        {
            List<List<PizzaPO>> pizzaMatrix = new List<List<PizzaPO>>();

            // Create a matrix of PizzaPOs for responive design.
            // Max of 3 "cards" in a row.
            for (int i = 0; i < (float)pizzaList.Count / 3; i++)
            {
                pizzaMatrix.Add(pizzaList.Skip(i * 3).Take(3).ToList());
            }

            return pizzaMatrix;
        }

        private void SetSafePizzaValues(PizzaPO pizzaPO, bool resetDescription = true)
        {
            pizzaPO.ImagePath = null;

            if (resetDescription)
            {
                pizzaPO.Description = null;
            }

            // The OrderID will be set later when the user creates an order.
            pizzaPO.OrderID = null;
        }

        /******************************/

        // PIZZA SHOP
        [HttpGet]
        public ActionResult Index()
        {
            ActionResult response = null;

            try
            {
                List<PizzaPO> prefabPizzas =
                    Mapping
                        .PizzaMapper
                        .PizzaDOListToPizzaPOList(_pizzaDAO.GetAllPrefabPizzas());

                response = View(GeneratePizzaMatrix(prefabPizzas));
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
        [SessionRoleFilter("Role", 1)]
        public ActionResult CreatePrefabPizza()
        {
            PizzaPO pizzaPO = PizzaWithSelectItemsFilled();

            return View(pizzaPO);
        }

        [HttpPost]
        [SessionRoleFilter("Role", 1)]
        public ActionResult CreatePrefabPizza(PizzaPO form)
        {
            ActionResult response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    string imagesPath = "/Content/Images/";
                    form.Price = form.Price < 4.99M ? 4.99M : form.Price;

                    if (!System.IO.File.Exists(Server.MapPath("~/") + form.ImagePath))
                    {
                        form.ImagePath = imagesPath + "NoImageAvailable.png";
                    }

                    _pizzaDAO.AddNewPrefabPizza(Mapping.PizzaMapper.PizzaPOtoPizzaDO(form));
                }
                else
                {
                    FillPizzaSelectItems(form);
                    response = View(form);
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
        [SessionRoleFilter("Role", 1)]
        public ActionResult PrefabPizzas()
        {
            ActionResult response = null;

            try
            {
                List<PizzaPO> prefabPizzas =
                    Mapping.PizzaMapper
                        .PizzaDOListToPizzaPOList(_pizzaDAO.GetAllPrefabPizzas());


                response = View(GeneratePizzaMatrix(prefabPizzas));
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
        [SessionRoleFilter("Role", 1)]
        public ActionResult UpdatePrefabPizza(long ID)
        {
            ActionResult response = null;

            try
            {
                PizzaDO pizzaDO = _pizzaDAO.ViewPizzaByID(ID);

                if (pizzaDO != null) // If a pizza by that Id exists.
                {
                    PizzaPO pizzaPO = Mapping.PizzaMapper.PizzaDOtoPizzaPO(pizzaDO);
                    FillPizzaSelectItems(pizzaPO);

                    if (pizzaPO.OrderID == null)
                    {
                        pizzaPO.Description = null;
                        response = View(pizzaPO);
                    }
                    else // It's not a prefab pizza.
                    {
                        // TODO: Show admin the messup. Also log this.
                    }
                }
                else
                {
                    RedirectingPage("The product with ID " + ID + " doesn't exist.", "");
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

        [HttpPost]
        [SessionRoleFilter("Role", 1)]
        public ActionResult UpdatePrefabPizza(PizzaPO form)
        {
            ActionResult response = null;

            try
            {
                PizzaDO pizzaDO = _pizzaDAO.ViewPizzaByID(form.PizzaID);

                if (pizzaDO != null) // That pizza exists
                {
                    PizzaPO pizzaPO = Mapping.PizzaMapper.PizzaDOtoPizzaPO(pizzaDO);

                    if (pizzaPO.OrderID == null) // If this pizza is a prefab pizza.
                    {
                        string imagesPath = "/Content/Images/";
                        form.Price = form.Price < 4.99M ? 4.99M : form.Price;

                        if (!System.IO.File.Exists(Server.MapPath("~/") + form.ImagePath))
                        {
                            form.ImagePath = imagesPath + "NoImageAvailable.png";
                        }

                        _pizzaDAO.UpdatePizza(Mapping.PizzaMapper.PizzaPOtoPizzaDO(form));

                        TempData["SuccessMessage"] = "Pizza was successfully updated.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not update pizza.";
                }

                response = RedirectToAction("PrefabPizzas", "Pizza");
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
        [SessionRoleFilter("Role", 1)]
        public ActionResult DeletePrefabPizza(long ID)
        {
            ActionResult response = null;

            try
            {
                PizzaDO pizzaDO = _pizzaDAO.ViewPizzaByID(ID);

                if (pizzaDO != null) // If that pizza exists
                {
                    PizzaPO existingPizza = Mapping.PizzaMapper.PizzaDOtoPizzaPO(pizzaDO);

                    if (existingPizza.OrderID == null)  // If the pizza is in fact a prefab
                    {
                        _pizzaDAO.DeletePizza(ID);
                        TempData["SuccessMessage"] = "Pizza was successfully deleted";
                        response = RedirectToAction("PrefabPizzas", "Pizza");
                    }
                    else
                    {
                        response = RedirectingPage("That pizza is not a prefab.", "../PrefabPizzas");
                    }
                }
                else
                {
                    response = RedirectToAction("That pizza doesn't exist.", "../PrefabPizzas");
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
        public ActionResult CreatePizza()
        {
            return View(PizzaWithSelectItemsFilled());
        }

        [HttpPost]
        public ActionResult CreatePizza(PizzaPO form)
        {
            ActionResult response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    SetSafePizzaValues(form);

                    // Send the pizza to the business layer for price calculation.
                    form.Price = _pizzaBLO.GetPizzaCost(Mapping.PizzaMapper.PizzaPOtoPizzaBO(form));

                    (Session["Cart"] as List<PizzaPO>).Add(form);

                    response = RedirectToAction("Index", "Cart");
                }
                else
                {
                    TempData["ErrorMessage"] = "Please fix the problems shown below";
                    FillPizzaSelectItems(form);
                    response = View(form);
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
        public ActionResult UpdatePizzaInCart(int index)
        {
            ActionResult response = null;

            try
            {
                List<PizzaPO> cart = (Session["Cart"] as List<PizzaPO>);

                if (index < 0 || index >= cart.Count)
                {
                    RedirectingPage("Couldn't find that Item in the Cart.", "../../Cart");
                }
                else
                {
                    FillPizzaSelectItems(cart[index]);

                    response = View(cart[index]);
                    Session["CartIndex"] = index;
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

        [HttpPost]
        public ActionResult UpdatePizzaInCart(PizzaPO form)
        {
            ActionResult response = null;

            try
            {
                if (ModelState.IsValid)
                {
                    if (Session["CartIndex"] == null)
                    {
                        TempData["ErrorMessage"] = "An a problem occured will updating your pizza, try again.";
                        response = RedirectToAction("Index", "Cart");
                    }
                    else
                    {
                        int index = (int)Session["CartIndex"];
                        Session.Remove("CartIndex");

                        SetSafePizzaValues(form);

                        // Send the pizza to the business layer for the price calculation.
                        form.Price = _pizzaBLO.GetPizzaCost(Mapping.PizzaMapper.PizzaPOtoPizzaBO(form));

                        (Session["Cart"] as List<PizzaPO>)[index] = form;

                        response = RedirectToAction("Index", "Cart");
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
        [SessionRoleFilter("Role", 1, 2, 3)]
        public ActionResult UpdatePizzaInOrder(long ID)
        {
            ActionResult response = null;

            try
            {
                PizzaDO pizzaDOtoUpdate = _pizzaDAO.ViewPizzaByID(ID);

                if (pizzaDOtoUpdate != null)
                {
                    // This pizza exists in the database.

                    OrderDO pizzaOrderDO = _orderDAO.GetOrderByID((long)pizzaDOtoUpdate.OrderID);

                    if (pizzaOrderDO.UserID == GetSessionUserID() || GetSessionRole() == 1)
                    {
                        // The user is associated with this pizza OR the admin is trying to update the pizza.
                        PizzaPO pizzaPOtoUpdate = Mapping.PizzaMapper.PizzaDOtoPizzaPO(pizzaDOtoUpdate);

                        FillPizzaSelectItems(pizzaPOtoUpdate);

                        response = View(pizzaPOtoUpdate);
                    }
                    else
                    {
                        // A regular user tried to update someone elses pizza.
                        Logger.Log("WARNING", "PizzaController", "UpdatePizzaInOrder",
                            "UserID: " + GetSessionUserID() + " tried to update someone else's pizza.");

                        response = RedirectToAction("MyOrders", "Order");
                    }
                }
                else
                {
                    // Pizza doesn't exist.
                    if (GetSessionRole() == 1) // If the admin is using 
                    {
                        TempData["ErrorMessage"] = "That doesn't exist.";
                        RedirectToAction("ViewPendingOrders", "Order");
                    }
                    else
                    {
                        response = RedirectToAction("MyOrders", "Order");
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

        [HttpPost]
        [SessionRoleFilter("Role", 1, 2, 3)]
        public ActionResult UpdatePizzaInOrder(PizzaPO form)
        {
            // Give response a default value.
            ActionResult response = RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                OrderDO pizzasOrder = _orderDAO.GetOrderByID((long)form.OrderID);
                if (pizzasOrder != null) // If that order exists
                {
                    // Check if the pizza form is associated with this user OR if the user is an admin
                    if (pizzasOrder.UserID == GetSessionUserID() || GetSessionRole() == 1)
                    {
                        // Get the new price for the pizza.
                        form.Price = _pizzaBLO.GetPizzaCost(Mapping.PizzaMapper.PizzaPOtoPizzaBO(form));

                        if (_pizzaDAO.UpdatePizza(Mapping.PizzaMapper.PizzaPOtoPizzaDO(form)) > 0)
                        {
                            // If the pizza was able to update then try to update the Order.

                            // First get all the pizzas associated with this order.
                            List<PizzaDO> pizzas = _pizzaDAO.GetPizzasByOrderID((long)form.OrderID);

                            // Get the total cost for the pizzas that are linked to the orderID
                            decimal newTotal = _pizzaBLO.GetCostOfPizzas(Mapping.PizzaMapper.PizzaDOListToPizzaBOList(pizzas));

                            // Update the orders total cost.
                            if (_orderDAO.UpdateOrderTotal((long)form.OrderID, newTotal)) // If updated the price
                            {
                                response = RedirectToAction("OrderDetails", "Order", new { ID = form.OrderID });
                            }
                            else // Otherwise the order is now out of sync
                            {
                                Logger.Log("WARNING", "PizzaController", "UpdatePizzaInOrder",
                                    "After trying to update a pizza in orderID: " + form.OrderID +
                                    " the total was not updated.");
                            }
                        }
                        else // Otherwise the pizza couldn't update.
                        {
                            TempData["ErrorMessage"] = "Could not update the pizza, please try again later.";
                            response = RedirectToAction("OrderDetails", "Order", new { ID = form.OrderID });
                        }
                    }
                    else // Otherwise the user shouldn't be trying to change this order.
                    {
                        Logger.Log("WARNING", "PizzaController", "UpdatePizzaInOrder",
                            "UserID: " + GetSessionUserID() + " tried to update someone elses pizza.");
                    }
                }
                else
                {
                    // That pizza doesn't exist.
                    TempData["ErrorMessage"] = "That pizza doesn't exist.";
                    response = RedirectToAction("OrderDetails", "Order", new { ID = form.OrderID });
                }
            }
            else
            {
                // The form is not valid.
                TempData["ErrorMessage"] = "Please fix the errors shown below.";
                FillPizzaSelectItems(form);

                response = View(form);
            }

            return response;

        }
    }
}