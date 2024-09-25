using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;

namespace SammiPlugin;

public static class Sammi
{
    private static HttpClient httpClient = new HttpClient();

	static Sammi()
	{
	}

    public static async void sendAPI (string uri, string password, StringContent content, int timeout, bool debug)
    {
        try
        {
            if (password != "")
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(password);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
            using var response = await httpClient.PostAsync(uri + "/api", content, cts.Token);

            if (debug)
            {
                var responseString = response.ToString();
                if (responseString.Contains("StatusCode: 401"))
                {
                    Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                    {
                        Type = Dalamud.Game.Text.XivChatType.Notice,
                        Message = "SAMMI Webhook Plugin: Unauthorized request, check password in /psammi",
                    });
                }
                Service.PluginLog.Debug(responseString);
            }
        }
        catch (HttpRequestException)
        {
            if (debug)
            {
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = new SeStringBuilder().AddText("SAMMI Webhook Plugin: Unable to connect, check address in /psammi, make sure SAMMI is open and \"Open Local API Server\" is enabled under Settings -> Connections")
                    .Build()
                });
            }
        }
        catch (UriFormatException)
        {
            if (debug)
            {
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = "SAMMI Webhook Plugin: Invalid URI, check address in /psammi"
                });
            }
        }
        catch (TaskCanceledException)
        {
            if (debug)
            {
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = "SAMMI Webhook Plugin: Task cancelled, check address in /psammi, or the request may be timing out"
                });
            }
        }
        catch (Exception e)
        {
            if (debug)
            {
                Service.PluginLog.Debug(e, "error, " + content.ToString()); 
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = new SeStringBuilder().AddText("SAMMI Webhook Plugin: Misc. error the dev didn't think to check for, sorry :)")
                .Build()
                });
            }
        }
    }

    public static async void sendWebhook(string uri, string password, StringContent content, int timeout, bool debug)
    {
        try
        {
            if (password != "")
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(password);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
            using var response = await httpClient.PostAsync(uri + "/webhook", content, cts.Token);
            if (debug)
            {
                var responseString = response.ToString();
                Service.PluginLog.Debug(responseString);
                if (responseString.Contains("StatusCode: 401"))
                {
                    Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                    {
                        Type = Dalamud.Game.Text.XivChatType.Notice,
                        Message = "SAMMI Webhook Plugin: Unauthorized request, check password in /psammi",
                    });
                }
            }
        }
        catch (HttpRequestException)
        {
            if (debug)
            {
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = new SeStringBuilder().AddText("SAMMI Webhook Plugin: Unable to connect, check address in /psammi, make sure SAMMI is open and \"Open Local API Server\" is enabled under Settings -> Connections")
                    .Build()
                });
            }
        }
        catch (UriFormatException)
        {
            if (debug)
            {
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = "SAMMI Webhook Plugin: Invalid URI, check address in /psammi"
                });
            }
        }
        catch (TaskCanceledException)
        {
            if (debug)
            {
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = "SAMMI Webhook Plugin: Task cancelled, check address in /psammi, or the request may be timing out"
                });
            }
        }
        catch (Exception e)
        {
            if (debug)
            {
                Service.PluginLog.Debug(e, "error, " + content.ToString());
                Service.ChatGui.Print(new Dalamud.Game.Text.XivChatEntry
                {
                    Type = Dalamud.Game.Text.XivChatType.Notice,
                    Message = new SeStringBuilder().AddText("SAMMI Webhook Plugin: Misc. error the dev didn't think to check for, sorry :)")
                .Build()
                });
            }
        }
    }
}
