using ApiSkyrimRP.Core;
using ApiSkyrimRP.Models.Account;
using Domain.Entities;
using Domain.Services.JwtAuthManager.Abstractions;
using Domain.Services.JwtAuthManager.Models;
using Domain.Services.Users.Abstractions;
using Domain.Services.Users.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiSkyrimRP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IUsersService usersService;
        private readonly MailService mailService;
        private readonly IWebHostEnvironment env;
        private readonly IJwtAuthManager jwtAuthManager;

        public AccountController(IUsersService users, IWebHostEnvironment environment, IJwtAuthManager jwtAuth, MailService mail)
        {
            usersService = users;
            mailService = mail;
            env = environment;
            jwtAuthManager = jwtAuth;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> Register(RegUserInfo info)
        {
            User user = await usersService.AddAsync(info);

            if (mailService.Enable)
            {
                string mail = System.IO.File.ReadAllText(Path.Combine(env.ContentRootPath, "Assets", "ActivationMail.tpl"));
                mail = mail.Replace("{USERNAME}", user.Username).Replace("{URL}", Url.ActionLink("MailVerify", "Account", new { UID = user.Id, code = user.Code }));
                await mailService.SendMailAsync(user.Email, "Активация аккаунта Skyrim", mail);
            }
            else
            {
                await MailVerify(user.Id, user.Code);
            }
            return Ok();
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="info">Данные авторизации</param>
        /// <returns>Результат операции</returns>
        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> Login([Required] LoginUserInfo info)
        {
            if (await usersService.VerifyPasswordAsync(info))
            {
                User user = await usersService.GetUserAsync(info.Email);
                if (!user.IsEmailConfirmed) return BadRequest(new { errorText = "Email is not confirmed." });
                if (user.IsBlocked) return BadRequest(new { errorText = "Account is blocked." });
                List<Claim> claims = new()
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username),
                    new Claim("Email", user.Email),
                    new Claim("UID", user.Id.ToString())
                };
                claims.AddRange(user.Roles.ConvertAll(c => new Claim(ClaimsIdentity.DefaultNameClaimType, c.Name)));

                JwtAuthResult jwtResult = jwtAuthManager.GenerateTokens(user.Id, claims.ToArray(), DateTime.Now);

                var response = new
                {
                    id = user.Id,
                    access_token = jwtResult.AccessToken,
                    refresh_token = jwtResult.RefreshToken.TokenString
                };

                return Ok(response);
            }
            return BadRequest(new { errorText = "Invalid email or password." });
        }

        [HttpGet("Logout")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public IActionResult Logout()
        {
            Claim uid = User.Claims.FirstOrDefault(f => f.Type == "UID");
            if (int.TryParse(uid.Value, out int id))
            {
                jwtAuthManager.RemoveRefreshTokenByUserID(id);
                return Ok();
            }
            else { return StatusCode(520); }
        }

        [HttpPost("RefreshToken")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> RefreshToken([Required] RefreshTokenRequest info)
        {
            try
            {
                Claim uid = User.Claims.FirstOrDefault(f => f.Type == "UID");
                if (int.TryParse(uid.Value, out int id))
                {
                    if ((await usersService.GetUserAsync(id)).IsBlocked)
                    {
                        jwtAuthManager.RemoveRefreshTokenByUserID(id);
                        return Unauthorized();
                    }
                    if (string.IsNullOrWhiteSpace(info.RefreshToken)) return Unauthorized();

                    string accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                    JwtAuthResult jwtResult = jwtAuthManager.Refresh(info.RefreshToken, accessToken, DateTime.Now);

                    var response = new
                    {
                        id,
                        access_token = jwtResult.AccessToken,
                        refresh_token = jwtResult.RefreshToken.TokenString
                    };

                    return Ok(response);
                }
                else { return StatusCode(520); }
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message);
            }
        }

        [HttpGet("GetUsername")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public IActionResult GetUsername()
        {
            var reponse = new { Username = User.Identity.Name };
            return Ok(reponse);
        }

        [HttpGet("GetUsername/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> GetUsername([Required] int id)
        {
            User user = await usersService.GetUserAsync(id);
            if (user != null) return Ok(new { user.Username });
            else return BadRequest(new { errorText = "User not found." });
        }

        [HttpGet("MailVerify/{UID}/{code}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> MailVerify([Required] int UID, [Required] Guid code)
        {
            try
            {
                int success = 0;
                await usersService.EditAsync(UID, user =>
                {
                    if (user.IsEmailConfirmed)
                    {
                        success = 1;
                        return false;
                    }
                    if (user.Code != code)
                    {
                        success = 2;
                        return false;
                    }
                    user.IsEmailConfirmed = true;

                    return true;
                });

                if (success == 0) return Ok();
                else if (success == 1) return Ok(new { text = "Already confirmed." });
                else if (success == 2) return BadRequest(new { errorText = "Invalid url." });
                else return StatusCode(520);
            }
            catch (Exception e)
            {
                return BadRequest(new { errorText = e.Message });
            }
        }

        [HttpPost("ResetPasswordRequest")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> ResetPasswordRequest([Required] ResetPasswordRequest info)
        {
            if (!mailService.Enable) return StatusCode(503, new { errorText = "Email Service not available." });

            User user = await usersService.GetUserAsync(info.Email);
            if (user == null || !user.IsEmailConfirmed) return BadRequest(new { errorText = "User not found." });

            Guid code = Guid.NewGuid();

            string mail = System.IO.File.ReadAllText(Path.Combine(env.ContentRootPath, "Assets", "ResetMail.tpl"));
            mail = mail.Replace("{USERNAME}", user.Username).Replace("{URL}", $"http://local.lo/{user.Id}/{code}");
            await mailService.SendMailAsync(user.Email, "Сброс пароля Skyrim", mail);

            await usersService.EditAsync(user.Id, user =>
            {
                user.Code = code;
                return true;
            });
            return Ok();
        }

        [HttpPost("ResetPassword/{UID}/{code}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> ResetPassword([Required] int UID, [Required] Guid code, [Required] string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword)) return BadRequest(new { errorText = "NewPassword field required." });

            User user = await usersService.GetUserAsync(UID);
            if (user == null) return BadRequest(new { errorText = "Invalid url." });
            if (user.Code != code) return BadRequest(new { errorText = "Invalid url." });

            await usersService.EditAsync(user.Id, user =>
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                return true;
            });
            return Ok();
        }
    }
}
