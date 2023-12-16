using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NextYTB.Model;

public partial class NextTubContext : DbContext
{
    public NextTubContext()
    {
    }

    public NextTubContext(DbContextOptions<NextTubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Subcriber> Subcribers { get; set; }

    public virtual DbSet<UserDisLikeComment> UserDisLikeComments { get; set; }

    public virtual DbSet<UserDisLikeVideo> UserDisLikeVideos { get; set; }

    public virtual DbSet<UserLikeComment> UserLikeComments { get; set; }

    public virtual DbSet<UserLikeVideo> UserLikeVideos { get; set; }

    public virtual DbSet<UserWatchLateVideo> UserWatchLateVideos { get; set; }

    public virtual DbSet<UserWatchedVideo> UserWatchedVideos { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=NextTub;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC072892CB95");

            entity.ToTable("Account");

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.RefreshToken).IsUnicode(false);
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Channel__3214EC072C756ADB");

            entity.ToTable("Channel");

            entity.HasIndex(e => e.AccountOwnerId, "UQ__Channel__CB75F2FF3539E6EE").IsUnique();

            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.AccountOwner).WithOne(p => p.Channel)
                .HasForeignKey<Channel>(d => d.AccountOwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Channel__Account__3A81B327");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comment__3214EC077BE51353");

            entity.ToTable("Comment");

            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AccountOwner).WithMany(p => p.CommentAccountOwners)
                .HasForeignKey(d => d.AccountOwnerId)
                .HasConstraintName("FK__Comment__Account__45F365D3");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("FK__Comment__ParentC__48CFD27E");

            entity.HasOne(d => d.ReplyToAccount).WithMany(p => p.CommentReplyToAccounts)
                .HasForeignKey(d => d.ReplyToAccountId)
                .HasConstraintName("FK__Comment__ReplyTo__46E78A0C");

            entity.HasOne(d => d.Video).WithMany(p => p.Comments)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("FK__Comment__VideoId__47DBAE45");
        });

        modelBuilder.Entity<Subcriber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subcribe__3214EC0725DDCA8F");

            entity.ToTable("Subcriber");

            entity.HasOne(d => d.Account).WithMany(p => p.Subcribers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subcriber__Accou__3D5E1FD2");

            entity.HasOne(d => d.Channel).WithMany(p => p.Subcribers)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subcriber__Chann__3E52440B");
        });

        modelBuilder.Entity<UserDisLikeComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserDisL__3214EC07D7991C26");

            entity.ToTable("UserDisLikeComment");

            entity.Property(e => e.DisLikeAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.UserDisLikeComments)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserDisLi__Accou__5AEE82B9");

            entity.HasOne(d => d.Comment).WithMany(p => p.UserDisLikeComments)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserDisLi__Comme__5BE2A6F2");
        });

        modelBuilder.Entity<UserDisLikeVideo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserDisL__3214EC074FA7B540");

            entity.ToTable("UserDisLikeVideo");

            entity.Property(e => e.DisLikeAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.UserDisLikeVideos)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserDisLi__Accou__5165187F");

            entity.HasOne(d => d.Video).WithMany(p => p.UserDisLikeVideos)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("FK__UserDisLi__Video__52593CB8");
        });

        modelBuilder.Entity<UserLikeComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserLike__3214EC0740321150");

            entity.ToTable("UserLikeComment");

            entity.Property(e => e.LikeAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.UserLikeComments)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserLikeC__Accou__5629CD9C");

            entity.HasOne(d => d.Comment).WithMany(p => p.UserLikeComments)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserLikeC__Comme__571DF1D5");
        });

        modelBuilder.Entity<UserLikeVideo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserLike__3214EC071F874332");

            entity.ToTable("UserLikeVideo");

            entity.Property(e => e.LikeAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.UserLikeVideos)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserLikeV__Accou__4CA06362");

            entity.HasOne(d => d.Video).WithMany(p => p.UserLikeVideos)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("FK__UserLikeV__Video__4D94879B");
        });

        modelBuilder.Entity<UserWatchLateVideo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserWatc__3214EC072E29FAB6");

            entity.ToTable("UserWatchLateVideo");

            entity.HasOne(d => d.Account).WithMany(p => p.UserWatchLateVideos)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserWatch__Accou__6383C8BA");

            entity.HasOne(d => d.Video).WithMany(p => p.UserWatchLateVideos)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("FK__UserWatch__Video__6477ECF3");
        });

        modelBuilder.Entity<UserWatchedVideo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserWatc__3214EC07FC21AFF8");

            entity.ToTable("UserWatchedVideo");

            entity.Property(e => e.WatchedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.UserWatchedVideos)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserWatch__Accou__5FB337D6");

            entity.HasOne(d => d.Video).WithMany(p => p.UserWatchedVideos)
                .HasForeignKey(d => d.VideoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserWatch__Video__60A75C0F");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Video__3214EC074790DAA7");

            entity.ToTable("Video");

            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Duration)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Poster)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Src)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.ChannelOwner).WithMany(p => p.Videos)
                .HasForeignKey(d => d.ChannelOwnerId)
                .HasConstraintName("FK__Video__ChannelOw__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
