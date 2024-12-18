﻿namespace Kraken.Net.Objects.Internal
{
    internal class KrakenSocketCancelOrdersRequestV2 : KrakenSocketAuthRequestV2
    {
        [JsonPropertyName("order_id")]
        public string[] OrderIds { get; set; } = Array.Empty<string>();
    }
}
