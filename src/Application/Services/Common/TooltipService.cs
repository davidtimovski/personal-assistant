using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PersonalAssistant.Application.Contracts;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Services.Common
{
    public class TooltipService : ITooltipService
    {
        private readonly ITooltipsRepository _tooltipsRepository;
        private readonly IMapper _mapper;

        public TooltipService(
            ITooltipsRepository tooltipsRepository,
            IMapper mapper)
        {
            _tooltipsRepository = tooltipsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TooltipDto>> GetAllAsync(string application, int userId)
        {
            var tooltips = await _tooltipsRepository.GetAllAsync(application, userId);

            var tooltipDtos = tooltips.Select(x => _mapper.Map<TooltipDto>(x));

            return tooltipDtos;
        }

        public async Task<TooltipDto> GetByKeyAsync(int userId, string key)
        {
            Tooltip tooltip = await _tooltipsRepository.GetByKeyAsync(userId, key);
            return _mapper.Map<TooltipDto>(tooltip);
        }

        public async Task ToggleDismissedAsync(int userId, string key, bool isDismissed)
        {
            await _tooltipsRepository.ToggleDismissedAsync(userId, key, isDismissed);
        }
    }
}
