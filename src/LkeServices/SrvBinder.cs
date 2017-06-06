﻿using Autofac;
using AzureStorage.Queue;
using Core.AddressTransactionReport;
using Core.Asset;
using Core.Queue;
using Core.Settings;
using LkeServices.AddressTransactionReport;
using LkeServices.Asset;
using LkeServices.ReportsCommands;
using LkeServices.Settings;
using QBitNinja.Client;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindCommonServices(this ContainerBuilder ioc, BaseSettings settings)
        {
            ioc.RegisterType<AssetDefinitionService>().As<IAssetDefinitionService>();
            ioc.RegisterType<AddressXlsxRenderer>().As<IAddressXlsxRenderer>();
            ioc.RegisterType<AddressXlsxService>().As<IAddressXlsxService>().SingleInstance();

            ioc.Register(p => new ReportCommandProducer(new AzureQueueExt(settings.Db.DataConnString, QueueNames.AddressTransactionsReport)))
                .As<IReportCommandProducer>();

            ioc.Register(p => new QBitNinjaClient(settings.NinjaUrl, settings.UsedNetwork()){Colored = true});
        }
    }
}
