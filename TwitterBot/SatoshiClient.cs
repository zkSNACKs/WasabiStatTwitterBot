using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwitterBot
{
	public class SatoshiClient
	{
		public Uri BaseUri { get; }

		public SatoshiClient(Uri baseUri, IPEndPoint torSocks5EndPoint = null)
		{
			BaseUri = baseUri;
		}

		public async Task<IEnumerable<CcjRunningRoundState>> GetAllRoundStatesAsync()
		{
			var apiVersion = Bot.Config.Get<string>("WASABI_BOT_API_VERSION") ?? "2";
			var requestUri = new Uri(BaseUri,  $"/api/v{apiVersion}/btc/chaumiancoinjoin/states/");

			using (var httpClient = new HttpClient())
			{
				using(var response = await httpClient.GetAsync(requestUri))
				{
					response.EnsureSuccessStatusCode();

					var content = await response.Content.ReadAsStringAsync();
					return JsonConvert.DeserializeObject<IEnumerable<CcjRunningRoundState>>(content);
				}
			}
		}
	}
}
