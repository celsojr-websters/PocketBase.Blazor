namespace PocketBase.Blazor.UnitTests.Domain.Settings;

using System.Collections.Generic;
using System.Text.Json;
using Blazor.Options;
using Blazor.Requests.Settings;
using Blazor.Responses.Settings;
using FluentAssertions;

[Trait("Category", "Unit")]
public class SettingsRequestResponseTests
{
    [Fact]
    public void SettingsUpdateRequest_ShouldSerializeWithPocketBaseFieldNames()
    {
        // Arrange
        SettingsUpdateRequest request = new SettingsUpdateRequest
        {
            SuperuserIPs = ["127.0.0.1"],
            Smtp = new SmtpSettingsUpdateRequest
            {
                Enabled = true,
                Port = 587,
                Host = "smtp.example.com",
                AuthMethod = "PLAIN",
                LocalName = "hello.example.com",
            },
            Backups = new BackupSettingsUpdateRequest
            {
                Cron = "0 0 * * *",
                CronMaxKeep = 3,
                S3 = new S3SettingsUpdateRequest { Enabled = true },
            },
            S3 = new S3SettingsUpdateRequest
            {
                Enabled = true,
                Bucket = "my-bucket",
                Region = "us-east-1",
                ForcePathStyle = true,
            },
            Meta = new MetaSettingsUpdateRequest
            {
                AccentColor = "#1055c9",
                AppName = "Acme",
                HideControls = false,
            },
            RateLimits = new RateLimitSettingsUpdateRequest
            {
                Enabled = true,
                ExcludedIPs = ["10.0.0.0/8"],
                Rules =
                [
                    new RateLimitRuleUpdateRequest { Label = "*:auth", MaxRequests = 2, Duration = 3 }
                ],
            },
            TrustedProxy = new TrustedProxySettingsUpdateRequest
            {
                Headers = ["X-Forwarded-For"],
                UseLeftmostIP = false,
            },
            Batch = new BatchSettingsUpdateRequest
            {
                Enabled = true,
                MaxRequests = 50,
                Timeout = 3,
            },
            Logs = new LogSettingsUpdateRequest
            {
                MaxDays = 7,
                LogIP = true,
                LogAuthId = false,
            },
        };

        // Act
        string json = JsonSerializer.Serialize(request, new PocketBaseOptions().JsonSerializerOptions);
        JsonDocument doc = JsonDocument.Parse(json);

        // Assert - top level sections
        doc.RootElement.TryGetProperty("superuserIPs", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("smtp", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("backups", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("s3", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("meta", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("rateLimits", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("trustedProxy", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("batch", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("logs", out _).Should().BeTrue();

        // Assert - nested field casing that differs from default PascalCase
        doc.RootElement.GetProperty("trustedProxy")
            .TryGetProperty("useLeftmostIP", out _).Should().BeTrue();
        doc.RootElement.GetProperty("rateLimits")
            .TryGetProperty("excludedIPs", out _).Should().BeTrue();
        doc.RootElement.GetProperty("meta")
            .TryGetProperty("accentColor", out _).Should().BeTrue();
        doc.RootElement.GetProperty("logs")
            .TryGetProperty("logIP", out _).Should().BeTrue();
        doc.RootElement.GetProperty("logs")
            .TryGetProperty("logAuthId", out _).Should().BeTrue();
        doc.RootElement.GetProperty("backups")
            .TryGetProperty("cronMaxKeep", out _).Should().BeTrue();
    }

    [Fact]
    public void SettingsResponse_ShouldDeserializeFromV023PlusSettingsJson()
    {
        // Arrange
        const string json = """
            {
              "superuserIPs": ["127.0.0.1", "10.0.0.0/8"],
              "smtp": {
                "enabled": false,
                "port": 587,
                "host": "smtp.example.com",
                "username": "",
                "authMethod": "",
                "tls": true,
                "localName": ""
              },
              "backups": {
                "cron": "0 0 * * *",
                "cronMaxKeep": 3,
                "s3": {
                  "enabled": true,
                  "bucket": "backup-bucket",
                  "region": "eu-west-1",
                  "endpoint": "https://s3.example.com",
                  "accessKey": "key",
                  "forcePathStyle": true
                }
              },
              "s3": {
                "enabled": false,
                "bucket": "",
                "region": "",
                "endpoint": "",
                "accessKey": "",
                "forcePathStyle": false
              },
              "meta": {
                "accentColor": "#1055c9",
                "appName": "Acme",
                "appURL": "https://example.com",
                "senderName": "Support",
                "senderAddress": "support@example.com",
                "hideControls": false
              },
              "rateLimits": {
                "rules": [
                  { "label": "*:auth", "audience": "", "duration": 3, "maxRequests": 2 },
                  { "label": "/api/", "audience": "@guest", "duration": 10, "maxRequests": 300 }
                ],
                "excludedIPs": ["10.0.0.0/8"],
                "enabled": false
              },
              "trustedProxy": {
                "headers": ["X-Forwarded-For"],
                "useLeftmostIP": false
              },
              "batch": {
                "enabled": true,
                "maxRequests": 50,
                "timeout": 3,
                "maxBodySize": 0
              },
              "logs": {
                "maxDays": 7,
                "minLevel": 0,
                "logIP": true,
                "logAuthId": false
              }
            }
            """;

        // Act
        SettingsResponse? settings = JsonSerializer.Deserialize<SettingsResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        // Assert
        settings.Should().NotBeNull();
        settings!.SuperuserIPs.Should().Equal("127.0.0.1", "10.0.0.0/8");

        settings.Smtp.Should().NotBeNull();
        settings.Smtp!.Host.Should().Be("smtp.example.com");
        settings.Smtp.Tls.Should().BeTrue();
        settings.Smtp.AuthMethod.Should().Be("");

        settings.Backups.Should().NotBeNull();
        settings.Backups!.Cron.Should().Be("0 0 * * *");
        settings.Backups.CronMaxKeep.Should().Be(3);
        settings.Backups.S3.Should().NotBeNull();
        settings.Backups.S3!.Bucket.Should().Be("backup-bucket");
        settings.Backups.S3.ForcePathStyle.Should().BeTrue();

        settings.S3.Should().NotBeNull();
        settings.S3!.Enabled.Should().BeFalse();

        settings.Meta.Should().NotBeNull();
        settings.Meta!.AccentColor.Should().Be("#1055c9");
        settings.Meta.AppName.Should().Be("Acme");
        settings.Meta.AppUrl.Should().Be("https://example.com");
        settings.Meta.HideControls.Should().BeFalse();

        settings.RateLimits.Should().NotBeNull();
        settings.RateLimits!.Enabled.Should().BeFalse();
        settings.RateLimits.ExcludedIPs.Should().Equal("10.0.0.0/8");
        settings.RateLimits.Rules.Should().HaveCount(2);
        settings.RateLimits.Rules[0].Label.Should().Be("*:auth");
        settings.RateLimits.Rules[1].Audience.Should().Be("@guest");

        settings.TrustedProxy.Should().NotBeNull();
        settings.TrustedProxy!.Headers.Should().Equal("X-Forwarded-For");
        settings.TrustedProxy.UseLeftmostIp.Should().BeFalse();

        settings.Batch.Should().NotBeNull();
        settings.Batch!.Enabled.Should().BeTrue();
        settings.Batch.MaxRequests.Should().Be(50);
        settings.Batch.Timeout.Should().Be(3);

        settings.Logs.Should().NotBeNull();
        settings.Logs!.MaxDays.Should().Be(7);
        settings.Logs.LogIp.Should().BeTrue();
        settings.Logs.LogAuthId.Should().BeFalse();
    }
}
