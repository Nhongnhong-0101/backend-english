using Core.Models;
using Infrastructure.Repository;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly IAccountRepository accountRepository;
        private readonly IEmailTemplateRepository emailTemplateRepository;

        private MailSetting mailSetting = new MailSetting();
        public AuthService (IConfiguration configuration, IAccountRepository accountRepository, IEmailTemplateRepository emailTemplateRepository)
        {
            this.configuration = configuration;
            this.accountRepository = accountRepository;
            this.emailTemplateRepository = emailTemplateRepository;
        } 
        public async Task<string> LoginAccount(string username, string password)
        {
            try
            {
                var account = await accountRepository.GetAccountByEmailAsync(username);
                bool isValid = BCrypt.Net.BCrypt.Verify(password, account.passwordHash);
                if (isValid)
                {
                    var token = GenerateJwtToken(account.email);
                    if (!string.IsNullOrEmpty(token))
                    {
                        return token;
                    }
                }
                return null;
            }
            catch {
                throw;
            }
        }

        public string GenerateJwtToken(string email)
        {
            try
            {
                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: creds);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> RegisterAccount(string fullName, string Email, string password)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                Account account = new Account();
                account.fullName = fullName;
                account.email = Email;
                account.passwordHash = hashedPassword;
                var saved = await accountRepository.AddNewAccountAsync(account);
                if (saved != null)
                {
                    return "Account created successfully";
                }
                return null;
            }
            catch
            {
                throw;
            }
        }
        public async Task<string> SendRegisterVerificationEmail(string email)
        {
            try
            {
                string code = GenerateCode();
                var template = await emailTemplateRepository.GetEmailTemplate("RegisterVerification");
                if (!string.IsNullOrEmpty(template.BodyHtml) && !string.IsNullOrEmpty(template.Subject))
                {
                    string subject = template.Subject;
                    string body = template.BodyHtml.Replace("{{CODE}}", code);
                    MailData mailData = new MailData();
                    mailData.EmailSubject = subject;
                    mailData.EmailBody = body;
                    mailData.EmailToName = email;
                    await SendEmailAsync(mailData, email);

                    return code;
                }

                return null;
            }
            catch
            {
                throw;
            }
        }
        public async Task SendEmailAsync(MailData mailData, string toEmail)
            {
                try
                {
                    mailSetting.Host = configuration.GetValue<string>("MailSettings:Host");
                    mailSetting.Port = configuration.GetValue<int>("MailSettings:Port");
                    mailSetting.Name = configuration.GetValue<string>("MailSettings:Name");
                    mailSetting.SenderEmail = configuration.GetValue<string>("MailSettings:SenderEmail");
                    mailSetting.UserName = configuration.GetValue<string>("MailSettings:Username");
                    mailSetting.Password = configuration.GetValue<string>("MailSettings:Password");
                    mailSetting.UseSSL = configuration.GetValue<bool>("MailSettings:UseSSL");
                    bool isValid = !string.IsNullOrWhiteSpace(mailSetting.Host)
                        && mailSetting.Port > 0
                        && !string.IsNullOrWhiteSpace(mailSetting.Name)
                        && !string.IsNullOrWhiteSpace(mailSetting.SenderEmail)
                        && !string.IsNullOrWhiteSpace(mailSetting.Password);
                    if (isValid) 
                    {
                        var client = new SmtpClient(mailSetting.Host, mailSetting.Port)
                        {
                            Credentials = new NetworkCredential(mailSetting.UserName, mailSetting.Password),
                            EnableSsl = mailSetting.UseSSL,
                        };
                    
                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(mailSetting.SenderEmail, mailSetting.Name),
                            Subject = mailData.EmailSubject,
                            Body = mailData.EmailBody,
                            IsBodyHtml = true
                        };

                        mailMessage.To.Add(new MailAddress(toEmail));
                        await client.SendMailAsync(mailMessage);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        public string GenerateCode()
        {
            var rand = new Random();
            return rand.Next(1000, 9999).ToString();
        }
    }
}
