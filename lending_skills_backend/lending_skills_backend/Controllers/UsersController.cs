﻿using lending_skills_backend.DataAccess;
using lending_skills_backend.Dtos.Requests;
using lending_skills_backend.Dtos.Responses;
using lending_skills_backend.Mappers;
using lending_skills_backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lending_skills_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersRepository _usersRepository;
        private readonly ProgramsRepository _programsRepository;
        private readonly SkillsRepository _skillsRepository;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UsersRepository usersRepository,
            ProgramsRepository programsRepository,
            SkillsRepository skillsRepository)
        {
            _usersRepository = usersRepository;
            _programsRepository = programsRepository;
            _skillsRepository = skillsRepository;
        }

        [HttpPost("GetProfiles")]
        public async Task<ActionResult<List<ProfileResponse>>> GetProfiles([FromBody] GetProfilesRequest request)
        {
            var userId = Guid.NewGuid(); // Заменить на реальный UserId
            var isSuperAdmin = await _usersRepository.IsSuperAdminAsync(userId);
            var isProgramAdmin = request.ProgramId.HasValue && await _programsRepository.IsAdminOfProgramAsync(userId, request.ProgramId.Value);
            if (!isSuperAdmin && !isProgramAdmin)
            {
                return Forbid("Only super admins or program admins can view profiles.");
            }

            var profiles = await _usersRepository.GetProfilesAsync(
                request.PageNumber,
                request.PageSize,
                request.ProgramId,
                request.SearchQuery);

            var profileResponses = new List<ProfileResponse>();
            foreach (var profile in profiles)
            {
                var skillsForUser = (await _context.SkillsUsers
                    .Where(su => su.UserId == profile.Id)
                    .Select(su => su.Skill)
                    .ToListAsync())
                    .Select(SkillResponseMapper.Map)
                    .ToList();

                profileResponses.Add(ProfileResponseMapper.Map(profile, skillsForUser));
            }

            return Ok(profileResponses);
        }

        [HttpGet("GetProfile/{userId}")]
        public async Task<ActionResult<ProfileResponse>> GetProfile(Guid userId)
        {
            var profile = await _usersRepository.GetProfileByIdAsync(userId);
            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            var skillsForUser = (await _context.SkillsUsers
                .Where(su => su.UserId == profile.Id)
                .Select(su => su.Skill)
                .ToListAsync())
                .Select(SkillResponseMapper.Map)
                .ToList();

            return Ok(ProfileResponseMapper.Map(profile, skillsForUser));
        }

        [HttpPost("CreateStudentProfile")]
        public async Task<ActionResult<ProfileResponse>> CreateStudentProfile([FromBody] CreateStudentProfileRequest request)
        {
            var userId = Guid.NewGuid(); // Заменить на реальный UserId
            var isSuperAdmin = await _usersRepository.IsSuperAdminAsync(userId);
            var isProgramAdmin = request.ProgramId.HasValue && await _programsRepository.IsAdminOfProgramAsync(userId, request.ProgramId.Value);
            if (!isSuperAdmin && !isProgramAdmin)
            {
                return Forbid("Only super admins or program admins can create student profiles.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest("User with this email already exists.");
            }

            var user = DbUserMapper.Map(request);
            user.Id = Guid.NewGuid();
            user.Role = "Student";
            user.IsActive = true;
            await _usersRepository.AddUserAsync(user);

            return CreatedAtAction(
                nameof(GetProfile),
                new { userId = user.Id },
                ProfileResponseMapper.Map(user, new List<SkillResponse>()));
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var user = await _usersRepository.GetProfileByIdAsync(request.Id);
            if (user == null)
            {
                return NotFound("Profile not found.");
            }

            var userId = Guid.NewGuid(); // Заменить на реальный UserId
            if (userId != request.Id)
            {
                return Forbid("Only the student can update their own profile.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != request.Id);
            if (existingUser != null)
            {
                return BadRequest("Another user with this email already exists.");
            }

            DbUserMapper.Map(user, request);
            await _usersRepository.UpdateUserAsync(user);
            return NoContent();
        }

        [HttpPost("HideProfile")]
        public async Task<IActionResult> HideProfile([FromBody] HideProfileRequest request)
        {
            var user = await _usersRepository.GetProfileByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("Profile not found.");
            }

            var userId = Guid.NewGuid(); // Заменить на реальный UserId
            if (userId != request.UserId)
            {
                return Forbid("Only the student can hide their own profile.");
            }

            await _usersRepository.HideProfileAsync(request.UserId);
            return NoContent();
        }

        [HttpPost("ShowProfile")]
        public async Task<IActionResult> ShowProfile([FromBody] ShowProfileRequest request)
        {
            var user = await _usersRepository.GetProfileByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("Profile not found.");
            }

            var userId = Guid.NewGuid(); // Заменить на реальный UserId
            if (userId != request.UserId)
            {
                return Forbid("Only the student can show their own profile.");
            }

            await _usersRepository.ShowProfileAsync(request.UserId);
            return NoContent();
        }

        [HttpDelete("DeleteProfile/{userId}")]
        public async Task<IActionResult> DeleteProfile(Guid userId)
        {
            var user = await _usersRepository.GetProfileByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Profile not found.");
            }

            var currentUserId = Guid.NewGuid(); // Заменить на реальный UserId
            var isSuperAdmin = await _usersRepository.IsSuperAdminAsync(currentUserId);
            if (!isSuperAdmin)
            {
                return Forbid("Only super admins can delete profiles.");
            }

            await _usersRepository.DeleteProfileAsync(userId);
            return NoContent();
        }
    }
}