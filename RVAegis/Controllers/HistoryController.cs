using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVAegis.Contexts;
using RVAegis.DTOs.HistoryDTOs;
using RVAegis.DTOs.UserDTOs;
using RVAegis.Models.HistoryModels;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/logs")]
    public class HistoryController(ApplicationContext applicationContext) : Controller
    {
        // GET api/logs
        /// <summary>
        /// Получение всех записей из истории активности.
        /// </summary>
        /// <returns>Список записей</returns>
        [ProducesResponseType(typeof(List<HistoryRecordDto>), 200)]
        [ProducesResponseType(401)]
        [HttpGet]
        public IActionResult GetHistoryRecords()
        {
            var historyRecord = applicationContext.HistoryRecords
                                        .Include(hr => hr.TypeAction)
                                        .Include(hr => hr.User)
                                        .ToList();

            var historyRecordDtos = new List<HistoryRecordDto>();

            foreach (var record in historyRecord)
                historyRecordDtos.Add(new HistoryRecordDto(record));

            return Ok(historyRecordDtos);
        }

        // GET api/logs/type-actions
        /// <summary>
        /// Получение всех типов действий.
        /// </summary>
        /// <returns>Список типов действий</returns>
        [ProducesResponseType(typeof(List<TypeActionDto>), 200)]
        [ProducesResponseType(401)]
        [HttpGet("type-actions")]
        public IActionResult GetTypeActions()
        {
            var typeActionsDtos = new List<TypeActionDto>();

            foreach (var typeAction in applicationContext.TypeActions)
                typeActionsDtos.Add(new TypeActionDto(typeAction));

            return Ok(typeActionsDtos);
        }
    }
}
