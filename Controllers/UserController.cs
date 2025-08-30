using ASP_421.Data;
using ASP_421.Models.User;
using ASP_421.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_421.Controllers
{
    public class UserController(
        DataContext dataContext, 
        IKdfService kdfService) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;

        const String RegisterKey = "RegisterFormModel";

        public IActionResult SignUp()
        {
            UserSignupViewModel viewModel = new();
            
            if(HttpContext.Session.Keys.Contains(RegisterKey))
            {
                UserSignupFormModel formModel =
                    JsonSerializer.Deserialize<UserSignupFormModel>(
                        HttpContext.Session.GetString(RegisterKey)!)!;

                viewModel.FormModel = formModel;
                viewModel.ValidationErrors = ValidateSignupForm(formModel);

                if(viewModel.ValidationErrors.Count == 0)
                {
                    Data.Entities.User user = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel.Name,
                        Email = formModel.Email,
                        Birthdate = formModel.Birthday,
                        RegisteredAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    String salt = Guid.NewGuid().ToString();
                    Data.Entities.UserAccess userAccess = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = "Guest",
                        Login = formModel.Login,
                        Salt = salt,
                        Dk = _kdfService.Dk(formModel.Password, salt),
                    };
                    _dataContext.Users.Add(user);
                    _dataContext.UserAccesses.Add(userAccess);
                    try
                    {
                        _dataContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        viewModel
                            .ValidationErrors[nameof(viewModel.ValidationErrors)] = ex.Message;
                    }
                }

                HttpContext.Session.Remove(RegisterKey);
            }
            return View(viewModel);
        }

        [HttpPost]
        public RedirectToActionResult Register(UserSignupFormModel formModel)
        {
            HttpContext.Session.SetString(
                RegisterKey,
                JsonSerializer.Serialize(formModel));

            return RedirectToAction(nameof(SignUp));
        }

        private Dictionary<String, String> ValidateSignupForm(UserSignupFormModel formModel)
        {
            Dictionary<String, String> res = new();

            if (String.IsNullOrEmpty(formModel.Name))
            {
                res[nameof(formModel.Name)] = "Ім'я не може бути порожнім";
            }
            if (formModel.Login?.Contains(':') ?? false)
            {
                res[nameof(formModel.Login)] = "У логіні не допускається ':' (двокрапка)";
            }
            else if(_dataContext.UserAccesses.Any(ua => ua.Login == formModel.Login))
            {
                res[nameof(formModel.Login)] = "Логін вже у вжитку";
            }
            if (formModel.Password != formModel.Repeat)
            {
                res[nameof(formModel.Repeat)] = "Паролі не збігаються";
            }

            return res;
        }

    
    }
}
/*  Browser           НЕПРАВИЛЬНО               Server
 * POST name=User ----------------------------->  
 *     <---------------------------------------- HTML
 *  Оновити: POST name=User -------------------> ?Conflict - повторні дані
 */

/*  Browser             ПРАВИЛЬНО               Server
 * POST /Register name=User -------------------> Зберігає дані (у сесії)
 *     <------------------302------------------- Redirect /SignUp
 * GET /SignUp  -------------------------------> Відновлює та оброблює дані 
 *     <------------------200------------------- HTML
 * Оновити: GET /SignUp -----------------------> Немає конфлікту    
 */
/* Д.З. Реалізувати повну валідацію даних форми реєстрації користувача:
 * - правильність імені (починається з великої літери, не містить спецзнаки тощо)
 * - правильність E-mail
 * - вимогу до паролю (довжина, склад)
 * Вивести відповідні повідмолення на формі
 */