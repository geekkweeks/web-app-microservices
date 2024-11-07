using System;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _cofig;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration cofig)
    {
        _httpClient = httpClient;
        _cofig = cofig;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _httpClient.GetFromJsonAsync<List<Item>>(_cofig["AuctionServiceUrl"] 
            + "/api/auctions?date=" + lastUpdated);
    }
}
