using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LssTraining.Web.Data;
using LssTraining.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LssTraining.Web.Pages.CapacitySimulator
{
    public class DeviceModel : PageModel
    {
        private readonly CapacityRepository _repository;

        public DeviceModel(CapacityRepository repository)
        {
            _repository = repository;
        }

        public CapacitySummary Summary { get; set; } = new CapacitySummary();
        public List<ProductDeviceItem> AvailableDevices { get; set; } = new List<ProductDeviceItem>();

        [BindProperty]
        public DeviceCapacitySimulationInput Input { get; set; } = new DeviceCapacitySimulationInput();

        public DeviceCapacityAnalysisResult? Result { get; set; }
        public bool HasExecuted { get; set; }

        public async Task OnGetAsync()
        {
            await LoadMasterDataAsync();

            // Default initial setup: 2 sample device inputs
            if (Input.Devices == null || Input.Devices.Count == 0)
            {
                Input.Devices = new List<DeviceDemandInput>
                {
                    new DeviceDemandInput { DeviceCode = "3346007", DemandQuantity = 25000, DemandPeriod = "Weekly" },
                    new DeviceDemandInput { DeviceCode = "96320668", DemandQuantity = 15000, DemandPeriod = "Weekly" }
                };

                // Populate names from master data
                PopulateDeviceMetadata();
            }
        }

        public async Task<IActionResult> OnPostAnalyzeAsync()
        {
            await LoadMasterDataAsync();

            // Filter out empty rows
            if (Input.Devices != null)
            {
                Input.Devices = Input.Devices
                    .Where(d => !string.IsNullOrWhiteSpace(d.DeviceCode))
                    .ToList();
            }

            PopulateDeviceMetadata();

            if (Input.Devices != null && Input.Devices.Any(d => d.DemandQuantity > 0))
            {
                Result = await _repository.AnalyzeDeviceCapacityAsync(Input);
                HasExecuted = true;
            }

            return Page();
        }

        public async Task<JsonResult> OnGetLookupDeviceAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new JsonResult(null);

            await LoadMasterDataAsync();
            var dev = AvailableDevices.FirstOrDefault(d => string.Equals(d.DeviceCode, code.Trim(), StringComparison.OrdinalIgnoreCase));
            if (dev != null)
            {
                return new JsonResult(new { success = true, deviceCode = dev.DeviceCode, deviceName = dev.DeviceName, stepCount = dev.StepCount });
            }

            return new JsonResult(new { success = false });
        }

        private async Task LoadMasterDataAsync()
        {
            Summary = await _repository.GetSummaryAsync();
            AvailableDevices = await _repository.GetAllDevicesQuickListAsync();
        }

        private void PopulateDeviceMetadata()
        {
            if (Input.Devices == null || AvailableDevices == null || AvailableDevices.Count == 0)
                return;

            var devMap = AvailableDevices.ToDictionary(d => d.DeviceCode, d => d, StringComparer.OrdinalIgnoreCase);
            foreach (var d in Input.Devices)
            {
                if (!string.IsNullOrWhiteSpace(d.DeviceCode) && devMap.TryGetValue(d.DeviceCode.Trim(), out var master))
                {
                    d.DeviceName = master.DeviceName;
                    d.StepCount = master.StepCount;
                }
            }
        }
    }
}

