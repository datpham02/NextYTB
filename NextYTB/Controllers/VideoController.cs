
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NextYTB.DTO;
using NextYTB.Model;
using System.Collections.Generic;


namespace NextYTB.Controllers
{
    [Route("api/video")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly NextTubContext _dbContext;
        private readonly IMapper _mapper;

        public VideoController(NextTubContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        [HttpGet("gets")]
        public ActionResult Gets(int page = 1, int limit = 10)
        {

            var totalCount = _dbContext.Videos.Include(v => v.ChannelOwner).Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / limit);
            var videos = _dbContext.Videos
                                 .Include(v => v.ChannelOwner)
                                 .OrderByDescending(v => v.CreateAt)
                                 .Skip((page - 1) * limit)
                                 .Take(limit)
                                 .ToList();

            var nextPage = page < totalPages ? true : false;

            var previousPage = page > 1 ? true : false;

            var videosDTO = _mapper.Map<List<VideoDTO>>(videos);



            return StatusCode(StatusCodes.Status200OK, new { success = true, pageResult = new { totalPage = totalPages, currentPage = page, limitVideo = limit, nextPage, previousPage, videos = videosDTO } });
        }
        [HttpGet("get_all")]
        public ActionResult GetAll()
        {



            var videos = _dbContext.Videos
                                 .Include(v => v.ChannelOwner)
                                 .OrderByDescending(v => v.CreateAt)

                                 .ToList();



            var videosDTO = _mapper.Map<List<VideoDTO>>(videos);



            return StatusCode(StatusCodes.Status200OK, new { succes = true, videos = videosDTO });
        }
        [HttpGet("get/{id}")]
        public ActionResult Get(int id)
        {

            if (id != 0)
            {
                var video = _dbContext.Videos
                    .Include(v => v.ChannelOwner)
                    .Include(v => v.UserLikeVideos)
                    .Include(v => v.UserDisLikeVideos)
                    .SingleOrDefault(v => v.Id == id);
                if (video != null)
                {
                    VideoDTO videoDTO = _mapper.Map<VideoDTO>(video);
                    return StatusCode(StatusCodes.Status200OK, new { success = true, video = videoDTO });
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { success = true, msg = "Video không tồn tại" });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }

        [HttpGet("search")]

        public async Task<ActionResult> SearchVideos(string keywords, int page = 1, int limit = 10)
        {
            using (var context = new NextTubContext())
            {


                var totalCount = _dbContext.Videos.Where(v => EF.Functions.Like(v.Title, $"%{keywords}%")).Include(v => v.ChannelOwner).Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / limit);
                var videos = await _dbContext.Videos
                                     .Include(v => v.ChannelOwner)
                                     .Where(v => EF.Functions.Like(v.Title, $"%{keywords}%"))
                                     .OrderByDescending(v => v.CreateAt)
                                        .Skip((page - 1) * limit)
                                     .Take(limit)
                                     .ToListAsync();

                var nextPage = page < totalPages ? true : false;

                var previousPage = page > 1 ? true : false;

                var videosDTO = _mapper.Map<List<VideoDTO>>(videos);



                return StatusCode(StatusCodes.Status200OK, new { success = true, pageResult = new { totalPage = totalPages, currentPage = page, limitVideo = limit, nextPage, previousPage, videos = videosDTO } });
                List<VideoDTO> videoDTO = _mapper.Map<List<VideoDTO>>(videos);



                return StatusCode(StatusCodes.Status200OK, new { success = true, videos = videoDTO });
            }
        }

    }
}

