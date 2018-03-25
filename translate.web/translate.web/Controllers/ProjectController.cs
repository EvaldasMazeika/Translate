using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;
using translate.web.Models;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    [Route("Project/{projectId}")]
    [Authorize(Roles = "Administrator, Translator")]
    public class ProjectController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ApplContext _context;

        public ProjectController(UserManager<AppIdentityUser> userManager, ApplContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(Guid projectId)
        {
            if (String.IsNullOrEmpty(projectId.ToString()))
            {
                return RedirectToAction("Projects", "Home");
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var exist = await _context.ProjectMembers.Where(x => x.Employee == user && x.ProjectId == projectId && x.AcceptedInvitation == true).SingleOrDefaultAsync();

            if (exist == null)
            {
                return RedirectToAction("Projects", "Home");
            }

            var model = await _context.Projects.Where(a => a.Id == projectId).SingleOrDefaultAsync();

            return View(model);
        }

        [HttpPost]
        [Route("AddToProjectAsync")]
        public async Task<IActionResult> AddToProjectAsync([FromBody] AddToProjectViewModel model)
        {
            if (model.Email == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // TODO: CHECK IF IN LIST AND SEND FAILURE

            if (user != null)
            {
                var result = new ProjectMember
                {
                    Employee = user,
                    IsCreator = false,
                    AcceptedInvitation = false,
                    ProjectId = model.ProjectId
                };

                _context.Add(result);
                if (_context.SaveChanges() > 0)
                {
                    return new JsonResult("success");
                }
                else
                {
                    return BadRequest();
                }
            }
            return NotFound();
        }

        [HttpPost]
        [Route("RemoveFromProjectAsync")]
        public async Task<IActionResult> RemoveFromProjectAsync([FromBody] RemoveFromProjectViewModel model)
        {
            if (model.MemberId == null || model.ProjectId == null)
            {
                return BadRequest();
            }

            var result = await _context.ProjectMembers.Where(x => x.ProjectId == model.ProjectId && x.EmployeeId == model.MemberId).SingleOrDefaultAsync();

            if (result == null)
            {
                return BadRequest();
            }

            _context.Remove(result);

            if (_context.SaveChanges() > 0)
            {
                return new JsonResult("success");
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("TeamMembers")]
        public IActionResult TeamMembers(Guid projectId)
        {
            return ViewComponent("ProjectMembers", projectId);
        }

        [Route("NewLocale")]
        [HttpGet]
        public async Task<IActionResult> NewLocale(Guid projectId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var isCreator = _context.ProjectMembers.Where(x => x.ProjectId == projectId && x.EmployeeId == user.Id).Single().IsCreator;
            if (isCreator == false)
            {
                ViewBag.creator = false;
                ViewBag.id = user.Id;
            }
            else
            {
                ViewBag.creator = true;
            }

            PopulateLanguagesDropDown();
            PopulateDocumentsDropDown(project: projectId);
            PopulateTranslatorsDropDown(project: projectId);
            return View();
        }

        private void PopulateTranslatorsDropDown(Guid project, object selected = null)
        {
            IQueryable translators = _context.ProjectMembers.Include(a => a.Employee).Where(x => x.ProjectId == project).Select(s => new { Id = s.EmployeeId, Name = s.Employee.Name + " " + s.Employee.Surname });

            ViewBag.TranslatorId = new SelectList(translators, "Id", "Name", selected);
        }

        [Route("NewLocale")]
        [HttpPost]
        public async Task<IActionResult> NewLocale(Guid projectId, NewTranslationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doc = await _context.ProjectDocuments.Where(x => x.Id == model.DocumentId).Include(a => a.ProjectDocumentDictionarys).SingleOrDefaultAsync();
                var lang = await _context.Languages.Where(x => x.Id == model.LanguageId).SingleOrDefaultAsync();
                var translator = await _userManager.FindByIdAsync(model.TranslatorId.ToString());

                var result = new Translation
                {
                    Title = model.Title,
                    Description = model.Description,
                    Document = doc,
                    IsCompleted = false,
                    Language = lang,
                    Translator = translator
                };
                _context.Add(result);

                if (_context.SaveChanges() > 0)
                {
                    foreach (var item in doc.ProjectDocumentDictionarys)
                    {
                        var it = new TranslationDictionary
                        {
                            Translations = result,
                            Name = item.Name,
                            GivenValue = item.Value,
                            NewValue = null
                        };

                        _context.Add(it);
                    }

                    if (_context.SaveChanges() > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }

            return View(model);
        }

        [Route("GetEditorAsync")]
        public async Task<IActionResult> GetEditorAsync([FromQuery] Guid id)
        {
            var model = await _context.TranslationDictionarys.Where(x => x.Id == id).SingleOrDefaultAsync();

            return ViewComponent("EditorPanel", model);
        }

        private void PopulateDocumentsDropDown(Guid project, object selected = null)
        {
            var query = from d in _context.ProjectDocuments.Where(x => x.ProjectId == project && x.IsLoaded == true)
                        orderby d.Name
                        select d;
            ViewBag.DocumentId = new SelectList(query.AsNoTracking(), "Id", "Name", selected);
        }

        [Route("GetEditorListAsync")]
        public IActionResult GetEditorListAsync([FromQuery] Guid id)
        {
            return ViewComponent("WordsList", id);
        }

        [Route("NewDocument")]
        [HttpGet]
        public IActionResult NewDocument(Guid projectId)
        {
            PopulateLanguagesDropDown();
            return View();
        }

        private void PopulateLanguagesDropDown(object selected = null)
        {
            var query = from d in _context.Languages
                        orderby d.Name
                        select d;
            ViewBag.LanguageId = new SelectList(query.AsNoTracking(), "Id", "Name", selected);
        }

        [Route("NewDocument")]
        [HttpPost]
        public async Task<IActionResult> NewDocument(Guid projectId, UploadFileViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return Content("file not selected");
            }

            var type = model.File.FileName.Substring(model.File.FileName.Length - 5);

            if (type != ".resx")
            {
                TempData["message"] = "Netinkamas formatas.";
                return RedirectToAction("NewDocument");
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Uploads", projectId.ToString());
            var itemPath = Path.Combine(folderPath, model.File.FileName);

            if (Directory.Exists(folderPath))
            {
                var dirInfo = Directory.GetFiles(folderPath);

                if (dirInfo.Contains(itemPath))
                {
                    TempData["message"] = "Toks dokumentas jau egzistuoja.";
                    return RedirectToAction("NewDocument");
                }
            }

            Directory.CreateDirectory(folderPath);

            using (var stream = new FileStream(itemPath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var lang = await _context.Languages.Where(x => x.Id == model.LanguageId).SingleOrDefaultAsync();
            var project = await _context.Projects.Where(x => x.Id == projectId).SingleOrDefaultAsync();

            var result = new ProjectDocument
            {
                Name = model.File.FileName,
                Language = lang,
                FullPath = itemPath,
                Project = project
            };

            _context.Add(result);

            if (_context.SaveChanges() > 0)
            {
                TempData["messo"] = "Sėkmingai dokumentas įkeltas.";
                return RedirectToAction("NewDocument");
            }

            return RedirectToAction("NewDocument");
        }

        [HttpGet]
        [Route("LoadDoc")]
        public async Task<IActionResult> LoadDoc(Guid ProjectId, Guid id)
        {
            var doc = await _context.ProjectDocuments.Where(x => x.Id == id).SingleOrDefaultAsync();
            XmlDocument docs = new XmlDocument();
            docs.Load(doc.FullPath);
            XmlNodeList node = docs.GetElementsByTagName("data");
            foreach (XmlNode item in node)
            {
                var nodeId = item.Attributes["name"].Value;
                var nodeValue = item.SelectSingleNode("value").InnerText;
                var dictionary = new ProjectDocumentDictionary
                {
                    Document = doc,
                    Name = nodeId,
                    Value = nodeValue
                };
                _context.Add(dictionary);
            }

            doc.IsLoaded = true;
            _context.Update(doc);

            if (_context.SaveChanges() > 0)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Editor/{id}")]
        public async Task<IActionResult> Editor(Guid ProjectId, Guid id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var model = await _context.Translations.Where(x => x.Id == id)
                .Include(a => a.Document)
                .ThenInclude(z => z.Project).SingleOrDefaultAsync();

            if (model.TranslatorId == user.Id)
            {
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [Route("AddWordToDict")]
        public async Task<IActionResult> AddWordToDict([FromBody] NewWordViewModel model)
        {
            var word = await _context.TranslationDictionarys.Where(x => x.Id == model.Id).SingleOrDefaultAsync();

            word.NewValue = model.NewWord;
            _context.Update(word);

            if (_context.SaveChanges() > 0)
            {
                return new JsonResult("success");
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("CompleteTranslation")]
        public async Task<IActionResult> CompleteTranslation(Guid transId)
        {
            var translation = await _context.Translations.Where(x => x.Id == transId).SingleOrDefaultAsync();

            translation.IsCompleted = true;
            _context.Update(translation);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [Route("DownloadTranslation")]
        public IActionResult DownloadTranslation(Guid ProjectId, Guid id)
        {
            return ExportXML(id);
        }

        [HttpPost]
        [Route("DeleteTranslation")]
        public async Task<IActionResult> DeleteTranslation([FromBody] Translation model)
        {
            var translation = await _context.Translations.Where(x => x.Id == model.Id).SingleOrDefaultAsync();
            if (translation != null)
            {
                _context.Remove(translation);
                if (_context.SaveChanges() > 0)
                {
                    return new JsonResult("success");
                }
            }
            return BadRequest();
        }


        [HttpGet]
        [Route("DownloadDoc")]
        public async Task<IActionResult> DownloadDoc(Guid ProjectId, Guid id)
        {
            var result = await _context.ProjectDocuments.Where(x => x.Id == id).SingleOrDefaultAsync();

            var path = result.FullPath;

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }



        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".resx", "text/xml" }
            };
        }

        private FileStreamResult ExportXML(Guid mineId)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "utf-8";

            doc.AppendChild(xmldecl);

            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            var what = _context.Translations.Where(x => x.Id == mineId).SingleOrDefault();
            var document = _context.ProjectDocuments.Where(x => x.Id == what.DocumentId).SingleOrDefault();
            XmlDocument docs = new XmlDocument();
            docs.Load(document.FullPath);

            //schema add
            XmlNodeList nodeScheme = docs.GetElementsByTagName("xsd:schema");
            foreach (XmlNode item in nodeScheme)
            {
                //doc.AppendChild(item);
                doc.DocumentElement.AppendChild(doc.ImportNode(item, true));
            }

            //resheader add
            XmlNodeList node = docs.GetElementsByTagName("resheader");

            foreach (XmlNode item in node)
            {
                //doc.AppendChild(item);
                doc.DocumentElement.AppendChild(doc.ImportNode(item, true));
            }

            // data add
            var translations = _context.TranslationDictionarys.Where(x => x.TranslationId == mineId).ToList();

            foreach (var item in translations)
            {
                XmlElement trans = doc.CreateElement("data");
                var atrName = doc.CreateAttribute("name");
                var space = doc.CreateAttribute("xml:space");
                atrName.Value = item.Name;
                space.Value = "preserve";
                trans.Attributes.Append(atrName);
                trans.Attributes.Append(space);
                var word = doc.CreateElement("value");
                word.InnerText = item.NewValue;
                trans.AppendChild(word);
                root.AppendChild(trans);
            }



            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;

            doc.WriteTo(writer);
            writer.Flush();

            stream.Position = 0;
            


            return File(stream, "text/xml", "Books.resx");



        }
        string FormatXml(string xml)
        {
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }
    }
}
