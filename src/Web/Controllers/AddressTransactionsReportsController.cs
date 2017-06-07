﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.AddressTransactionReport;
using Core.Queue;
using Core.ReportMetadata;
using Core.Settings;
using LkeServices.BitcoinHelpers;
using LkeServices.Settings;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    public class AddressTransactionsReportsController:Controller
    {
        private readonly IReportCommandProducer _commandProducer;
        private readonly IReportMetadataRepository _reportMetadataRepository;
        private readonly BaseSettings _baseSettings;

        public AddressTransactionsReportsController(BaseSettings baseSettings, 
            IReportCommandProducer commandProducer, 
            IReportMetadataRepository reportMetadataRepository)
        {
            _baseSettings = baseSettings;
            _commandProducer = commandProducer;
            _reportMetadataRepository = reportMetadataRepository;
        }

        [HttpPost]
        public async Task<CommandResult> CreateReport([FromBody]AddressTransactionsReportsRequest input)
        {
            if (!ModelState.IsValid)
            {
                return CommandResultBuilder.Fail(ModelState.GetErrorsList().ToArray());
            }

            if (!BitcoinAddressHelper.IsAddress(input.BitcoinAddress, _baseSettings.UsedNetwork()))
            {
                return CommandResultBuilder.Fail("Invalid base58 address string.");
            }

            await _reportMetadataRepository.InsertOrReplace(ReportMetadata.Create(input.BitcoinAddress));
            await _commandProducer.CreateAddressTransactionsReportCommand(input.BitcoinAddress, input.Email);
            
            return CommandResultBuilder.Ok();
        }
    }
}
