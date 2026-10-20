using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CleanAuthApi.Application.Common;
using CleanAuthApi.Application.DTOs.Auth;
using CleanAuthApi.Application.DTOs.SecurityLogs;
using CleanAuthApi.Application.DTOs.Users;
using CleanAuthApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CleanAuthApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"CleanAuthTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DatabaseProvider", "Testing");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}

public class ApiIntegrationAndSecurityTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ProtectedEndpoint_WithoutJwtToken_ShouldReturn401Unauthorized()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithForgedOrInvalidToken_ShouldReturn401Unauthorized()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.forged.signature");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FullAuthLifecycle_And_SecurityLogTables_ShouldWorkAndRecordAllEvents()
    {
        var email = $"test_{Guid.NewGuid():N}@ornek.com";
        var password = "SecurePassword123!";

        // 1. Kullanıcı Kaydı (Register) -> SecurityAuditLogs tablosuna kaydedilmeli
        var registerReq = new RegisterRequestDto(email, password, "Test", "Kullanıcı");
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        Assert.Equal(HttpStatusCode.Created, registerRes.StatusCode);

        // 2. Aynı E-posta ile Tekrar Kayıt Denemesi -> 409 Conflict Dönmeli ve Loglanmalı
        var duplicateRes = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        Assert.Equal(HttpStatusCode.Conflict, duplicateRes.StatusCode);

        // 3. Yanlış Şifre ile Giriş Denemesi -> 401 Unauthorized Dönmeli ve FailedLoginLogs tablosuna yazılmalı
        var wrongLoginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(email, "WrongPass123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, wrongLoginRes.StatusCode);

        // 4. Doğru Bilgilerle Giriş (Login)
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto(email, password));
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
        var loginBody = await loginRes.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        var accessToken = loginBody!.Data!.AccessToken;
        var refreshToken = loginBody.Data.RefreshToken;

        // 5. Hatalı Mevcut Şifre ile Şifre Değiştirme Denemesi -> 400 BadRequest & PasswordChangeLogs (IsSuccess = false)
        using var wrongPassChangeReq = new HttpRequestMessage(HttpMethod.Put, "/api/users/me/password")
        {
            Content = JsonContent.Create(new ChangePasswordRequestDto("WrongCurrentPass!", "NewPass456!"))
        };
        wrongPassChangeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var wrongPassChangeRes = await _client.SendAsync(wrongPassChangeReq);
        Assert.Equal(HttpStatusCode.BadRequest, wrongPassChangeRes.StatusCode);

        // 6. Doğru Mevcut Şifre ile Şifre Değiştirme -> 200 OK & PasswordChangeLogs (IsSuccess = true)
        using var validPassChangeReq = new HttpRequestMessage(HttpMethod.Put, "/api/users/me/password")
        {
            Content = JsonContent.Create(new ChangePasswordRequestDto(password, "NewPass456!"))
        };
        validPassChangeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var validPassChangeRes = await _client.SendAsync(validPassChangeReq);
        Assert.Equal(HttpStatusCode.OK, validPassChangeRes.StatusCode);

        // 7. Başarısız Giriş Loglarını (FailedLoginLogs) Kontrol Et
        using var failedLoginsReq = new HttpRequestMessage(HttpMethod.Get, "/api/securitylogs/failed-logins");
        failedLoginsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var failedLoginsRes = await _client.SendAsync(failedLoginsReq);
        Assert.Equal(HttpStatusCode.OK, failedLoginsRes.StatusCode);
        var failedLoginsBody = await failedLoginsRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<FailedLoginLogDto>>>();
        Assert.Contains(failedLoginsBody!.Data!, x => x.Email == email && x.FailureReason.Contains("Hatalı şifre"));

        // 8. Şifre Değişikliği Loglarını (PasswordChangeLogs) Kontrol Et (Hem başarısız hem başarılı kayıt olmalı)
        using var passLogsReq = new HttpRequestMessage(HttpMethod.Get, "/api/securitylogs/password-changes");
        passLogsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var passLogsRes = await _client.SendAsync(passLogsReq);
        Assert.Equal(HttpStatusCode.OK, passLogsRes.StatusCode);
        var passLogsBody = await passLogsRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PasswordChangeLogDto>>>();
        Assert.Contains(passLogsBody!.Data!, x => x.Email == email && !x.IsSuccess);
        Assert.Contains(passLogsBody.Data!, x => x.Email == email && x.IsSuccess);

        // 9. Genel Güvenlik Denetim Loglarını (SecurityAuditLogs) Kontrol Et
        using var auditLogsReq = new HttpRequestMessage(HttpMethod.Get, "/api/securitylogs/audit-logs");
        auditLogsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var auditLogsRes = await _client.SendAsync(auditLogsReq);
        Assert.Equal(HttpStatusCode.OK, auditLogsRes.StatusCode);
        var auditLogsBody = await auditLogsRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<SecurityAuditLogDto>>>();
        Assert.Contains(auditLogsBody!.Data!, x => x.Email == email && x.EventType == "UserRegistered");
        Assert.Contains(auditLogsBody.Data!, x => x.Email == email && x.EventType == "LoginSuccess");
    }
}
