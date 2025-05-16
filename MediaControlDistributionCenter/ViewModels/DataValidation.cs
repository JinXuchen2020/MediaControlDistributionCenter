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

namespace MediaControlDistributionCenter.ViewModels
{
    public class DataValidation
    {
        public static ValidationResult ValidateAccount(string input, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            var errorMessage = string.Empty;
            switch (instance)
            {
                case var o when o is UserViewModel userViewModel:
                    var query = new UserDto();
                    var property = query.GetType().GetProperty(context.MemberName!);
                    if (property != null)
                    {
                        property.SetValue(query, input);
                    }
                    var userService = Utility.GetService<IUserService>();
                    var response = Task.Run(async () => await userService.GetAll(query)).Result?.Data?.ToList() ?? new List<UserDto>(); // (await userService.GetAll(new UserDto { Account = account })).Data?.ToList() ?? new List<UserDto>();
                    bool isValid = response.Where(c => c.Id != userViewModel.Id).Count() == 0;

                    if (isValid)
                    {
                        return ValidationResult.Success;
                    }

                    errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_101");
                    break;
                case var o when o is DeviceViewModel deviceViewModel:
                    var queryDevice = new MonitorDto();
                    var proDevice = queryDevice.GetType().GetProperty(context.MemberName!);
                    if (proDevice != null)
                    {
                        proDevice.SetValue(queryDevice, input);
                    }
                    var monitorService = Utility.GetService<IMonitorService>();
                    var responseDevice = Task.Run(async () => await monitorService.GetAll(queryDevice)).Result?.Data?.ToList() ?? new List<MonitorDto>(); // (await userService.GetAll(new UserDto { Account = account })).Data?.ToList() ?? new List<UserDto>();

                    if (!responseDevice.Any(c => c.Id != deviceViewModel.Id))
                    {
                        return ValidationResult.Success;
                    }

                    errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_101");
                    break;
            }

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
