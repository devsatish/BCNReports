﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Asset;
using Core.Settings;
using Flurl;
using Flurl.Http;

namespace LkeServices.Asset
{
    public class AssetTransaction : IAssetTransaction
    {
        public string TransactionId { get; set; }

        public static AssetTransaction Create(AssetTransactionContract source)
        {
            return new AssetTransaction
            {
                TransactionId = source.TransactionHash
            };
        }
    }

    public class AssetTransactionContract
    {
        public string TransactionHash { get; set; }
    }

    public class AssetTransactionsesService:IAssetTransactionsService
    {
        private readonly BaseSettings _baseSettings;

        public AssetTransactionsesService(BaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
        }

        public async Task<IEnumerable<IAssetTransaction>> GetTransactionsForAsset(string assetId)
        {
            var resp = await _baseSettings.BlockChainExplolerUrl
                .AppendPathSegment($"api/assetstransactions/{assetId}")
                .GetJsonAsync<AssetTransactionContract[]>();

            return resp.Select(AssetTransaction.Create);
        }
    }
}
