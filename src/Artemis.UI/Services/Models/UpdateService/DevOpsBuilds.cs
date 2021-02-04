using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Artemis.UI.Services.Models.UpdateService
{
    public class DevOpsBuilds
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public List<DevOpsBuild> Builds { get; set; }
    }

    public class DevOpsBuild
    {
        [JsonProperty("_links")]
        public BuildLinks Links { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("tags")]
        public List<object> Tags { get; set; }

        [JsonProperty("validationResults")]
        public List<object> ValidationResults { get; set; }

        [JsonProperty("plans")]
        public List<Plan> Plans { get; set; }

        [JsonProperty("triggerInfo")]
        public TriggerInfo TriggerInfo { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("buildNumber")]
        public string BuildNumber { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("queueTime")]
        public DateTimeOffset QueueTime { get; set; }

        [JsonProperty("startTime")]
        public DateTimeOffset StartTime { get; set; }

        [JsonProperty("finishTime")]
        public DateTimeOffset FinishTime { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("definition")]
        public Definition Definition { get; set; }

        [JsonProperty("buildNumberRevision")]
        public long BuildNumberRevision { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("sourceBranch")]
        public string SourceBranch { get; set; }

        [JsonProperty("sourceVersion")]
        public string SourceVersion { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("requestedFor")]
        public LastChangedBy RequestedFor { get; set; }

        [JsonProperty("requestedBy")]
        public LastChangedBy RequestedBy { get; set; }

        [JsonProperty("lastChangedDate")]
        public DateTimeOffset LastChangedDate { get; set; }

        [JsonProperty("lastChangedBy")]
        public LastChangedBy LastChangedBy { get; set; }

        [JsonProperty("orchestrationPlan")]
        public Plan OrchestrationPlan { get; set; }

        [JsonProperty("logs")]
        public Logs Logs { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("keepForever")]
        public bool KeepForever { get; set; }

        [JsonProperty("retainedByRelease")]
        public bool RetainedByRelease { get; set; }

        [JsonProperty("triggeredByBuild")]
        public object TriggeredByBuild { get; set; }
    }

    public class Definition
    {
        [JsonProperty("drafts")]
        public List<object> Drafts { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("queueStatus")]
        public string QueueStatus { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }
    }

    public class Project
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("lastUpdateTime")]
        public DateTimeOffset LastUpdateTime { get; set; }
    }

    public class LastChangedBy
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public LastChangedByLinks Links { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uniqueName")]
        public object UniqueName { get; set; }

        [JsonProperty("imageUrl")]
        public object ImageUrl { get; set; }

        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }
    }

    public class LastChangedByLinks
    {
        [JsonProperty("avatar")]
        public Badge Avatar { get; set; }
    }

    public class Badge
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public class BuildLinks
    {
        [JsonProperty("self")]
        public Badge Self { get; set; }

        [JsonProperty("web")]
        public Badge Web { get; set; }

        [JsonProperty("sourceVersionDisplayUri")]
        public Badge SourceVersionDisplayUri { get; set; }

        [JsonProperty("timeline")]
        public Badge Timeline { get; set; }

        [JsonProperty("badge")]
        public Badge Badge { get; set; }
    }

    public class Logs
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public class Plan
    {
        [JsonProperty("planId")]
        public Guid PlanId { get; set; }
    }

    public class Properties
    {
    }

    public class Repository
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("clean")]
        public object Clean { get; set; }

        [JsonProperty("checkoutSubmodules")]
        public bool CheckoutSubmodules { get; set; }
    }

    public class TriggerInfo
    {
        [JsonProperty("ci.sourceBranch")]
        public string CiSourceBranch { get; set; }

        [JsonProperty("ci.sourceSha")]
        public string CiSourceSha { get; set; }

        [JsonProperty("ci.message")]
        public string CiMessage { get; set; }

        [JsonProperty("ci.triggerRepository")]
        public string CiTriggerRepository { get; set; }
    }
}