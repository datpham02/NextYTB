using AutoMapper;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using NextYTB.Config;
using NextYTB.DTO;
using NextYTB.IRequest;
using NextYTB.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;




namespace NextYTB.Controllers
{

    [Route("api/user")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly NextTubContext _dbContext;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly EmailSetting _emailSetting;

        public AccountController(NextTubContext dbContext, IConfiguration config, IMapper mapper, EmailSetting emailSetting)
        {
            _dbContext = dbContext;
            _config = config;
            _mapper = mapper;
            _emailSetting = emailSetting;
        }

        [HttpPost("register")]
        public ActionResult Create([FromBody] ICreateAccount data)
        {
            if (data.Email != "" && data.Password != "" && data.ConfirmPassword != "")
            {
                if (!IsValidEmail(data.Email)) return Ok(new { success = false, msg = "Tài khoản không đúng định dạng email" });
                var check = _dbContext.Accounts.FirstOrDefault(account => account.Email == data.Email);

                if (check == null)
                {
                    if (data.Password == data.ConfirmPassword)
                    {
                        if (!IsStrongPassword(data.Password)) return Ok(new { success = false, msg = "Mật khẩu phải có ít nhất 1 chữ hoa ,1 chứ số và có độ dài lớn hơn 8" });
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(data.Password);
                        Account newUser = new()
                        {
                            Email = data.Email,
                            Password = hashedPassword,

                        };


                        _dbContext.Add(newUser);

                        _dbContext.SaveChanges();

                        Channel newChannel = new()
                        {
                            AccountOwnerId = newUser.Id,
                            Avatar = "https://ui-avatars.com/api/?name=" + ExtractUsername(data.Email),
                            Name = ExtractUsername(data.Email),
                        };
                        _dbContext.Add(newChannel);

                        _dbContext.SaveChanges();
                        return StatusCode(StatusCodes.Status201Created, new { success = true, msg = "Tạo tài khoản thành công" });
                    }
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Mật khẩu không khớp" });
                }

                return StatusCode(StatusCodes.Status409Conflict, new { success = false, msg = "Tài khoản đã tồn tại", check });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("login")]
        public ActionResult Login([FromBody] ILogin data)
        {
            if (data.Email != "" && data.Password != "")
            {
                if (!IsValidEmail(data.Email)) return Ok(new { success = false, msg = "Tài khoản không đúng định dạng email" });
                var checkUser = _dbContext.Accounts.Include(a => a.Channel).Include(a => a.Subcribers).ThenInclude(s => s.Channel).FirstOrDefault(account => account.Email == data.Email);
                if (checkUser != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(data.Password, checkUser.Password))
                    {
                        var checkRefreshToken = checkUser.RefreshToken;
                        var accessToken = GenerateToken(checkUser, 1 * 24 * 60 * 60);

                        AccountDTO accountDTO = _mapper.Map<AccountDTO>(checkUser);
                        if (checkRefreshToken == null || IsTokenExpired(checkRefreshToken))
                        {
                            var refreshToken = GenerateToken(checkUser, 30 * 24 * 60 * 60);
                            checkUser.RefreshToken = refreshToken.ToString();

                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddMonths(1)
                            };

                            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);

                            _dbContext.Update(checkUser);
                            _dbContext.SaveChanges();
                            return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Đăng nhập thành công", accessToken, account = accountDTO });
                        }
                        return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Đăng nhập thành công", accessToken, account = accountDTO });

                    }
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Tài khoản hoặc mật khẩu không đúng" });
                }
                return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Tài khoản hoặc mật khẩu không đúng" });
            }
            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("create_video_watch_late")]
        public ActionResult CreateVideoWatchLate([FromBody] ICreateWatchLateVideo data)
        {

            if (data.VideoId != 0 && data.AccountId != 0)
            {

                UserWatchLateVideo userWatchLateVideo = new()
                {
                    AccountId = data.AccountId,
                    VideoId = data.VideoId,
                };
                _dbContext.Add(userWatchLateVideo);
                _dbContext.SaveChanges();

                return StatusCode(StatusCodes.Status200OK, new { success = true });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("create_video_watched")]
        public ActionResult CreateVideoWatched([FromBody] ICreateUserWatchedVideo data)
        {

            if (data.VideoId != 0 && data.AccountId != 0)
            {

                UserWatchedVideo userWatchedVideo = new()
                {
                    AccountId = data.AccountId,
                    VideoId = data.VideoId,
                };
                _dbContext.Add(userWatchedVideo);
                _dbContext.SaveChanges();

                return StatusCode(StatusCodes.Status200OK, new { success = true });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("like_video")]
        public ActionResult LikeVideo([FromBody] ICreateLikeDisLikeVideo data)
        {

            if (data.VideoId != 0 && data.AccountId != 0)
            {
                var check = _dbContext.UserLikeVideos.Where(ulv => ulv.AccountId == data.AccountId && ulv.VideoId == data.VideoId)
                .FirstOrDefault();

                if (check == null)
                {
                    var checkDisLike = _dbContext.UserDisLikeVideos.Where(ulv => ulv.AccountId == data.AccountId && ulv.VideoId == data.VideoId).FirstOrDefault();


                    UserLikeVideo newLike = new()
                    {
                        AccountId = data.AccountId,
                        VideoId = data.VideoId,
                    };

                    if (checkDisLike != null)
                    {
                        _dbContext.Remove(checkDisLike);

                    }
                    _dbContext.Add(newLike);
                    _dbContext.SaveChanges();

                    var video = _dbContext.Videos
                   .Include(v => v.ChannelOwner)
                   .Include(v => v.UserLikeVideos)
                   .Include(v => v.UserDisLikeVideos)
                   .SingleOrDefault(v => v.Id == data.VideoId);
                    VideoDTO videoDTO = _mapper.Map<VideoDTO>(video);
                    return StatusCode(StatusCodes.Status200OK, new { success = true, video = videoDTO });
                }
                else
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    var video = _dbContext.Videos
                         .Include(v => v.ChannelOwner)
                         .Include(v => v.UserLikeVideos)
                         .Include(v => v.UserDisLikeVideos)
                         .SingleOrDefault(v => v.Id == data.VideoId);
                    VideoDTO videoDTO = _mapper.Map<VideoDTO>(video);
                    return StatusCode(StatusCodes.Status200OK, new { success = true, video = videoDTO });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }

        [HttpPost("dislike_video")]
        public ActionResult DisLikeVideo([FromBody] ICreateLikeDisLikeVideo data)
        {

            if (data.VideoId != 0 && data.AccountId != 0)
            {
                var check = _dbContext.UserDisLikeVideos.Where(ulv => ulv.AccountId == data.AccountId && ulv.VideoId == data.VideoId).FirstOrDefault();

                if (check == null)
                {
                    var checkLike = _dbContext.UserLikeVideos.Where(ulv => ulv.AccountId == data.AccountId && ulv.VideoId == data.VideoId).FirstOrDefault();
                    UserDisLikeVideo newDisLike = new()
                    {
                        AccountId = data.AccountId,
                        VideoId = data.VideoId,
                    };
                    if (checkLike != null)
                    {
                        _dbContext.Remove(checkLike);

                    }
                    _dbContext.Add(newDisLike);
                    _dbContext.SaveChanges();
                    var video = _dbContext.Videos
                      .Include(v => v.ChannelOwner)
                      .Include(v => v.UserLikeVideos)
                      .Include(v => v.UserDisLikeVideos)
                      .SingleOrDefault(v => v.Id == data.VideoId);
                    VideoDTO videoDTO = _mapper.Map<VideoDTO>(video);
                    return StatusCode(StatusCodes.Status200OK, new { success = true, video = videoDTO });
                }
                else
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    var video = _dbContext.Videos
                      .Include(v => v.ChannelOwner)
                      .Include(v => v.UserLikeVideos)
                      .Include(v => v.UserDisLikeVideos)
                      .SingleOrDefault(v => v.Id == data.VideoId);
                    VideoDTO videoDTO = _mapper.Map<VideoDTO>(video);
                    return StatusCode(StatusCodes.Status200OK, new { success = true, video = videoDTO });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("create_comment")]
        public async Task<ActionResult> CreateComment([FromBody] ICreateComment commentData)
        {

            if (commentData.Content != null && commentData.VideoId != 0 && commentData.AccountOwnerId != 0)
            {

                Comment newComment = new()
                {
                    AccountOwnerId = commentData.AccountOwnerId,
                    VideoId = commentData.VideoId,
                    Content = commentData.Content,
                    ParentCommentId = commentData.ParentCommentId == 0 ? null : commentData.ParentCommentId,
                    ReplyToAccountId = commentData.ReplyToAccountId == 0 ? null : commentData.ReplyToAccountId,

                };
                await _dbContext.AddAsync(newComment);
                await _dbContext.SaveChangesAsync();


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
                    .Where(c => c.VideoId == commentData.VideoId);



                List<CommentDTO> commentDTO = _mapper.Map<List<CommentDTO>>(comments);

                return StatusCode(StatusCodes.Status200OK, new { success = true, comments = commentDTO });

            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("create_watched_video")]
        public async Task<ActionResult> CreateWatchedVideo([FromBody] ICreateUserWatchedVideo data)
        {

            if (data.VideoId != 0 && data.AccountId != 0)
            {

                var check = _dbContext.UserWatchedVideos.Where(u => u.AccountId == data.AccountId && u.VideoId == data.VideoId).FirstOrDefault();
                if (check == null)
                {
                    UserWatchedVideo newUserWatchedVideo = new()
                    {
                        AccountId = data.AccountId,
                        VideoId = data.VideoId
                    };

                    _dbContext.Add(newUserWatchedVideo);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
                else
                {
                    UserWatchedVideo newUserWatchedVideo = new()
                    {
                        AccountId = data.AccountId,
                        VideoId = data.VideoId
                    };

                    _dbContext.Remove(check);
                    _dbContext.Add(newUserWatchedVideo);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }

            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("create_watch_late_video")]
        public async Task<ActionResult> CreateWatchLateVideo([FromBody] ICreateUserWatchedVideo data)
        {

            if (data.VideoId != 0 && data.AccountId != 0)
            {

                var check = _dbContext.UserWatchLateVideos.Where(u => u.AccountId == data.AccountId && u.VideoId == data.VideoId).FirstOrDefault();
                if (check == null)
                {
                    UserWatchLateVideo newUserWatchLateVideo = new()
                    {
                        AccountId = data.AccountId,
                        VideoId = data.VideoId
                    };

                    _dbContext.Add(newUserWatchLateVideo);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
                else
                {
                    UserWatchLateVideo newUserWatchLateVideo = new()
                    {
                        AccountId = data.AccountId,
                        VideoId = data.VideoId
                    };

                    _dbContext.Remove(check);
                    _dbContext.Add(newUserWatchLateVideo);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }

            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }


        [HttpDelete("delete_comment")]
        public ActionResult DeleteComment(int id, int accountId)
        {

            if (id != 0 && accountId != 0)
            {
                var check = _dbContext.Comments.Where(c => c.Id == id && c.AccountOwnerId == accountId).FirstOrDefault();

                if (check != null)
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Xóa bình luận thành công" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { success = true, msg = "Bình luận không tồn tại" });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpPost("like_comment")]
        public ActionResult LikeComment([FromBody] ICreateLikeDisLikeComment data)
        {

            if (data.CommentId != 0 && data.AccountId != 0)
            {
                var check = _dbContext.UserLikeComments.Where(l => l.AccountId ==  data.AccountId && l.CommentId == data.CommentId  ).FirstOrDefault();

                if (check == null)

                {
                    var checkDisLike = _dbContext.UserDisLikeComments.Where(l => l.AccountId == data.AccountId && l.CommentId == data.CommentId).FirstOrDefault();
                    UserLikeComment newLike = new()
                    {
                        AccountId = data.AccountId,
                        CommentId = data.CommentId,

                    };
                    if(checkDisLike != null)
                    {

                        _dbContext.Remove(checkDisLike);
                    }
                    _dbContext.Add(newLike);
                    _dbContext.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
                else
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }

        [HttpPost("dislike_comment")]
        public ActionResult DisLikeComment([FromBody] ICreateLikeDisLikeComment data)
        {

            if (data.CommentId != 0 && data.AccountId != 0 )
            {
                var check = _dbContext.UserDisLikeComments.Where(l => l.AccountId == data.AccountId && l.CommentId == data.CommentId).FirstOrDefault();

                if (check == null)
                {
                    var checkLike = _dbContext.UserLikeComments.Where(l => l.AccountId == data.AccountId && l.CommentId == data.CommentId).FirstOrDefault();
                    UserDisLikeComment newDisLike = new()
                    {
                        AccountId = data.AccountId,
                        CommentId = data.CommentId,
                    };

                    if(checkLike != null)
                    {
                        _dbContext.Remove(checkLike);
                    }
                    _dbContext.Add(newDisLike);
                    _dbContext.SaveChanges();




                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
                else
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }

            }


            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }

        [HttpPost("subcribe_channel")]
        public ActionResult SubcribeChannel([FromBody] ICreateSubcribeChannel data)
        {
            if (data.AccountId != 0 && data.ChannelId != 0)
            {
                var check = _dbContext.Subcribers.Where(s => s.AccountId == data.AccountId && s.ChannelId == data.ChannelId).FirstOrDefault();
                if (check == null)
                {
                    Subcriber newSubcriber = new()
                    {
                        AccountId = data.AccountId,
                        ChannelId = data.ChannelId
                    };
                    _dbContext.Add(newSubcriber);
                    _dbContext.SaveChanges();

                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
                else
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }

        [HttpPost("un_subcribe_channel")]
        public ActionResult UnSubcribeChannel([FromBody] ICreateSubcribeChannel data)
        {
            if (data.AccountId != 0 && data.ChannelId != 0)
            {
                var check = _dbContext.Subcribers.Where(s => s.AccountId == data.AccountId && s.ChannelId == data.ChannelId).FirstOrDefault();
                if (check == null)
                {

                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
                }
                else
                {
                    _dbContext.Remove(check);
                    _dbContext.SaveChanges();
                    return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Hủy đăng ký thành công" });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("info/{id}")]
        public ActionResult GetUserInfo(string id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
            }
            var user = _dbContext.Accounts.Include(a => a.Channel).SingleOrDefault(a => a.Id == Int16.Parse(id));

            AccountDTO accountDTO = _mapper.Map<AccountDTO>(user);

            return StatusCode(StatusCodes.Status200OK, new { success = true, account = accountDTO });
        }
        [HttpPost("forget_password")]
        public async Task<ActionResult> ForgetPassword([FromBody] ISendMail mailRequest)
        {
            Account account = _dbContext.Accounts.FirstOrDefault(a => a.Email == mailRequest.ToEmail);

            if (account == null) return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Tài khoản không tồn tại" });

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSetting.Email);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;


            var builder = new BodyBuilder();


            var token = GenerateToken(account, 30 * 60);

            builder.HtmlBody = MailContent(mailRequest.ToEmail, token);
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_emailSetting.Host, _emailSetting.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_emailSetting.Email, _emailSetting.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);

            return StatusCode(StatusCodes.Status200OK, new { success = true });
        }
        [HttpPost("change_password")]
        public ActionResult ChangePasswordAsync([FromQuery] string token, [FromBody] IChangePassword data)
        {

            if (VerifyToken(token) && IsTokenExpired(token))
            {

                ClaimsPrincipal tokenData = DecodeToken(token);

                if (tokenData == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
                }
                int accountId = Int16.TryParse(tokenData?.FindFirst("Id")?.Value, out var parsedValue) ? parsedValue : 0;


                if (accountId == 0)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
                }


                Account account = _dbContext.Accounts.Find(accountId);



                if (account == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
                }


                if (data.ConfirmNewPassword != data.NewPassword)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
                }

                if (!IsStrongPassword(data.NewPassword))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(data.NewPassword);
                account.Password = hashedPassword;

                _dbContext.Accounts.Update(account);

                _dbContext.SaveChanges();


                return StatusCode(StatusCodes.Status200OK, new { success = true, msg = "Đổi mật khẩu thành công" });
            }

            return StatusCode(StatusCodes.Status401Unauthorized, new { success = false, msg = "Invalid token" });
        }
        [HttpGet("gets")]
        public ActionResult GetAccount()
        {
            var accounts = _dbContext.Accounts.ToList();

            return StatusCode(StatusCodes.Status200OK, new { accounts });
        }
        [HttpGet("{accountId}/get_video_like")]
        public ActionResult GetVideoLike(int accountId)
        {
            var ulvs = _dbContext.UserLikeVideos.Include(ulv => ulv.Video).Where(ulv => ulv.AccountId == accountId).OrderBy(ulv => ulv.LikeAt).ToList();

            if (ulvs != null)
            {
                List<UserLikeVideoDTO> userLikeVideoDTOs = _mapper.Map<List<UserLikeVideoDTO>>(ulvs);
                return StatusCode(StatusCodes.Status200OK, new { success = true, videos = userLikeVideoDTOs });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("{accountId}/get_video_dis_like")]
        public ActionResult GetVideoDisLike(int accountId)
        {
            var ulvs = _dbContext.UserDisLikeVideos.Include(ulv => ulv.Video).Where(ulv => ulv.AccountId == accountId).OrderBy(ulv => ulv.DisLikeAt).ToList();

            if (ulvs != null)
            {
                List<UserDisLikeVideoDTO> userDisLikeVideoDTOs = _mapper.Map<List<UserDisLikeVideoDTO>>(ulvs);
                return StatusCode(StatusCodes.Status200OK, new { success = true, videos = userDisLikeVideoDTOs });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("{accountId}/get_video_watched")]
        public ActionResult GetVideoWatched(int accountId)
        {
            var ulvs = _dbContext.UserWatchedVideos.Include(ulv => ulv.Video).ThenInclude(c => c.ChannelOwner).Where(ulv => ulv.AccountId == accountId).OrderBy(ulv => ulv.WatchedAt).ToList();

            if (ulvs != null)
            {
                List<UserVideoWatchedDTO> userLikeVideoDTOs = _mapper.Map<List<UserVideoWatchedDTO>>(ulvs);
                return StatusCode(StatusCodes.Status200OK, new { success = true, videos = userLikeVideoDTOs });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("{accountId}/get_video_watch_late")]
        public ActionResult GetVideoWatcheLate(int accountId)
        {
            var ulvs = _dbContext.UserWatchLateVideos.Include(ulv => ulv.Video).ThenInclude(c => c.ChannelOwner).Where(ulv => ulv.AccountId == accountId).ToList();

            if (ulvs != null)
            {
                List<UserVideoWatchLateDTO> userLikeVideoDTOs = _mapper.Map<List<UserVideoWatchLateDTO>>(ulvs);
                return StatusCode(StatusCodes.Status200OK, new { success = true, videos = userLikeVideoDTOs });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("{accountId}/get_channel_subcribed")]
        public ActionResult GetChannelSubcribe(int accountId)
        {
            var ulvs = _dbContext.Subcribers.Include(s => s.Channel).Where(s => s.AccountId == accountId).ToList();

            if (ulvs != null)
            {
                List<SubcriberDTO> subcriberDTOs = _mapper.Map<List<SubcriberDTO>>(ulvs);
                return StatusCode(StatusCodes.Status200OK, new { success = true, subcribedChannel = subcriberDTOs });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        [HttpGet("{accountId}/get_channel_subcribed_video")]
        public ActionResult GetChannelSubcribeVideo(int accountId, int page = 1, int limit = 10)
        {
            if (accountId == 0) return StatusCode(StatusCodes.Status400BadRequest, new { success = false });
            var channelSubcribeds = _dbContext.Subcribers.Include(s => s.Channel).Where(s => s.AccountId == accountId).ToList();

            List<SubcribedChannelIdIDTO> channelIdIDTOs = _mapper.Map<List<SubcribedChannelIdIDTO>>(channelSubcribeds);
            List<int> channelIdList = channelIdIDTOs.Select(info => info.ChannelId).ToList();
            if (channelIdIDTOs != null)
            {

                var totalCount = _dbContext.Videos.Where(v => channelIdList.Contains(v.ChannelOwnerId)).Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / limit);
                var videos = _dbContext.Videos
                                        .Include(v => v.ChannelOwner)
                                        .Where(v => channelIdList.Contains(v.ChannelOwnerId))
                                        .OrderByDescending(v => v.CreateAt)
                                         .Skip((page - 1) * limit)
                                         .Take(limit)
                                         .ToList();

                var nextPage = page < totalPages ? true : false;

                var previousPage = page > 1 ? true : false;
                List<VideoDTO> videoDTOs = _mapper.Map<List<VideoDTO>>(videos);
                return StatusCode(StatusCodes.Status200OK, new { success = true, pageResult = new { totalPage = totalPages, currentPage = page, limitVideo = limit, nextPage, previousPage, videos = videoDTOs } });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new { success = false, msg = "Thiếu dữ liệu" });
        }
        private ClaimsPrincipal DecodeToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
        }
        private bool VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                return true; // Token hợp lệ
            }
            catch (SecurityTokenException)
            {
                return false; // Token không hợp lệ
            }
        }


        private string GenerateToken(Account user, int expiresSecond)
        {
            var key = _config["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("Id",user.Id.ToString()),

            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMilliseconds(expiresSecond),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        private bool IsTokenExpired(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return true;

                var now = DateTime.UtcNow;


                return now > jwtToken.ValidTo;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private string MailContent(string email, string token)
        {
            var link = $"http://localhost:3000/change_password?token={token}";
            string MailCustome = $"<div style=\"font-family: Arial, sans-serif; margin: 0; padding: 0; display: flex; align-items: center; justify-content: center;width: 100wh; height: 100vh;\">\r\n  <div style=\"background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); overflow: hidden; \">\r\n    <div style=\"background-color: #3498db; color: #fff; text-align: center; padding: 20px;\">\r\n      <h1>Khôi phục mật khẩu</h1>\r\n    </div>\r\n    <div style=\"padding: 30px;\">\r\n      <p>Xin chào {email},</p>\r\n      <p>Chúng tôi đã nhận được yêu cầu khôi phục mật khẩu của bạn. Nếu bạn không phải là người gửi yêu cầu này, hãy bỏ qua email này.</p>\r\n      <p>Để khôi phục mật khẩu, hãy nhấn vào nút bên dưới:</p>\r\n      <a href=\"{link}\" style=\"display: inline-block; font-size: 16px; padding: 10px 20px; margin-top: 20px; text-decoration: none; background-color: #3498db; color: #fff; border-radius: 4px;\">Khôi phục mật khẩu</a>\r\n      <p>Nếu nút trên không hoạt động, bạn cũng có thể sao chép và dán đường link sau vào trình duyệt của mình:</p>\r\n      <p>{link}/p>\r\n      <p>Đường link này sẽ hết hạn sau 30 phút.</p>\r\n    </div>\r\n     </div>\r\n</div>";
            return MailCustome;
        }
        public static bool IsValidEmail(string email)
        {

            string pattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$";


            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

        public static bool IsStrongPassword(string password)
        {

            if (password.Length <= 8)
            {
                return false;
            }


            if (!password.Any(char.IsUpper))
            {
                return false;
            }


            if (!password.Any(char.IsDigit))
            {
                return false;
            }


            return true;
        }
        public static string ExtractUsername(string email)
        {
            int atIndex = email.IndexOf('@');

            if (atIndex != -1)
            {
                return email.Substring(0, atIndex);
            }
            else
            {
                // Handle the case where the email does not contain '@'
                return email;
            }
        }

    }
}
