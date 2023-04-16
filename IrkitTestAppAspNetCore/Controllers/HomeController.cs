using IrkitTestAppAspNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace IrkitTestAppAspNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        irkitdbContext irkitdbContext = new irkitdbContext();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> addNewEmployee(IFormFile xmlFile)
        {

            if (Path.GetExtension(xmlFile.FileName) != ".xml")
            {
                ViewBag.ErrorMessage = "Это не xml";
                return View("Index", irkitdbContext.Employees.FirstOrDefault());
            }

            string filePath = Path.Combine(Path.GetTempPath(), xmlFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await xmlFile.CopyToAsync(stream);
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            Employee employee = new Employee();
            XmlNodeList nodes = xmlDocument.SelectNodes("//CardDocument");

            foreach (XmlNode node in nodes)
            {
                // Поиск максимального id
                int? MaxID = irkitdbContext.Employees.Max(p => p.Id);
                if (MaxID == null)
                {
                    employee.Id = 1;
                }
                else
                {
                    employee.Id = (int)MaxID + 1;
                }
                // Присвоение атрибутов xml файла в сущность класса employee
                employee.FirstName = xmlDocument.SelectSingleNode("//MainInfo/@FirstName").Value;
                employee.RegDate = DateTime.Parse(node.SelectSingleNode("//MainInfo/@RegDate").Value);
                employee.Content = node.SelectSingleNode("//MainInfo/@Content").Value;
                employee.KindName = node.SelectSingleNode("//System/@Kind_Name").Value;
                employee.ReferenceList = node.SelectSingleNode("//MainInfo/@ReferenceList").Value;
                employee.AuthorId = node.SelectSingleNode("//MainInfo/@Author").Value;

                XmlElement? xmlElement = xmlDocument.DocumentElement;
                XmlNodeList? personNodes = xmlElement?.SelectNodes("//EmployeesRow");

                foreach (XmlNode _node in personNodes) // Вычисление должности сотрудника по полю RowID
                {
                    if (employee.AuthorId == _node.SelectSingleNode("@RowID")?.Value)
                    {
                        employee.Performer = _node.SelectSingleNode("@PositionName")?.Value;
                    }
                }
                // Добавление и сохранение нового сотрудника в базу данных
                irkitdbContext.Employees.Add(employee);
                var temp = irkitdbContext.Employees.Where(p => p.AuthorId == employee.AuthorId); // Проверка на наличие дублирующихся сущностей в бд (если такая сущность уже имеется в базе то выйдет ошибка)
                if (temp != null)
                {
                    ViewBag.ErrorMessage = "Такой пользователь уже существует";
                }
                else
                {
                    irkitdbContext.SaveChanges();
                    return View("Index", employee);
                }
            }
            return View("Index", irkitdbContext.Employees.FirstOrDefault());
        }

        public IActionResult Index()
        {
            var employee = irkitdbContext.Employees.FirstOrDefault();
            return View(employee);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
