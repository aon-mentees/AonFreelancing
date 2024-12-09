﻿using AonFreelancing.Contexts;
using AonFreelancing.Controllers;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Responses;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

[Authorize]
[Route("api/mobile/v1/tasks")]
[ApiController]
public class TasksController(MainAppContext mainAppContext,AuthService authService, TaskService taskService, UserService userService, RoleService roleService) : BaseController
{
    // Start task (Freelacner can) To Do -> in progress (Update started at)
    [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
    [HttpPut("{id}/StartTask")]
    public async Task<IActionResult> StartTaskAsync(long id)
    {
        if (!ModelState.IsValid)
            return CustomBadRequest();
        long authenticatedUserId = authService.GetUserId((ClaimsIdentity) HttpContext.User.Identity);
        TaskEntity? storedTask = await taskService.FindTaskByIdAsync(id);
        User? storedUser = await userService.FindByIdAsync(authenticatedUserId);
        if(storedUser is null)
            return Unauthorized();
        if(storedTask is null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "task not found"));
        if(authenticatedUserId != storedTask.Project.FreelancerId)
            return Forbid();
        if(storedTask.Status != Constants.TASK_STATUS_TO_DO)
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"task status must be { Constants.TASK_STATUS_TO_DO } to start."));
        await taskService.StartTaskAsync(storedTask);
        return Ok(CreateSuccessResponse(TaskOutputDTO.FromTask(storedTask)));
    }

    // Submit task (Freelacner can) In progress -> in review
    [Authorize(Roles = Constants.USER_TYPE_FREELANCER)]
    [HttpPut("{id}/SubmitTask")]
    public async Task<IActionResult> SubmitTaskAsync(long id)
    {
        if (!ModelState.IsValid)
            return CustomBadRequest();
        long authenticatedUserId = authService.GetUserId((ClaimsIdentity) HttpContext.User.Identity);
        TaskEntity? storedTask = await taskService.FindTaskByIdAsync(id);
        User? storedUser = await userService.FindByIdAsync(authenticatedUserId);
        if(storedUser is null)
            return Unauthorized();
        if(storedTask is null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "task not found"));
        if(authenticatedUserId != storedTask.Project.FreelancerId)
            return Forbid();
        if(storedTask.Status != Constants.TASK_STATUS_IN_PROGRESS)
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"task status must be { Constants.TASK_STATUS_IN_PROGRESS } to start."));
        await taskService.SubmitTaskAsync(storedTask);
        return Ok(CreateSuccessResponse(TaskOutputDTO.FromTask(storedTask)));
    }

    // Implement Task approval (only clients owner can) In review -> Done (Update CompletedAt )
    [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
    [HttpPut("{id}/ApproveTask")]
    public async Task<IActionResult> ApproveTaskAsync(long id)
    {
        if (!ModelState.IsValid)
            return CustomBadRequest();
        long authenticatedUserId = authService.GetUserId((ClaimsIdentity) HttpContext.User.Identity);
        TaskEntity? storedTask = await taskService.FindTaskByIdAsync(id);
        User? storedUser = await userService.FindByIdAsync(authenticatedUserId);
        if(storedUser is null)
            return Unauthorized();
        if(storedTask is null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "task not found"));
        if(authenticatedUserId != storedTask.Project.FreelancerId)
            return Forbid();
        if(storedTask.Status != Constants.TASK_STATUS_IN_REVIEW)
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"task status must be { Constants.TASK_STATUS_IN_REVIEW } to start."));
        await taskService.ApproveTaskAsync(storedTask);
        return Ok(CreateSuccessResponse(TaskOutputDTO.FromTask(storedTask)));
    }

    // rejection (only clients owner can) In review -> (In progress ? To Do) 
    [Authorize(Roles = Constants.USER_TYPE_CLIENT)]
    [HttpPut("{id}/RejectTask")]
    public async Task<IActionResult> RejectTaskAsync(long id)
    {
        if (!ModelState.IsValid)
            return CustomBadRequest();
        long authenticatedUserId = authService.GetUserId((ClaimsIdentity) HttpContext.User.Identity);
        TaskEntity? storedTask = await taskService.FindTaskByIdAsync(id);
        User? storedUser = await userService.FindByIdAsync(authenticatedUserId);
        if(storedUser is null)
            return Unauthorized();
        if(storedTask is null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "task not found"));
        if(authenticatedUserId != storedTask.Project.FreelancerId)
            return Forbid();
        if(storedTask.Status != Constants.TASK_STATUS_IN_REVIEW)
            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest.ToString(), $"task status must be { Constants.TASK_STATUS_IN_REVIEW } to start."));
        await taskService.ApproveTaskAsync(storedTask);
        return Ok(CreateSuccessResponse(TaskOutputDTO.FromTask(storedTask)));
    }
}