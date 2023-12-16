using AutoMapper;
using NextYTB.DTO;
using NextYTB.Model;


namespace NextYTB.Config
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {

            CreateMap<VideoDTO, Video>();
            CreateMap<Account, AccountDTO>()
                .ForMember(a => a.Channel, opt => opt.MapFrom(src => src.Channel));
            CreateMap<Account, AccountInfoDTO>();


            CreateMap<Channel, ChannelDTO>();
            CreateMap<Channel, ChannelInfoDTO>()
                .ForMember(c => c.SubcriberCount, opt => opt.MapFrom(src => src.Subcribers.Count))
                .ForMember(dest => dest.VideoCount, opt => opt.MapFrom(src => src.Videos.Count));
            CreateMap<Video, VideoDTO>()
                .ForMember(c => c.View, opt => opt.MapFrom(src => src.UserWatchedVideos.Count))
                .ForMember(c => c.Channel, opt => opt.MapFrom(src => src.ChannelOwner))
                 .ForMember(c => c.Like, opt => opt.MapFrom(src => src.UserLikeVideos.Count))
                .ForMember(c => c.DisLike, opt => opt.MapFrom(src => src.UserDisLikeVideos.Count));

            CreateMap<Comment, CommentDTO>()
                .ForMember(c => c.Reply, opt => opt.MapFrom(src => src.InverseParentComment.OrderBy(r => r.CreateAt)))
                .ForMember(c => c.Like, opt => opt.MapFrom(src => src.UserLikeComments))
                .ForMember(c => c.DisLike, opt => opt.MapFrom(src => src.UserDisLikeComments))
                .ForMember(c => c.ReplyToAccount, opt => opt.MapFrom(src => src.ReplyToAccount));



            CreateMap<Subcriber, SubcriberDTO>();

            CreateMap<UserLikeComment, UserLikeDisLikeCommentDTO>()
                .ForMember(u => u.CreateAt, opt => opt.MapFrom(src => src.LikeAt));
            CreateMap<UserDisLikeComment, UserLikeDisLikeCommentDTO>()
                .ForMember(u => u.CreateAt, opt => opt.MapFrom(src => src.DisLikeAt));

            CreateMap<UserLikeVideo, UserLikeVideoDTO>();
            CreateMap<UserDisLikeVideo, UserDisLikeVideoDTO>();
            CreateMap<UserWatchedVideo, UserVideoWatchedDTO>();
            CreateMap<UserWatchLateVideo, UserVideoWatchLateDTO>();
            CreateMap<Subcriber, SubcribedChannelIdIDTO>();
        }
    }
}
