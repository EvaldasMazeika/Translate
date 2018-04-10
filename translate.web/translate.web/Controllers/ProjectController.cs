using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ProjectController> _logger;
        private readonly IHostingEnvironment _environment;
        private readonly IFileProvider _fileProvider;

        public ProjectController(UserManager<AppIdentityUser> userManager,
            ApplContext context, ILogger<ProjectController> logger,
            IHostingEnvironment environment,
            IFileProvider fileProvider)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _environment = environment;
            _fileProvider = fileProvider;
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
                        var word = new TranslationDictionary
                        {
                            Translations = result,
                            Name = item.Name,
                            GivenValue = item.Value,
                            NewValue = null
                        };

                        _context.Add(word);
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
            var query = from d in _context.ProjectDocuments.Where(x => x.ProjectId == project)
                        orderby d.Name
                        select d;
            ViewBag.DocumentId = new SelectList(query.AsNoTracking(), "Id", "Name", selected);
        }

        [Route("GetEditorListAsync")]
        public IActionResult GetEditorListAsync([FromQuery] Guid id, int PageIndex)
        {
            var model = new EditorListViewModel() { Id = id, PageIndex = PageIndex };

            return ViewComponent("WordsList", model);
        }

        [Route("NewDocument")]
        [HttpGet]
        public IActionResult NewDocument(Guid projectId)
        {
            ViewBag.projectId = projectId;
            PopulateLanguagesDropDown();
            PopulateDocumentsTypesDropDown();
            return View();
        }

        [HttpGet]
        [Route("GetExampleAsync")]
        public async Task<IActionResult> GetExampleAsync([FromQuery] Guid Id)
        {
            var docType = await _context.DocumentTypes.Where(w => w.Id == Id).SingleOrDefaultAsync();
            if (docType != null)
            {
                return new JsonResult(docType.Example);
            }

            return BadRequest();
        }

        private void PopulateDocumentsTypesDropDown(object selected = null)
        {
            var query = from d in _context.DocumentTypes
                        orderby d.Name
                        select d;

            ViewBag.DocumentTypeId = new SelectList(query.AsNoTracking(), "Id", "Name", selected);
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
                TempData["message"] = "Nepridėtas dokumentas";
                return RedirectToAction("NewDocument");
            }
            if (model.File.Length > 11000000)
            {
                TempData["message"] = "Failas neturi viršyti 10MB";
                return RedirectToAction("NewDocument");
            }

            var docType = await _context.DocumentTypes.Where(w => w.Id == model.DocumentTypeId).SingleOrDefaultAsync();

            var type = model.File.FileName.Substring(model.File.FileName.Length - 5);

            var extArray = model.File.FileName.Split(".");
            var ext = $".{extArray.Last()}";

            if (ext != docType.Name)
            {
                TempData["message"] = "Formatai nesutampa";
                return RedirectToAction("NewDocument");
            }

            var docName = "";

            if (model.File.FileName.Contains("\\"))
            {
                var array = model.File.FileName.Split("\\");
                docName = array.Last();
            }
            else
            {
                docName = model.File.FileName;
            }

            var exist = await _context.ProjectDocuments.Where(x => x.Name == docName && x.ProjectId == projectId).FirstOrDefaultAsync();

            if (exist != null)
            {
                TempData["message"] = "Toks dokumentas jau egzistuoja";
                return RedirectToAction("NewDocument");
            }

            switch (docType.Name)
            {
                case ".resx":
                    await LoadResxDocumentAsync(projectId, model, docName);
                    return RedirectToAction("NewDocument");
                case ".xml":
                    await LoadXmlDocumentAsync(projectId, model, docName);
                    return RedirectToAction("NewDocument");
                default:
                    TempData["message"] = "Formatas šiuo metu nepalaikomas";
                    return RedirectToAction("NewDocument");
            }
        }

        private async Task LoadXmlDocumentAsync(Guid projectId, UploadFileViewModel model, string docName)
        {
            using (var memoryStream = new MemoryStream())
            {
                await model.File.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                XmlDocument docs = new XmlDocument();
                docs.Load(memoryStream);
                XmlNodeList dataNodes = docs.GetElementsByTagName("data");

                var lang = await _context.Languages.Where(w => w.Id == model.LanguageId).SingleOrDefaultAsync();
                var project = await _context.Projects.Where(x => x.Id == projectId).SingleOrDefaultAsync();
                var doc = await _context.DocumentTypes.Where(w => w.Id == model.DocumentTypeId).SingleOrDefaultAsync();

                var projectDocument = new ProjectDocument
                {
                    Name = docName,
                    Language = lang,
                    Project = project,
                    DocumentType = doc
                };

                _context.Add(projectDocument);

                if (_context.SaveChanges() > 0)
                {
                    foreach (XmlNode item in dataNodes)
                    {
                        var nodeId = item.Attributes["name"].Value;
                        var nodeValue = item.SelectSingleNode("value").InnerText;
                        var dictionary = new ProjectDocumentDictionary
                        {
                            Document = projectDocument,
                            Name = nodeId,
                            Value = nodeValue
                        };
                        _context.Add(dictionary);
                    }

                    await _context.SaveChangesAsync();

                    TempData["messo"] = "Dokumentas sėkmingai įkeltas";
                }
            }
        }

        private async Task LoadResxDocumentAsync(Guid projectId, UploadFileViewModel model, string docName)
        {
            using (var memoryStream = new MemoryStream())
            {
                await model.File.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                XmlDocument docs = new XmlDocument();
                docs.Load(memoryStream);
                XmlNodeList nodeScheme = docs.GetElementsByTagName("xsd:schema");
                XmlNodeList node = docs.GetElementsByTagName("resheader");
                XmlNodeList dataNodes = docs.GetElementsByTagName("data");

                var header = nodeScheme.Item(0).OuterXml;

                var sb = new StringBuilder();
                foreach (XmlNode item in node)
                {
                    sb.Append(item.OuterXml);
                }
                var header2 = sb.ToString();
                var finalHeader = String.Concat(header, header2);
                finalHeader = $"<root>{finalHeader}</root>";

                var lang = await _context.Languages.Where(x => x.Id == model.LanguageId).SingleOrDefaultAsync();
                var project = await _context.Projects.Where(x => x.Id == projectId).SingleOrDefaultAsync();
                var doc = await _context.DocumentTypes.Where(w => w.Id == model.DocumentTypeId).SingleOrDefaultAsync();

                var projectDocument = new ProjectDocument
                {
                    Name = docName,
                    Language = lang,
                    Header = finalHeader,
                    Project = project,
                    DocumentType = doc
                };

                _context.Add(projectDocument);

                if (_context.SaveChanges() > 0)
                {
                    foreach (XmlNode item in dataNodes)
                    {
                        var nodeId = item.Attributes["name"].Value;
                        var nodeValue = item.SelectSingleNode("value").InnerText;
                        var dictionary = new ProjectDocumentDictionary
                        {
                            Document = projectDocument,
                            Name = nodeId,
                            Value = nodeValue
                        };
                        _context.Add(dictionary);
                    }

                    await _context.SaveChangesAsync();

                    TempData["messo"] = "Dokumentas sėkmingai įkeltas";
                }
            }
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

        [HttpGet]
        [Route("ReviewTranslation")]
        public async Task<IActionResult> ReviewTranslation(Guid ProjectId, Guid id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var translation = _context.Translations.Where(w => w.Id == id).SingleOrDefault().IsCompleted;
            if (translation)
            {
                var model = await _context.TranslationDictionarys.Where(w => w.TranslationId == id).ToListAsync();
                ViewBag.projectid = ProjectId;

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

        [Route("CheckTranslation")]
        public async Task<IActionResult> CheckTranslation(Guid ProjectId, Guid id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var access = _context.ProjectMembers.Where(w => w.ProjectId == ProjectId && w.EmployeeId == user.Id).SingleOrDefault().IsCreator;
            var translation = _context.Translations.Where(w => w.Id == id).SingleOrDefault().IsWaiting;
            var docType = _context.Translations.Where(w => w.Id == id)
                .Include(i => i.Document)
                    .ThenInclude(t => t.DocumentType)
                .SingleOrDefault().Document.DocumentType.Name;

            if (access && translation)
            {
                var model = await _context.TranslationDictionarys.Where(w => w.TranslationId == id).ToListAsync();
                ViewBag.projectid = ProjectId;
                ViewBag.docType = docType;

                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("CompleteTranslation")]
        public async Task<IActionResult> CompleteTranslation(Guid transId)
        {
            var translation = await _context.Translations.Where(x => x.Id == transId).SingleOrDefaultAsync();

            translation.IsWaiting = true;
            _context.Update(translation);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("VerifyTranslation")]
        public async Task<IActionResult> VerifyTranslation(Guid ProjectId, [FromBody] VerifyTranslationViewModel model)
        {
            var translation = await _context.Translations.Where(w => w.Id == model.Id)
                .Include(i=>i.Document)
                    .ThenInclude(t=>t.DocumentType)
                .SingleOrDefaultAsync();
            

            translation.IsWaiting = false;
            translation.IsCompleted = true;
            translation.FileName = $"{model.Name}{translation.Document.DocumentType.Name}";
            _context.Update(translation);

            await _context.SaveChangesAsync();
            return new JsonResult("success");
        }

        [HttpPost]
        [Route("DeclineTranslation")]
        public async Task<IActionResult> DeclineTranslation(Guid ProjectId, [FromBody] DeclineTranslationViewModel model)
        {
            var translation = await _context.Translations.Where(w => w.Id == model.Id).SingleOrDefaultAsync();

            if (translation != null)
            {
                translation.IsWaiting = false;
                translation.DeclineComment = model.Message;
                _context.Update(translation);

                await _context.SaveChangesAsync();
                return new JsonResult("success");
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("GetTranslationsAjax")]
        public async Task<IActionResult> GetTranslationsAjax(Guid ProjectId, [FromQuery] Guid tranId)
        {
            var model = await _context.TranslationDictionarys.Where(w => w.TranslationId == tranId).ToListAsync();
            return new JsonResult(model);
        }

        [Route("DownloadTranslation")]
        public IActionResult DownloadTranslation(Guid ProjectId, Guid id)
        {
            var docType = _context.Translations.Where(w => w.Id == id)
                .Include(i=>i.Document)
                    .ThenInclude(t=>t.DocumentType)
                .SingleOrDefault().Document.DocumentType.Name;

            switch (docType)
            {
                case ".resx":
                    return ExportResx(id);
                case ".xml":
                    return ExportXml(id);
                default:
                    return View("index");
            }      
        }

        private FileStreamResult ExportXml(Guid mineId)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "utf-8";

            doc.AppendChild(xmldecl);

            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            var translation = _context.Translations.Where(x => x.Id == mineId).SingleOrDefault();
            var document = _context.ProjectDocuments.Where(x => x.Id == translation.DocumentId).SingleOrDefault();
            
            var translations = _context.TranslationDictionarys.Where(x => x.TranslationId == mineId).ToList();

            foreach (var item in translations)
            {
                XmlElement trans = doc.CreateElement("data");
                var atrName = doc.CreateAttribute("name");
                atrName.Value = item.Name;
                trans.Attributes.Append(atrName);
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

            return File(stream, "text/xml", translation.FileName);
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

        [HttpPost]
        [Route("DeleteDocument")]
        public async Task<IActionResult> DeleteDocument([FromBody] ProjectDocument document)
        {
            var doc = await _context.ProjectDocuments.SingleOrDefaultAsync(x => x.Id == document.Id);

            if (doc != null)
            {
                _context.Remove(doc);
                if (_context.SaveChanges() > 0)
                {
                    return new JsonResult("success");
                }
            }
            return BadRequest();
        }


        [HttpGet]
        [Route("DownloadDoc")]
        public IActionResult DownloadDoc(Guid ProjectId, Guid id)
        {
            var docType = _context.ProjectDocuments.Where(w => w.Id == id)
                .Include(i => i.DocumentType)
                .SingleOrDefault().DocumentType.Name;

            switch (docType)
            {
                case ".resx":
                    return DownloadResx(ProjectId, id);
                case ".xml":
                    return DownloadXml(ProjectId, id);
                default:
                    return View("Index");
            }
        }

        private FileStreamResult DownloadXml(Guid projectId, Guid id)
        {
            var projectDocument = _context.ProjectDocuments.Where(x => x.Id == id).SingleOrDefault();

            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "utf-8";

            doc.AppendChild(xmldecl);

            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            var dictionary = _context.ProjectDocumentDictionarys.Where(w => w.DocumentId == id).ToList();

            foreach (var item in dictionary)
            {
                XmlElement trans = doc.CreateElement("data");
                var atrName = doc.CreateAttribute("name");
                atrName.Value = item.Name;
                trans.Attributes.Append(atrName);
                var word = doc.CreateElement("value");
                word.InnerText = item.Value;
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

            return File(stream, "text/xml", projectDocument.Name);
        }

        private FileStreamResult DownloadResx(Guid ProjectId, Guid id)
        {
            var projectDocument = _context.ProjectDocuments.Where(x => x.Id == id).SingleOrDefault();

            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "utf-8";

            doc.AppendChild(xmldecl);

            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            XmlDocument headerXml = new XmlDocument();

            headerXml.LoadXml(projectDocument.Header);

            var rooth = headerXml.FirstChild;

            foreach (XmlNode item in rooth)
            {
                doc.DocumentElement.AppendChild(doc.ImportNode(item, true));
            }

            var dictionary = _context.ProjectDocumentDictionarys.Where(w => w.DocumentId == id).ToList();

            foreach (var item in dictionary)
            {
                XmlElement trans = doc.CreateElement("data");
                var atrName = doc.CreateAttribute("name");
                var space = doc.CreateAttribute("xml:space");
                atrName.Value = item.Name;
                space.Value = "preserve";
                trans.Attributes.Append(atrName);
                trans.Attributes.Append(space);
                var word = doc.CreateElement("value");
                word.InnerText = item.Value;
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

            return File(stream, "text/xml", projectDocument.Name);
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

        private FileStreamResult ExportResx(Guid mineId)
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

            XmlDocument headerXml = new XmlDocument();

            headerXml.LoadXml(document.Header);

            var rooth = headerXml.FirstChild;

            foreach (XmlNode item in rooth)
            {
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
            
            return File(stream, "text/xml", what.FileName);
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
