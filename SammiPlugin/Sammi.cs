using System;
using System.Net.Http;
using System.Threading;

namespace SammiPlugin;


public static class Sammi
{
    private static HttpClient httpClient = new HttpClient();

	static Sammi()
	{
	}

    public static async void sendAPI (string uri, StringContent content, int timeout, bool debug)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
            using var response = await httpClient.PostAsync(uri + "/api", content, cts.Token);
            if (debug)
            {
                Service.PluginLog.Debug("api sent");
            }
        }
        catch (Exception e)
        {
            if (debug)
            {
                Service.PluginLog.Debug(e, "error, " + content.ToString());
            }
        }
    }

    public static async void sendWebhook(String uri, StringContent content, int timeout, bool debug)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
            using var response = await httpClient.PostAsync(uri + "/webhook", content, cts.Token);
            if (debug)
            {
                Service.PluginLog.Debug("webhook sent");
            }

        }
        catch (Exception e)
        {
            if (debug)
            { 
                Service.PluginLog.Debug(e, "error, " + content.ToString());
            }
        }
    }
}
