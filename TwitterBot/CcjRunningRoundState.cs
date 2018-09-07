namespace TwitterBot
{

	public class CcjRunningRoundState
	{
		public string Phase { get; set; }

		public decimal Denomination { get; set; }

		public int RegisteredPeerCount { get; set; }

		public int RequiredPeerCount { get; set; }

		public int MaximumInputCountPerPeer { get; set; }

		public int RegistrationTimeout { get; set; }

		public decimal FeePerInputs { get; set; }

		public decimal FeePerOutputs { get; set; }

		public decimal CoordinatorFeePercent { get; set; }

		public long RoundId { get; set; }
    }
}