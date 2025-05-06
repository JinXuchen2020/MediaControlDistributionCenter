using MediaControlDistributionCenter.Services.DTO.Models;
using MediaControlDistributionCenter.Services;
using MediaControlDistributionCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace MediaControlDistributionCenter.Helpers
{
    public class DataValidation
    {
        public static async Task<ValidationResult> ValidateAccount(string account, ValidationContext context)
        {
            UserViewModel instance = (UserViewModel)context.ObjectInstance;
            var userService = GetService<IUserService>();
            var response = (await userService.GetAll(new UserDto { Account = account })).Data?.ToList() ?? new List<UserDto>();
            bool isValid = response.Where(c => c.Id != instance.Id).Count() == 0;

            if (isValid)
            {
                return ValidationResult.Success;
            }

            var errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_101");
            return new(errorMessage);
        }

        public static ValidationResult RequiredValidation(string input, ValidationContext context)
        {
            bool isValid = !string.IsNullOrEmpty(input);

            if (isValid)
            {
                return ValidationResult.Success;
            }

            string errorMessage = string.Empty;
            if (context.ObjectInstance is UserViewModel)
            {
                switch (context.MemberName)
                {
                    case "Account":
                        errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_100");
                        break;
                    case "Name":
                        errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_103");
                        break;
                    case "Password":
                        errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_102");
                        break;
                }
            }

            return new(errorMessage);
        }

        protected static T GetService<T>() where T : class
        {
            var connectionMode = App.ServicesProvider.GetRequiredService<ConnectionMode>();
            switch (connectionMode.Mode)
            {
                case "Local":
                    return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                case "Remote":
                    if (string.IsNullOrEmpty(connectionMode.ServiceUri))
                    {
                        return App.ServicesProvider.GetServices<T>().First(c => c.GetType().Name.EndsWith("Local"));
                    }
                    else
                    {
                        return App.ServicesProvider.GetServices<T>().First(c => !c.GetType().Name.EndsWith("Local"));
                    }
                default:
                    throw new ArgumentException("未知的服务名称");
            }
        }
    }
}
