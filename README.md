# Wasabi TwitterBot

Wasabi TwitterBot is a simple bot that checks periodically  the Wasabi wallet coinjoin round in order to tweet everytime the required number of peers is achieved.

## How to run

The bot can be run simply by:
```
$ dotnet run
```

However, before doing that, it is necessary to configure the user authentication data in the `Config.ini` file. To do this go to https://apps.twitter.com/ once there swith to the `Keys and Access Tokens` tab and copy the required information:

```
# Twitter app authentication data
# This information is available https://apps.twitter.com/
Consumer-Key=----
Consumer-Secret=----
User-Access-Token=----
User-Access-Secret=----

```

