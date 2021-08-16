using System;
using System.Diagnostics;
using Bitmex.Client.Websocket.Utils;
using Newtonsoft.Json;

namespace Bitmex.Client.Websocket.Responses.Wallets
{
    /// <summary>
    /// Information about your wallet (balance, changes, address, etc)
    /// </summary>
    [DebuggerDisplay("Wallet: {Currency} - {BalanceBtc}")]
    public class Wallet
    {
        /// <summary>
        /// Account identification
        /// </summary>
        [JsonProperty("account")]
        public long Account { get; set; }

        /// <summary>
        /// Current `Amount` currency, for example `XBt` which is satoshi
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("prevDeposited")]
        public long? PrevDeposited { get; set; }
        [JsonProperty("prevWithdrawn")]
        public long? PrevWithdrawn { get; set; }
        [JsonProperty("prevTransferIn")]
        public long? PrevTransferIn { get; set; }
        [JsonProperty("prevTransferOut")]
        public long? PrevTransferOut { get; set; }
        [JsonProperty("prevAmount")]
        public long? PrevAmount { get; set; }

        [JsonProperty("transferIn")]
        public long? TransferIn { get; set; }
        [JsonProperty("transferOut")]
        public long? TransferOut { get; set; }

        /// <summary>
        /// Current balance in satoshis (call `BalanceBtc` property to get BTC representation)
        /// </summary>
        [JsonProperty("amount")]
        public long? Amount { get; set; }


        [JsonProperty("pendingCredit")]
        public long? PendingCredit { get; set; }
        [JsonProperty("pendingDebit")]
        public long? PendingDebit { get; set; }
        [JsonProperty("confirmedDebit")]
        public long? ConfirmedDebit { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("addr")]
        public string Addr { get; set; }
        [JsonProperty("script")]
        public string Script { get; set; }
        [JsonProperty("withdrawalLock")]
        public string[] WithdrawalLock {get; set; }

        /// <summary>
        /// Converted satoshis 'Amount' into double BTC representation
        /// </summary>
        [JsonIgnore]
        public double BalanceBtc => BitmexConverter.ConvertToBtc(Currency, Amount ?? 0);

        /// <summary>
        /// Converted satoshis 'Amount' into decimal BTC representation
        /// </summary>
        [JsonIgnore]
        public decimal BalanceBtcDecimal => BitmexConverter.ConvertToBtcDecimal(Currency, Amount ?? 0);
    }
}
