## Creating client
There are 2 clients available to interact with the Kraken API, the `KrakenClient` and `KrakenSocketClient`.

*Create a new rest client*
````C#
var krakenClient = new KrakenClient(new KrakenClientOptions()
{
	// Set options here for this client
});
````

*Create a new socket client*
````C#
var krakenSocketClient = new KrakenSocketClient(new KrakenSocketClientOptions()
{
	// Set options here for this client
});
````

Different options are available to set on the clients, see this example
````C#
var krakenClient = new KrakenClient(new KrakenClientOptions()
{
	ApiCredentials = new ApiCredentials("API-KEY", "API-SECRET"),
	LogLevel = LogLevel.Trace,
	RequestTimeout = TimeSpan.FromSeconds(60)
});
````
Alternatively, options can be provided before creating clients by using `SetDefaultOptions`:
````C#
KrakenClient.SetDefaultOptions(new KrakenClientOptions{
	// Set options here for all new clients
});
var krakenClient = new KrakenClient();
````
More info on the specific options can be found on the [CryptoExchange.Net wiki](https://github.com/JKorf/CryptoExchange.Net/wiki/Options)

### Dependency injection
See [CryptoExchange.Net wiki](https://github.com/JKorf/CryptoExchange.Net/wiki/Clients#dependency-injection)

## Usage
Make sure to read the [CryptoExchange.Net wiki](https://github.com/JKorf/CryptoExchange.Net/wiki/Clients#processing-request-responses) on processing responses.

#### Get market data
````C#
// Getting info on all symbols
var symbolData = await krakenClient.SpotApi.ExchangeData.GetSymbolsAsync();

// Getting ticker
var tickerData = await krakenClient.SpotApi.ExchangeData.GetTickerAsync("XBTUSD");

// Getting the order book of a symbol
var orderBookData = await krakenClient.SpotApi.ExchangeData.GetOrderBookAsync("XBTUSD");

// Getting recent trades of a symbol
var tradeHistoryData = await krakenClient.SpotApi.ExchangeData.GetTradeHistoryAsync("XBTUSD");
````

#### Requesting balances
````C#
var balanceData = await krakenClient.SpotApi.Account.GetBalancesAsync();
````
#### Placing order
````C#
// Placing a buy limit order for 0.001 BTC at a price of 50000USDT each
var orderData = await krakenClient.SpotApi.Trading.PlaceOrderAsync(
                "BTC-USDT",
                OrderSide.Buy,
                OrderType.Limit,
                0.001m,
                50000);
															
// Place a stop loss order, place a limit order of 0.001 BTC at 39000USDT each when the last trade price drops below 40000USDT
var orderData = await krakenClient.SpotApi.Trading.PlaceOrderAsync(
                "BTC-USDT",
                OrderSide.Sell,
                OrderType.StopLossLimit,
                0.001m,
                40000,
                secondaryPrice: 39000);
````

#### Requesting a specific order
````C#
// Request info on order with id `1234`
var orderData = await krakenClient.SpotApi.Trading.GetOrderAsync("1234");
````

#### Requesting order history
````C#
// Get all orders conform the parameters
 var ordersData = await krakenClient.SpotApi.Trading.GetOrdersAsync();
````

#### Cancel order
````C#
// Cancel order with id `1234`
var orderData = await krakenClient.SpotApi.Trading.CancelOrderAsync("1234");
````

#### Get user trades
````C#
var userTradesResult = await krakenClient.SpotApi.Trading.GetUserTradesAsync();
````

#### Subscribing to market data updates
````C#
var subscribeResult = await krakenSocket.SpotStreams.SubscribeToTickerUpdatesAsync("XBT/USD", data =>
{
	// Handle ticker data
});
````

#### Subscribing to order updates
````C#
var socketToken = await krakenClient.SpotApi.Account.GetWebsocketTokenAsync();
if(!socketToken.Success)
{
	// Handle error
	return;
}
var symbolData = await krakenSocket.SpotStreams.SubscribeToOrderUpdatesAsync(socketToken.Data.Token, data =>
{
	// Handle update
});
````
