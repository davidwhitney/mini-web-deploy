﻿using BitDeploy.Deployer.Features.Installation.Configuration;
using BitDeploy.Deployer.Features.Installation.Installation;
using BitDeploy.Deployer.Features.Installation.PreInstallation;
using BitDeploy.Deployer.Infrastructure.IIS7Plus;

namespace BitDeploy.Deployer.Features.Installation
{
    public class SiteDeployer
    {
        private readonly InstallationConfiguration _installationConfiguration;

        public SiteDeployer(InstallationConfiguration installationConfiguration)
        {
            _installationConfiguration = installationConfiguration;
        }

        public void Deploy()
        {
            using (var serverManager = new ServerManagerWrapper())
            {
                var preInstall = new PreInstallationTaskList
                {
                    new DeleteExistingSite(serverManager),
                };

                var installation = new CreateSite(serverManager);

                var configuration = new ConfigurationTaskList
                {
                    new ConfigureAppPool(serverManager),
                    new ConfigureBindings(serverManager),
                    new ConfigureLogging(serverManager),
                    new ConfigureAdditionalDirectories(serverManager)
                };

                Execute(preInstall, installation, configuration);
                serverManager.CommitChanges();
            }
        }

        public void Execute(PreInstallationTaskList preInstall, CreateSite installation, ConfigurationTaskList configuration)
        {
            preInstall.PerformTasks(_installationConfiguration);
            
            var site = installation.Install(_installationConfiguration);
            
            configuration.Configure(site, _installationConfiguration);
        }
    }
}