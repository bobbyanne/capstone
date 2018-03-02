using SpaceSidePizzariaBLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSidePizzariaBLL
{
    public class PizzaBLO
    {
        public decimal GetPizzaCost(PizzaBO pizzaBO)
        {
            return PriceCalculator.CalculateBasePizzaCost(pizzaBO);
        }

        /// <summary>
        /// Calculates the total for a given list of pizzas. Returns the total with
        /// tax added.
        /// </summary>
        public decimal GetCostOfPizzas(List<PizzaBO> pizzaBOList)
        {
            return PriceCalculator.CalculateCostOfPizzas(pizzaBOList);
        }
    }
}
