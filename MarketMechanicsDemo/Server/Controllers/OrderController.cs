using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMechanicsDemo.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace MarketMechanicsDemo.Server.Controllers
{    
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderContext _orderContext;
        private readonly IHubContext<Hubs.OrderHub> _hubContext;

        public OrderController(OrderContext orderContext, IHubContext<Hubs.OrderHub> hubContext)
        {
            _orderContext = orderContext;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Route("[controller]/getPendingOrders")]
        
        public ActionResult<IEnumerable<Order>> GetPendingOrders(string symbol, string type)
        {
            var buySell = (BuySell)Enum.Parse(typeof(BuySell), type);            
            if(buySell == BuySell.Sell)
            {
                return _orderContext.Orders
                    .Where(o => o.BuyOrSell == buySell && o.Symbol == symbol && o.NumSharesRemaining > 0)
                    .OrderBy(o => o.Price).ToList();
            }
            else
            {
                return _orderContext.Orders
                    .Where(o => o.BuyOrSell == buySell && o.Symbol == symbol && o.NumSharesRemaining > 0)
                    .OrderByDescending(o => o.Price).ToList();
            }
            
        }

        [HttpGet]
        [Route("[controller]/getFilledOrders")]
        public ActionResult<IEnumerable<Order>> GetFilledOrders()
        {
            return _orderContext.Orders.Where(o => o.NumSharesRemaining == 0).ToList();
        }

        [HttpGet]
        [Route("[controller]/getsymbols")]
        public ActionResult<List<string>> GetSymbols()
        {
            return _orderContext.Orders.Select(o => o.Symbol).Distinct().ToList();
        }

        [HttpPost]
        [Route("[controller]/placeOrder")]        
        public async Task<ActionResult<Order>> PlaceOrder([FromBody]Order order)
        {
            IEnumerable<Order> candidateOrders;
            List<Order> updatedOrders = new List<Order>();
            if(order.LimitOrMarket == LimitMarket.Market)
            {
                if(order.BuyOrSell == BuySell.Buy)
                {
                    candidateOrders = _orderContext.Orders
                        .Where(o => o.BuyOrSell == BuySell.Sell && o.NumSharesRemaining > 0 && o.Symbol == order.Symbol)
                        .OrderBy(o => o.Price).ThenBy(o => order.NumSharesRemaining);
                }
                else
                {
                    candidateOrders = _orderContext.Orders
                        .Where(o => o.BuyOrSell == BuySell.Buy && o.NumSharesRemaining > 0 && o.Symbol == order.Symbol)
                        .OrderByDescending(o => o.Price).ThenBy(o => o.NumSharesRemaining);
                }
            }
            else
            {
                if(order.BuyOrSell == BuySell.Buy)
                {
                    candidateOrders = _orderContext.Orders
                        .Where(o => o.BuyOrSell == BuySell.Sell && o.NumSharesRemaining > 0 && o.Price <= order.Price && o.Symbol == order.Symbol)
                        .OrderBy(o => o.Price).ThenBy(o => order.NumSharesRemaining); ;
                }
                else
                {
                    candidateOrders = _orderContext.Orders
                        .Where(o => o.BuyOrSell == BuySell.Buy && o.NumSharesRemaining > 0 && o.Price >= order.Price && o.Symbol == order.Symbol)
                        .OrderByDescending(o => o.Price).ThenBy(o => order.NumSharesRemaining);
                }
            }
            foreach (var candidate in candidateOrders)
            {
                int numSharesToTrade;
                if(candidate.NumSharesRemaining>=order.NumSharesRemaining)
                {
                    numSharesToTrade = order.NumSharesRemaining;
                }
                else
                {
                    numSharesToTrade = candidate.NumSharesRemaining;
                }
                candidate.NumSharesRemaining -= numSharesToTrade;
                order.NumSharesRemaining -= numSharesToTrade;
                candidate.FillPrice += numSharesToTrade * candidate.Price;
                order.FillPrice += numSharesToTrade * candidate.Price;                
                _orderContext.Update(candidate);
                updatedOrders.Add(candidate);
                if (order.NumSharesRemaining == 0)
                {
                    break;
                }                    
            }
            updatedOrders.Add(order);
            _orderContext.Add(order);
            _orderContext.SaveChanges();
            await _hubContext.Clients.All.SendAsync("updateOrders", updatedOrders);
            return order;
        }        
    }
}
