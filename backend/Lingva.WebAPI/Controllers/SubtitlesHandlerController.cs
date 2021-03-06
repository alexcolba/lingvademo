﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Lingva.BusinessLayer.Contracts;
using Lingva.BusinessLayer.DTO;
using Lingva.BusinessLayer.Services;
using Lingva.DataAccessLayer.Entities;
using Lingva.WebAPI.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Lingva.WebAPI.Controllers
{
    [Route("api/subtitle")]
    [ApiController]
    public class SubtitlesHandlerController : ControllerBase
    {
        private readonly ISubtitlesHandlerService _subtitleService;
        private readonly IParserWordService _wordService;

        private readonly IMapper _mapper;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public SubtitlesHandlerController(ISubtitlesHandlerService parser)
        {
            _subtitleService = parser;
        }

        //GET: api/subtitle/3
        /// <summary>
        /// Getting subtitles by ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /subltitle/{id}
        ///     { }
        ///
        /// </remarks>
        /// <returns>Subtitles</returns>
        /// <response code="200">Returns subtitles</response>
        /// <response code="404">If the exception is handled</response> 
        /// <param name="id">id of needed subtitles</param>
        /// <returns>Subtitles by requested id</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubtitleById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(BaseStatusDto.CreateErrorDto("Id of subtitle is incorrect."));
            }

            Subtitle subtitle = _subtitleService.GetSubtitleById(id);

            if (subtitle == null)
            {
                return BadRequest(BaseStatusDto.CreateErrorDto($"There is no a Subtitle record with Id = {id} in the Subtitles table."));
            }

            SubtitleDTO subtitleDTO = _mapper.Map<SubtitleDTO>(subtitle);
            subtitleDTO.CreateSuccess("GET request by Subtitle Id succeeds.");

            return Ok(subtitleDTO);

        }

        //GET: api/subtitle/path
        /// <summary>
        /// Getting subtitles by path.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /subltitle/path
        ///     { 
        ///         "Path" : "string"
        ///         "FilmId" : 123
        ///         "LanguageName" : "language"
        ///     }
        ///
        /// </remarks>
        /// <returns>Subtitles</returns>
        /// <response code="200">Returns subtitles from path</response>
        /// <response code="404">If the exception is handled</response> 
        /// <param name="path"></param>
        /// <returns>Returns subtitle from chosen path</returns>
        [HttpGet]
        [Route("path")]
        public async Task<IActionResult> GetSubtitleByPath([FromBody] string path)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(path))
            {
                return BadRequest(BaseStatusDto.CreateErrorDto("The Path of the subtitle is incorrect."));
            }

            Subtitle subtitle = _subtitleService.GetSubtitleByPath(path);

            if (subtitle == null)
            {
                return BadRequest(BaseStatusDto.CreateErrorDto(
                    $"There is no any Subtitle record with Path = {path} in the Subtitles table."));
            }

            SubtitleDTO subtitleDTO = _mapper.Map<SubtitleDTO>(subtitle);
            subtitleDTO.CreateSuccess("GET request by Subtitle Path succeeds.");

            return Ok(subtitleDTO);
        }

        //POST: api/subtitle/add
        /// <summary>
        /// Adding subtitles into database.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /subtitle/add
        ///      { 
        ///         "Path" : "string"
        ///         "FilmId" : 123
        ///         "LanguageName" : "language"
        ///     }
        ///
        /// </remarks>
        /// <returns>Sutiltle adding complete</returns>
        /// <response code="200">Returns status</response>
        /// <response code="404">If the exception is handled</response> 
        /// <param name="subtitleDto"></param>
        /// <returns>Status and added subtitles</returns>
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> AddSubtitle([FromBody]SubtitleDTO subtitleDto)
        {
            if (!ModelState.IsValid || subtitleDto == null)
            {
                return BadRequest(BaseStatusDto.CreateErrorDto("ModelState is not valid or the SubtitleDTO object is null."));
            }

            try
            {
                Subtitle subtitle = _mapper.Map<Subtitle>(subtitleDto);

                await Task.Run(() => _subtitleService.AddSubtitle(subtitle));

                subtitleDto.CreateSuccess("Subtitle record is created successfully.");

                return Ok(subtitleDto);
            }
            catch (Exception ex)
            {
                _logger.Debug($"{ex.GetType()} exception is generated.");
                _logger.Debug($"{ex.Message}");

                return BadRequest(BaseStatusDto.CreateErrorDto(ex.Message));
            }
        }
        //---Parsing only with Path
        //POST: api/subtitle/parse
        /// <summary>
        /// Parsing subtitles.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /subtitle/parse
        ///      { 
        ///         "Path" : "string"
        ///         "FilmId" : 123
        ///         "LanguageName" : "language"
        ///     }
        ///
        /// </remarks>
        /// <returns>Parsing complete</returns>
        /// <response code="200">Returns status</response>
        /// <response code="404">If the exception is handled</response>
        /// <param name="subtitleDto"></param>
        /// <returns>Status</returns>
        [HttpPost]
        [Route("parse")]
        public async Task<IActionResult> PostParse([FromBody]SubtitleDTO subtitleDto)
        {
            if (!ModelState.IsValid || subtitleDto == null)
            {
                return BadRequest(BaseStatusDto.CreateErrorDto("WordParserDTO request object is not correct."));
            }

            try
            {
                Subtitle subtitle = _mapper.Map<Subtitle>(subtitleDto);

                IEnumerable<SubtitleRow> rows = await Task.Run(() => _subtitleService.ParseSubtitle(subtitle));

                if (rows == null)
                {
                    return BadRequest(BaseStatusDto.CreateSuccessDto(
                        "There are no any rows from parsing subtitle by the ParserWordService."));
                }

                return Ok(BaseStatusDto.CreateSuccessDto("Subtitle parsing operation is successful."));
            }
            catch (Exception ex)
            {
                _logger.Debug($"{ex.GetType()} exception is generated.");
                _logger.Debug($"{ex.Message}");

                return BadRequest(BaseStatusDto.CreateErrorDto(ex.Message));
            }
        }
    }
}
