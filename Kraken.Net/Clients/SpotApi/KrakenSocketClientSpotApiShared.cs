﻿using Kraken.Net.Interfaces.Clients.SpotApi;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Objects.Sockets;
using Kraken.Net.Enums;

namespace Kraken.Net.Clients.SpotApi
{
    internal partial class KrakenSocketClientSpotApi : IKrakenSocketClientSpotApiShared
    {
        public string Exchange => KrakenExchange.ExchangeName;
        public TradingMode[] SupportedTradingModes { get; } = new [] { TradingMode.Spot };

        public void SetDefaultExchangeParameter(string key, object value) => ExchangeParameters.SetStaticParameter(Exchange, key, value);
        public void ResetDefaultExchangeParameters() => ExchangeParameters.ResetStaticParameters();

        #region Ticker client
        EndpointOptions<SubscribeTickerRequest> ITickerSocketClient.SubscribeTickerOptions { get; } = new EndpointOptions<SubscribeTickerRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> ITickerSocketClient.SubscribeToTickerUpdatesAsync(SubscribeTickerRequest request, Action<ExchangeEvent<SharedSpotTicker>> handler, CancellationToken ct)
        {
            var validationError = ((ITickerSocketClient)this).SubscribeTickerOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToTickerUpdatesAsync(symbol, update => handler(update.AsExchangeEvent(Exchange, new SharedSpotTicker(symbol, update.Data.LastPrice, update.Data.HighPrice, update.Data.LowPrice, update.Data.Volume, update.Data.PriceChangePercentage))), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Trade client

        EndpointOptions<SubscribeTradeRequest> ITradeSocketClient.SubscribeTradeOptions { get; } = new EndpointOptions<SubscribeTradeRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> ITradeSocketClient.SubscribeToTradeUpdatesAsync(SubscribeTradeRequest request, Action<ExchangeEvent<IEnumerable<SharedTrade>>> handler, CancellationToken ct)
        {
            var validationError = ((ITradeSocketClient)this).SubscribeTradeOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToTradeUpdatesAsync(symbol, update => handler(update.AsExchangeEvent(Exchange, update.Data.Select(x => new SharedTrade(x.Quantity, x.Price, x.Timestamp)))), false, ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Book Ticker client

        EndpointOptions<SubscribeBookTickerRequest> IBookTickerSocketClient.SubscribeBookTickerOptions { get; } = new EndpointOptions<SubscribeBookTickerRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> IBookTickerSocketClient.SubscribeToBookTickerUpdatesAsync(SubscribeBookTickerRequest request, Action<ExchangeEvent<SharedBookTicker>> handler, CancellationToken ct)
        {
            var validationError = ((IBookTickerSocketClient)this).SubscribeBookTickerOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToTickerUpdatesAsync(symbol, update => handler(update.AsExchangeEvent(Exchange, new SharedBookTicker(update.Data.BestAskPrice, update.Data.BestAskQuantity, update.Data.BestBidPrice, update.Data.BestBidQuantity))), ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Kline client
        SubscribeKlineOptions IKlineSocketClient.SubscribeKlineOptions { get; } = new SubscribeKlineOptions(false);
        async Task<ExchangeResult<UpdateSubscription>> IKlineSocketClient.SubscribeToKlineUpdatesAsync(SubscribeKlineRequest request, Action<ExchangeEvent<SharedKline>> handler, CancellationToken ct)
        {
            var interval = (Enums.KlineInterval)request.Interval;
            if (!Enum.IsDefined(typeof(Enums.KlineInterval), interval))
                return new ExchangeResult<UpdateSubscription>(Exchange, new ArgumentError("Interval not supported"));

            var validationError = ((IKlineSocketClient)this).SubscribeKlineOptions.ValidateRequest(Exchange, request, request.Symbol.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var symbol = request.Symbol.GetSymbol(FormatSymbol);
            var result = await SubscribeToKlineUpdatesAsync(symbol, interval, update =>
            {
                if (update.UpdateType == SocketUpdateType.Snapshot)
                    return;

                foreach(var item in update.Data)
                    handler(update.AsExchangeEvent(Exchange, new SharedKline(item.OpenTime,  item.ClosePrice, item.HighPrice, item.LowPrice, item.OpenPrice, item.Volume)));
            }, ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Balance client
        EndpointOptions<SubscribeBalancesRequest> IBalanceSocketClient.SubscribeBalanceOptions { get; } = new EndpointOptions<SubscribeBalancesRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> IBalanceSocketClient.SubscribeToBalanceUpdatesAsync(SubscribeBalancesRequest request, Action<ExchangeEvent<IEnumerable<SharedBalance>>> handler, CancellationToken ct)
        {
            var validationError = ((IBalanceSocketClient)this).SubscribeBalanceOptions.ValidateRequest(Exchange, request, request.TradingMode, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);
            var result = await SubscribeToBalanceUpdatesAsync(
                null,
                update => handler(update.AsExchangeEvent<IEnumerable<SharedBalance>>(Exchange, update.Data.Select(x =>
                new SharedBalance(x.Asset, x.Balance, x.Balance)).ToArray())),
                false,
                ct: ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }
        #endregion

        #region Spot Order client

        EndpointOptions<SubscribeSpotOrderRequest> ISpotOrderSocketClient.SubscribeSpotOrderOptions { get; } = new EndpointOptions<SubscribeSpotOrderRequest>(false);
        async Task<ExchangeResult<UpdateSubscription>> ISpotOrderSocketClient.SubscribeToSpotOrderUpdatesAsync(SubscribeSpotOrderRequest request, Action<ExchangeEvent<IEnumerable<SharedSpotOrder>>> handler, CancellationToken ct)
        {
            var validationError = ((ISpotOrderSocketClient)this).SubscribeSpotOrderOptions.ValidateRequest(Exchange, request, TradingMode.Spot, SupportedTradingModes);
            if (validationError != null)
                return new ExchangeResult<UpdateSubscription>(Exchange, validationError);

            var result = await SubscribeToOrderUpdatesAsync(
                update => handler(update.AsExchangeEvent<IEnumerable<SharedSpotOrder>>(Exchange, update.Data.Select(
                    x => new SharedSpotOrder(
                        x.Symbol ?? string.Empty,
                        x.OrderId,
                        x.OrderType == Enums.OrderType.Limit ? SharedOrderType.Limit : x.OrderType == Enums.OrderType.Market ? SharedOrderType.Market : SharedOrderType.Other,
                        x.OrderSide == Enums.OrderSide.Buy ? SharedOrderSide.Buy : SharedOrderSide.Sell,
                        ParseStatus(x.OrderStatus),
                        x.Timestamp)
                    {
                        AveragePrice = x.AveragePrice,
                        ClientOrderId = x.ClientOrderId,
                        OrderId = x.OrderId,
                        OrderPrice = x.LimitPrice,
                        Quantity = x.OrderQuantity,
                        QuantityFilled = x.QuantityFilled,
                        QuoteQuantity = x.QuoteOrderQuantity,
                        QuoteQuantityFilled = x.ValueFilled,
                        TimeInForce = x.TimeInForce == TimeInForce.IOC ? SharedTimeInForce.ImmediateOrCancel : x.TimeInForce == TimeInForce.GTC ? SharedTimeInForce.GoodTillCanceled : null,
                        LastTrade = x.LastTradeId == null ? null : new SharedUserTrade(x.Symbol ?? string.Empty, x.OrderId, x.LastTradeId.ToString(), x.OrderSide == OrderSide.Sell ? SharedOrderSide.Sell : SharedOrderSide.Buy, x.LastTradeQuantity ?? 0, x.LastTradePrice ?? 0, x.Timestamp)
                    }
                    ).ToArray())),
                false,
                false,
                ct: ct).ConfigureAwait(false);

            return new ExchangeResult<UpdateSubscription>(Exchange, result);
        }

        private SharedOrderStatus ParseStatus(OrderStatusUpdate orderStatus)
        {
            if (orderStatus == OrderStatusUpdate.New || orderStatus == OrderStatusUpdate.Pending || orderStatus == OrderStatusUpdate.PartiallyFilled) return SharedOrderStatus.Open;
            if (orderStatus == OrderStatusUpdate.Canceled || orderStatus == OrderStatusUpdate.Expired) return SharedOrderStatus.Canceled;
            return SharedOrderStatus.Filled;
        }
        #endregion
    }
}