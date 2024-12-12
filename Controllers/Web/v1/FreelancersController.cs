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

namespace AonFreelancing.Controllers.Web.v1
{
    [Authorize]
    [Route("api/web/v1/freelancers")]
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

            await freelancerService.UpdateAsync(storedCertification);

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
    }
}
