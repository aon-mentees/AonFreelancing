using AonFreelancing.Models.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class WorkExperienceValidator
{
    public static IEnumerable<ValidationResult> Validate(WorkExperienceInputDTO dto)
    {
        if (dto.IsCurrent && dto.EndDate.HasValue)
        {
            yield return new ValidationResult(
                "EndDate must be null if IsCurrent is true.",
                new[] { nameof(dto.EndDate) }
            );
        }

        if (!dto.IsCurrent && !dto.EndDate.HasValue)
        {
            yield return new ValidationResult(
                "EndDate is required if IsCurrent is false.",
                new[] { nameof(dto.EndDate) }
            );
        }

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
        {
            yield return new ValidationResult(
                "EndDate cannot be earlier than StartDate.",
                new[] { nameof(dto.EndDate) }
            );
        }
    }
}
