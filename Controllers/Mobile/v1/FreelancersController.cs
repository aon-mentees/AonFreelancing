using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AonFreelancing.Controllers.Mobile.v1
{
    [Authorize]
    [Route("api/mobile/v1/freelancers")]
    [ApiController]
    public class FreelancersController(FreelancerService freelancerService, AuthService authService)
        : BaseController
    {
        [HttpGet("{id}/certifications")]
        public async Task<IActionResult> GetAllCertificationsAsync([FromRoute] long id)
        {
            Freelancer? storedFreelancer = await freelancerService.FindFreelancerWithCertifications(id);

            if (storedFreelancer == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Freelancer not found"));

            List<CertificationOutDTO> certificationDTOs = storedFreelancer.Certifications
                .Select(c => new CertificationOutDTO(c)).ToList();
            return Ok(CreateSuccessResponse(certificationDTOs));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("certifications")]
        public async Task<IActionResult> AddCertificationAsync([FromForm] CertificationInputDTO certificationInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            bool isFreelancerCertificationExists = await freelancerService
                .FindExistingFreelancerCertificationnAsync(freelancerId, certificationInputDTO.Name, certificationInputDTO.Issuer, certificationInputDTO.ExpiryDate);

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
            [FromForm] CertificationInputDTO certificationInputDTO)
        {
            if (!ModelState.IsValid)
                return CustomBadRequest();

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);

            Certification? storedCertification = await freelancerService.FindFreelancerCertification(certificationId);

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

            Certification? storedCertification = await freelancerService.FindFreelancerCertification(certificationId);

            if (storedCertification == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Certification not found"));
            if (freelancerId != storedCertification.FreelancerId)
                return Forbid();

            await freelancerService.DeleteAsync(storedCertification);

            return NoContent();
        }

        [HttpGet("{Id}/education")]
        public async Task<IActionResult> GetAllEducationAsync([FromRoute] long Id)
        {
            Freelancer? storedFreelancer = await freelancerService.FindFreelancerWithEducation(Id);

            if (storedFreelancer == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Freelancer not found"));

            List<EducationOutputDTO> educationOutputDTOs = storedFreelancer.Education.Select(c => new EducationOutputDTO(c)).ToList();

            return Ok(CreateSuccessResponse(educationOutputDTOs));

        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpPost("education")]
        public async Task<IActionResult> SAddEducationAsync([FromForm] EducationInputDTO educationInputDTO)
        {

            if (!ModelState.IsValid)
                return base.CustomBadRequest();

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

            Education? storedEducation = await freelancerService.FindFreelancerEducation(educationId);

            if (storedEducation == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Education not found."));

            if (freelancerId != storedEducation.freelancerId)
                return Forbid();

            storedEducation.Institution = educationInputDTO.Institution;
            storedEducation.Degree = educationInputDTO.Degree;
            storedEducation.startDate = educationInputDTO.startDate;
            storedEducation.endDate = educationInputDTO.endDate;

            await freelancerService.SaveChangesAsync();

            var educationDTO = EducationOutputDTO.FromEducation(storedEducation);
            return Ok(CreateSuccessResponse(educationDTO));
        }

        [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
        [HttpDelete("education/{educationId}")]
        public async Task<IActionResult> DeleteEducationAsync([FromRoute] long educationId)
        {

            long freelancerId = authService.GetUserId((ClaimsIdentity)HttpContext.User.Identity);
            Education? storedEducation = await freelancerService.FindFreelancerEducation(educationId);

            if (storedEducation == null)
                return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "Education not found"));
            if (freelancerId != storedEducation.freelancerId)
                return Forbid();

            await freelancerService.DeleteAsync(storedEducation);
            return NoContent();
        }

    }
}
