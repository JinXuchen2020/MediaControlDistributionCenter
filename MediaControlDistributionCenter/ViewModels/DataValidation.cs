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
                    var response = Task.Run(async () => await userService.GetAll(query)).Result?.Data?.ToList() ?? new List<UserDto>();

                    if (!response.Any(c => c.Id != userViewModel.Id))
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
                    var responseDevice = Task.Run(async () => await monitorService.GetAll(queryDevice)).Result?.Data?.ToList() ?? new List<MonitorDto>();

                    if (!responseDevice.Any(c => c.Id != deviceViewModel.Id))
                    {
                        return ValidationResult.Success;
                    }

                    errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_105");
                    break;
                case var o when o is ProgramViewModel programViewModel:
                    var queryProgram = new ProgramDto()
                    {
                        UserAccount = programViewModel.UserId
                    };
                    var proProgram = queryProgram.GetType().GetProperty(context.MemberName!);
                    if (proProgram != null)
                    {
                        proProgram.SetValue(queryProgram, input);
                    }
                    var programService = Utility.GetService<IProgramService>();
                    var responseProgram = Task.Run(async () => await programService.GetAll(queryProgram)).Result?.Data?.ToList() ?? new List<ProgramDto>();

                    if (!responseProgram.Any(c => c.Id != programViewModel.Id))
                    {
                        return ValidationResult.Success;
                    }

                    errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_105");
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
            switch (context.ObjectInstance)
            {
                case var o when o is UserViewModel userViewModel:
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
                    break;
                case var o when o is DeviceViewModel deviceViewModel:
                    switch (context.MemberName)
                    {
                        case "Name":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_106");
                            break;
                        case "SNumber":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_107");
                            break;
                        case "Width":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_108");
                            break;
                        case "Height":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_109");
                            break;
                        case "StartDate":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_110");
                            break;
                        case "EndDate":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_111");
                            break;
                    }
                    break;
                case var o when o is ProgramViewModel programViewModel:
                    switch (context.MemberName)
                    {
                        case "Name":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_106");
                            break;
                        case "Type":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_112");
                            break;
                        case "Width":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_108");
                            break;
                        case "Height":
                            errorMessage = (string)LanguageTool.Instance.FindResource("LanguageKey_Code_Error_Tooltip_109");
                            break;
                    }
                    break;
            }

            return new(errorMessage);
        }
    }
}
