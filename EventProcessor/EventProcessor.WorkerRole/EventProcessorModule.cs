﻿using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.DataInitialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole.Processors;
using Microsoft.Azure.IoT.Samples.EventProcessor.WorkerRole.Processors;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WorkerRole
{
    public sealed class EventProcessorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigurationProvider>()
                .As<IConfigurationProvider>()
                .SingleInstance();

            builder.RegisterType<DeviceEventProcessor>()
                .As<IDeviceEventProcessor>()
                .SingleInstance();

            builder.RegisterType<ActionEventProcessor>()
                .As<IActionEventProcessor>()
                .SingleInstance();

            builder.RegisterType<DeviceLogic>()
                .As<IDeviceLogic>();

            builder.RegisterType<DeviceRulesLogic>()
                .As<IDeviceRulesLogic>();

            builder.RegisterType<DeviceRegistryRepository>()
                .As<IDeviceRegistryCrudRepository>();

            builder.RegisterType<DeviceRegistryRepository>()
                .As<IDeviceRegistryListRepository>();

            builder.RegisterType<DeviceRulesRepository>()
                .As<IDeviceRulesRepository>();

            builder.RegisterType<IotHubRepository>()
                .As<IIotHubRepository>();

            builder.RegisterType<SecurityKeyGenerator>()
                .As<ISecurityKeyGenerator>();

            builder.RegisterType<VirtualDeviceTableStorage>()
                .As<IVirtualDeviceStorage>();

            builder.RegisterType<ActionMappingLogic>()
                .As<IActionMappingLogic>();

            builder.RegisterType<ActionMappingRepository>()
                .As<IActionMappingRepository>();

            builder.RegisterType<ActionLogic>()
                .As<IActionLogic>();

            builder.RegisterType<DataInitializer>()
                .As<IDataInitializer>();

            builder.RegisterType<ActionRepository>()
                .As<IActionRepository>();

            builder.RegisterType<DocDbRestHelper>()
                .As<IDocDbRestHelper>();

            builder.RegisterType<MessageFeedbackProcessor>()
                .As<IMessageFeedbackProcessor>().SingleInstance();
        }
    }
}
