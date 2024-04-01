﻿using CryptoExchange.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kraken.Net.Enums
{
    /// <summary>
    /// Yield source
    /// </summary>
    public enum YieldSource
    {
        /// <summary>
        /// Staking
        /// </summary>
        [Map("staking")]
        Staking,
        /// <summary>
        /// Off chain
        /// </summary>
        [Map("staking")]
        OffChain,
        /// <summary>
        /// Opt in rewards
        /// </summary>
        [Map("opt_in_rewards")]
        OptInRewards
    }
}
