
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextYTB.DTO;
using NextYTB.IRequest;
using NextYTB.Model;



namespace NextYTB.Controllers
{
    [Route("api/channel")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly NextTubContext _dbContext;
        private readonly IMapper _mapper;

        public ChannelController(NextTubContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        [HttpGet("info/{id}")]

        //[ProducesResponseType(201)]
        //[ProducesResponseType(typeof(APIErrorReponse), StatusCodes.Status400BadRequest)]
        public ActionResult Get(int id)
        {
            if (id != 0)
            {
                var check = _dbContext.Channels
                    .Include(c => c.Subcribers)
                    .Include(c => c.Videos)
                    .SingleOrDefault(c => c.Id == id);

                if (check != null)
                {
                    ChannelInfoDTO channel = _mapper.Map<ChannelInfoDTO>(check);
                    return StatusCode(StatusCodes.Status200OK, new { success = true, channel });
                }

                return StatusCode(StatusCodes.Status404NotFound, new { success = false });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("{channelId}/video")]

        public ActionResult Get(int channelId, int page = 1, int limit = 10)
        {
            if (channelId == 0) return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
            var totalCount = _dbContext.Videos.Where(v => v.ChannelOwnerId == channelId).Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / limit);
            var videos = _dbContext.Videos
                                 .Where(v => v.ChannelOwnerId == channelId)
                                  .Include(v => v.UserLikeVideos)
                                  .Include(v => v.UserDisLikeVideos)
                                 .OrderByDescending(v => v.CreateAt)
                                 .Skip((page - 1) * limit)
                                 .Take(limit)
                                 .ToList();

            var nextPage = page < totalPages ? true : false;

            var previousPage = page > 1 ? true : false;

            var videosDTO = _mapper.Map<List<VideoDTO>>(videos);



            return StatusCode(StatusCodes.Status200OK, new { success = true, pageResult = new { totalPage = totalPages, currentPage = page, limitVideo = limit, nextPage, previousPage, videos = videosDTO } });
        }
        [HttpPost("create_video")]

        public ActionResult CreateVideo([FromBody] ICreateVideo videoData)
        {

            if (videoData.Title != null && videoData.Src != null && videoData.ChannelOwnerId != 0 && videoData.Duration != null)
            {
                Video newVideo = new()
                {
                    Title = videoData.Title,
                    Poster = videoData.Poster,
                    Src = videoData.Src,
                    ChannelOwnerId = videoData.ChannelOwnerId,
                    Description = videoData.Description,
                    Duration = videoData.Duration

                };
                _dbContext.Add(newVideo);
                _dbContext.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Tạo video thành công" });
            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("update_video")]

        public ActionResult UpdateVideo([FromBody] IUpdateVideo videoData)
        {

            if (videoData.VideoId != 0)
            {
                var video = _dbContext.Videos.Find(videoData.VideoId);


                if (videoData.Title != null || videoData.Title != "")
                {
                    video.Title = videoData.Title;
                }
                if (videoData.Poster != null || videoData.Poster != "")
                {
                    video.Poster = videoData.Poster;
                }
                if (videoData.Description != null || videoData.Description != "")
                {
                    video.Description = videoData.Description;
                }


                _dbContext.Update(video);
                _dbContext.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Cập nhập video thành công" });
            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpDelete("{channelId}/delete_video/{id}")]
        public ActionResult DeleteVideo(int channelId, int id)
        {

            if (id != 0)
            {

                var check = _dbContext.Videos.Include(v => v.UserLikeVideos).Include(v => v.UserDisLikeVideos).Where(v => v.Id == id && v.ChannelOwnerId == channelId).FirstOrDefault();

                if (check != null)
                {
                    _dbContext.RemoveRange(check.UserLikeVideos, check.UserDisLikeVideos);
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Xóa video thành công" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { success = true, msg = "Video không tồn tại" });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("get_video_comment/{videoId}")]
        public ActionResult GetVideoComment(int videoId)
        {

            if (videoId != 0)
            {

                var comments = _dbContext.Comments
                    .Include(c => c.AccountOwner)
                        .ThenInclude(a => a.Channel)
                    .Include(c => c.UserLikeComments)
                        .ThenInclude(ulc => ulc.Account)
                    .Include(c => c.UserDisLikeComments)
                        .ThenInclude(udl => udl.Account)
                    .Include(c => c.ReplyToAccount)
                        .ThenInclude(rta => rta.Channel)
                    .Include(c => c.InverseParentComment)
                        .ThenInclude(i => i.ReplyToAccount)
                            .ThenInclude(rta => rta.Channel)
                    .OrderBy(c => c.CreateAt)
                    .Where(c => c.VideoId == videoId);


                if (comments != null)
                {
                    List<CommentDTO> commentDTO = _mapper.Map<List<CommentDTO>>(comments);

                    return StatusCode(StatusCodes.Status200OK, new { success = true, comments = commentDTO });
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { success = true, msg = "Không tồn tại" });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
    }
}
