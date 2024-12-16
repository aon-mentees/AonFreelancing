using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/freelancers")]
    [ApiController]
    public class FreelancersController(
        FreelancerService freelancerService, AuthService authService,
        UserService userService, ActivitiesService activitiesService,
        UserManager<User> userManager, MainAppContext mainAppContext)
        : BaseController
    {
        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPatch]
        public async Task<IActionResult> UpdateFreelancerAsync([FromBody] FreelancerUpdateDTO freelancerUpdateDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            var storedUser = (Freelancer?)await userManager.GetUserAsync(HttpContext.User);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Authenticated user not found"));

            storedUser.Name = freelancerUpdateDTO.Name;
            storedUser.QualificationName = freelancerUpdateDTO.QualificationName;

            await mainAppContext.SaveChangesAsync();

            return Ok(CreateSuccessResponse("Freelancer updated successfully"));
        }

        [HttpGet("{id}/certifications")]
        public async Task<IActionResult> GetAllCertificationsAsync([FromRoute] long id, int page = 0, int pageSize = Constants.CERTIFICATION_DEFAULT_PAGE_SIZE)
        {
            bool isExists = await freelancerService.IsFreelancerExistsAsync(id);

            if (!isExists)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Freelancer not found"));

            PaginatedResult<Certification> paginatedCertifications = await freelancerService.FindCertificationByFreelancerIdAsync(id, page, pageSize);
            List<CertificationOutDTO> certificationOutDTOs = paginatedCertifications.Result.Select(c => CertificationOutDTO.FromCertification(c)).ToList();
            PaginatedResult<CertificationOutDTO> paginatedCertificationOutputDTO = new PaginatedResult<CertificationOutDTO>(paginatedCertifications.Total, certificationOutDTOs);

            return Ok(CreateSuccessResponse(paginatedCertificationOutputDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("certifications")]
        public async Task<IActionResult> AddCertificationAsync([FromBody] CertificationInputDTO certificationInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            bool isFreelancerCertificationExists = await freelancerService
                .FindExistingFreelancerCertificationAsync(freelancerId, certificationInputDTO.Name, certificationInputDTO.Issuer, certificationInputDTO.ExpiryDate);

            if (isFreelancerCertificationExists)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "You already have this certification in your profile."));
            Certification? certification = Certification.FromCertificationInputDTO(certificationInputDTO, freelancerId);


            await freelancerService.AddAsync(certification);
            await freelancerService.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCertificationsAsync), new { id = certification.Id }, null);
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPut("certifications/{certificationId}")]
        public async Task<IActionResult> UpdateCertificationAsync([FromRoute] long certificationId,
            [FromBody] CertificationInputDTO certificationInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            Certification? storedCertification = await freelancerService.FindFreelancerCertificationAsync(certificationId);

            if (storedCertification == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Certification not found"));

            if (freelancerId != storedCertification.FreelancerId)
                return Forbid();

            storedCertification.Name = certificationInputDTO.Name;
            storedCertification.Issuer = certificationInputDTO.Issuer;
            storedCertification.CredentialId = certificationInputDTO.CredentialId;
            storedCertification.CredentialUrl = certificationInputDTO.CredentialUrl;
            storedCertification.IssueDate = certificationInputDTO.IssueDate;
            storedCertification.ExpiryDate = certificationInputDTO.ExpiryDate;

            await freelancerService.SaveChangesAsync();

            var certificationDTO = CertificationOutDTO.FromCertification(storedCertification);
            return Ok(CreateSuccessResponse(certificationDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpDelete("certifications/{certificationId}")]
        public async Task<IActionResult> DeleteCertificationAsync([FromRoute] long certificationId)
        {
            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            Certification? storedCertification = await freelancerService.FindFreelancerCertificationAsync(certificationId);

            if (storedCertification == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Certification not found"));
            if (freelancerId != storedCertification.FreelancerId)
                return Forbid();

            await freelancerService.DeleteAsync(storedCertification);

            return NoContent();
        }

        [HttpGet("{id}/education")]
        public async Task<IActionResult> GetAllEducationAsync([FromRoute] long id, int page = 0, int pageSize = Constants.EDUCATION_DEFAULT_PAGE_SIZE)
        {
            bool isExists = await freelancerService.IsFreelancerExistsAsync(id);

            if (!isExists)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Freelancer not found"));

            PaginatedResult<Education> paginatedEducation = await freelancerService.FindEducationByFreelancerIdAsync(id, page, pageSize);
            List<EducationOutputDTO> educationOutputDTOs = paginatedEducation.Result.Select(e => EducationOutputDTO.FromEducation(e)).ToList();
            PaginatedResult<EducationOutputDTO> paginatedEducationOutputDTO = new PaginatedResult<EducationOutputDTO>(paginatedEducation.Total, educationOutputDTOs);

            return Ok(CreateSuccessResponse(paginatedEducationOutputDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("education")]
        public async Task<IActionResult> AddEducationAsync([FromForm] EducationInputDTO educationInputDTO)
        {

            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            if (educationInputDTO.StartDate > DateTime.Now)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Start date should be less than today's date."));

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            bool isFreelancerEducationExists = await freelancerService.FindExistingFreelancerEducationAsync(freelancerId, educationInputDTO.Institution, educationInputDTO.Degree);

            if (isFreelancerEducationExists)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "You already have this education in your profile."));
            Education? education = Education.FromEducationInputDTO(educationInputDTO, freelancerId);
            await freelancerService.AddAsync(education);
            await freelancerService.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllEducationAsync), new { id = education.Id }, null);

        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPut("education/{educationId}")]
        public async Task<IActionResult> UpdateEducationAsync([FromForm] EducationInputDTO educationInputDTO, [FromRoute] long educationId)
        {

            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            Education? storedEducation = await freelancerService.FindFreelancerEducationAsync(educationId);

            if (storedEducation == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Education not found."));

            if (freelancerId != storedEducation.freelancerId)
                return Forbid();

            storedEducation.Institution = educationInputDTO.Institution;
            storedEducation.Degree = educationInputDTO.Degree;
            storedEducation.startDate = educationInputDTO.StartDate;
            storedEducation.endDate = educationInputDTO.EndDate;

            await freelancerService.SaveChangesAsync();

            var educationDTO = EducationOutputDTO.FromEducation(storedEducation);
            return Ok(CreateSuccessResponse(educationDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpDelete("education/{educationId}")]
        public async Task<IActionResult> DeleteEducationAsync([FromRoute] long educationId)
        {

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Education? storedEducation = await freelancerService.FindFreelancerEducationAsync(educationId);

            if (storedEducation == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Education not found"));
            if (freelancerId != storedEducation.freelancerId)
                return Forbid();

            await freelancerService.DeleteAsync(storedEducation);
            return NoContent();
        }

        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetActivitiesAsync(long id)
        {
            var storedUser = await userService.FindByIdAsync(id);
            if (storedUser == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Not Found"));
            var isFreelancer = await userService.IsFreelancer(storedUser);
            if (!isFreelancer)
                return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), "Not a freelancer"));
            var responseDTO = activitiesService.FreelancerActivities(id);
            return Ok(CreateSuccessResponse(responseDTO));
        }

        [HttpGet("{id}/work-experience")]
        public async Task<IActionResult> GetWorkExperiencesAsync([FromRoute] long id, int page = 0, int pageSize = Constants.WORK_EXPERIENCES_DEFAULT_PAGE_SIZE)
        {
            bool isExists = await freelancerService.IsFreelancerExistsAsync(id);

            if (!isExists)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Freelancer not found"));

            PaginatedResult<WorkExperience> paginatedWorkExperiences = await freelancerService.FindWorkExperienceByFreelancerIdAsync(id, page, pageSize);
            List<WorkExperienceOutputDTO> workExperienceOutDTOs = paginatedWorkExperiences.Result.Select(w => WorkExperienceOutputDTO.FromWorkExperience(w)).ToList();
            PaginatedResult<WorkExperienceOutputDTO> paginatedWorkExperienceOutputDTO = new PaginatedResult<WorkExperienceOutputDTO>(paginatedWorkExperiences.Total, workExperienceOutDTOs);

            return Ok(CreateSuccessResponse(paginatedWorkExperienceOutputDTO));
        }
        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("work-experience")]
        public async Task<IActionResult> AddWorkExperinceAsync([FromForm] WorkExperienceInputDTO workExperienceInputDTO)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            bool isFreelancerWorkExperinceExists = await freelancerService.
                FindExistingFreelancerWorkExperienceAsync(freelancerId, workExperienceInputDTO.JobTitle, workExperienceInputDTO.EmploymentType,
                workExperienceInputDTO.EmployerName);
           
            if (isFreelancerWorkExperinceExists)
                return Conflict(CreateErrorResponse(StatusCodes.Status409Conflict.ToString(), "You already have this work experince in your profile."));
            WorkExperience? workExperience = WorkExperience.FromWorkExperienceinputDTO(workExperienceInputDTO, freelancerId);

            await freelancerService.AddAsync(workExperience);
            await freelancerService.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkExperiencesAsync), new { id = workExperience.Id }, null);
        }
        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPut("work-experience/{workExperienceId}")]
        public async Task<IActionResult> UpdateWorkExperinceAsync([FromForm] WorkExperienceInputDTO workExperienceInputDTO, [FromRoute] long workExperienceId)
        {
            if (!ModelState.IsValid)
                return base.CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            WorkExperience? storedworkExperience = await freelancerService.FindFreelancerWorkExperienceAsync(workExperienceId);

            if (storedworkExperience == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Work experience not found."));

            if (freelancerId != storedworkExperience.FreelancerId)
                return Forbid();

            storedworkExperience.JobTitle = workExperienceInputDTO.JobTitle;
            storedworkExperience.EmployerName = workExperienceInputDTO.EmployerName;
            storedworkExperience.EmploymentType = workExperienceInputDTO.EmploymentType;
            storedworkExperience.IsCurrent = workExperienceInputDTO.IsCurrent;
            storedworkExperience.StartDate = workExperienceInputDTO.StartDate;
            storedworkExperience.EndDate = workExperienceInputDTO.EndDate;

            await freelancerService.SaveChangesAsync();

            WorkExperienceOutputDTO? workExperienceDTO = WorkExperienceOutputDTO.FromWorkExperience(storedworkExperience);
            return Ok(CreateSuccessResponse(workExperienceDTO));
        }
        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpDelete("work-experience/{workExperienceId}")]
        public async Task<IActionResult> DeleteWorkExperienceAsync([FromRoute] long workExperienceId)
        {
            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            WorkExperience? storedworkExperience = await freelancerService.FindFreelancerWorkExperienceAsync(workExperienceId);

            if (storedworkExperience == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Work experience not found"));
            if (freelancerId != storedworkExperience.FreelancerId)
                return Forbid();

            await freelancerService.DeleteAsync(storedworkExperience);
            return NoContent();
        }

    }
}
