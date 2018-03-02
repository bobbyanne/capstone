using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpaceSidePizzaria.ViewModels;
using SpaceSidePizzaria.Models;
using SpaceSidePizzariaDAL.Models;
using SpaceSidePizzariaDAL;
using SpaceSidePizzariaBLL;
using System.Configuration;
using SpaceSidePizzaria.Custom;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace SpaceSidePizzaria.Controllers
{
    public class CartController : CustomController
    {
        private readonly PizzaDAO _pizzaDAO;
        private readonly OrderDAO _orderDAO;
        private readonly UserDAO _userDAO;
        private readonly PizzaBLO _pizzaBLO;

        public CartController()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dataSource"].ConnectionString;

            _pizzaDAO = new PizzaDAO(connectionString);
            _orderDAO = new OrderDAO(connectionString);
            _userDAO = new UserDAO(connectionString);
            _pizzaBLO = new PizzaBLO();
        }

        // View items in the cart.
        [HttpGet]
        public ActionResult Index()
        {
            List<PizzaPO> cart = (Session["Cart"] as List<PizzaPO>);

            PaymentPO paymentPO = new PaymentPO();
            paymentPO.Total = _pizzaBLO.GetCostOfPizzas(Mapping.PizzaMapper.PizzaPOListToPizzaBOList(cart));

            CartPaymentVM cartPaymentVM = new CartPaymentVM(cart, paymentPO);

            return View(cartPaymentVM);
        }

        [HttpGet]
        [Route("{long ID}")]
        public ActionResult AddPizzaToCart(long ID)
        {
            ActionResult response = null;

            try
            {
                PizzaDO pizzaDO = _pizzaDAO.ViewPizzaByID(ID);

                if (pizzaDO == null) //  If that prefab doesn't exist...
                {
                    // redirect to home.
                    response = RedirectToAction("Index", "Home");
                }
                else
                {
                    // First check if this pizza is actually a prefab pizza created
                    // by the Admin.  Prefabs won't have an OrderID.
                    if (pizzaDO.OrderID != null)
                    {
                        response = RedirectToAction("Index", "Pizza");
                    }
                    else
                    {
                        // Add the pizza to the cart.
                        List<PizzaPO> cart = (List<PizzaPO>)Session["Cart"];
                        cart.Add(Mapping.PizzaMapper.PizzaDOtoPizzaPO(pizzaDO));

                        TempData["SuccessMessage"] = "Item added to cart.";

                        response = RedirectToAction("Index", "Pizza");
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
        public ActionResult ViewPizzaDetails(int index)
        {
            ActionResult response = null;

            try
            {
                List<PizzaPO> cart = (Session["Cart"] as List<PizzaPO>);

                if (index < 0 || index >= cart.Count)
                {
                    TempData["ErrorMessage"] = "That item doesn't exist.";
                    response = RedirectToAction("Index", "Cart");
                }
                else
                {
                    response = View(cart[index]);
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
        public ActionResult RemovePizza(int index)
        {
            ActionResult response = null;

            try
            {
                List<PizzaPO> cart = (Session["Cart"] as List<PizzaPO>);

                if (index < 0 || index >= cart.Count)
                {
                    TempData["ErrorMessage"] = "That item doesn't exist";
                }
                else
                {
                    cart.RemoveAt(index);
                }

                response = RedirectToAction("Index", "Cart");
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
        public ActionResult CreateOrder(CartPaymentVM form)
        {
            ActionResult response = null;

            if (ModelState.IsValid)
            {
                if (!form.PaymentPO.PayWithCash && !ValidCreditCard(form.PaymentPO.CreditCard))
                {
                    TempData["PaymentErrorMessage"] = "You must fill out the credit card info.";
                    response = RedirectToAction("Index", "Cart");
                }
                else
                {
                    try
                    {
                        List<PizzaPO> cart = Session["Cart"] as List<PizzaPO>;

                        if (cart.Count == 0)
                        {
                            response = RedirectToAction("Index", "Pizza");
                        }
                        else
                        {
                            bool isUserInfoValid = true;

                            if (form.PaymentPO.ForDelivery)
                            {
                                // TODO: Check for null
                                UserPO currentUser = Mapping.UserMapper.UserDOtoUserPO(_userDAO.GetUserByID(GetSessionUserID()));
                                List<string> invalidInfo = GetInvalidDeliveryInfo(currentUser);

                                if (invalidInfo.Count > 0)
                                {
                                    isUserInfoValid = false;

                                    string errorMessage = "Some information is required before a delivery order can be submitted: ";
                                    errorMessage += string.Join(", ", invalidInfo);

                                    if (GetSessionRole() == 2)
                                    {
                                        errorMessage += " Your manager must update your account.";
                                    }

                                    TempData["ErrorMessage"] = errorMessage;

                                    response = RedirectToAction("Update", "Account", new { ID = GetSessionUserID() });
                                }
                            }

                            if (isUserInfoValid)
                            {
                                OrderDO newOrder = new OrderDO();

                                newOrder.IsDelivery = form.PaymentPO.ForDelivery;
                                newOrder.UserID = GetSessionUserID();
                                newOrder.Status = "Prepping";
                                newOrder.OrderDate = DateTime.Now;

                                newOrder.Total = _pizzaBLO.GetCostOfPizzas(Mapping.PizzaMapper.PizzaPOListToPizzaBOList(cart));
                                
                                if (form.PaymentPO.PayWithCash)
                                {
                                    newOrder.Paid = false;
                                }
                                else
                                {
                                    newOrder.Paid = true;
                                }

                                long createdOrderID = _orderDAO.CreateOrder(newOrder);

                                foreach (PizzaPO pizzaPO in cart)
                                {
                                    pizzaPO.OrderID = createdOrderID;
                                    PizzaDO pizzaDO = Mapping.PizzaMapper.PizzaPOtoPizzaDO(pizzaPO);

                                    if (!_pizzaDAO.AddNewPizza(pizzaDO))
                                    {
                                        Logger.Log("WARNING", "CartController", "CreateOrder",
                                            "Unable to add a pizza from the cart to the database.");
                                    }
                                }

                                Session["Cart"] = new List<PizzaPO>();
                                TempData["SuccessMessage"] = "Successfully created the order.";
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
                }
            }
            else
            {
                if (!ModelState.IsValidField("PaymentPO.CreditCard"))
                {
                    TempData["CreditCardError"] = "Invalid credit card number.";
                }

                TempData["PaymentErrorMessage"] = "Please fix the errors shown below";
                response = RedirectToAction("Index", "Cart");
            }

            return response;
        }

        /******* HELPER METHODS START ********/

        bool ValidCreditCard(string creditCardNum)
        {
            CreditCardAttribute cardAttribute = new CreditCardAttribute();

            return cardAttribute.IsValid(creditCardNum);
        }

        /******** HELPER METHODS END *********/
    }
}