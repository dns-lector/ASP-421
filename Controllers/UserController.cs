using ASP_421.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_421.Controllers
{
    public class UserController : Controller
    {
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