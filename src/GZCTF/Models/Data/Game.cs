﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using GZCTF.Models.Request.Edit;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace GZCTF.Models.Data;

public class Game
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// 比赛标题
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Token 签名公钥
    /// </summary>
    [Required]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Token 签名私钥
    /// </summary>
    [Required]
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [Required]
    public bool Hidden { get; set; }

    /// <summary>
    /// 头图哈希
    /// </summary>
    [MaxLength(64)]
    public string? PosterHash { get; set; }

    /// <summary>
    /// 比赛描述
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// 比赛详细介绍
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 报名队伍免审核
    /// </summary>
    public bool AcceptWithoutReview { get; set; }

    /// <summary>
    /// 比赛邀请码
    /// </summary>
    public string? InviteCode { get; set; }

    /// <summary>
    /// 参赛所属单位列表
    /// </summary>
    public HashSet<string>? Organizations { get; set; }

    /// <summary>
    /// 队员数量限制, 0 为无上限
    /// </summary>
    public int TeamMemberCountLimit { get; set; }

    /// <summary>
    /// 队伍同时开启的容器数量限制
    /// </summary>
    public int ContainerCountLimit { get; set; } = 3;

    /// <summary>
    /// 开始时间
    /// </summary>
    [Required]
    [JsonPropertyName("start")]
    public DateTimeOffset StartTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// 结束时间
    /// </summary>
    [Required]
    [JsonPropertyName("end")]
    public DateTimeOffset EndTimeUTC { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Writeup 提交截止时间
    /// </summary>
    [Required]
    [JsonPropertyName("wpddl")]
    public DateTimeOffset WriteupDeadline { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

    /// <summary>
    /// Writeup 附加说明
    /// </summary>
    [Required]
    [JsonPropertyName("wpnote")]
    public string WriteupNote { get; set; } = string.Empty;

    /// <summary>
    /// 三血加分
    /// </summary>
    [Required]
    public BloodBonus BloodBonus { get; set; } = BloodBonus.Default;

    [NotMapped]
    [JsonIgnore]
    public bool IsActive => StartTimeUTC <= DateTimeOffset.Now && DateTimeOffset.Now <= EndTimeUTC;

    [NotMapped]
    public string? PosterUrl => PosterHash is null ? null : $"/assets/{PosterHash}/poster";

    [NotMapped]
    public string TeamHashSalt => $"GZCTF@{PrivateKey}@PK".StrSHA256();

    internal void GenerateKeyPair(byte[]? xorkey)
    {
        SecureRandom sr = new();
        Ed25519KeyPairGenerator kpg = new();
        kpg.Init(new Ed25519KeyGenerationParameters(sr));
        AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();
        var privateKey = (Ed25519PrivateKeyParameters)kp.Private;
        var publicKey = (Ed25519PublicKeyParameters)kp.Public;

        if (xorkey is null)
            PrivateKey = Base64.ToBase64String(privateKey.GetEncoded());
        else
            PrivateKey = Base64.ToBase64String(Codec.Xor(privateKey.GetEncoded(), xorkey));

        PublicKey = Base64.ToBase64String(publicKey.GetEncoded());
    }

    internal string Sign(string str, byte[]? xorkey)
    {
        Ed25519PrivateKeyParameters privateKey;
        if (xorkey is null)
            privateKey = new(Codec.Base64.DecodeToBytes(PrivateKey), 0);
        else
            privateKey = new(Codec.Xor(Codec.Base64.DecodeToBytes(PrivateKey), xorkey), 0);

        return DigitalSignature.GenerateSignature(str, privateKey, SignAlgorithm.Ed25519);
    }

    internal bool Verify(string data, string sign)
    {
        Ed25519PublicKeyParameters publicKey = new(Codec.Base64.DecodeToBytes(PublicKey), 0);

        return DigitalSignature.VerifySignature(data, sign, publicKey, SignAlgorithm.Ed25519);
    }

    internal Game Update(GameInfoModel model)
    {
        Title = model.Title;
        Content = model.Content;
        Summary = model.Summary;
        Hidden = model.Hidden;
        PracticeMode = model.PracticeMode;
        AcceptWithoutReview = model.AcceptWithoutReview;
        InviteCode = model.InviteCode;
        Organizations = model.Organizations ?? Organizations;
        EndTimeUTC = model.EndTimeUTC;
        StartTimeUTC = model.StartTimeUTC;
        WriteupDeadline = model.WriteupDeadline;
        TeamMemberCountLimit = model.TeamMemberCountLimit;
        ContainerCountLimit = model.ContainerCountLimit;
        WriteupNote = model.WriteupNote;
        BloodBonus = BloodBonus.FromValue(model.BloodBonusValue);

        return this;
    }

    #region Db Relationship

    /// <summary>
    /// 比赛事件
    /// </summary>
    [JsonIgnore]
    public List<GameEvent> GameEvents { get; set; } = new();

    /// <summary>
    /// 比赛通知
    /// </summary>
    [JsonIgnore]
    public List<GameNotice> GameNotices { get; set; } = new();

    /// <summary>
    /// 比赛题目
    /// </summary>
    [JsonIgnore]
    public List<Challenge> Challenges { get; set; } = new();

    /// <summary>
    /// 比赛提交
    /// </summary>
    [JsonIgnore]
    public List<Submission> Submissions { get; set; } = new();

    /// <summary>
    /// 比赛队伍参赛对象
    /// </summary>
    [JsonIgnore]
    public HashSet<Participation> Participations { get; set; } = new();

    /// <summary>
    /// 比赛队伍
    /// </summary>
    [JsonIgnore]
    public ICollection<Team>? Teams { get; set; }

    /// <summary>
    /// 比赛是否为练习模式（比赛结束够依然可以进行大部分操作）
    /// </summary>
    public bool PracticeMode { get; set; } = true;

    #endregion Db Relationship
}