using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MoviesMadeEasy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MoviesMadeEasy.DTOs;
using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesMadeEasy.Controllers
{
    public class UserController : BaseController
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ISubscriptionRepository _subscriptionService;

        public UserController(
            ILogger<UserController> logger,
            UserManager<IdentityUser> userManager,
            IUserRepository userRepository,
            ISubscriptionRepository subscriptionService) : base(userManager, userRepository, logger)
        {
            _logger = logger;
            _userManager = userManager;
            _userRepository = userRepository;
            _subscriptionService = subscriptionService;
        }

        private DashboardDTO BuildDashboardDTO(int userId)
        {
            var user = _userRepository.GetUser(userId);
            var userSubscriptions = _subscriptionService.GetUserSubscriptions(userId);
            var allServices = _subscriptionService.GetAllServices()?.ToList() ?? new List<StreamingService>();

            return new DashboardDTO
            {
                UserId = userId,
                UserName = user != null ? user.FirstName : "",
                HasSubscriptions = userSubscriptions != null && userSubscriptions.Any(),
                SubList = userSubscriptions?.ToList() ?? new List<StreamingService>(),
                AllServicesList = allServices,
                PreSelectedServiceIds = userSubscriptions != null
                                        ? string.Join(",", userSubscriptions.Select(s => s.Id))
                                        : ""
            };
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var identityUser = await _userManager.GetUserAsync(User);
                if (identityUser == null)
                {
                    return Unauthorized();
                }

                var user = _userRepository.GetUser(identityUser.Id);
                var dto = BuildDashboardDTO(user.Id);
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard for user");
                return RedirectToAction("Error");
            }
        }

        public IActionResult SubscriptionForm(DashboardDTO dto)
        {
            var updatedDto = BuildDashboardDTO(dto.UserId);
            return View(updatedDto);
        }

        [HttpPost]
        public IActionResult SaveSubscriptions(int userId, string selectedServices)
        {
            selectedServices = selectedServices ?? "";

            try
            {
                List<int> selectedServiceIds =
                    string.IsNullOrWhiteSpace(selectedServices)
                        ? new List<int>()
                        : selectedServices.Split(',').Select(int.Parse).ToList();

                _subscriptionService.UpdateUserSubscriptions(userId, selectedServiceIds);

                TempData["Message"] = "Subscriptions managed successfully!";

                var dto = BuildDashboardDTO(userId);
                return View("Dashboard", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving subscriptions for userId: {userId}", userId);

                TempData["Message"] = "There was an issue managing your subscription. Please try again later.";

                var dto = BuildDashboardDTO(userId);
                return View("SubscriptionForm", dto);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
