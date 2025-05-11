using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebThermoThinApp.Data;

namespace WebThermoThinApp.Controllers
{

        // Контроллер для управления аутентификацией пользователей
        public class AuthController : Controller
        {
            // Контекст базы данных для доступа к данным о пользователях
            private readonly ThermoThinContext _context;

            // Конструктор контроллера, принимает контекст базы данных через dependency injection
            public AuthController(ThermoThinContext context)
            {
                _context = context;
            }

            // Метод для выхода пользователя из системы
            public async Task<IActionResult> Logout()
            {
                // Завершение аутентификации и удаление информации о пользователе из контекста
                await HttpContext.SignOutAsync();

                // Перенаправление на главную страницу
                return RedirectToAction("Index", "Home");
            }

            // Метод для обработки POST-запросов на вход в систему
            [HttpPost]
            public async Task<IActionResult> Index(string login, string password)
            {
                // Проверка введённых данных (логина и пароля) в базе данных
                var user = _context.Users.FirstOrDefault(x => x.Login == login && x.Password == password);
                if (user != null) // Если пользователь найден
                {
                    // Формирование списка утверждений (claims), представляющих пользователя
                    var claims = new List<Claim> {
                    new("UserId", user.Id.ToString()), // Уникальный идентификатор пользователя
                    new Claim(ClaimTypes.Name, login) // Имя пользователя
                };

                    // Создание объекта ClaimsIdentity для сохранения данных об аутентификации
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                    // Выполнение входа с использованием cookie-аутентификации
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                }

                // Перенаправление на главную страницу
                return RedirectToAction("Index", "Home");
            }

            // Метод для отображения страницы входа (обработка GET-запроса)
            [HttpGet]
            public IActionResult Index()
            {
                // Возвращает представление страницы входа
                return View();
            }
        }
}
