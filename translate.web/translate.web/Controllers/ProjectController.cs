﻿using System;
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
using Newtonsoft.Json;
using translate.web.Data;
using translate.web.Models;
using translate.web.Resources;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    [Route("Project/{projectId}")]
    [Authorize(Roles = "Administrator, Translator, Webmaster")]
    public class ProjectController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ApplContext _context;
        private readonly ILogger<ProjectController> _logger;
        private readonly IHostingEnvironment _environment;
        private readonly IFileProvider _fileProvider;
        private readonly LocService _locService;

        public ProjectController(UserManager<AppIdentityUser> userManager,
            ApplContext context, ILogger<ProjectController> logger,
            IHostingEnvironment environment,
            IFileProvider fileProvider,
            LocService locService)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _environment = environment;
            _fileProvider = fileProvider;
            _locService = locService;
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
            ViewBag.projectId = projectId;

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
                    ProjectId = model.ProjectId,
                    ShowOnlyMine = false
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
            var hasDoc = _context.Projects.Where(w => w.Id == projectId).SingleOrDefault().HasDocument;


            ViewBag.hasDoc = (hasDoc == true) ? true : false;

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
                var language = await _context.Languages.Where(w => w.Id == model.LanguageId).SingleOrDefaultAsync();
                var translator = await _userManager.FindByIdAsync(model.TranslatorId.ToString());
                var project = await _context.Projects.Where(w => w.Id == projectId).SingleOrDefaultAsync();

                ProjectDocument document = null;

                if (model.HasDoc)
                {
                    document = _context.ProjectDocuments.Where(w => w.ProjectId == projectId)
                        .Include(i => i.ProjectDocumentDictionarys)
                        .SingleOrDefault();
                }

                var result = new Translation
                {
                    Title = model.Title,
                    IsCompleted = false,
                    Language = language,
                    Translator = translator,
                    AddedDate = DateTime.Now,
                    HasDocument = model.HasDoc,
                    Project = project
                };
                _context.Add(result);

                if (_context.SaveChanges() > 0)
                {
                    if (model.HasDoc)
                    {
                        foreach (var item in document.ProjectDocumentDictionarys)
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

                        _context.SaveChanges();
                    }
                    return RedirectToAction("Index");
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

        [Route("GetNewWordAsync")]
        public IActionResult GetNewWordAsync(Guid projectId, [FromQuery] Guid id)
        {
            InsertWordViewModel model = new InsertWordViewModel { Project = projectId, Translation = id};

            return ViewComponent("NewWord", model);
        }

        [Route("CreateNewWord")]
        [HttpPost]
        public IActionResult CreateNewWord(Guid projectId, [FromBody] CreateNewWordViewModel model)
        {
            var translation = _context.Translations.Where(w => w.Id == model.TranslationId).SingleOrDefault();

            var result = new TranslationDictionary { Name = model.KeyValue, GivenValue = model.ValueValue, NewValue = model.ValueValue, Translations = translation };
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
                TempData["message"] = $"{_locService.GetLocalizedHtmlString("noDocAttached")}";
                return RedirectToAction("NewDocument");
            }
            if (model.File.Length > 11000000)
            {
                TempData["message"] = $"{_locService.GetLocalizedHtmlString("maxSizeValidation")}";
                return RedirectToAction("NewDocument");
            }

            var docType = await _context.DocumentTypes.Where(w => w.Id == model.DocumentTypeId).SingleOrDefaultAsync();

            var type = model.File.FileName.Substring(model.File.FileName.Length - 5);

            var extArray = model.File.FileName.Split(".");
            var ext = $".{extArray.Last()}";

            if (ext != docType.Name)
            {
                TempData["message"] = $"{_locService.GetLocalizedHtmlString("formatsValidation")}";
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
                TempData["message"] = $"{_locService.GetLocalizedHtmlString("docexists")}";
                return RedirectToAction("NewDocument");
            }

            switch (docType.Name)
            {
                case ".resx":
                    await LoadResxDocumentAsync(projectId, model, docName);
                    return RedirectToAction("ProjectDocument");
                case ".xml":
                    await LoadXmlDocumentAsync(projectId, model, docName);
                    return RedirectToAction("ProjectDocument");
                default:
                    TempData["message"] = $"{_locService.GetLocalizedHtmlString("falseFormat")}";
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
                    DocumentType = doc,
                    AddedDate = DateTime.Now
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

                    var pro = _context.Projects.Where(w => w.Id == projectId).FirstOrDefault();
                    pro.HasDocument = true;

                    _context.Update(pro);

                    await _context.SaveChangesAsync();

                   // TempData["messo"] = $"{_locService.GetLocalizedHtmlString("documentUpload")}";
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
                    DocumentType = doc,
                    AddedDate = DateTime.Now
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

                    TempData["messo"] = $"{_locService.GetLocalizedHtmlString("documentUpload")}";
                }
            }
        }

        [HttpGet]
        [Route("Editor/{id}")]
        public async Task<IActionResult> Editor(Guid ProjectId, Guid id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var hasDocument = _context.Translations.Where(w => w.Id == id).SingleOrDefault().HasDocument;

            if (hasDocument)
            {
                ViewBag.language = _context.ProjectDocuments.Where(w => w.ProjectId == ProjectId)
                    .Include(i=>i.Language)
                    .SingleOrDefault().Language.Name;
            }

            var model = await _context.Translations.Where(x => x.Id == id)
                .Include(i=>i.Language)
                .SingleOrDefaultAsync();

            if (model.TranslatorId == user.Id)
            {
                PopulateDocumentsTypesDropDown();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("ReviewTranslation")]
        public async Task<IActionResult> ReviewTranslation(Guid ProjectId, Guid id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var translation = _context.Translations.Where(w => w.Id == id)
                .Include(i=>i.TranslationDictionarys)
                .SingleOrDefault().TranslationDictionarys.FirstOrDefault();
            if (translation != null)
            {
              //  var model = await _context.TranslationDictionarys.Where(w => w.TranslationId == id && w.NewValue == null).ToListAsync();
                ViewBag.translationId = id;
                ViewBag.projectid = ProjectId;
                PopulateDocumentsTypesDropDown();
                return View(/*model*/);
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
                //.Include(i => i.Document)
                //    .ThenInclude(t => t.DocumentType)
                .SingleOrDefault()/*.Document.DocumentType.Name*/;

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
            var translation = await _context.Translations.Where(w => w.Id == model.Id).SingleOrDefaultAsync();
            
            translation.IsWaiting = false;
            translation.IsCompleted = true;
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
            var model = await _context.TranslationDictionarys.Where(w => w.TranslationId == tranId && w.NewValue != null).ToListAsync();
            return new JsonResult(model);
        }

        [HttpGet]
        [Route("ProjectMembers")]
        public async  Task<IActionResult> ProjectMembers(Guid ProjectId)
        {
            var user = _userManager.GetUserId(HttpContext.User);
            ViewBag.creator = _context.ProjectMembers.Where(x => x.ProjectId == ProjectId && x.EmployeeId.ToString() == user).Single().IsCreator;
            ViewBag.userId = user;
            ViewBag.project = ProjectId;

            var model = await _context.ProjectMembers.Where(w => w.ProjectId == ProjectId && w.AcceptedInvitation == true)
                .Include(i=>i.Employee)
                .OrderByDescending(o=>o.JoinDate)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        [Route("Translations")]
        public IActionResult Translations(Guid ProjectId)
        {
            ViewBag.projectId = ProjectId;

            return View();
        }

        [HttpGet]
        [Route("GetDocsCountAsync")]
        public IActionResult GetDocsCountAsync(Guid ProjectId)
        {
            var closed = _context.Translations.Where(w => w.IsCompleted == true && w.ProjectId == ProjectId).Count();
            var open = _context.Translations.Where(w => w.ProjectId == ProjectId && w.IsCompleted == false).Count();
            var waiting = _context.Translations.Where(w => w.IsWaiting == true && w.ProjectId == ProjectId).Count();

            var result = new TranslationsDocsViewModel { ClosedTtranslations = closed, OpenTranslations = open, WaitingTranslations = waiting };

            string json = JsonConvert.SerializeObject(result);
            return new JsonResult(json);
        }

        [HttpGet]
        [Route("GetWordsCountAsync")]
        public IActionResult GetWordsCountAsync(Guid ProjectId)
        {
            var translated = _context.TranslationDictionarys.Where(w => w.Translations.ProjectId == ProjectId && w.NewValue != null).Count();
            var leftTranslate = _context.TranslationDictionarys.Where(w => w.Translations.ProjectId == ProjectId && w.NewValue == null).Count();

            var result = new TranslationsWordsViewModel { TranslatedCount = translated, LeftToTranslateCount = leftTranslate };

            string json = JsonConvert.SerializeObject(result);
            return new JsonResult(json);
        }


        [Route("DownloadTranslation")]
        public IActionResult DownloadTranslation(Guid ProjectId, Guid id)
        {
            //var docType = _context.Translations.Where(w => w.Id == id)
            //    //.Include(i=>i.Document)
            //    //    .ThenInclude(t=>t.DocumentType)
            //    .SingleOrDefault()/*.Document.DocumentType.Name*/;

            switch (/*docType*/"")
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
          //  var document = _context.ProjectDocuments.Where(x => x.Id == translation.DocumentId).SingleOrDefault();
            
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
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 2;

            doc.WriteTo(writer);
            writer.Flush();

            stream.Position = 0;

            return File(stream, "text/xml", /*translation.FileName*/"");
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

        [HttpPost]
        [Route("ChangeFilterState")]
        public async Task<IActionResult> ChangeFilterState(Guid projectId, [FromBody] FilterStateViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var projectMember = await _context.ProjectMembers.Where(w => w.ProjectId == projectId && w.EmployeeId == user.Id).FirstOrDefaultAsync();

            if (projectMember != null)
            {
                projectMember.ShowOnlyMine = model.FilterState;
                _context.Update(projectMember);
                await _context.SaveChangesAsync();

                return new JsonResult("success");
            }

            return BadRequest();
        }

        [Route("ReloadLocalesListAsync")]
        public IActionResult ReloadLocalesListAsync(Guid projectId)
        {
            return ViewComponent("LocalesListIndex", new { ProjectId = projectId });
        }

        [Route("ProjectDocument")]
        [HttpGet]
        public IActionResult ProjectDocument(Guid projectId)
        {
            var hasDoc = _context.Projects.Where(w => w.Id == projectId)
                .Include(i=>i.ProjectDocument)
                    .ThenInclude(t=>t.Language)
                .FirstOrDefault();

            var lang = "";
            if (hasDoc.HasDocument == true)
            {
                lang = hasDoc.ProjectDocument.Language.Name;
            }

            PrDocViewModel model = new PrDocViewModel { HasDoc = hasDoc.HasDocument, Language = lang };

            ViewBag.projectId = projectId;
            PopulateDocumentsTypesDropDown();

            return View(model);
        }

        [Route("GetDocWordsAsync")]
        [HttpGet]
        public IActionResult GetDocWordsAsync(Guid projectId)
        {
            var model = _context.ProjectDocumentDictionarys.Where(w => w.Document.ProjectId == projectId).ToList();

            var result = model.Select(s => new { key = s.Name, value = s.Value }).ToList();

            return new JsonResult(result);
        }

        [Route("DownloadDocAsync")]
        public IActionResult DownloadDocAsync(Guid projectId, [FromQuery] DownloadDocViewModel model)
        {
            var extension = _context.DocumentTypes.Where(w => w.Id == model.ExtensionId).FirstOrDefault();
            var docId = _context.Projects.Where(w => w.Id == projectId)
                .Include(i=>i.ProjectDocument)
                .FirstOrDefault().ProjectDocument.Id;

            switch (extension.Name)
            {
                case ".xml":
                    return DownloadXmlAjax(projectId, docId, model.Title);
                default:
                    return BadRequest();
            }
        }

        [Route("DownloadTranslationAsync")]
        public IActionResult DownloadTranslationAsync(Guid projectId, [FromQuery] DownloadTranslationViewModel model)
        {
            var extension = _context.DocumentTypes.Where(w => w.Id == model.ExtensionId).FirstOrDefault();
            switch (extension.Name)
            {
                case ".xml":
                    return DownloadTranslationXml(projectId, model.Id, model.Title);
                default:
                    return BadRequest();
            }
        }

        private IActionResult DownloadTranslationXml(Guid projectId, Guid id, string title)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "utf-8";

            doc.AppendChild(xmldecl);

            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            var dictionary = _context.TranslationDictionarys.Where(w => w.TranslationId == id && w.NewValue != null).ToList();

            foreach (var item in dictionary)
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
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 2;

            doc.WriteTo(writer);
            writer.Flush();

            stream.Position = 0;

            return File(stream, "text/xml", $"{title}.xml");
        }

        private IActionResult DownloadXmlAjax(Guid projectId, Guid docId, string title)
        {
            //var projectDocument = _context.ProjectDocuments.Where(x => x.Id == id).SingleOrDefault();

            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "utf-8";

            doc.AppendChild(xmldecl);

            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            var dictionary = _context.ProjectDocumentDictionarys.Where(w => w.DocumentId == docId).ToList();

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
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 2;

            doc.WriteTo(writer);
            writer.Flush();

            stream.Position = 0;

            return File(stream, "text/xml", $"{title}.xml");
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
            writer.Formatting = System.Xml.Formatting.Indented;
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
            writer.Formatting = System.Xml.Formatting.Indented;
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
           // var document = _context.ProjectDocuments.Where(x => x.Id == what.DocumentId).SingleOrDefault();

            XmlDocument headerXml = new XmlDocument();

          //  headerXml.LoadXml(document.Header);

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
            writer.Formatting = System.Xml.Formatting.Indented;
            writer.Indentation = 2;

            doc.WriteTo(writer);
            writer.Flush();

            stream.Position = 0;
            
            return File(stream, "text/xml", /*what.FileName*/"");
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
