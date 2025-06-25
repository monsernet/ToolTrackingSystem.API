using ToolTrackingSystem.API.Models.DTOs.Calibrations;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;

namespace ToolTrackingSystem.API.Services
{
    public class CalibrationService : ICalibrationService
    {
        private readonly ICalibrationRepository _calibrationRepo;
        private readonly IGenericRepository<Tool> _toolRepo;

        public CalibrationService(
            ICalibrationRepository calibrationRepo,
            IGenericRepository<Tool> toolRepo)
        {
            _calibrationRepo = calibrationRepo;
            _toolRepo = toolRepo;
        }

        public async Task<IEnumerable<UpcomingCalibrationDto>> GetUpcomingCalibrationsAsync(int daysAhead)
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(daysAhead);

            var tools = await _toolRepo.GetAllAsync();

            return tools
                .Where(t => t.CalibrationRequired &&
                           t.NextCalibrationDate.HasValue &&
                           t.NextCalibrationDate.Value.Date >= today &&
                           t.NextCalibrationDate.Value.Date <= endDate)
                .Select(t => new UpcomingCalibrationDto
                {
                    ToolId = t.Id,
                    ToolCode = t.Code,
                    ToolName = t.Name,
                    LastCalibrationDate = t.LastCalibrationDate,
                    NextCalibrationDate = t.NextCalibrationDate.Value,
                    DaysUntilDue = (t.NextCalibrationDate.Value.Date - today).Days
                })
                .ToList();
        }

        public async Task<IEnumerable<CalibrationRecordDto>> GetToolCalibrationHistoryAsync(int toolId)
        {
            var calibrations = await _calibrationRepo.GetByToolIdAsync(toolId);
            var tool = await _toolRepo.GetByIdAsync(toolId);

            return calibrations.Select(c => new CalibrationRecordDto
            {
                Id = c.Id,
                ToolId = c.ToolId,
                ToolCode = tool?.Code ?? string.Empty,
                ToolName = tool?.Name ?? string.Empty,
                CalibrationDate = c.CalibrationDate,
                NextCalibrationDate = c.NextCalibrationDate,
                PerformedBy = c.PerformedBy,
                CertificateNumber = c.CertificateNumber,
                Notes = c.Notes,
                CalibrationMethod = c.CalibrationMethod
            });
        }

        public async Task<CalibrationRecordDto> CreateCalibrationRecordAsync(CreateCalibrationDto dto)
        {
            var tool = await _toolRepo.GetByIdAsync(dto.ToolId);
            if (tool == null) throw new ArgumentException("Tool not found");
            if (!tool.CalibrationRequired) throw new InvalidOperationException("Tool doesn't require calibration");

            var nextDate = (dto.CalibrationDate ?? DateTime.UtcNow)
                .AddDays(tool.CalibrationFrequencyDays ?? 180);

            var calibration = new ToolCalibration
            {
                ToolId = dto.ToolId,
                CalibrationDate = dto.CalibrationDate ?? DateTime.UtcNow,
                NextCalibrationDate = nextDate,
                PerformedBy = dto.PerformedBy,
                CertificateNumber = dto.CertificateNumber,
                Notes = dto.Notes,
                CalibrationMethod = dto.CalibrationMethod
            };

            await _calibrationRepo.AddAsync(calibration);

            // Update tool's calibration dates
            tool.LastCalibrationDate = calibration.CalibrationDate;
            tool.NextCalibrationDate = calibration.NextCalibrationDate;
            await _toolRepo.UpdateAsync(tool);

            return await GetCalibrationRecordDtoAsync(calibration.Id);
        }

        public async Task<bool> DeleteCalibrationRecordAsync(int id)
        {
            var calibration = await _calibrationRepo.GetByIdAsync(id);
            if (calibration == null) return false;

            await _calibrationRepo.DeleteAsync(calibration);
            return true;
        }

        public async Task<IEnumerable<UpcomingCalibrationDto>> GetOverdueCalibrationsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tools = await _toolRepo.GetAllAsync();

            return tools
                .Where(t => t.CalibrationRequired &&
                           t.NextCalibrationDate.HasValue &&
                           t.NextCalibrationDate.Value.Date < today)
                .Select(t => new UpcomingCalibrationDto
                {
                    ToolId = t.Id,
                    ToolCode = t.Code,
                    ToolName = t.Name,
                    LastCalibrationDate = t.LastCalibrationDate,
                    NextCalibrationDate = t.NextCalibrationDate.Value,
                    DaysUntilDue = (t.NextCalibrationDate.Value.Date - today).Days
                })
                .ToList();
        }

        private async Task<CalibrationRecordDto> GetCalibrationRecordDtoAsync(int id)
        {
            var calibration = await _calibrationRepo.GetByIdAsync(id);
            if (calibration == null) throw new ArgumentException("Calibration record not found");

            var tool = await _toolRepo.GetByIdAsync(calibration.ToolId);

            return new CalibrationRecordDto
            {
                Id = calibration.Id,
                ToolId = calibration.ToolId,
                ToolCode = tool?.Code ?? string.Empty,
                ToolName = tool?.Name ?? string.Empty,
                CalibrationDate = calibration.CalibrationDate,
                NextCalibrationDate = calibration.NextCalibrationDate,
                PerformedBy = calibration.PerformedBy,
                CertificateNumber = calibration.CertificateNumber,
                Notes = calibration.Notes,
                CalibrationMethod = calibration.CalibrationMethod
            };
        }
    }
}