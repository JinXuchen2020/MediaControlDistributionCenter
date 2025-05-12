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
        public static ValidationResult ValidateAccount(string account, ValidationContext context)
        {
            UserViewModel instance = (UserViewModel)context.ObjectInstance;
            var userService = Utility.GetService<IUserService>();
            var response = Task.Run(async () => await userService.GetAll(new UserDto { Account = account })).Result?.Data?.ToList() ?? new List<UserDto>(); // (await userService.GetAll(new UserDto { Account = account })).Data?.ToList() ?? new List<UserDto>();
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
    }
}
