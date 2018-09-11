using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitterBot
{
	class Config
	{
		private Dictionary<string, string> _values;
		private Config()
		{
			_values = new Dictionary<string, string>();
		}

		public T Get<T>(string key)
		{
			var value = Environment.GetEnvironmentVariable(key);
			if(string.IsNullOrEmpty(value))
			{
				if(!_values.TryGetValue(key, out value))
				{
					return default(T);
				}
			}
			return (T)Convert.ChangeType(value, typeof(T));
		}

		public static Config LoadFromFile(string path)
		{
			var config = new Config();
			var lines = File.ReadAllLines(path);
			foreach(var rawLine in lines)
			{
				var line = rawLine.Trim();
				if(string.IsNullOrWhiteSpace(line)) continue;
				if(line.StartsWith("#")) continue;

				var parts = line.Split("=", StringSplitOptions.RemoveEmptyEntries);
				if(parts.Length < 2) continue;
				config._values.Add(parts[0], parts[1]);
			}
			return config;
		}
	}
}