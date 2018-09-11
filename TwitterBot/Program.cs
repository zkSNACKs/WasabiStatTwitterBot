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

		static void Main(string[] args)
		{
			Config = Config.LoadFromFile("Config.ini");
 			Logger = new FileLogger(Config.Get<string>("WASABI_BOT_LOG_FILE_PATH") ?? "twitterbot.log");
			Logger.Info("Wasabi TwitterBot starting");

			var exitEvent = new AutoResetEvent(false);

			System.AppDomain.CurrentDomain.ProcessExit += (s, e) => {
				exitEvent.Set();
			};
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				exitEvent.Set();
			};

			var timeIntervalInSeconds = Config.Get<int>("WASABI_BOT_TIME_INTERVAL");
			Logger.Info($"Checking interval setted to {timeIntervalInSeconds} seconds");

			Worker = new TimedWorker();
			Worker.QueueForever("Check Coinjoin round status and tweet about it",
				CheckCoinJoinRoundStatus, 
				TimeSpan.FromSeconds(timeIntervalInSeconds));
			Worker.Start();

			exitEvent.WaitOne();
			Worker.Stop();
			Logger.Info("Wasabi TwitterBot finished.");
		}

		static void CheckCoinJoinRoundStatus()
		{
			try
			{
				var wasabiApiEndpoint = Config.Get<string>("WASABI_BOT_WASABI_URL");
				var satoshiClient = new SatoshiClient(new Uri(wasabiApiEndpoint));
				var states = satoshiClient.GetAllRoundStatesAsync().GetAwaiter().GetResult();
				var state = states.First();

				Logger.Info("Checking coinjoin round status");
				if(IsNewStateImportant(state))
				{
					var tweetContent = $"@WasabiWallet's just helped another {state.RegisteredPeerCount} people improve their financial privacy. {Config.Get<string>("WASABI_BOT_TAGS")}";
					Logger.Info($"Tweeting: {tweetContent}");

					try
					{
						if(!CanTweet) return;

						ExceptionHandler.SwallowWebExceptions = false;
						Auth.SetUserCredentials(
							Config.Get<string>("WASABI_BOT_TWITTER_CONSUMER_KEY"), 
							Config.Get<string>("WASABI_BOT_TWITTER_CONSUMER_SECRET"), 
							Config.Get<string>("WASABI_BOT_TWITTER_USER_ACCESS_TOKEN"), 
							Config.Get<string>("WASABI_BOT_TWITTER_USER_ACCESS_SECRET"));
						
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
			return state.RegisteredPeerCount == state.RequiredPeerCount;
		}
	}
}
