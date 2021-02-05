using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Artemis.UI.Services.Models.UpdateService
{
    public class GitHubDifference
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("permalink_url")]
        public Uri PermalinkUrl { get; set; }

        [JsonProperty("diff_url")]
        public Uri DiffUrl { get; set; }

        [JsonProperty("patch_url")]
        public Uri PatchUrl { get; set; }

        [JsonProperty("base_commit")]
        public BaseCommitClass BaseCommit { get; set; }

        [JsonProperty("merge_base_commit")]
        public BaseCommitClass MergeBaseCommit { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("ahead_by")]
        public long AheadBy { get; set; }

        [JsonProperty("behind_by")]
        public long BehindBy { get; set; }

        [JsonProperty("total_commits")]
        public long TotalCommits { get; set; }

        [JsonProperty("commits")]
        public List<BaseCommitClass> Commits { get; set; }

        [JsonProperty("files")]
        public List<File> Files { get; set; }
    }

    public class BaseCommitClass
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("commit")]
        public BaseCommitCommit Commit { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("comments_url")]
        public Uri CommentsUrl { get; set; }

        [JsonProperty("author")]
        public BaseCommitAuthor Author { get; set; }

        [JsonProperty("committer")]
        public BaseCommitAuthor Committer { get; set; }

        [JsonProperty("parents")]
        public List<Parent> Parents { get; set; }
    }

    public class BaseCommitAuthor
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("followers_url")]
        public Uri FollowersUrl { get; set; }

        [JsonProperty("following_url")]
        public string FollowingUrl { get; set; }

        [JsonProperty("gists_url")]
        public string GistsUrl { get; set; }

        [JsonProperty("starred_url")]
        public string StarredUrl { get; set; }

        [JsonProperty("subscriptions_url")]
        public Uri SubscriptionsUrl { get; set; }

        [JsonProperty("organizations_url")]
        public Uri OrganizationsUrl { get; set; }

        [JsonProperty("repos_url")]
        public Uri ReposUrl { get; set; }

        [JsonProperty("events_url")]
        public string EventsUrl { get; set; }

        [JsonProperty("received_events_url")]
        public Uri ReceivedEventsUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class BaseCommitCommit
    {
        [JsonProperty("author")]
        public PurpleAuthor Author { get; set; }

        [JsonProperty("committer")]
        public PurpleAuthor Committer { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("tree")]
        public Tree Tree { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("comment_count")]
        public long CommentCount { get; set; }

        [JsonProperty("verification")]
        public Verification Verification { get; set; }
    }

    public class PurpleAuthor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }
    }

    public class Tree
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public class Verification
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }
    }

    public class Parent
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }
    }

    public class File
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("additions")]
        public long Additions { get; set; }

        [JsonProperty("deletions")]
        public long Deletions { get; set; }

        [JsonProperty("changes")]
        public long Changes { get; set; }

        [JsonProperty("blob_url")]
        public Uri BlobUrl { get; set; }

        [JsonProperty("raw_url")]
        public Uri RawUrl { get; set; }

        [JsonProperty("contents_url")]
        public Uri ContentsUrl { get; set; }

        [JsonProperty("patch")]
        public string Patch { get; set; }

        [JsonProperty("previous_filename", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviousFilename { get; set; }
    }
}