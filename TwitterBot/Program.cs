using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;

namespace TwitterBot
{
	class Bot
	{
		private static TimedWorker Worker;
		internal static Config Config;
		internal static FileLogger Logger;

		private static CcjRunningRoundState previousState;
		private static bool CanTweet = true;

		static async Task Main(string[] args)
		{
			Config = Config.LoadFromFile("Config.ini");
 			Logger = new FileLogger(Config.Get<string>("Log-File-Path") ?? "twitterbot.log");
			Logger.Info("Wasabi TwitterBot starting");

			var exitEvent = new AutoResetEvent(false);

			System.AppDomain.CurrentDomain.ProcessExit += (s, e) => {
				exitEvent.Set();
			};
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				exitEvent.Set();
			};

			Worker = new TimedWorker();
			Worker.QueueForever("Check Coinjoin round status and tweet about it",
				async()=>await CheckCoinJoinRoundStatusAsync(), 
				TimeSpan.FromSeconds(Config.Get<int>("Time-Interval")));
			Worker.Start();

			exitEvent.WaitOne();
			Worker.Stop();
			Logger.Info("Wasabi TwitterBot finished.");
		}

		static async Task CheckCoinJoinRoundStatusAsync()
		{
			try
			{
				var wasabiApiEndpoint = Config.Get<string>("Wasabi-URL");
				var satoshiClient = new SatoshiClient(new Uri(wasabiApiEndpoint));
				var states = await satoshiClient.GetAllRoundStatesAsync();
				var state = states.First();

				Logger.Info("Checking coinjoin round status");
				if(IsNewStateImportant(state))
				{
					var tweetContent = $"@WasabiWallet's just helped another {state.RegisteredPeerCount} people improve their financial privacy. {Config.Get<string>("Tags")}";
					Logger.Info($"Tweeting: {tweetContent}");

					try
					{
						if(!CanTweet) return;

						ExceptionHandler.SwallowWebExceptions = false;
						Auth.SetUserCredentials(
							Config.Get<string>("Consumer-Key"), 
							Config.Get<string>("Consumer-Secret"), 
							Config.Get<string>("User-Access-Token"), 
							Config.Get<string>("User-Access-Secret"));
						
						var tweet = Tweet.PublishTweet(tweetContent);
						
						if(tweet != null && tweet.IsTweetPublished)
						{
							Logger.Info($"Tweet url {tweet.Url}");
						}
						else
						{
							Logger.Error($"Tweet was not published!!!");
						}
						
						CanTweet = false;
						Worker.QueueOneTime("Do not tweet the same twice",
							()=>CanTweet=true, 
							TimeSpan.FromSeconds(60));
					}
					catch(Exception e)
					{
						Logger.Error("Error tweeting coinjoin status.", e);
					}
				}
				previousState = state;
			}
			catch(Exception e)
			{
				Bot.Logger.Error($"Error during executing scheduled task", e);
			}
		}

		private static bool IsNewStateImportant(CcjRunningRoundState state)
		{
			return state.RegisteredPeerCount != state.RequiredPeerCount;
		}
	}
}
