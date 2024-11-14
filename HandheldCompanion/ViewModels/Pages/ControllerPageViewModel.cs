﻿using HandheldCompanion.Controllers;
using HandheldCompanion.Extensions;
using HandheldCompanion.Managers;
using HandheldCompanion.Views.Pages;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace HandheldCompanion.ViewModels
{
    public class ControllerPageViewModel : BaseViewModel
    {
        private ControllerPage controllerPage;

        public ObservableCollection<ControllerViewModel> PhysicalControllers { get; set; } = [];
        public ObservableCollection<ControllerViewModel> VirtualControllers { get; set; } = [];

        public ControllerPageViewModel(ControllerPage controllerPage)
        {
            this.controllerPage = controllerPage;

            // Enable thread-safe access to the collection
            BindingOperations.EnableCollectionSynchronization(PhysicalControllers, new object());
            BindingOperations.EnableCollectionSynchronization(VirtualControllers, new object());

            // manage events
            ControllerManager.ControllerPlugged += ControllerPlugged;
            ControllerManager.ControllerUnplugged += ControllerUnplugged;
            ControllerManager.ControllerSelected += ControllerManager_ControllerSelected;

            // send events
            if (ControllerManager.IsInitialized)
            {
                ControllerManager_ControllerSelected(ControllerManager.GetTargetController());
            }
        }

        private void ControllerPlugged(IController Controller, bool IsPowerCycling)
        {
            ObservableCollection<ControllerViewModel> controllers = Controller.IsVirtual() ? VirtualControllers : PhysicalControllers;
            ControllerViewModel? foundController = controllers.FirstOrDefault(controller => controller.Controller.GetInstancePath() == Controller.GetInstancePath());
            if (foundController is null)
            {
                controllers.SafeAdd(new ControllerViewModel(Controller));
            }
            else
            {
                foundController.Controller = Controller;
            }

            controllerPage.ControllerRefresh();
        }


        private void ControllerUnplugged(IController Controller, bool IsPowerCycling, bool WasTarget)
        {
            ObservableCollection<ControllerViewModel> controllers = Controller.IsVirtual() ? VirtualControllers : PhysicalControllers;
            ControllerViewModel? foundController = controllers.ToList().FirstOrDefault(controller => controller.Controller.GetInstancePath() == Controller.GetInstancePath());
            if (foundController is not null && !IsPowerCycling)
            {
                controllers.SafeRemove(foundController);
                foundController.Dispose();
            }

            // do something
            controllerPage.ControllerRefresh();
        }

        private void ControllerManager_ControllerSelected(IController Controller)
        {
            foreach (ControllerViewModel controller in PhysicalControllers)
                controller.Updated();

            // do something
            controllerPage.ControllerRefresh();
        }
    }
}