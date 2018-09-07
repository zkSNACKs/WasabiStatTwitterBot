using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotNetTor.SocksPort;
using Newtonsoft.Json;

namespace TwitterBot
{
	public class SatoshiClient
	{
		public Uri BaseUri { get; }
		public IPEndPoint TorSocks5EndPoint { get; }

		public SatoshiClient(Uri baseUri, IPEndPoint torSocks5EndPoint = null)
		{
			BaseUri = baseUri;
			TorSocks5EndPoint = torSocks5EndPoint ?? new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
		}

		public async Task<IEnumerable<CcjRunningRoundState>> GetAllRoundStatesAsync()
		{
			var apiVersion = Bot.Config.Get<string>("Api-Version") ?? "2";
			var requestUri = new Uri(BaseUri,  $"/api/v{apiVersion}/btc/chaumiancoinjoin/states/");

			using (var httpClient = new HttpClient(new SocksPortHandler(TorSocks5EndPoint)))
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
