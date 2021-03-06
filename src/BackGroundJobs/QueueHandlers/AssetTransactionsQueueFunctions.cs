﻿using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.AddressTransactionReport;
using Core.AssetTransactionReport;
using Core.Queue;
using Core.ReportMetadata;
using Core.ReportStorage;
using Lykke.EmailSenderProducer;
using Lykke.EmailSenderProducer.Models;
using Lykke.JobTriggers.Triggers.Attributes;

namespace BackGroundJobs.QueueHandlers
{
    public class AssetTransactionsQueueFunctions
    {
        private readonly IAssetTransactionsReportService _reportService;
        private readonly IAssetTransactionsReportMetadataRepository _metadataRepository;
        private readonly ILog _log;
        private readonly IAssetTransactionsReportStorage _reportStorage;
        private readonly EmailSenderProducer _emailSenderProducer;

        public AssetTransactionsQueueFunctions(EmailSenderProducer emailSenderProducer, 
            IAssetTransactionsReportService reportService, 
            IAssetTransactionsReportMetadataRepository metadataRepository, 
            ILog log, 
            IAssetTransactionsReportStorage reportStorage)
        {
            _emailSenderProducer = emailSenderProducer;
            _reportService = reportService;
            _metadataRepository = metadataRepository;
            _log = log;
            _reportStorage = reportStorage;
        }

        [QueueTrigger(QueueNames.AssetTransactionsReport, notify:true)]
        public async Task CreateReport(AssetTransactionReportQueueCommand command)
        {
            try
            {
                await _metadataRepository.SetProcessing(command.AssetId);
                var reportDate = DateTime.UtcNow;

                var reportData = await _reportService.GetTransactionsReport(command.AssetId);


                var saveResult = await _reportStorage.Save(command.AssetId, reportData);

                var emailMes = new EmailMessage
                {
                    Subject = $"Report for assetId {command.AssetId} at {reportDate:f}",
                    Body = $"Report for assetId {command.AssetId} at {reportDate:f} - {saveResult.Url}",
                };

                if (!string.IsNullOrEmpty(command.Email))
                {
                    await _emailSenderProducer.SendEmailAsync(command.Email, emailMes);
                }

                await _metadataRepository.SetDone(command.AssetId, saveResult.Url);

                await _log.WriteInfoAsync(nameof(AssetTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), "Report proceeded");
            }
            catch (Exception e)
            {
                await _log.WriteFatalErrorAsync(nameof(AssetTransactionsQueueFunctions), 
                    nameof(CreateReport),
                    command.ToJson(), e);

                await _metadataRepository.SetError(command.AssetId, e.ToString());
                throw;
            }
        }
    }
}
